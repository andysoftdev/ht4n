/** -*- C# -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    using Hypertable.Explorer.Properties;

    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    internal partial class ConnectPage
    {
        #region Constants and Fields

        private DispatcherTimer timer;

        #endregion

        #region Constructors and Destructors

        public ConnectPage() {
            this.InitializeComponent();
            this.RecentConnections = new ObservableStringCollectionDecorator(Settings.Default.RecentConnections ?? (Settings.Default.RecentConnections = new StringCollection()));
            this.recentConnections.Visibility = this.RecentConnections.Count > 0 ? Visibility.Visible : Visibility.Hidden;

            this.Loaded += (s, e) => this.connectionString.Focus();

            this.connectionString.GotKeyboardFocus += (s, e) => this.CollapseConnectionErrorMessage();

            this.connect.PreviewMouseDown += (s, e) => this.CollapseConnectionErrorMessage();

            this.connect.Click += (s, e) => this.Connect(this.connectionString.Text);

            DatabaseSession.ConnectionStateChanged += (s, e) =>
                {
                    if (e.ConnectionState == ConnectionState.Connected) {
                        this.AddToRecentConnections(e.ConnectionString);
                    }
                };
        }

        #endregion

        #region Public Properties

        public ObservableStringCollectionDecorator RecentConnections { get; private set; }

        #endregion

        #region Methods

        private void AddToRecentConnections(string connectionString) {
            var rc = this.RecentConnections.ToList();
            rc.Insert(0, connectionString);
            rc = rc.Distinct().ToList();
            while (rc.Count > Settings.Default.MaxRecentConnections) {
                rc.RemoveAt(rc.Count - 1);
            }

            this.RecentConnections.Clear();
            this.RecentConnections.AddRange(rc);
            this.recentConnections.Visibility = this.RecentConnections.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void CollapseConnectionErrorMessage() {
            this.connectionError.Visibility = Visibility.Collapsed;
            if (this.timer != null) {
                this.timer.Stop();
                this.timer = null;
            }
        }

        private void Connect(string connectionString) {
            if (!string.IsNullOrEmpty(connectionString)) {
                try {
                    Mouse.OverrideCursor = Cursors.Wait;
                    DatabaseSession.Connect(connectionString);
                }
                catch {
                    this.connectionError.Visibility = Visibility.Visible;
                    this.timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
                    this.timer.Tick += (t, tick) => this.CollapseConnectionErrorMessage();
                    this.timer.Start();
                }
                finally {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void HandleRequestNavigate(object sender, RequestNavigateEventArgs e) {
            this.Connect(e.Uri.ToString());
            e.Handled = true;
        }

        #endregion
    }
}