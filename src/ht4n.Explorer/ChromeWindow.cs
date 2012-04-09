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

namespace Hypertable.Explorer
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// The chrome window.
    /// </summary>
    internal class ChromeWindow : Window
    {
        #region Constants and Fields

        private const int EdgeSize = 23;

        private const int WM_SYSCOMMAND = 0x112;

        private static readonly Brush InactiveBorderBrush = new SolidColorBrush(Color.FromRgb(0x91, 0x91, 0x91));

        private static readonly TimeSpan doubleClickDelay = TimeSpan.FromMilliseconds(500);

        private Grid chromeFrame;

        private DateTime headerLastClicked;

        private HwndSource hwndSource;

        private HwndSource[] hwndSourceSurrounds;

        private Path resize;

        private Border roundBorder;

        private Window wndBottom;

        private Window wndLeft;

        private Window wndRight;

        private Window wndTop;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public ChromeWindow() {
            this.Loaded += (s, e) =>
                {
                    if (InactiveBorderBrush.CanFreeze) {
                        InactiveBorderBrush.Freeze();
                    }

                    this.ResizeSurrounds();
                    this.ShowSurrounds();

                    this.roundBorder = (Border)this.Template.FindName("RoundBorder", this);
                    this.chromeFrame = (Grid)this.Template.FindName("ChromeFrame", this);
                    this.resize = (Path)this.Template.FindName("Resize", this);

                    var chromeHeaderRectangle = (Rectangle)this.Template.FindName("ChromeHeader", this);
                    chromeHeaderRectangle.PreviewMouseDown += this.HandleChromeHeaderPreviewMouseDown;

                    var chromeButton = (Button)this.Template.FindName("ChromeMinimizeButton", this);
                    chromeButton.Click += this.HandleChromeMinimizeClick;

                    chromeButton = (Button)this.Template.FindName("ChromeRestoreButton", this);
                    chromeButton.Click += this.HandleChromeRestoreClick;

                    chromeButton = (Button)this.Template.FindName("ChromeCloseButton", this);
                    chromeButton.Click += this.HandleChromeCloseClick;

                    foreach (var chromeBorder in this.chromeFrame.Children.OfType<Rectangle>()) {
                        chromeBorder.PreviewMouseDown += this.HandleChromeFramePreviewMouseDown;
                        chromeBorder.MouseMove += this.HandleChromeFrameMouseMove;
                    }

                    this.PreviewMouseMove += this.HandlePreviewMouseMove;
                    this.LocationChanged += this.HandleLocationChanged;
                    this.SizeChanged += this.HandleLocationChanged;
                    this.StateChanged += this.HandleWndStateChanged;

                    this.Activated += this.HandleActivated;
                    this.Deactivated += this.HandleDeactivated;
                };
        }

        #endregion

        #region Enums

        public enum ResizeDirection
        {
            Left = 1, 

            Right = 2, 

            Top = 3, 

            TopLeft = 4, 

            TopRight = 5, 

            Bottom = 6, 

            BottomLeft = 7, 

            BottomRight = 8, 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e) {
            this.CloseSurrounds();
            base.OnClosed(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. 
        /// This method is invoked whenever 
        /// <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> 
        /// that contains the event data.</param>
        protected override void OnInitialized(EventArgs e) {
            this.AllowsTransparency = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;

            this.InitializeSurrounds();
            base.OnInitialized(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.SourceInitialized"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            this.hwndSource = (HwndSource)PresentationSource.FromVisual(this);

            // Returns the HwndSource object for the window
            // which presents WPF content in a Win32 window.
            HwndSource.FromHwnd(this.hwndSource.Handle).AddHook(NativeMethods.WindowProc);
        }

        /// <summary>
        /// Creates an empty window.
        /// </summary>
        /// <returns>Window created</returns>
        private static Window CreateTransparentWindow() {
            return new Window
                {
                   AllowsTransparency = true, Visibility = Visibility.Hidden, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Background = null, Width = 0, Height = 0 
                };
        }

        /// <summary>
        /// Closes the surrounding windows.
        /// </summary>
        private void CloseSurrounds() {
            this.wndTop.Close();
            this.wndLeft.Close();
            this.wndBottom.Close();
            this.wndRight.Close();
        }

        [DebuggerStepThrough]
        private Decorator GetDecorator(string imageUri, double radius = 0) {
            return new Border { CornerRadius = new CornerRadius(radius), Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imageUri))) };
        }

        /// <summary>
        /// Handles the activated event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        private void HandleActivated(object sender, EventArgs e) {
            this.SetSurroundShadows();
            this.roundBorder.BorderBrush = Brushes.Black;
            this.resize.Fill = Brushes.Black;
        }

        /// <summary>
        /// Handles the close click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> 
        /// instance containing the event data.</param>
        private void HandleChromeCloseClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Handles the rectangle mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> 
        /// instance containing the event data.</param>
        private void HandleChromeFrameMouseMove(object sender, MouseEventArgs e) {
            var clickedRectangle = (Rectangle)sender;

            switch (clickedRectangle.Name) {
                case "Top":
                    this.Cursor = Cursors.SizeNS;
                    break;
                case "Bottom":
                    this.Cursor = Cursors.SizeNS;
                    break;
                case "Left":
                    this.Cursor = Cursors.SizeWE;
                    break;
                case "Right":
                    this.Cursor = Cursors.SizeWE;
                    break;
                case "TopLeft":
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case "TopRight":
                    this.Cursor = Cursors.SizeNESW;
                    break;
                case "BottomLeft":
                    this.Cursor = Cursors.SizeNESW;
                    break;
                case "BottomRight":
                    this.Cursor = Cursors.SizeNWSE;
                    break;
            }
        }

        /// <summary>
        /// Handles the rectangle preview mouse down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> 
        /// instance containing the event data.</param>
        private void HandleChromeFramePreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var clickedRectangle = (Rectangle)sender;

            switch (clickedRectangle.Name) {
                case "Top":
                    this.Cursor = Cursors.SizeNS;
                    this.ResizeWindow(ResizeDirection.Top);
                    break;
                case "Bottom":
                    this.Cursor = Cursors.SizeNS;
                    this.ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "Left":
                    this.Cursor = Cursors.SizeWE;
                    this.ResizeWindow(ResizeDirection.Left);
                    break;
                case "Right":
                    this.Cursor = Cursors.SizeWE;
                    this.ResizeWindow(ResizeDirection.Right);
                    break;
                case "TopLeft":
                    this.Cursor = Cursors.SizeNWSE;
                    this.ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "TopRight":
                    this.Cursor = Cursors.SizeNESW;
                    this.ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "BottomLeft":
                    this.Cursor = Cursors.SizeNESW;
                    this.ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "BottomRight":
                    this.Cursor = Cursors.SizeNWSE;
                    this.ResizeWindow(ResizeDirection.BottomRight);
                    break;
            }
        }

        /// <summary>
        /// Handles the header preview mouse down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/>
        /// instance containing the event data.</param>
        private void HandleChromeHeaderPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (DateTime.Now.Subtract(this.headerLastClicked) <= doubleClickDelay) {
                // Execute the code inside the event handler for the 
                // restore button click passing null for the sender
                // and null for the event args.
                this.HandleChromeRestoreClick(null, null);
            }

            this.headerLastClicked = DateTime.Now;
            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        /// <summary>
        /// Handles the minimize click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> 
        /// instance containing the event data.</param>
        private void HandleChromeMinimizeClick(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handles the restore click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> 
        /// instance containing the event data.</param>
        private void HandleChromeRestoreClick(object sender, RoutedEventArgs e) {
            this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
            this.chromeFrame.IsHitTestVisible = this.WindowState != WindowState.Maximized;
            this.resize.Visibility = (this.WindowState == WindowState.Maximized) ? Visibility.Hidden : Visibility.Visible;
            this.roundBorder.Visibility = (this.WindowState == WindowState.Maximized) ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Handles the deactivated event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        private void HandleDeactivated(object sender, EventArgs e) {
            this.SetSurroundShadows(false);
            this.roundBorder.BorderBrush = InactiveBorderBrush;
            this.resize.Fill = InactiveBorderBrush;
        }

        /// <summary>
        /// Handles the location changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> 
        /// instance containing the event data.</param>
        private void HandleLocationChanged(object sender, EventArgs e) {
            this.ResizeSurrounds();
        }

        /// <summary>
        /// Handles the preview mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> 
        /// instance containing the event data.</param>
        [DebuggerStepThrough]
        private void HandlePreviewMouseMove(object sender, MouseEventArgs e) {
            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Handles the windows state changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        private void HandleWndStateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Normal) {
                this.ShowSurrounds();
            }
            else {
                this.HideSurrounds();
            }
        }

        /// <summary>
        /// Hides the surrounding windows.
        /// </summary>
        private void HideSurrounds() {
            this.wndTop.Hide();
            this.wndLeft.Hide();
            this.wndBottom.Hide();
            this.wndRight.Hide();
        }

        /// <summary>
        /// Initializes the surrounding windows.
        /// </summary>
        private void InitializeSurrounds() {
            this.wndTop = CreateTransparentWindow();
            this.wndLeft = CreateTransparentWindow();
            this.wndBottom = CreateTransparentWindow();
            this.wndRight = CreateTransparentWindow();

            this.SetSurroundShadows();
        }

        /// <summary>
        /// Resize the surrounding windows.
        /// </summary>
        private void ResizeSurrounds() {
            if (PresentationSource.FromVisual(this.wndTop) != null) {
                if (this.hwndSourceSurrounds == null) {
                    this.hwndSourceSurrounds = new[]
                        {
                            (HwndSource)PresentationSource.FromVisual(this.wndTop), (HwndSource)PresentationSource.FromVisual(this.wndLeft), 
                            (HwndSource)PresentationSource.FromVisual(this.wndBottom), (HwndSource)PresentationSource.FromVisual(this.wndRight)
                        };
                }

                var dwp = NativeMethods.BeginDeferWindowPos(4);

                NativeMethods.DeferWindowPos(
                    dwp, this.hwndSourceSurrounds[0].Handle, IntPtr.Zero, (int)(this.Left - EdgeSize), (int)(this.Top - EdgeSize), (int)(this.Width + EdgeSize * 2), EdgeSize, 0x10);

                NativeMethods.DeferWindowPos(dwp, this.hwndSourceSurrounds[1].Handle, IntPtr.Zero, (int)(this.Left - EdgeSize), (int)this.Top, EdgeSize, (int)this.Height, 0x10);

                NativeMethods.DeferWindowPos(
                    dwp, 
                    this.hwndSourceSurrounds[2].Handle, 
                    IntPtr.Zero, 
                    (int)(this.Left - EdgeSize), 
                    (int)(this.Top + this.Height), 
                    (int)(this.Width + EdgeSize * 2), 
                    EdgeSize, 
                    0x10);

                NativeMethods.DeferWindowPos(dwp, this.hwndSourceSurrounds[3].Handle, IntPtr.Zero, (int)(this.Left + this.Width), (int)this.Top, EdgeSize, (int)this.Height, 0x10);

                NativeMethods.EndDeferWindowPos(dwp);
            }
            else {
                this.wndTop.Left = this.Left - EdgeSize;
                this.wndTop.Top = this.Top - EdgeSize;
                this.wndTop.Width = this.Width + EdgeSize * 2;
                this.wndTop.Height = EdgeSize;

                this.wndLeft.Left = this.Left - EdgeSize;
                this.wndLeft.Top = this.Top;
                this.wndLeft.Width = EdgeSize;
                this.wndLeft.Height = this.Height;

                this.wndBottom.Left = this.Left - EdgeSize;
                this.wndBottom.Top = this.Top + this.Height;
                this.wndBottom.Width = this.Width + EdgeSize * 2;
                this.wndBottom.Height = EdgeSize;

                this.wndRight.Left = this.Left + this.Width;
                this.wndRight.Top = this.Top;
                this.wndRight.Width = EdgeSize;
                this.wndRight.Height = this.Height;
            }
        }

        /// <summary>
        /// Resizes the window.
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void ResizeWindow(ResizeDirection direction) {
            NativeMethods.SendMessage(this.hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        /// <summary>
        /// Sets the artificial drop shadow.
        /// </summary>
        /// <param name="active">if set to <c>true</c> [active].</param>
        private void SetSurroundShadows(bool active = true) {
            if (active) {
                const double CornerRadius = 1.75;

                this.wndTop.Content = this.GetDecorator("Resources/ActiveShadowTop.png");
                this.wndLeft.Content = this.GetDecorator("Resources/ActiveShadowLeft.png", CornerRadius);
                this.wndBottom.Content = this.GetDecorator("Resources/ActiveShadowBottom.png");
                this.wndRight.Content = this.GetDecorator("Resources/ActiveShadowRight.png", CornerRadius);
            }
            else {
                this.wndTop.Content = this.GetDecorator("Resources/InactiveShadowTop.png");
                this.wndLeft.Content = this.GetDecorator("Resources/InactiveShadowLeft.png");
                this.wndBottom.Content = this.GetDecorator("Resources/InactiveShadowBottom.png");
                this.wndRight.Content = this.GetDecorator("Resources/InactiveShadowRight.png");
            }

            this.ResizeSurrounds();
        }

        /// <summary>
        /// Shows the surrounding windows.
        /// </summary>
        private void ShowSurrounds() {
            this.wndTop.Show();
            this.wndLeft.Show();
            this.wndBottom.Show();
            this.wndRight.Show();
        }

        #endregion

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr BeginDeferWindowPos(int nNumWindows);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

            [DebuggerStepThrough]
            internal static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
                switch (msg) {
                    case 0x0024:
                        WmGetMinMaxInfo(hwnd, lParam);
                        handled = true;
                        break;
                }

                return (IntPtr)0;
            }

            [DllImport("user32")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

            [DllImport("User32")]
            private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

            private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
                var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area 
                // of the correct monitor.
                const int MONITOR_DEFAULTTONEAREST = 0x00000002;

                var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero) {
                    var monitorInfo = new MONITORINFO();
                    GetMonitorInfo(monitor, monitorInfo);

                    var rcWorkArea = monitorInfo.rcWork;
                    var rcMonitorArea = monitorInfo.rcMonitor;

                    mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                    mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);

                    mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                    mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            private struct RECT
            {
                public readonly int left;

                public readonly int top;

                public readonly int right;

                public readonly int bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;

                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MINMAXINFO
            {
                public readonly POINT ptReserved;

                public POINT ptMaxSize;

                public POINT ptMaxPosition;

                public readonly POINT ptMinTrackSize;

                public readonly POINT ptMaxTrackSize;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private sealed class MONITORINFO
            {
                public readonly int cbSize;

                public readonly RECT rcMonitor;

                public readonly RECT rcWork;

                public readonly int dwFlags;

                public MONITORINFO() {
                    this.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    this.rcMonitor = new RECT();
                    this.rcWork = new RECT();
                    this.dwFlags = 0;
                }
            }
        }
    }
}