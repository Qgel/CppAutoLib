using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace CppAutoLib
{
    class ErrorListScanner
    {
        private readonly DTE2 _dte;

        private readonly Dictionary<Project, List<string>> _unresolvedSymbols = new Dictionary<Project, List<string>>(); 

        public ErrorListScanner(DTE2 dte)
        {
            _dte = dte;

            var errorItems = dte.ToolWindows.ErrorList.ErrorItems;
            dte.ExecuteCommand("View.ErrorList", " ");

            //Yup these iterator count from 1..
            for (int i = 1; i <= errorItems.Count; i++)
            {
                var item = errorItems.Item(i);
                var symbol = GetUnresolvedSymbol(item);
                //Check if this is an unresolved external symbol error
                if (symbol == null)
                    continue;

                var project = GetProject(item);
                AddUnresolvedSymbol(project, symbol);
            }
        }

        /// <summary>
        /// All projects with unresolved external symbols.
        /// </summary>
        public List<Project> Projects
        {
            get { return _unresolvedSymbols.Keys.ToList(); }
        }

        /// <summary>
        /// Get the mangled names of all unresolved external for a given project.
        /// </summary>
        /// <param name="project">The project to get the symbols for</param>
        /// <returns>A list of mangled names</returns>
        public List<string> GetUnresolvedSymbols(Project project)
        {
            if(!_unresolvedSymbols.ContainsKey(project))
                return new List<string>();
            return _unresolvedSymbols[project];
        } 

        /// <summary>
        /// Add unresolved symbol to map
        /// </summary>
        /// <param name="project"></param>
        /// <param name="symbol"></param>
        private void AddUnresolvedSymbol(Project project, string symbol)
        {
            if(!_unresolvedSymbols.ContainsKey(project))
                _unresolvedSymbols.Add(project, new List<string>());
            _unresolvedSymbols[project].Add(symbol);
        }
        
        /// <summary>
        /// Get the unresolved symbols mangled name from an ErrorItem
        /// </summary>
        /// <param name="item">The ErrorItem</param>
        /// <returns>The symbols mangled name, or null if the item is not an 'unresolved external symbol' error</returns>
        private string GetUnresolvedSymbol(ErrorItem item)
        {
            string error = item.Description;
            if (!error.StartsWith("unresolved external symbol"))
                return null;

            error = error.Substring(0, error.LastIndexOf(")", StringComparison.Ordinal));
            error = error.Substring(error.LastIndexOf('(') + 1);
            return error;
        }

        /// <summary>
        /// Get project associated with the ErrorItem
        /// </summary>
        /// <param name="item">The ErrorItem</param>
        /// <returns>Project associated with the ErrorItem</returns>
        private Project GetProject(ErrorItem item)
        {
            Projects projects = _dte.Solution.Projects;
            for (int i = 1; i <= projects.Count; i++)
            {
                if (projects.Item(i).UniqueName == item.Project)
                    return projects.Item(i);
            }

            return null;
        }
    }
}
