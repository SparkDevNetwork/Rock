// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Windows;
using System.Windows.Controls;

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

            DateTime firstDayOfYear = new DateTime( DateTime.Now.Year, 1, 1 );
            dpStartDate.SelectedDate = ReportOptions.Current.StartDate ?? firstDayOfYear;

            if ( ReportOptions.Current.EndDate.HasValue )
            {
                // set displayed EndDate to 1 day before since user would expect the entire full day of enddate to be included
                dpEndDate.SelectedDate = ReportOptions.Current.EndDate.Value.AddDays( -1 );
            }
            else
            {
                dpEndDate.SelectedDate = ReportOptions.Current.EndDate ?? DateTime.Now.Date;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                var nextPage = new IndividualSaveSettings();
                this.NavigationService.Navigate( nextPage );
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            if ( dpEndDate.SelectedDate < dpStartDate.SelectedDate )
            {
                if ( showWarnings )
                {
                    lblWarning.Content = "Start date must be earlier than end date";
                    lblWarning.Visibility = Visibility.Visible;
                    return false;
                }
            }

            if ( !dpStartDate.SelectedDate.HasValue )
            {
                if ( showWarnings )
                {
                    lblWarning.Content = "Please select a start date";
                    lblWarning.Visibility = Visibility.Visible;
                    return false;
                }
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

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            SaveChanges( false );
            this.NavigationService.GoBack();
        }
    }
}
