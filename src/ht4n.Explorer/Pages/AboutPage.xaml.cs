/** -*- C# -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    internal partial class AboutPage
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPage"/> class.
        /// </summary>
        public AboutPage()
        {
            this.InitializeComponent();
            this.description.Text = GetAssemblyAttribute<AssemblyDescriptionAttribute>().Description;
            this.version.Text = "Version " + GetAssemblyAttribute<AssemblyFileVersionAttribute>().Version;
            this.copyright.Text = GetAssemblyAttribute<AssemblyCopyrightAttribute>().Copyright;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get assembly attribute.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        private static T GetAssemblyAttribute<T>()
        {
            return (T)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false)[0];
        }

        /// <summary>
        /// The handle mouse down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://ht4n.softdev.ch"));
            e.Handled = true;
        }

        #endregion
    }
}