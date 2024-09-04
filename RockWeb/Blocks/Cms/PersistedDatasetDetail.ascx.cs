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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Persisted Dataset Detail" )]
    [Category( "CMS" )]
    [Description( "Edit details of a Persisted Dataset" )]

    [Rock.SystemGuid.BlockTypeGuid( "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F" )]
    public partial class PersistedDatasetDetail : RockBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string PersistedDatasetId = "PersistedDatasetId";
        }

        #endregion PageParameterKey

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            etpEntityType.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().ToList();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.PersistedDatasetId ).AsInteger() );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="persistedDatasetId">The persisted dataset identifier.</param>
        public void ShowDetail( int persistedDatasetId )
        {
            if ( persistedDatasetId > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( PersistedDataset.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( PersistedDataset.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            var rockContext = new RockContext();
            var persistedDatasetService = new PersistedDatasetService( rockContext );
            PersistedDataset persistedDataset;

            if ( persistedDatasetId > 0 )
            {
                persistedDataset = persistedDatasetService.Get( persistedDatasetId );
            }
            else
            {
                persistedDataset = new PersistedDataset();
            }

            hfPersistedDatasetId.Value = persistedDatasetId.ToString();

            nbEditModeMessage.Text = string.Empty;

            if ( persistedDataset.IsSystem )
            {
                ceBuildScript.ReadOnly = true;
                ceBuildScript.Enabled = false;
                tbAccessKey.Enabled = false;
                nbEditModeMessage.Text = EditModeMessage.System( Category.FriendlyTypeName );
            }

            tbName.Text = persistedDataset.Name;
            cbIsActive.Checked = persistedDataset.IsActive;
            tbAccessKey.Text = persistedDataset.AccessKey;
            tbDescription.Text = persistedDataset.Description;
            ceBuildScript.Text = persistedDataset.BuildScript;
            lcpEnabledLavacommands.SelectedLavaCommands = persistedDataset.EnabledLavaCommands.SplitDelimitedValues().ToList();
            nbRefreshIntervalHours.Text = persistedDataset.RefreshIntervalMinutes.HasValue ? TimeSpan.FromMinutes( persistedDataset.RefreshIntervalMinutes.Value ).TotalHours.ToString( "F" ) : string.Empty;
            nbMemoryCacheDurationHours.Text = persistedDataset.MemoryCacheDurationMS.HasValue ? TimeSpan.FromMilliseconds( persistedDataset.MemoryCacheDurationMS.Value ).TotalHours.ToString( "F" ) : string.Empty;
            dtpExpireDateTime.SelectedDate = persistedDataset.ExpireDateTime;
            etpEntityType.SelectedEntityTypeId = persistedDataset.EntityTypeId;
            cbAllowManualRefresh.Checked = persistedDataset.AllowManualRefresh;
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            PersistedDatasetService persistedDatasetService = new PersistedDatasetService( rockContext );

            PersistedDataset persistedDataset;

            int persistedDatasetId = hfPersistedDatasetId.Value.AsInteger();
            if ( persistedDatasetId == 0 )
            {
                persistedDataset = new PersistedDataset();
                persistedDatasetService.Add( persistedDataset );
            }
            else
            {
                persistedDataset = persistedDatasetService.Get( persistedDatasetId );
            }

            var accessKey = tbAccessKey.Text;
            if ( accessKey.IsNullOrWhiteSpace() )
            {
                nbAccessKeyWarning.Visible = true;
                nbAccessKeyWarning.Text = "Access Key cannot be blank";
                return;
            }

            var accessKeyAlreadyExistsForDataSet = persistedDatasetService.Queryable().Where( a => a.AccessKey == accessKey && a.Id != persistedDataset.Id ).AsNoTracking().FirstOrDefault();

            if ( accessKeyAlreadyExistsForDataSet != null )
            {
                nbAccessKeyWarning.Visible = true;
                nbAccessKeyWarning.Text = string.Format( "Access Key must be unique. {0} is using '{1}' as the access key", accessKeyAlreadyExistsForDataSet.Name, accessKey );
                return;
            }

            persistedDataset.Name = tbName.Text;
            persistedDataset.IsActive = cbIsActive.Checked;
            persistedDataset.AccessKey = tbAccessKey.Text;
            persistedDataset.Description = tbDescription.Text;
            persistedDataset.BuildScript = ceBuildScript.Text;
            persistedDataset.EnabledLavaCommands = lcpEnabledLavacommands.SelectedLavaCommands.AsDelimited( "," );
            var refreshIntervalHours = nbRefreshIntervalHours.Text.AsDoubleOrNull();

            if ( refreshIntervalHours.HasValue )
            {
                persistedDataset.RefreshIntervalMinutes = ( int ) TimeSpan.FromHours( refreshIntervalHours.Value ).TotalMinutes;
            }
            else
            {
                persistedDataset.RefreshIntervalMinutes = null;
            }

            var memoryCacheDurationHours = nbMemoryCacheDurationHours.Text.AsDoubleOrNull();

            if ( memoryCacheDurationHours.HasValue )
            {
                persistedDataset.MemoryCacheDurationMS = ( int ) TimeSpan.FromHours( memoryCacheDurationHours.Value ).TotalMilliseconds;
            }
            else
            {
                persistedDataset.MemoryCacheDurationMS = null;
            }

            persistedDataset.ExpireDateTime = dtpExpireDateTime.SelectedDate;
            persistedDataset.EntityTypeId = etpEntityType.SelectedEntityTypeId;
            persistedDataset.AllowManualRefresh = cbAllowManualRefresh.Checked;

            // just in case anything has changed, null out the LastRefreshDateTime and TimeToBuild to mark this as needing to be refreshed the next time the Persisted Dataset job runs
            persistedDataset.LastRefreshDateTime = null;
            persistedDataset.TimeToBuildMS = null;

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
    }
}