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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using Rock.UniversalSearch;
using System.Reflection;
using Rock.UniversalSearch.IndexModels;
using Rock.Transactions;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Universal Search Control Panel" )]
    [Category( "Core" )]
    [Description( "Block to configure Rock's universal search features." )]
    public partial class UniversalSearchControlPanel : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            gEntityList.RowDataBound += gEntityList_RowDataBound;
            gEntityList.DataKeyNames = new string[] { "Id" };

            mdEditEntityType.SaveClick += MdEditEntityType_SaveClick;
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
                LoadIndexDetails();
                LoadEntities();
                ShowSmartSearchView();

                ddlSearchType.BindToEnum<SearchType>();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSmartSearchEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSmartSearchEdit_Click( object sender, EventArgs e )
        {
            var entities = EntityTypeCache.All();
            var indexableEntities = entities.Where( i => i.IsIndexingSupported == true ).ToList();

            cblSmartSearchEntities.DataTextField = "FriendlyName";
            cblSmartSearchEntities.DataValueField = "Id";
            cblSmartSearchEntities.DataSource = indexableEntities;
            cblSmartSearchEntities.DataBind();

            var entitySetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

            List<string> entityList = entitySetting.Split( ',' ).ToList();

            foreach(ListItem checkbox in cblSmartSearchEntities.Items )
            {
                if ( entityList.Contains( checkbox.Value ) ) {
                    checkbox.Selected = true;
                }
            }

            tbSmartSearchFieldCrieria.Text = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );

            var searchType = ((int)SearchType.Wildcard).ToString();

            if ( !string.IsNullOrWhiteSpace( Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchSearchType" ) ) )
            {
                searchType = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchSearchType" );
            }

            ddlSearchType.SelectedValue = searchType;

            pnlSmartSearchEdit.Visible = true;
            pnlSmartSearchView.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbSmartSearchSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSmartSearchSave_Click( object sender, EventArgs e )
        {
            Rock.Web.SystemSettings.SetValue( "core_SmartSearchUniversalSearchEntities", string.Join(",", cblSmartSearchEntities.SelectedValues.Select( n => n.ToString() ).ToArray() ));
            Rock.Web.SystemSettings.SetValue( "core_SmartSearchUniversalSearchFieldCriteria", tbSmartSearchFieldCrieria.Text );
            Rock.Web.SystemSettings.SetValue( "core_SmartSearchUniversalSearchSearchType", ddlSearchType.SelectedValue );

            ShowSmartSearchView();

            pnlSmartSearchEdit.Visible = false;
            pnlSmartSearchView.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbSmartSearchCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSmartSearchCancel_Click( object sender, EventArgs e )
        {
            pnlSmartSearchEdit.Visible = false;
            pnlSmartSearchView.Visible = true;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the SaveClick event of the MdEditEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void MdEditEntityType_SaveClick( object sender, EventArgs e )
        {
            using(RockContext rockContext = new RockContext() )
            {
                EntityTypeService entityTypeService = new EntityTypeService( rockContext );
                var entityType = entityTypeService.Get( hfIdValue.ValueAsInt() );

                entityType.IsIndexingEnabled = cbEnabledIndexing.Checked;

                rockContext.SaveChanges();

                if ( cbEnabledIndexing.Checked )
                {
                    IndexContainer.CreateIndex( entityType.IndexModelType );

                    // call for bulk indexing
                    BulkIndexEntityTypeTransaction bulkIndexTransaction = new BulkIndexEntityTypeTransaction();
                    bulkIndexTransaction.EntityTypeId = entityType.Id;

                    RockQueue.TransactionQueue.Enqueue( bulkIndexTransaction );
                }
                else
                {
                    IndexContainer.DeleteIndex( entityType.IndexModelType );
                }

                // flush item from cache
                EntityTypeCache.Flush( entityType.Id );
            }

            mdEditEntityType.Hide();
            LoadEntities();
        }

        /// <summary>
        /// Handles the RowDataBound event of the GEntityList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gEntityList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                var isIndexingEnabled = dataItem.GetPropertyValue( "IsIndexingEnabled" ).ToString().AsBoolean();

                var entityAssembly = dataItem.GetPropertyValue( "AssemblyName" ).ToString();
                Type modelType = Type.GetType( entityAssembly );

                if (modelType != null )
                {
                    var modelInstance = Activator.CreateInstance( modelType );
                    var supportBulkIndexing = (bool)modelType.GetProperty( "AllowsInteractiveBulkIndexing" ).GetValue( modelInstance, null );

                    if ( !supportBulkIndexing )
                    {
                        e.Row.Cells[3].Controls[0].Visible = false;
                    }
                }

                if ( !isIndexingEnabled )
                {
                    e.Row.Cells[2].Controls[0].Visible = false;
                    e.Row.Cells[3].Controls[0].Visible = false;
                    //e.Row.Cells[4].Controls[0].Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gEntityList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gEntityList_RowSelected( object sender, RowEventArgs e )
        {
            var entityType = EntityTypeCache.Read( e.RowKeyId );

            hfIdValue.Value = e.RowKeyId.ToString();

            cbEnabledIndexing.Checked = entityType.IsIndexingEnabled;

            mdEditEntityType.Title = entityType.FriendlyName + " Configuration";

            mdEditEntityType.Show();
        }

        /// <summary>
        /// Handles the Click event of the gContentItemBulkLoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBulkLoad_Click( object sender, RowEventArgs e )
        {
            var entityType = EntityTypeCache.Read( e.RowKeyId );

            if (entityType != null )
            {
                BulkIndexEntityTypeTransaction bulkIndexTransaction = new BulkIndexEntityTypeTransaction();
                bulkIndexTransaction.EntityTypeId = entityType.Id;

                RockQueue.TransactionQueue.Enqueue( bulkIndexTransaction );

                maMessages.Show( string.Format("A request has been sent to index {0}.", entityType.FriendlyName.Pluralize()), ModalAlertType.Information );
            }
            else
            {
                maMessages.Show( "An error occurred launching the bulk index request. Could not find the entity type.", ModalAlertType.Alert );
            }
        }

        /// <summary>
        /// Handles the Click event of the gClearIndex control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gClearIndex_Click( object sender, RowEventArgs e )
        {
            var entityType = EntityTypeCache.Read( e.RowKeyId );
            Type type = entityType.GetEntityType();

            if ( type != null )
            {
                object classInstance = Activator.CreateInstance( type, null );
                MethodInfo deleteItemsMethod = type.GetMethod( "DeleteIndexedDocuments" );

                if ( classInstance != null && deleteItemsMethod != null )
                {
                    deleteItemsMethod.Invoke( classInstance, null );

                    maMessages.Show( string.Format( "All documents in the {0} index have been deleted.", entityType.FriendlyName ), ModalAlertType.Information );
                }
            }
        }


        /// <summary>
        /// Handles the Click event of the gRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRefresh_Click( object sender, RowEventArgs e )
        {
            RockContext rockContext = new RockContext();
            EntityTypeService entityTypeService = new EntityTypeService( rockContext );
            var entityType = entityTypeService.Get( e.RowKeyId );

            if ( entityType != null )
            {
                IndexContainer.DeleteIndex( entityType.IndexModelType );
                IndexContainer.CreateIndex( entityType.IndexModelType );

                maMessages.Show( string.Format( "The index for {0} has been re-created.", entityType.FriendlyName ), ModalAlertType.Information );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the smart search view.
        /// </summary>
        private void ShowSmartSearchView()
        {
            var entitySetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

            if ( !string.IsNullOrWhiteSpace( entitySetting ) )
            {
                List<int> entityIds = entitySetting.Split( ',' ).Select( int.Parse ).ToList();

                var selected = string.Join( ", ", EntityTypeCache.All().Where( e => entityIds.Contains( e.Id ) ).Select( e => e.FriendlyName ).ToList() );

                lSmartSearchEntities.Text = string.Join( ",", EntityTypeCache.All().Where( e => entityIds.Contains( e.Id ) ).Select( e => e.FriendlyName ).ToList() );
            }
            lSmartSearchFilterCriteria.Text = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );

            var searchType = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchSearchType" ).ConvertToEnumOrNull<SearchType>() ?? SearchType.Wildcard;
            lSearchType.Text = searchType.ToString();
        }

        /// <summary>
        /// Loads the index details.
        /// </summary>
        private void LoadIndexDetails()
        {
            bool searchEnabled = false;

            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive )
                {
                    hlblEnabled.Text = component.EntityType.FriendlyName;
                    hlblEnabled.LabelType = LabelType.Success;
                    searchEnabled = true;

                    if ( !component.IsConnected )
                    {
                        hlblEnabled.LabelType = LabelType.Warning;
                        nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                        nbMessages.Text = string.Format( "Could not connect to the {0} server at {1}.", component.EntityType.FriendlyName, component.IndexLocation );

                        // add friendly check to see if the url provided is valid
                        Uri uriTest;
                        bool isValidUrl = Uri.TryCreate( component.IndexLocation, UriKind.Absolute, out uriTest )
                            && (uriTest.Scheme == Uri.UriSchemeHttp || uriTest.Scheme == Uri.UriSchemeHttps);

                        if ( !isValidUrl )
                        {
                            nbMessages.Text += " Note that the URL provided is not valid. The pattern should be http(s)://server:port.";
                        }
                    }

                    lIndexLocation.Text = component.IndexLocation;

                    break;
                }
            }

            if ( !searchEnabled )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbMessages.Text = "No universal search index components are currently enabled. You must enable a index component under <span class='navigation-tip'>Admin Tools &gt; System Settngs &gt; Universal Search Index Components</span>.";
            }
        }

        /// <summary>
        /// Loads the entities.
        /// </summary>
        private void LoadEntities()
        {
            using ( RockContext rockContext = new RockContext() ) {
                var entities = new EntityTypeService( rockContext ).Queryable().AsNoTracking().ToList();

                var indexableEntities = entities.Where( e => e.IsIndexingSupported == true ).ToList();

                gEntityList.DataSource = indexableEntities;
                gEntityList.DataBind();
            }
        }

        #endregion
    }
}