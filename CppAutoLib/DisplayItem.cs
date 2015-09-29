using System.Collections.Generic;

namespace CppAutoLib
{
    class DisplayItem
    {
        public bool AddLib { get; set; }
        public List<string> Libraries { get; set; }
        public List<string> Symbols { get; set; }

        public string Project { get; set; }

        public string SymbolCount { get { return Symbols.Count.ToString(); } }
    }
}