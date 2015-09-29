using System.Collections.Generic;
using EnvDTE;

namespace CppAutoLib
{

    public class MissingSymbol
    {
        public enum SymbolStatus
        {
            New,
            Found,
            NotFound
        }

        public string MangledName { get; set; }
        public Project Project { get; set; }
        public SymbolStatus Status { get; set; } = SymbolStatus.New;
        public List<string> ContainingLibraries { get; set; } = new List<string>();
    }

}