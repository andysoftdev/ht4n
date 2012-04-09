namespace Hypertable.Explorer.Pages
{
    using System;

    /// <summary>
    /// Interaction logic for ExceptionPage.xaml
    /// </summary>
    public partial class ExceptionPage
    {
        #region Constructors and Destructors

        public ExceptionPage(Exception e) {
            this.InitializeComponent();
            this.exceptionText.Text = e != null ? e.ToString() : "Unknown exception occured.";
        }

        #endregion
    }
}