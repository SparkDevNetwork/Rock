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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using RestSharp;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectFinancialStatementTemplatePage.xaml
    /// </summary>
    public partial class SelectFinancialStatementTemplatePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFinancialStatementTemplatePage"/> class.
        /// </summary>
        public SelectFinancialStatementTemplatePage()
        {
            InitializeComponent();
            LoadFinancialStatementTemplates();
        }

        /// <summary>
        /// Loads the financial statement templates.
        /// </summary>
        public void LoadFinancialStatementTemplates()
        {
            RockConfig rockConfig = RockConfig.Load();

            var restClient = new RestClient( rockConfig.RockBaseUrl );
            restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

            var getFinancialStatementTemplatesRequest = new RestRequest( "api/FinancialStatementTemplates" );
            var getFinancialStatementTemplatesResponse = restClient.Execute<List<Client.FinancialStatementTemplate>>( getFinancialStatementTemplatesRequest );

            if ( getFinancialStatementTemplatesResponse.ErrorException != null )
            {
                throw getFinancialStatementTemplatesResponse.ErrorException;
            }

            if ( getFinancialStatementTemplatesResponse.StatusCode != System.Net.HttpStatusCode.OK )
            {
                lblWarning.Content = $"Error: { getFinancialStatementTemplatesResponse.StatusCode}";
                lblWarning.Visibility = Visibility.Visible;
                return;
            }

            List<Client.FinancialStatementTemplate> financialStatementTemplateList = getFinancialStatementTemplatesResponse.Data.Where( a => a.IsActive ).ToList();

            if ( !financialStatementTemplateList.Any() )
            {
                lblWarning.Content = "No Templates available. Use the Rock website to define Financial Statement Templates.";
                lblWarning.Visibility = Visibility.Visible;
            }

            List<RadioButton> radioButtonList = new List<RadioButton>();
            foreach ( var financialStatementTemplate in financialStatementTemplateList.OrderBy( a => a.Name ) )
            {
                RadioButton radFinancialStatementTemplate = new RadioButton();
                radFinancialStatementTemplate.Tag = financialStatementTemplate;
                radFinancialStatementTemplate.Content = financialStatementTemplate.Name;
                radFinancialStatementTemplate.ToolTip = financialStatementTemplate.Description;

                radFinancialStatementTemplate.IsChecked = rockConfig.FinancialStatementTemplateGuid == financialStatementTemplate.Guid;
                radioButtonList.Add( radFinancialStatementTemplate );
            }

            if ( !radioButtonList.Any( a => a.IsChecked ?? false ) )
            {
                if ( radioButtonList.FirstOrDefault() != null )
                {
                    radioButtonList.First().IsChecked = true;
                }
            }

            lstFinancialStatementTemplates.Items.Clear();
            foreach ( var item in radioButtonList )
            {
                lstFinancialStatementTemplates.Items.Add( item );
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            var selected = lstFinancialStatementTemplates.Items.OfType<RadioButton>().Where( a => a.IsChecked == true ).FirstOrDefault();
            if ( selected == null )
            {
                if ( showWarnings )
                {
                    lblWarning.Content = "Please select a template.";
                    lblWarning.Visibility = Visibility.Visible;
                    return false;
                }
            }

            var rockConfig = RockConfig.Load();
            var financialStatementTemplate = selected?.Tag as Rock.Client.FinancialStatementTemplate;
            rockConfig.FinancialStatementTemplateGuid = financialStatementTemplate?.Guid;
            rockConfig.Save();

            ReportOptions.Current.FinancialStatementTemplateId = financialStatementTemplate?.Id;

            return true;
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
                var nextPage = new SelectDateRangePage();
                this.NavigationService.Navigate( nextPage );
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
    }
}
