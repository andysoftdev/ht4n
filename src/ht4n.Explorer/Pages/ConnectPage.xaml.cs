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
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

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

            this.Loaded += (s, e) => this.connectionString.Focus();

            this.connectionString.GotKeyboardFocus += (s, e) => this.CollapseConnectionErrorMessage();

            this.connect.PreviewMouseDown += (s, e) => this.CollapseConnectionErrorMessage();

            this.connect.Click += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(this.connectionString.Text)) {
                        try {
                            Mouse.OverrideCursor = Cursors.Wait;
                            DatabaseSession.Connect(this.connectionString.Text);
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
                };
        }

        #endregion

        #region Methods

        private void CollapseConnectionErrorMessage() {
            this.connectionError.Visibility = Visibility.Collapsed;
            if (this.timer != null) {
                this.timer.Stop();
                this.timer = null;
            }
        }

        #endregion
    }
}