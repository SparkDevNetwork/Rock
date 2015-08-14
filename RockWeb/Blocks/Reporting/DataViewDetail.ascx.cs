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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Data View Detail" )]
    [Category( "Reporting" )]
    [Description( "Shows the details of the given data view." )]

    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180 )]
    public partial class DataViewDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Switch does not automatically initialize again after a partial-postback.  This script 
            // looks for any switch elements that have not been initialized and re-intializes them.
            string script = @"
$(document).ready(function() {
    $('.switch > input').each( function () {
        $(this).parent().switch('init');
    });
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "toggle-switch-init", script, true );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", DataView.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.DataView ) ).Id;

            gReport.GridRebind += gReport_GridRebind;

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
            }
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
                string itemId = PageParameter( "DataViewId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( itemId.AsInteger(), PageParameter( "ParentCategoryId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            RockContext rockContext = new RockContext();
            CreateFilterControl( ViewState["EntityTypeId"] as int?, DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false, rockContext );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DataViewFilter"] = ReportingHelper.GetFilterFromControls( phFilters ).ToJson();
            ViewState["EntityTypeId"] = etpEntityType.SelectedEntityTypeId;
            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var service = new DataViewService( new RockContext() );
            var item = service.Get( int.Parse( hfDataViewId.Value ) );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Handles the Click event of the Copy button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            // Create a new Data View using the current item as a template.
            var id = int.Parse( hfDataViewId.Value );
            
            var dataViewService = new DataViewService( new RockContext() );

            var newItem = dataViewService.GetNewFromTemplate( id );

            if (newItem == null)
                return;

            newItem.Name += " (Copy)";

            // Reset the stored identifier for the active Data View.
            hfDataViewId.Value = "0";

            ShowEditDetails( newItem );
        }

        /// <summary>
        /// Set the Guids on the datafilter and it's children to Guid.NewGuid
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        private void SetNewDataFilterGuids( DataViewFilter dataViewFilter )
        {
            if ( dataViewFilter != null )
            {
                dataViewFilter.Guid = Guid.NewGuid();
                foreach ( var childFilter in dataViewFilter.ChildFilters )
                {
                    SetNewDataFilterGuids( childFilter );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            DataView dataView = null;

            var rockContext = new RockContext();
            DataViewService service = new DataViewService( rockContext );

            int dataViewId = int.Parse( hfDataViewId.Value );
            int? origDataViewFilterId = null;

            if ( dataViewId == 0 )
            {
                dataView = new DataView();
                dataView.IsSystem = false;
            }
            else
            {
                dataView = service.Get( dataViewId );
                origDataViewFilterId = dataView.DataViewFilterId;
            }

            dataView.Name = tbName.Text;
            dataView.Description = tbDescription.Text;
            dataView.TransformEntityTypeId = ddlTransform.SelectedValueAsInt();
            dataView.EntityTypeId = etpEntityType.SelectedEntityTypeId;
            dataView.CategoryId = cpCategory.SelectedValueAsInt();

            var newDataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );
            

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !dataView.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            var adding = dataView.Id.Equals( 0 );
            if ( adding )
            {
                service.Add( dataView );
            }

            rockContext.WrapTransaction( () =>
            {
                
                if ( origDataViewFilterId.HasValue )
                {
                    // delete old report filter so that we can add the new filter (but with original guids), then drop the old filter
                    DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );
                    DataViewFilter origDataViewFilter = dataViewFilterService.Get( origDataViewFilterId.Value );

                    dataView.DataViewFilterId = null;
                    rockContext.SaveChanges();
                    
                    DeleteDataViewFilter( origDataViewFilter, dataViewFilterService );
                }
                
                dataView.DataViewFilter = newDataViewFilter;
                rockContext.SaveChanges();
            } );

            if ( adding )
            {
                // add EDIT and ADMINISTRATE to the person who added the dataView
                Rock.Security.Authorization.AllowPerson( dataView, Authorization.EDIT, this.CurrentPerson, rockContext );
                Rock.Security.Authorization.AllowPerson( dataView, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["DataViewId"] = dataView.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // Check if we are editing an existing Data View.
            int dataViewId = hfDataViewId.Value.AsInteger();

            if (dataViewId == 0)
            {
                // If not, check if we are editing a new copy of an existing Data View.
                dataViewId = PageParameter( "DataViewId" ).AsInteger();
            }

            if ( dataViewId == 0 )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["CategoryId"] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                DataViewService service = new DataViewService( new RockContext() );
                DataView item = service.Get( dataViewId );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? categoryId = null;

            var rockContext = new RockContext();
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Get( int.Parse( hfDataViewId.Value ) );

            if ( dataView != null )
            {
                string errorMessage;
                if ( !dataViewService.CanDelete( dataView, out errorMessage ) )
                {
                    ShowReadonlyDetails( dataView );
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                }
                else
                {
                    categoryId = dataView.CategoryId;
                    
                    // delete report filter
                    try
                    {
                        DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );
                        DeleteDataViewFilter( dataView.DataViewFilter, dataViewFilterService );
                    }
                    catch
                    {
                        //
                    }

                    dataViewService.Delete( dataView );
                    rockContext.SaveChanges();

                    // reload page, selecting the deleted data view's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != null )
                    {
                        qryParams["CategoryId"] = categoryId.ToString();
                    }

                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( DataView dataView )
        {
            var rockContext = new RockContext();
            etpEntityType.EntityTypes = new EntityTypeService( rockContext )
                .GetReportableEntities( this.CurrentPerson )
                .OrderBy( t => t.FriendlyName ).ToList();
        }

        /// <summary>
        /// Binds the data transformations.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public void BindDataTransformations( RockContext rockContext )
        {
            ddlTransform.Items.Clear();
            int? entityTypeId = etpEntityType.SelectedEntityTypeId;
            if ( entityTypeId.HasValue )
            {
                var filteredEntityType = EntityTypeCache.Read( entityTypeId.Value );
                foreach ( var component in DataTransformContainer.GetComponentsByTransformedEntityName( filteredEntityType.Name ).OrderBy( c => c.Title ) )
                {
                    if ( component.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                    {
                        var transformEntityType = EntityTypeCache.Read( component.TypeName );
                        ListItem li = new ListItem( component.Title, transformEntityType.Id.ToString() );
                        ddlTransform.Items.Add( li );
                    }
                }
            }

            ddlTransform.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        public void ShowDetail( int dataViewId )
        {
            ShowDetail( dataViewId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( int dataViewId, int? parentCategoryId )
        {
            pnlDetails.Visible = false;

            var rockContext = new RockContext();

            var dataViewService = new DataViewService( rockContext );
            DataView dataView = null;

            if ( !dataViewId.Equals( 0 ) )
            {
                dataView = dataViewService.Get( dataViewId );
            }

            if ( dataView == null )
            {
                dataView = new DataView { Id = 0, IsSystem = false, CategoryId = parentCategoryId };
                dataView.Name = string.Empty;
            }

            if ( !dataView.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfDataViewId.Value = dataView.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            string authorizationMessage = string.Empty;

            if ( !dataView.IsAuthorizedForAllDataViewComponents( Authorization.EDIT, CurrentPerson, rockContext, out authorizationMessage ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = authorizationMessage;
            }

            if ( dataView.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( DataView.FriendlyTypeName );
            }

            btnSecurity.Visible = dataView.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = dataView.Name;
            btnSecurity.EntityId = dataView.Id;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( dataView );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Enabled = dataViewService.CanDelete( dataView, out errorMessage );
                if (!btnDelete.Enabled)
                {
                    btnDelete.ToolTip = errorMessage;
                    btnDelete.Attributes["onclick"] = null;
                }

                if ( dataView.Id > 0 )
                {
                    ShowReadonlyDetails( dataView );
                }
                else
                {
                    ShowEditDetails( dataView );
                }
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        public void ShowEditDetails( DataView dataView )
        {
            if ( dataView.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( DataView.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( DataView.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );
            LoadDropDowns( dataView );

            if ( dataView.DataViewFilter == null || dataView.DataViewFilter.ExpressionType == FilterExpressionType.Filter )
            {
                dataView.DataViewFilter = new DataViewFilter();
                dataView.DataViewFilter.Guid = new Guid();
                dataView.DataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            }

            tbName.Text = dataView.Name;
            tbDescription.Text = dataView.Description;
            etpEntityType.SelectedEntityTypeId = dataView.EntityTypeId;
            cpCategory.SetValue( dataView.CategoryId );

            var rockContext = new RockContext();
            BindDataTransformations( rockContext );
            ddlTransform.SetValue( dataView.TransformEntityTypeId ?? 0 );

            CreateFilterControl( dataView.EntityTypeId, dataView.DataViewFilter, true, rockContext );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void ShowReadonlyDetails( DataView dataView )
        {
            SetEditMode( false );
            hfDataViewId.SetValue( dataView.Id );
            lReadOnlyTitle.Text = dataView.Name.FormatAsHtmlTitle();

            lDescription.Text = dataView.Description;

            DescriptionList descriptionListMain = new DescriptionList();

            if ( dataView.EntityType != null )
            {
                descriptionListMain.Add( "Applies To", dataView.EntityType.FriendlyName );
            }

            if ( dataView.Category != null )
            {
                descriptionListMain.Add( "Category", dataView.Category.Name );
            }

            if ( dataView.TransformEntityType != null )
            {
                descriptionListMain.Add( "Post-filter Transformation", dataView.TransformEntityType.FriendlyName );
            }

            lblMainDetails.Text = descriptionListMain.Html;

            DescriptionList descriptionListFilters = new DescriptionList();

            if ( dataView.DataViewFilter != null && dataView.EntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Read( dataView.EntityTypeId.Value );
                if ( entityTypeCache != null )
                {
                    var entityTypeType = entityTypeCache.GetEntityType();
                    if ( entityTypeType != null )
                    {
                        descriptionListFilters.Add( "Filter", dataView.DataViewFilter.ToString( entityTypeType ) );
                    }
                }
            }

            lFilters.Text = descriptionListFilters.Html;

            ShowReport( dataView );
        }

        /// <summary>
        /// Shows the report.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void ShowReport( DataView dataView )
        {
            var rockContext = new RockContext();
            if ( dataView.EntityTypeId.HasValue && dataView.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                string authorizationMessage = string.Empty;

                bool isPersonDataSet = dataView.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                if ( isPersonDataSet )
                {
                    gReport.PersonIdField = "Id";
                    gReport.DataKeyNames = new string[] { "Id" };
                }
                else
                {
                    gReport.PersonIdField = null;
                }

                if ( dataView.EntityTypeId.HasValue )
                {
                    var entityTypeCache = EntityTypeCache.Read( dataView.EntityTypeId.Value, rockContext );
                    if (entityTypeCache != null)
                    {
                        gReport.RowItemText = entityTypeCache.FriendlyName;
                    }
                }

                gReport.Visible = true;
                BindGrid( gReport, dataView );
            }
            else
            {
                gReport.Visible = false;
            }
        }

        /// <summary>
        /// Shows the preview.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="filter">The filter.</param>
        private void ShowPreview( DataView dataView )
        {
            BindGrid( gPreview, dataView, 15 );
            
            modalPreview.Show();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="dataView">The data view.</param>
        /// <returns></returns>
        private bool BindGrid( Grid grid, DataView dataView, int? fetchRowCount = null )
        {
            var errorMessages = new List<string>();
            grid.DataSource = null;
            var rockContext = new RockContext();

            if ( dataView.EntityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Read( dataView.EntityTypeId.Value );
                if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();

                    if ( entityType != null )
                    {
                        try
                        {
                            grid.CreatePreviewColumns( entityType );

                            var qry = dataView.GetQuery( grid.SortProperty, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180, out errorMessages );

                            if ( fetchRowCount.HasValue )
                            {
                                qry = qry.Take( fetchRowCount.Value );
                            }
                        
                            grid.SetLinqDataSource( qry.AsNoTracking() );
                            grid.DataBind();
                        }
                        catch ( Exception ex )
                        {
                            this.LogException( ex );
                            Exception exception = ex;
                            while ( exception != null )
                            {
                                if ( exception is System.Data.SqlClient.SqlException )
                                {
                                    // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                                    if ( ( exception as System.Data.SqlClient.SqlException ).Number == -2 )
                                    {
                                        nbEditModeMessage.NotificationBoxType = NotificationBoxType.Warning;
                                        nbEditModeMessage.Text = "This dataview did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                                        return false;
                                    }
                                    else
                                    {
                                        errorMessages.Add( exception.Message );
                                        exception = exception.InnerException;
                                    }
                                }
                                else
                                {
                                    errorMessages.Add( exception.Message );
                                    exception = exception.InnerException;
                                }
                            }
                        }
                    }
                }
            }

            var errorBox = ( grid == gPreview) ? nbPreviewError : nbGridError;

            if ( errorMessages.Any() )
            {
                errorBox.NotificationBoxType = NotificationBoxType.Warning;
                errorBox.Text = "WARNING: There was a problem with one or more of the filters for this data view...<br/><br/> " + errorMessages.AsDelimited( "<br/>" );
                errorBox.Visible = true;
            }
            else
            {
                errorBox.Visible = false;
            }

            if ( dataView.EntityTypeId.HasValue )
            {
                grid.RowItemText = EntityTypeCache.Read( dataView.EntityTypeId.Value ).FriendlyName;
            }

            if ( grid.DataSource != null )
            {
                grid.ExportFilename = dataView.Name;
                return true;
            }

            return false;
        }

        #endregion

        #region Activities and Actions

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            var service = new DataViewService( new RockContext() );
            var item = service.Get( int.Parse( hfDataViewId.Value ) );
            ShowReport( item );
        }

        /// <summary>
        /// Handles the Click event of the btnPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPreview_Click( object sender, EventArgs e )
        {
            DataView dv = new DataView();
            dv.TransformEntityTypeId = ddlTransform.SelectedValueAsInt();
            dv.EntityTypeId = etpEntityType.SelectedEntityTypeId;
            dv.DataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );
            ShowPreview( dv );
        }

        /// <summary>
        /// Handles the AddFilterClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();
            filterField.DataViewFilterGuid = Guid.NewGuid();
            filterField.DeleteClick += filterControl_DeleteClick;
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );
            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup();
            
            childGroupControl.AddFilterClick += groupControl_AddFilterClick;
            childGroupControl.AddGroupClick += groupControl_AddGroupClick;
            childGroupControl.DeleteGroupClick += groupControl_DeleteGroupClick;

            childGroupControl.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( childGroupControl );
            childGroupControl.ID = string.Format( "fg_{0}", childGroupControl.DataViewFilterGuid.ToString( "N" ) );
            childGroupControl.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            childGroupControl.FilterType = FilterExpressionType.GroupAll;
        }

        /// <summary>
        /// Handles the DeleteClick event of the filterControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );
        }

        /// <summary>
        /// Deletes the data view filter.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="service">The service.</param>
        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service )
        {
            if ( dataViewFilter != null )
            {
                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service );
                }

                service.Delete( dataViewFilter );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="filteredEntityTypeId">The filtered entity type identifier.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( int? filteredEntityTypeId, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter != null && filteredEntityTypeId.HasValue )
            {
                var filteredEntityType = EntityTypeCache.Read( filteredEntityTypeId.Value );
                CreateFilterControl( phFilters, filter, filteredEntityType.Name, setSelection, rockContext );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="filteredEntityTypeName">Name of the filtered entity type.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, string filteredEntityTypeName, bool setSelection, RockContext rockContext )
        {
            if ( filter.ExpressionType == FilterExpressionType.Filter )
            {
                var filterControl = new FilterField();
                parentControl.Controls.Add( filterControl );
                filterControl.DataViewFilterGuid = filter.Guid;
                filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );
                filterControl.FilteredEntityTypeName = filteredEntityTypeName;
                if ( filter.EntityTypeId.HasValue )
                {
                    var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( filter.EntityTypeId.Value, rockContext );
                    if ( entityTypeCache != null )
                    {
                        filterControl.FilterEntityTypeName = entityTypeCache.Name;
                    }
                }

                filterControl.Expanded = filter.Expanded;
                if ( setSelection )
                {
                    filterControl.SetSelection(filter.Selection);
                }

                filterControl.DeleteClick += filterControl_DeleteClick;
            }
            else
            {
                var groupControl = new FilterGroup();
                parentControl.Controls.Add( groupControl );
                groupControl.DataViewFilterGuid = filter.Guid;
                groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                groupControl.FilteredEntityTypeName = filteredEntityTypeName;
                groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                if ( setSelection )
                {
                    groupControl.FilterType = filter.ExpressionType;
                }

                groupControl.AddFilterClick += groupControl_AddFilterClick;
                groupControl.AddGroupClick += groupControl_AddGroupClick;
                groupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
                foreach ( var childFilter in filter.ChildFilters )
                {
                    CreateFilterControl( groupControl, childFilter, filteredEntityTypeName, setSelection, rockContext );
                }
            }
        }



        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var dataViewFilter = new DataViewFilter();
            dataViewFilter.Guid = Guid.NewGuid();
            dataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            var rockContext = new RockContext();

            BindDataTransformations( rockContext );

            var emptyFilter = new DataViewFilter();
            emptyFilter.ExpressionType = FilterExpressionType.Filter;
            emptyFilter.Guid = Guid.NewGuid();
            dataViewFilter.ChildFilters.Add( emptyFilter );

            CreateFilterControl( etpEntityType.SelectedEntityTypeId, dataViewFilter, true, rockContext );
        }

        #endregion
    }
}