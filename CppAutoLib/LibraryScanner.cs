using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CppAutoLib
{
    public class LibraryScanner
    {
        /// <summary>
        /// Equality comparer for list of LibArchives.
        /// This implementation only works in conjunction with LibraryScanner,
        /// and makes explicit use of ordering and the fact that the same objects
        /// will be added to the list.
        /// </summary>
        private class ProposalComparer : IEqualityComparer<List<LibArchive>>
        {
            public bool Equals(List<LibArchive> x, List<LibArchive> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<LibArchive> archives)
            {
                string s = "";
                foreach (var archive in archives)
                    s += archive.Path;
                return s.GetHashCode();
            }
        }


        public List<string> MissingSymbols { get; } 

        public List<LibArchive> Libraries { get; }

        /// <summary>
        /// Mapping from List of libraries to List of symbols, where each Library in the former list
        /// resolved all the symbols in the latter.
        /// </summary>
        private readonly Dictionary<List<LibArchive>, List<string>> _resolutionProposals = new Dictionary<List<LibArchive>, List<string>>(new ProposalComparer());

        /// <summary>
        /// Symbols that could not be resolved
        /// </summary>
        private readonly List<string> _unresolvedSymbols = new List<string>();

        /// <summary>
        /// Return computed resoltions
        /// </summary>
        /// <returns>computed resolutions</returns>
        public List<Resolution> GetResolutions()
        {
            List<Resolution> resolutions = new List<Resolution>();

            foreach (var prop in _resolutionProposals)
            {
                resolutions.Add(new Resolution(prop.Key, prop.Value));
            }

            return resolutions;
        }

        /// <summary>
        /// Symbols that could not be resolved
        /// </summary>
        /// <returns>List of unresolved symbols</returns>
        public List<string> GetUnresolvedSymbols()
        {
            return _unresolvedSymbols;
        } 

        public LibraryScanner(List<string> missingSymbols, List<LibArchive> libraries)
        {
            MissingSymbols = missingSymbols;
            Libraries = libraries;
        }

        public void Scan()
        {
            HandleScan();
            //ThreadPool.QueueUserWorkItem(ign => HandleScan());
        }

        
        private void HandleScan()
        {

            foreach (var symbol in MissingSymbols)
            {
                var containingArchives = new List<LibArchive>();
                foreach (var lib in Libraries)
                {
                    if (!lib.IsValid)
                        continue;

                    if (lib.MangledNames.BinarySearch(symbol) >= 0)
                        containingArchives.Add(lib);                    
                }
                if (containingArchives.Count > 0)
                {
                    AddProposal(containingArchives, symbol);
                }
                else
                {
                    _unresolvedSymbols.Add(symbol);
                }
            }
        }

        /// <summary>
        /// Add a symbol to the list of libs that resolve it.
        /// </summary>
        /// <param name="containingArchives">The libs that resolve the symbol</param>
        /// <param name="symbol">The symbol</param>
        private void AddProposal(List<LibArchive> containingArchives, string symbol)
        {
            if(!_resolutionProposals.ContainsKey(containingArchives))
                _resolutionProposals.Add(containingArchives, new List<string>());
            _resolutionProposals[containingArchives].Add(symbol);
        }

    }

}