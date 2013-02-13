/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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
namespace Hypertable.Explorer.Pages
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ExplorePage.xaml
    /// </summary>
    internal partial class ExplorePage
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorePage"/> class.
        /// </summary>
        public ExplorePage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) => this.ExpandRoot();

            DatabaseSession.ConnectionStateChanged += (s, e) =>
                {
                    if (e.ConnectionState == ConnectionState.Connecting)
                    {
                        this.connectionText.Text = e.ConnectionString;
                        this.header.Visibility = Visibility.Visible;
                    }
                    else if (e.ConnectionState == ConnectionState.Connected)
                    {
                        this.directoryView.ItemsSource = new ObservableList<DatabaseDirectoryInfo> { DatabaseSession.Instance.GetDirectoryInfo("/") };
                        this.ExpandRoot();
                        DatabaseScanner.ScannerStateChanged += this.HandleScannerStateChanged;
                    }
                    else if (e.ConnectionState == ConnectionState.Disconnecting)
                    {
                        this.directoryView.ItemsSource = null;
                        this.header.Visibility = Visibility.Collapsed;
                        DatabaseScanner.ScannerStateChanged -= this.HandleScannerStateChanged;
                    }
                };

            this.directoryView.SelectedItemChanged += (s, e) =>
                {
                    this.cellsScannedText.Text = string.Empty;
                    this.bytesScannedText.Text = string.Empty;
                };
        }

        #endregion

        #region Methods

        /// <summary>
        /// The expand root.
        /// </summary>
        private void ExpandRoot()
        {
            if (this.directoryView.ItemsSource != null)
            {
                var treeItem = this.directoryView.ItemContainerGenerator.ContainerFromItem(this.directoryView.Items[0]) as TreeViewItem;
                if (treeItem != null)
                {
                    treeItem.IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// The handle scanner state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleScannerStateChanged(object sender, NotifyScannerStateChangedEventArgs e)
        {
            switch (e.ScannerState)
            {
                case ScannerState.Begin:
                    this.cellsScannedText.Text = "scanning...";
                    this.bytesScannedText.Text = string.Empty;
                    this.cellsView.CanUserSortColumns = false;
                    break;
                case ScannerState.Executing:
                    this.cellsScannedText.Text = string.Format(CultureInfo.InvariantCulture, "scanning cells {0}", e.CellsScanned);
                    this.bytesScannedText.Text = string.Format(CultureInfo.InvariantCulture, "{0} bytes", e.BytesScanned);
                    break;
                case ScannerState.Completed:
                    this.cellsScannedText.Text = string.Format(CultureInfo.InvariantCulture, "{0} cells", e.CellsScanned);
                    this.bytesScannedText.Text = string.Format(CultureInfo.InvariantCulture, "{0} bytes", e.BytesScanned);
                    this.cellsView.CanUserSortColumns = true;
                    break;
            }
        }

        #endregion
    }
}