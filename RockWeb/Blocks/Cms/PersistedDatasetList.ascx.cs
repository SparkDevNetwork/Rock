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
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Persisted Dataset List" )]
    [Category( "CMS" )]
    [Description( "Lists Persisted Datasets" )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]

    [DecimalField(
        "Max Preview Size (MB)",
        Key = AttributeKey.MaxPreviewSizeMB,
        Description = "If the JSON data is large, it could cause the browser to timeout.",
        IsRequired = true,
        DefaultDecimalValue = 1,
        Order = 2 )]

    [Rock.SystemGuid.BlockTypeGuid( "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6" )]
    public partial class PersistedDatasetList : RockBlock, ICustomGridColumns
    {

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string MaxPreviewSizeMB = "MaxPreviewSizeMB";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
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
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gList.DataKeyNames = new string[] { "Id" };
            gList.Actions.ShowAdd = canAddEditDelete;
            gList.IsDeleteEnabled = canAddEditDelete;
            gList.Actions.AddClick += gList_AddClick;
            gList.GridRebind += gList_GridRebind;
            gList.EntityTypeId = EntityTypeCache.Get<PersistedDataset>().Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
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
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.PersistedDatasetId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.PersistedDatasetId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PersistedDatasetService persistedDatasetService = new PersistedDatasetService( rockContext );
            PersistedDataset persistedDataset = persistedDatasetService.Get( e.RowKeyId );
            persistedDataset.UpdateResultData();
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lbPreview_Click( object sender, RowEventArgs e )
        {
            try
            {
                var rockContext = new RockContext();
                PersistedDatasetService persistedDatasetService = new PersistedDatasetService( rockContext );
                PersistedDataset persistedDataset = persistedDatasetService.GetNoTracking( e.RowKeyId );
                if ( persistedDataset.LastRefreshDateTime == null )
                {
                    persistedDataset.UpdateResultData();
                }

                // limit preview size (default is 1MB)
                var maxPreviewSizeMB = this.GetAttributeValue( AttributeKey.MaxPreviewSizeMB ).AsDecimalOrNull() ?? 1;

                // make sure they didn't put in a negative number
                maxPreviewSizeMB = Math.Max( 1, maxPreviewSizeMB );

                var maxPreviewSizeLength = ( int ) ( maxPreviewSizeMB * 1024 * 1024 );

                var preViewObject = persistedDataset.ResultData.FromJsonDynamic().ToJson( true );

                lPreviewJson.Text = ( string.Format( "<pre>{0}</pre>", preViewObject ) ).Truncate( maxPreviewSizeLength );

                nbPreviewMessage.Visible = false;
                nbPreviewMaxLengthWarning.Visible = false;

                if ( preViewObject.Length > maxPreviewSizeLength )
                {
                    nbPreviewMaxLengthWarning.Text = string.Format( "JSON size is {0}. Showing first {1}.", preViewObject.Length.FormatAsMemorySize(), maxPreviewSizeLength.FormatAsMemorySize() );
                    nbPreviewMaxLengthWarning.Visible = true;
                }

                nbPreviewMessage.Text = string.Format( "Time to build Dataset: {0:F0}ms", persistedDataset.TimeToBuildMS );
                nbPreviewMessage.Details = null;
                nbPreviewMessage.NotificationBoxType = NotificationBoxType.Success;
                nbPreviewMessage.Visible = true;
            }
            catch ( Exception ex )
            {
                nbPreviewMessage.Text = "Error building Dataset object from the JSON generated from the Build Script";
                nbPreviewMessage.Details = ex.Message;
                nbPreviewMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbPreviewMessage.Visible = true;
            }

            mdPreview.Show();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gList_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            PersistedDatasetService persistedDatasetService = new PersistedDatasetService( rockContext );
            PersistedDataset persistedDataset = persistedDatasetService.Get( e.RowKeyId );
            if ( persistedDataset != null )
            {
                string errorMessage;
                if ( !persistedDatasetService.CanDelete( persistedDataset, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                persistedDatasetService.Delete( persistedDataset );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the DataBound event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnRefresh_DataBound( object sender, RowEventArgs e )
        {
            var button = sender as LinkButton;
            var persistedDataSet = e.Row.DataItem as PersistedDataset;
            if ( button != null && persistedDataSet != null )
            {
                button.Enabled = persistedDataSet.AllowManualRefresh;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            PersistedDatasetService persistedDatasetService = new PersistedDatasetService( rockContext );

            // Use AsNoTracking() since these records won't be modified, and therefore don't need to be tracked by the EF change tracker
            var qry = persistedDatasetService.Queryable().AsNoTracking();

            var sortProperty = gList.SortProperty;
            if ( gList.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.Name ).ThenBy( a => a.AccessKey );
            }

            // Get data. We need to do this so we can get the size of the return set. Confirmed that the anonymous type below does a SQL LEN() function.
            var persistedDataSet = qry.Select( d => new
            {
                d.Name,
                d.Id,
                Size = d.ResultData != null ? d.ResultData.Length / 1024 : 0,
                d.AccessKey,
                d.TimeToBuildMS,
                d.LastRefreshDateTime,
            } ).ToList();

            gList.DataSource = persistedDataSet;
            gList.DataBind();
        }

        #endregion
    }
}