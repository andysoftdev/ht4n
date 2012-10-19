namespace Hypertable.Explorer.Pages
{
    using System;

    /// <summary>
    /// Interaction logic for ExceptionPage.xaml
    /// </summary>
    public partial class ExceptionPage
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPage"/> class.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public ExceptionPage(Exception e)
        {
            this.InitializeComponent();
            this.exceptionText.Text = e != null ? e.ToString() : "Unknown exception occured.";
        }

        #endregion
    }
}