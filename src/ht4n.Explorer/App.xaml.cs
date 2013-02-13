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
namespace Hypertable.Explorer
{
    using System;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App
    {
        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="App"/> class from being created.
        /// </summary>
        private App()
        {
            this.Startup += (s, e) => { AppDomain.CurrentDomain.UnhandledException += this.HandleAppDomainUnhandledException; };
        }

        #endregion

        #region Methods

        /// <summary>
        /// The handle app domain unhandled exception.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.HandleException(e.ExceptionObject as Exception);
        }

        /// <summary>
        /// The handle dispatcher unhandled exception.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.HandleException(e.Exception);
            e.Handled = true;
        }

        /// <summary>
        /// The handle exception.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleException(Exception e)
        {
            var mainWindow = this.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                try
                {
                    mainWindow.ShowExceptionPage(e);
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}