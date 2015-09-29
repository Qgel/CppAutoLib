//------------------------------------------------------------------------------
// <copyright file="AutoLibWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace CppAutoLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;


    /// <summary>
    /// Interaction logic for AutoLibWindowControl.
    /// </summary>
    public partial class AutoLibWindowControl : UserControl
    {
        /// <summary>a
        /// Initializes a new instance of the <see cref="AutoLibWindowControl"/> class.
        /// </summary>
        public AutoLibWindowControl()
        {
            this.InitializeComponent();
            List<DisplayItem> items = new List<DisplayItem>();
            items.Add(new DisplayItem() { AddLib = true, Libraries = new List<string> { "Lib1", "Lib2" }, Project = "Foo", Symbols = new List<string> { "Sym1", "Sym2", "Sym3" } });
            items.Add(new DisplayItem() { AddLib = true, Libraries = new List<string> { "Lib2", "LibFoo" }, Project="AAAaaasdf fffdsf", Symbols = new List<string> { "void* foo(int)", "Sym2", "Sym3" } });
            items.Add(new DisplayItem() { AddLib = true, Libraries = new List<string> { "Lib2", "LibFoo" }, Project = "ASD", Symbols = new List<string> { "Sym1", "ADHASDHHDD", "Sym3" } });

            LbLibs.ItemsSource = items;
        }
    }
}