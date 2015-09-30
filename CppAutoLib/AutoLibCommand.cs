//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using System.Collections.Generic;
using System.Linq;

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

        private readonly DTE2 _dte;

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

            this._dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));
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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var errorListScanner = new ErrorListScanner(_dte);
            var archiveResolution = new LibraryArchiveResolution();

            var statusBar = ServiceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            var allResolutions = new List<Resolution>();
            var allUnresolved = new List<string>();

            foreach (var project in errorListScanner.Projects)
            {
                var archives = archiveResolution.GetLibraryArchives(project, statusBar);
                var unresolvedSymbols = errorListScanner.GetUnresolvedSymbols(project);

                var libraryScanner = new LibraryScanner(unresolvedSymbols, archives);
                libraryScanner.Scan(statusBar);

                allResolutions.AddRange(libraryScanner.GetResolutions(project));
                allUnresolved.AddRange(libraryScanner.GetUnresolvedSymbols());
            }

            AutoLibWindow window = package.FindToolWindow(typeof (AutoLibWindow), 0, true) as AutoLibWindow;
            if (window?.Frame == null)
                throw new NotSupportedException("Cannot create tool window");

            window.WindowControl.SetItemSource(allResolutions.Select(DisplayItem.Create).ToList());
            window.WindowControl.SetUnresolved(allUnresolved);

            IVsWindowFrame windowFrame = (IVsWindowFrame) window.Frame;
            window.WindowControl.SetFrame(windowFrame);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
