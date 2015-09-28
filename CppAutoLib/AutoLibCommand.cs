//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace CppAutoLib
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AutoLibCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e214e42e-9604-41a3-9593-cb0f325ba585");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private readonly DTE2 dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoLibCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AutoLibCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            this.dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AutoLibCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new AutoLibCommand(package);
        }

        private String GetMangledName(ErrorItem item)
        {
            String error = item.Description;
            if (!error.StartsWith("unresolved external symbol"))
                return null;

            error = error.Substring(0, error.IndexOf(") referenced in"));
            error = error.Substring(error.LastIndexOf('(')+1);
            return error;
        }

        private Project GetProject(ErrorItem item)
        {
            Projects projects = dte.Solution.Projects;
            for(int i = 1; i <= projects.Count; i++)
            {
                if (projects.Item(i).UniqueName == item.Project)
                    return projects.Item(i);
            }

            return null;
        }

        private string[] GetLibDirectories(Project project)
        {
            var pr = project.Object as Microsoft.VisualStudio.VCProjectEngine.VCProject;            
            return pr.ActiveConfiguration.Evaluate("$(LibraryPath)").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var items = dte.ToolWindows.ErrorList.ErrorItems;
            dte.ExecuteCommand("View.ErrorList", " ");
            for (int i = 1; i <= items.Count; i++)
            {
                var mangled = GetMangledName(items.Item(i));
                var project = GetProject(items.Item(i));
                var libs = GetLibraries(project);
                if (mangled != null)
                {
                    VsShellUtilities.ShowMessageBox(
                     this.ServiceProvider,
                     mangled,
                     "Item",
                     OLEMSGICON.OLEMSGICON_INFO,
                     OLEMSGBUTTON.OLEMSGBUTTON_OK,
                     OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
        }
    }
}
