//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Windows;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ErrorMessageWindow.xaml
    /// </summary>
    public partial class ErrorMessageWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessageWindow"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ErrorMessageWindow(string message)
        {
            InitializeComponent();
            txtErrorMessage.Text = message;
        }

        /// <summary>
        /// Handles the Click event of the OK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OK_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }
    }
}
