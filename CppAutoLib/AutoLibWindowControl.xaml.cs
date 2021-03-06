﻿//------------------------------------------------------------------------------
// <copyright file="AutoLibWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace CppAutoLib
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Used to disabled the Library file combobox if there
    /// is only one alternative.
    /// </summary>
    [ValueConversion(typeof(IList), typeof(bool))]
    public class MultipleElementsToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            return ((IList) value).Count > 1;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for AutoLibWindowControl.
    /// </summary>
    public partial class AutoLibWindowControl : UserControl
    {
        private List<DisplayItem> _items;
        private IVsWindowFrame _frame;

        /// <summary>a
        /// Initializes a new instance of the <see cref="AutoLibWindowControl"/> class.
        /// </summary>
        public AutoLibWindowControl()
        {
            this.InitializeComponent();;
        }

        public void SetFrame(IVsWindowFrame frame)
        {
            _frame = frame;
        }

        public void SetUnresolved(List<string> unresolved)
        {
            if (unresolved == null || unresolved.Count == 0)
                UnresolvedWarning.Visibility = Visibility.Collapsed;
            else
            {
                UnresolvedLabel.Content = unresolved.Count + " unresolved symbols remaining!";
                UnresolvedWarning.Visibility = Visibility.Visible;
            }
        }

        public void SetItemSource(List<DisplayItem> items)
        {
            LbLibs.ItemsSource = (_items = items);
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
                item.AddLib = true;
        }

        private void UnselectAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
                item.AddLib = false;
        }

        private void AddSelectedLibraries(object sender, RoutedEventArgs e)
        {
            var changedProjects = new List<Project>();

            foreach (var item in _items)
            {
                if (!item.AddLib)
                    continue;

                item.Resolution.Project.AddLinkerInput(item.Libraries[item.SelectedIndex]);
                if (!changedProjects.Contains(item.Resolution.Project))
                    changedProjects.Add(item.Resolution.Project);
            }
            
            foreach (var project in changedProjects)
                project.Build();

            _items.RemoveAll(item => item.AddLib);
            SetItemSource(_items);

            if (_items.Count == 0)
                _frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
        }
    }
}