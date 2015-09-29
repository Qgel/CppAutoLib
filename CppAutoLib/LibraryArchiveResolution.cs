﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CppAutoLib
{

    class LibraryArchiveResolution
    {
        /// <summary>
        /// Cache for already seen project
        /// </summary>
        private readonly Dictionary<Project, List<LibArchive>> _projectArchiveMap = new Dictionary<Project, List<LibArchive>>();

        /// <summary>
        /// Cache for already seen and parsed libraries.
        /// </summary>
        private readonly Dictionary<string, LibArchive> _pathArchiveMap = new Dictionary<string, LibArchive>(); 

        /// <summary>
        /// Get all library archives accessible to the linker of the specified project.
        /// </summary>
        /// <param name="project">The project to get the LibArchives for</param>
        /// <returns>Parsed libraries which are accessible by the linker of the given project.</returns>
        public List<LibArchive> GetLibraryArchives(Project project)
        {
            if (!_projectArchiveMap.ContainsKey(project))
            {
                var archives = new List<LibArchive>();
                var libraries = GetLibraries(project);
                foreach(var lib in libraries)
                {
                    archives.Add(GetArchive(lib));      
                }
                _projectArchiveMap.Add(project, archives);
            }

            return _projectArchiveMap[project];
        }

        /// <summary>
        /// Caching accessor to parsed .lib files.
        /// </summary>
        /// <param name="lib">Path to the lib file.</param>
        /// <returns>A LibArchive object which represents the parsed library</returns>
        private LibArchive GetArchive(string lib)
        {
            if (!_pathArchiveMap.ContainsKey(lib))
            {
                _pathArchiveMap.Add(lib, new LibArchive(lib));
            }

            return _pathArchiveMap[lib];
        }

        /// <summary>
        /// Get library directories configured for the project.
        /// </summary>
        /// <param name="project"> The project to get the library directories for.</param>
        /// <returns>string array of library directories</returns>
        private string[] GetLibDirectories(Project project)
        {
            var pr = project.Object as Microsoft.VisualStudio.VCProjectEngine.VCProject;
            return pr.ActiveConfiguration.Evaluate("$(LibraryPath)").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Get paths to all .lib files which are are accessible to the linker.
        /// </summary>
        /// <param name="project">The project to get the .lib file pathes for</param>
        /// <returns>A list of pathes to .lib files</returns>
        private List<string> GetLibraries(Project project)
        {
            var ret = new List<string>();

            var directories = GetLibDirectories(project);
            foreach (var dir in directories)
            {
                ret.AddRange(Directory.EnumerateFiles(dir, "*.lib"));
            }

            return ret;
        }

        

    }
}
