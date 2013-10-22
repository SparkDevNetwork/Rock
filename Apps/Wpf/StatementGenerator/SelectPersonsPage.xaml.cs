using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.XPath;
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
            if ( radAllPersons.IsChecked ?? false)
            {
                ReportOptions.Current.PersonId = null;
            }
            else
            {
                if ( grdPersons.SelectedValue != null )
                {
                    ReportOptions.Current.PersonId = (int)grdPersons.SelectedValue.GetPropertyValue( "Id" );
                }
                else
                {
                    lblWarning.Visibility = Visibility.Visible;
                    return;
                }
            }
            
            SelectAccountsPage nextPage = new SelectAccountsPage();
            this.NavigationService.Navigate( nextPage );
        }

        /// <summary>
        /// Handles the Checked event of the radSingle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void radSingle_Checked( object sender, RoutedEventArgs e )
        {
            if ( this.IsInitialized )
            {
                txtPersonSearch.IsEnabled = true;
                grdPersons.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Checked event of the radAllPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void radAllPersons_Checked( object sender, RoutedEventArgs e )
        {
            if ( this.IsInitialized )
            {
                txtPersonSearch.IsEnabled = false;
                grdPersons.IsEnabled = false;
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

            // sleep a few ms to make sure they are done typing, then only fire off the query if this bw was launched with the most recent search term
            // this helps reduce the chance that the webclient will get overloaded with multiple requested
            
            System.Threading.Thread.Sleep( 50 );
            string searchValue = e.Argument as string;

            if ( searchValue != _lastTypedSearchTerm )
            {
                e.Cancel = true;
                return;
            }

            if ( searchValue.Length > 2 )
            {
                //api/People/Search/{name}/{includeHtml}
                string uriFormat = "api/People/Search?name={0}&includeHtml=true";
                var searchResult = _rockRestClient.GetXml( string.Format( uriFormat, HttpUtility.UrlEncode(searchValue) ), 10000 );
                if ( searchResult != null )
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml( searchResult );
                    XmlNode root = doc.DocumentElement;
                    foreach ( var node in root.ChildNodes.OfType<XmlNode>() )
                    {
                        personResults.Add( new
                        {
                            Id = node["Id"].InnerText.AsInteger() ?? 0,
                            FullName = node["Name"].InnerText,
                            Age = node["Age"].InnerText,
                            Gender = node["Gender"].InnerText
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
                    lblWarning.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
