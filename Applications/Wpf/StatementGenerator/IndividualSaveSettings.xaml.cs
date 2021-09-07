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

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Page" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class IndividualSaveSettings : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSaveLocationPage"/> class.
        /// </summary>
        public IndividualSaveSettings()
        {
            InitializeComponent();
            var rockConfig = RockConfig.Load();

            Client.FinancialStatementIndividualSaveOptions saveOptions = null;

            try
            {
                if ( rockConfig.IndividualSaveOptionsJson.IsNotNullOrWhiteSpace() )
                {
                    saveOptions = rockConfig.IndividualSaveOptionsJson.FromJsonOrNull<FinancialStatementIndividualSaveOptions>();
                }
            }
            catch
            {
                // ignore if the JSON can't be de-serialized. We'll just create a new one instead.
            }

            saveOptions = saveOptions ?? new Client.FinancialStatementIndividualSaveOptions();

            LoadDocumentTypes( saveOptions.DocumentTypeId );

            cbSaveStatementsForIndividuals.IsChecked = saveOptions.SaveStatementsForIndividuals;
            txtDocumentDescription.Text = saveOptions.DocumentDescription;
            txtDocumentName.Text = saveOptions.DocumentName;
            txtDocumentPurposeKey.Text = saveOptions.DocumentPurposeKey;
            cbOverwriteDocumentsOfThisTypeCreatedOnSameDate.IsChecked = saveOptions.OverwriteDocumentsOfThisTypeCreatedOnSameDate;
            rbSaveForAllAdults.IsChecked = saveOptions.DocumentSaveFor == Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveAdultsInGivingGroup;
            rbSaveForPrimaryGiver.IsChecked = saveOptions.DocumentSaveFor == Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.PrimaryGiver;
            rbSaveForAllActiveFamilyMembers.IsChecked = saveOptions.DocumentSaveFor == Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveFamilyMembersInGivingGroup;

            ReportOptions.Current.IndividualSaveOptions = saveOptions;
        }

        /// <summary>
        /// Loads the document types.
        /// </summary>
        /// <param name="selectedDocumentTypeId">The selected document type identifier.</param>
        private void LoadDocumentTypes( int? selectedDocumentTypeId )
        {
            var rockConfig = RockConfig.Load();
            var restClient = new RestClient( rockConfig.RockBaseUrl );
            restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

            var getDocumentTypesRequest = new RestRequest( "api/DocumentTypes?$filter=EntityType/Name eq 'Rock.Model.Person'" );
            var getDocumentTypesResponse = restClient.Execute<List<Client.DocumentType>>( getDocumentTypesRequest );

            if ( getDocumentTypesResponse.ErrorException != null )
            {
                throw getDocumentTypesResponse.ErrorException;
            }

            List<Client.DocumentType> documentTypeList = getDocumentTypesResponse.Data;

            cboDocumentType.Items.Clear();
            foreach ( var documentType in documentTypeList.OrderBy( d => d.Name ) )
            {
                var item = new ComboBoxItem { Content = documentType.Name, Tag = documentType.Id };
                item.IsSelected = documentType.Id == selectedDocumentTypeId;
                cboDocumentType.Items.Add( item );
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            var saveOptions = ReportOptions.Current.IndividualSaveOptions ?? new Client.FinancialStatementIndividualSaveOptions();
            saveOptions.SaveStatementsForIndividuals = cbSaveStatementsForIndividuals.IsChecked ?? false;
            saveOptions.DocumentTypeId = ( int? ) ( cboDocumentType.SelectedItem as ComboBoxItem )?.Tag;
            saveOptions.DocumentDescription = txtDocumentDescription.Text;
            saveOptions.DocumentName = txtDocumentName.Text;
            saveOptions.DocumentPurposeKey = txtDocumentPurposeKey.Text;
            saveOptions.OverwriteDocumentsOfThisTypeCreatedOnSameDate = cbOverwriteDocumentsOfThisTypeCreatedOnSameDate.IsChecked ?? false;

            if ( rbSaveForAllAdults.IsChecked == true )
            {
                saveOptions.DocumentSaveFor = Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveAdultsInGivingGroup;
            }
            else if ( rbSaveForPrimaryGiver.IsChecked == true )
            {
                saveOptions.DocumentSaveFor = Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.PrimaryGiver;
            }
            else if ( rbSaveForAllActiveFamilyMembers.IsChecked == true )
            {
                saveOptions.DocumentSaveFor = Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveFamilyMembersInGivingGroup;
            }

            ReportOptions.Current.IndividualSaveOptions = saveOptions;

            var rockConfig = RockConfig.Load();
            rockConfig.IndividualSaveOptionsJson = saveOptions.ToJson();
            rockConfig.Save();

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
                var nextPage = new ReportSettings();
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

        /// <summary>
        /// Handles the Checked event of the cbSaveStatementsForIndividuals control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cbSaveStatementsForIndividuals_Checked( object sender, RoutedEventArgs e )
        {
            pnlIndividualStatementOptions.IsEnabled = true;
        }

        /// <summary>
        /// Handles the Unchecked event of the cbSaveStatementsForIndividuals control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cbSaveStatementsForIndividuals_Unchecked( object sender, RoutedEventArgs e )
        {
            pnlIndividualStatementOptions.IsEnabled = false;
        }
    }
}
