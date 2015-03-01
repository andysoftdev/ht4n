/** -*- C# -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */
namespace Hypertable.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    using Hypertable.Explorer.Pages;
    using Hypertable.Explorer.Properties;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : IDisposable
    {
        #region Fields

        /// <summary>
        /// The menu strip items.
        /// </summary>
        private List<TextBlock> menuStripItems;

        /// <summary>
        /// The pages.
        /// </summary>
        private Control[] pages;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
                {
                    var menuDockPanel = (DockPanel)this.menuStrip.Template.FindName("MenuStrip", this.menuStrip);
                    this.menuStripItems = menuDockPanel.Children.OfType<TextBlock>().ToList();
                    foreach (var menuStripItem in this.menuStripItems)
                    {
                        menuStripItem.PreviewMouseDown += this.HandleMenuStripPreviewMouseDown;
                    }

                    this.pages = new Control[] { new ConnectPage(), new ExplorePage(), new AboutPage() };
                    this.SelectMenuStripItem(this.menuStripItems[0]);
                };

            this.Closed += (s, e) =>
                {
                    try
                    {
                        Settings.Default.Save();
                        DatabaseSession.Disconnect();
                    }
                    catch
                    {
                    }
                };

            DatabaseSession.ConnectionStateChanged += (s, e) =>
                {
                    if (e.ConnectionState == ConnectionState.Connected)
                    {
                        this.SelectMenuStripItem(this.menuStripItems[1]);
                    }
                };
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            DatabaseSession.Disconnect();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The show exception page.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        internal void ShowExceptionPage(Exception e)
        {
            foreach (var menuStripItem in this.menuStripItems)
            {
                menuStripItem.IsEnabled = true;
            }

            this.currentPage.Tag = new ExceptionPage(e);
        }

        /// <summary>
        /// The handle menu strip preview mouse down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleMenuStripPreviewMouseDown(object sender, RoutedEventArgs e)
        {
            this.SelectMenuStripItem(sender as TextBlock);
        }

        /// <summary>
        /// The handle request navigate.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        /// <summary>
        /// The select menu strip item.
        /// </summary>
        /// <param name="selectedMenuStripItem">
        /// The selected menu strip item.
        /// </param>
        private void SelectMenuStripItem(TextBlock selectedMenuStripItem)
        {
            foreach (var menuStripItem in this.menuStripItems)
            {
                menuStripItem.IsEnabled = menuStripItem != selectedMenuStripItem;
            }

            this.currentPage.Tag = this.pages[this.menuStripItems.IndexOf(selectedMenuStripItem)];
        }

        #endregion
    }
}