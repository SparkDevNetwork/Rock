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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Rock.Net;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectLavaTemplatePage.xaml
    /// </summary>
    public partial class SelectLavaTemplatePage : Page
    {
        /// <summary>
        /// The _rock rest client
        /// </summary>
        private RockRestClient _rockRestClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectLavaTemplatePage"/> class.
        /// </summary>
        public SelectLavaTemplatePage()
        {
            InitializeComponent();

            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            LoadLavaTemplates();
        }

        /// <summary>
        /// Loads the lava templates.
        /// </summary>
        public void LoadLavaTemplates()
        {
            var rockConfig = RockConfig.Load();

            var lavaTemplateDefineValues = _rockRestClient.GetData<List<Rock.Client.DefinedValue>>( "api/FinancialTransactions/GetStatementGeneratorTemplates" );

            List<RadioButton> radioButtonList = new List<RadioButton>();
            foreach ( var lavaTemplateDefineValue in lavaTemplateDefineValues.OrderBy( a => a.Order ).ThenBy( a => a.Value ) )
            {
                RadioButton radLavaTemplate = new RadioButton();
                radLavaTemplate.Tag = lavaTemplateDefineValue.Guid;
                radLavaTemplate.Content = lavaTemplateDefineValue.Value;
                radLavaTemplate.ToolTip = lavaTemplateDefineValue.Description;

                radLavaTemplate.IsChecked = rockConfig.LayoutDefinedValueGuid == lavaTemplateDefineValue.Guid;
                radioButtonList.Add( radLavaTemplate );
            }

            if ( !radioButtonList.Any( a => a.IsChecked ?? false ) )
            {
                if ( radioButtonList.FirstOrDefault() != null )
                {
                    radioButtonList.First().IsChecked = true;
                }
            }

            lstLavaTemplates.Items.Clear();
            foreach ( var item in radioButtonList )
            {
                lstLavaTemplates.Items.Add( item );
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            var selected = lstLavaTemplates.Items.OfType<RadioButton>().First( a => a.IsChecked == true );
            if ( selected == null )
            {
                if ( showWarnings )
                {
                    return false;
                }
            }

            var rockConfig = RockConfig.Load();
            rockConfig.LayoutDefinedValueGuid = selected?.Tag as Guid?;
            rockConfig.Save();

            ReportOptions.Current.LayoutDefinedValueGuid = selected?.Tag as Guid?;

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
