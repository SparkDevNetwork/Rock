using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectDateRangePage.xaml
    /// </summary>
    public partial class SelectDateRangePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectDateRangePage"/> class.
        /// </summary>
        public SelectDateRangePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if ( dpEndDate.SelectedDate < dpStartDate.SelectedDate )
            {
                lblWarning.Content = "Start date must be earlier than end date";
                lblWarning.Visibility = Visibility.Visible;
                return;
            }
            
            if ( !dpStartDate.SelectedDate.HasValue)
            {
                lblWarning.Content = "Please select a start date";
                lblWarning.Visibility = Visibility.Visible;
                return;
            }

            ReportOptions.Current.StartDate = dpStartDate.SelectedDate.Value;

            if ( dpEndDate.SelectedDate.HasValue )
            {
                // set EndDate to 1 day ahead since user would expect the entire full day of enddate to be included
                ReportOptions.Current.EndDate = dpEndDate.SelectedDate.Value.AddDays( 1 );
            }
            else
            {
                ReportOptions.Current.EndDate = null;
            }

            SelectLayoutPage nextPage = new SelectLayoutPage();
            this.NavigationService.Navigate( nextPage );
        }
    }
}
