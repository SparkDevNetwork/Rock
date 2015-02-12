// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using Rock.Net;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectPersonsPage.xaml
    /// </summary>
    public partial class SelectPersonsPage : Page
    {
        /// <summary>
        /// The _rock rest client
        /// </summary>
        private RockRestClient _rockRestClient;

        /// <summary>
        /// The _lastTypedSearchTerm to assist in reducing unneccessary searches if the person is still typing
        /// </summary>
        private string _lastTypedSearchTerm;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectPersonsPage"/> class.
        /// </summary>
        public SelectPersonsPage()
        {
            InitializeComponent();
            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );
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
            if ( radAllPersons.IsChecked ?? false )
            {
                ReportOptions.Current.PersonId = null;
            }
            else
            {
                if ( grdPersons.SelectedValue != null )
                {
                    // they selected a person in the grid
                    ReportOptions.Current.PersonId = (int)grdPersons.SelectedValue.GetPropertyValue( "Id" );
                }
                else if ( grdPersons.Items.Count == 1 )
                {
                    // they didn't select a person in the grid, but there is only one listed. So, that is who they want to run the report for
                    ReportOptions.Current.PersonId = (int)grdPersons.Items[0].GetPropertyValue( "Id" );
                }
                else
                {
                    // no person is selected, show a warning message 
                    lblWarning.Visibility = Visibility.Visible;
                    return;
                }
            }

            ReportOptions.Current.IncludeIndividualsWithNoAddress = ckIncludeIndividualsWithNoAddress.IsChecked ?? false;

            SelectAccountsPage nextPage = new SelectAccountsPage();
            this.NavigationService.Navigate( nextPage );
        }

        /// <summary>
        /// Handles the Checked event of the radPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void radPersons_Checked( object sender, RoutedEventArgs e )
        {
            if ( this.IsInitialized )
            {
                if ( radSingle.IsChecked == true)
                {
                    txtPersonSearch.Visibility = Visibility.Visible;
                    grdPersons.Visibility = Visibility.Visible;
                    ckIncludeIndividualsWithNoAddress.Visibility = Visibility.Hidden;
                    
                }
                else
                {
                    txtPersonSearch.Visibility = Visibility.Hidden;
                    grdPersons.Visibility = Visibility.Hidden;
                    ckIncludeIndividualsWithNoAddress.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the txtPersonSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtPersonSearch_TextChanged( object sender, TextChangedEventArgs e )
        {
            var searchTerm = txtPersonSearch.Text.Trim();
            _lastTypedSearchTerm = searchTerm;

            BackgroundWorker searchBackgroundWorker = new BackgroundWorker();
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
                RockConfig rockConfig = RockConfig.Load();

                _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
                _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

                throw e.Error;
            }

            if ( !e.Cancelled )
            {
                grdPersons.DataContext = e.Result as List<object>;
            }
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        protected void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            List<object> personResults = new List<object>();

            //// sleep a few ms to make sure they are done typing, then only fire off the query if this bw was launched with the most recent search term
            //// this helps reduce the chance that the webclient will get overloaded with multiple requested

            System.Threading.Thread.Sleep( 50 );
            string searchValue = e.Argument as string;

            if ( searchValue != _lastTypedSearchTerm )
            {
                e.Cancel = true;
                return;
            }

            if ( searchValue.Length > 2 )
            {
                string uriFormat = "api/People/Search?name={0}&includeHtml=true&includeBusinesses=true";
                var searchResult = _rockRestClient.GetXml( string.Format( uriFormat, HttpUtility.UrlEncode( searchValue ) ), 10000 );
                if ( searchResult != null )
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml( searchResult );
                    XmlNode root = doc.DocumentElement;
                    foreach ( var node in root.ChildNodes.OfType<XmlNode>() )
                    {
                       personResults.Add( new
                        {
                            Id = node["Id"].InnerText.AsInteger(),
                            FullName = node["Name"].InnerText,
                            Age = node["Age"].InnerText == "-1" ? "" : node["Age"].InnerText,
                            Gender = node["Gender"].InnerText,
                            ToolTip = string.Format( 
                                string.IsNullOrWhiteSpace(node["SpouseName"].InnerText) ? "-" : node["SpouseName"].InnerText, 
                                node["Email"].InnerText, 
                                node["Address"].InnerText ),
                            SpouseName = node["SpouseName"].InnerText,
                            Email = node["Email"].InnerText,
                            Address = node["Address"].InnerText
                        } );
                    }
                }
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
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            radPersons_Checked( sender, e );
        }
    }
}
