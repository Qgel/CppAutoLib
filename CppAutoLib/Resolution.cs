using System.Collections.Generic;
using EnvDTE;

namespace CppAutoLib
{
    /// <summary>
    /// A Resolution represents one ore more .libs which can be added to the linker input
    /// in order to resolve all its containing symbols.
    /// </summary>
    public class Resolution
    {
        /// <summary>
        /// The libraries which would resolve the unresolved externals
        /// </summary>
        public List<LibArchive> Libraries { get; }

        /// <summary>
        /// The unresolved external symbols which would be resolved by adding one of the .libs
        /// </summary>
        public List<string> ResolvedSymbols
        {
            get;
            
        }

        public Project Project { get; }

        public Resolution(Project project, List<LibArchive> libraries, List<string> resolvedSymbols)
        {
            Libraries = libraries;
            ResolvedSymbols = resolvedSymbols;
            Project = project;
        }
    }
}
