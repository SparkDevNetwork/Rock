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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using RestSharp;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectPersonsPage.xaml
    /// </summary>
    public partial class SelectPersonsPage : System.Windows.Controls.Page
    {
        private RestClient _restClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectPersonsPage"/> class.
        /// </summary>
        public SelectPersonsPage()
        {
            InitializeComponent();
            RockConfig rockConfig = RockConfig.Load();

            _restClient = new RestClient( rockConfig.RockBaseUrl );
            _restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

            string queryParam = "?$filter=EntityType/Name eq 'Rock.Model.Person'";
            var getDataViewsRequest = new RestRequest( "api/DataViews" + queryParam );
            var getDataViewsResponse = _restClient.Execute<List<Rock.Client.DataView>>( getDataViewsRequest );

            if ( getDataViewsResponse.ErrorException != null )
            {
                throw getDataViewsResponse.ErrorException;
            }

            var dataViews = getDataViewsResponse.Data;

            ddlDataView.DisplayMemberPath = "Name";
            ddlDataView.ItemsSource = dataViews;
        }

        /// <summary>
        /// Handles the RowDoubleClick event of the grdItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void grdItems_RowDoubleClick( object sender, MouseButtonEventArgs e )
        {
            btnNext_Click( sender, e );
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
                var nextPage = new SelectFinancialStatementTemplatePage();
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
            var rockConfig = RockConfig.Load();
            if ( radAllPersons.IsChecked ?? false )
            {
                rockConfig.PersonSelectionOption = PersonSelectionOption.AllIndividuals;
                ReportOptions.Current.DataViewId = null;
                ReportOptions.Current.PersonId = null;
            }
            else if ( radDataView.IsChecked ?? false )
            {
                rockConfig.PersonSelectionOption = PersonSelectionOption.DataView;
                var selectedDataView = ddlDataView.SelectedValue as Rock.Client.DataView;
                if ( selectedDataView != null )
                {
                    ReportOptions.Current.DataViewId = selectedDataView.Id;
                    ReportOptions.Current.PersonId = null;
                }
                else
                {
                    if ( showWarnings )
                    {
                        // no dataview is selected, show a warning message 
                        lblWarning.Content = "Please select a Dataview when 'Dataview' is checked.";
                        lblWarning.Visibility = Visibility.Visible;
                        return false;
                    }
                }
            }
            else
            {
                rockConfig.PersonSelectionOption = PersonSelectionOption.SingleIndividual;
                PersonSearchResult personSearchResult = grdPersons.SelectedValue as PersonSearchResult;
                if ( personSearchResult == null && grdPersons.Items.Count == 1 )
                {
                    personSearchResult = grdPersons.Items[0] as PersonSearchResult;
                }

                if ( personSearchResult != null )
                {
                    ReportOptions.Current.PersonId = personSearchResult.Id;
                }
                else
                {
                    if ( showWarnings )
                    {
                        // no person is selected, show a warning message 
                        lblWarning.Content = "Please select a person when 'Single individual' is checked.";
                        lblWarning.Visibility = Visibility.Visible;
                        return false;
                    }
                }
            }

            ReportOptions.Current.ExcludeInActiveIndividuals = ckExcludeInActiveIndividuals.IsChecked ?? false;
            ReportOptions.Current.IncludeBusinesses = ckIncludeBusinesses.IsChecked ?? false;
            return true;
        }

        /// <summary>
        /// Handles the Checked event of any of the person selection radioboxes are checked
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void radPersons_Checked( object sender, RoutedEventArgs e )
        {
            if ( this.IsInitialized )
            {
                pnlSingleIndividualOptions.Visibility = radSingle.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
                pnlAllIndividualsOptions.Visibility = radAllPersons.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
                pnlDataViewOptions.Visibility = radDataView.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private ConcurrentBag<BackgroundWorker> activeSearchBackgroundWorkers = new ConcurrentBag<BackgroundWorker>();

        /// <summary>
        /// Handles the TextChanged event of the txtPersonSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtPersonSearch_TextChanged( object sender, TextChangedEventArgs e )
        {
            var searchTerm = txtPersonSearch.Text.Trim();

            var searchBackgroundWorkersList = activeSearchBackgroundWorkers.ToList();
            for ( int i = 0; i < searchBackgroundWorkersList.Count; i++ )
            {
                var bw = searchBackgroundWorkersList[i];
                bw?.CancelAsync();
            }

            var searchBackgroundWorker = new BackgroundWorker();
            activeSearchBackgroundWorkers.Add( searchBackgroundWorker );
            searchBackgroundWorker.WorkerSupportsCancellation = true;

            searchBackgroundWorker.DoWork += bw_DoWork;
            searchBackgroundWorker.RunWorkerCompleted += bw_RunWorkerCompleted;
            searchBackgroundWorker.RunWorkerAsync( searchTerm );
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        protected void bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if ( e.Error != null )
            {
                // if there was an error, re-create a new RestClient
                RockConfig rockConfig = RockConfig.Load();
                _restClient = new RestClient( rockConfig.RockBaseUrl );
                _restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

                throw e.Error;
            }

            if ( !e.Cancelled )
            {
                grdPersons.DataContext = e.Result;
            }

            activeSearchBackgroundWorkers = new ConcurrentBag<BackgroundWorker>( activeSearchBackgroundWorkers.Where( a => a != sender as BackgroundWorker ) );
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        protected void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            //// sleep a few ms to make sure they are done typing, then only fire off the query if this bw was launched with the most recent search term
            //// this helps reduce the chance that the webclient will get overloaded with multiple requested
            System.Threading.Thread.Sleep( 50 );

            string searchValue = e.Argument as string;

            // if the search term has changed and new backgroundworker is doing a search, cancel this one
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if ( backgroundWorker.CancellationPending )
            {
                e.Cancel = true;
                return;
            }

            if ( searchValue.Length < 3 )
            {
                e.Result = new List<PersonSearchResult>();
                return;
            }

            string searchUrl = $"api/People/Search?name={WebUtility.UrlEncode( searchValue )}&includeHtml=false&includeDetails=true&includeBusinesses=true&includeDeceased=true";
            var getSearchRequest = new RestRequest( searchUrl );
            getSearchRequest.Timeout = 10000;
            var getSearchResponse = _restClient.Execute<List<PersonSearchResult>>( getSearchRequest );

            if ( getSearchResponse.ErrorException != null )
            {
                throw getSearchResponse.ErrorException;
            }

            var personResults = getSearchResponse.Data;

            // if the search term has changed and new BackgroundWorker is doing a search, cancel this one
            if ( backgroundWorker.CancellationPending )
            {
                e.Cancel = true;
                return;
            }

            // if the search term has changed and new BackgroundWorker is doing a search, cancel this one
            if ( backgroundWorker.CancellationPending )
            {
                e.Cancel = true;
                return;
            }

            e.Result = personResults;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the grdPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void grdPersons_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( lblWarning.Visibility == Visibility.Visible )
            {
                if ( grdPersons.SelectedValue != null )
                {
                    lblWarning.Visibility = Visibility.Collapsed;
                }
            }
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

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            var rockConfig = RockConfig.Load();
            radAllPersons.IsChecked = rockConfig.PersonSelectionOption == PersonSelectionOption.AllIndividuals;
            radDataView.IsChecked = rockConfig.PersonSelectionOption == PersonSelectionOption.DataView;
            radSingle.IsChecked = rockConfig.PersonSelectionOption == PersonSelectionOption.SingleIndividual;
            
            ckExcludeInActiveIndividuals.IsChecked = ReportOptions.Current.ExcludeInActiveIndividuals;
            ckIncludeBusinesses.IsChecked = ReportOptions.Current.IncludeBusinesses;

            if ( ReportOptions.Current.DataViewId.HasValue )
            {
                ddlDataView.SelectedValue = ddlDataView.Items.OfType<Rock.Client.DataView>().FirstOrDefault( a => a.Id == ReportOptions.Current.DataViewId.Value );
            }

            radPersons_Checked( sender, e );
            lblWarning.Visibility = Visibility.Collapsed;
        }
    }
}
