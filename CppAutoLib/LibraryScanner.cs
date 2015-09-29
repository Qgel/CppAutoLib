using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CppAutoLib
{

    public class LibraryScanner
    {
        public List<MissingSymbol> MissingSymbols { get; } 

        public List<string> Libraries { get; } 

        public LibraryScanner(List<MissingSymbol> missingSymbols, List<string> libraries)
        {
            MissingSymbols = missingSymbols;
            Libraries = libraries;
        }

        public void Scan()
        {
            ThreadPool.QueueUserWorkItem(ign => HandleScan());
        }

        private void HandleScan()
        {
            foreach (string lib in Libraries)
            {
                var archive = new LibArchive(lib);
                if (!archive.IsValid)
                    continue;

                foreach (MissingSymbol symbol in MissingSymbols)
                {
                    if (archive.MangledNames.BinarySearch(symbol.MangledName) >= 0)
                    {
                        symbol.ContainingLibraries.Add(lib);
                        symbol.Status = MissingSymbol.SymbolStatus.Found;
                    }
                }
            }

            foreach (var symbol in MissingSymbols.Where(symb => symb.Status != MissingSymbol.SymbolStatus.Found))
                symbol.Status = MissingSymbol.SymbolStatus.NotFound;
        }
    }

}