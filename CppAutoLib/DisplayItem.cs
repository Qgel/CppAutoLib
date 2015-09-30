using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CppAutoLib.Annotations;
using EnvDTE;

namespace CppAutoLib
{
    public class DisplayItem : INotifyPropertyChanged
    {
        private bool _addLib = true;

        public bool AddLib
        {
            get
            {
                return _addLib;
            }
            set
            {
                _addLib = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex { get; set; }

        public List<string> Libraries { get; set; }
        public List<string> Symbols { get; set; }

        public Resolution Resolution { get; set; }

        public string Project { get; set; }

        public string SymbolCount => Symbols.Count.ToString();

        public static DisplayItem Create(Resolution res)
        {
            return new DisplayItem
            {
                Libraries = res.Libraries.Select(ar => Path.GetFileName(ar.Path)).ToList(),
                Symbols = res.ResolvedSymbols,
                Resolution = res,
                Project = res.Project.Name,
                SelectedIndex = 0
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}