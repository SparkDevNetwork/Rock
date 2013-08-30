//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Reporting;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
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

            RockPage.AddScriptLink( this.Page, "~/scripts/jquery.switch.js" );

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

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return confirmDelete(event, '{0}');", DataView.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.DataView ) ).Id;

            gReport.GridRebind += gReport_GridRebind;
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
                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( parentCategoryId ) )
                    {
                        ShowDetail( "DataViewId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "DataViewId", int.Parse( itemId ), int.Parse( parentCategoryId ) );
                    }
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
            CreateFilterControl( ViewState["EntityTypeId"] as int?, DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DataViewFilter"] = GetFilterControl().ToJson();
            ViewState["EntityTypeId"] = ddlEntityType.SelectedValueAsInt();
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
            var service = new DataViewService();
            var item = service.Get( int.Parse( hfDataViewId.Value ) );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            DataView dataView = null;

            using ( new UnitOfWorkScope() )
            {
                DataViewService service = new DataViewService();

                int dataViewId = int.Parse( hfDataViewId.Value );
                int? dataViewFilterId = null;

                if ( dataViewId == 0 )
                {
                    dataView = new DataView();
                    dataView.IsSystem = false;
                }
                else
                {
                    dataView = service.Get( dataViewId );
                    dataViewFilterId = dataView.DataViewFilterId;
                }

                dataView.Name = tbName.Text;
                dataView.Description = tbDescription.Text;
                dataView.TransformEntityTypeId = ddlTransform.SelectedValueAsInt();
                dataView.EntityTypeId = ddlEntityType.SelectedValueAsInt();
                dataView.CategoryId = cpCategory.SelectedValueAsInt();

                dataView.DataViewFilter = GetFilterControl();

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !dataView.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                RockTransactionScope.WrapTransaction( () =>
                {
                    if ( dataView.Id.Equals( 0 ) )
                    {
                        service.Add( dataView, CurrentPersonId );
                    }

                    service.Save( dataView, CurrentPersonId );

                    // Delete old report filter
                    if ( dataViewFilterId.HasValue )
                    {
                        DataViewFilterService dataViewFilterService = new DataViewFilterService();
                        DataViewFilter dataViewFilter = dataViewFilterService.Get( dataViewFilterId.Value );
                        DeleteDataViewFilter( dataViewFilter, dataViewFilterService );
                        dataViewFilterService.Save( dataViewFilter, CurrentPersonId );
                    }

                } );
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["DataViewId"] = dataView.Id.ToString();
            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfDataViewId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to tree view with parent category selected
                var qryParams = new Dictionary<string, string>();

                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( parentCategoryId ) )
                {
                    qryParams["CategoryId"] = parentCategoryId;
                }
                NavigateToPage( this.CurrentPage.Guid, qryParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                DataViewService service = new DataViewService();
                DataView item = service.Get( int.Parse( hfDataViewId.Value ) );
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

            var dataViewService = new DataViewService();
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

                    dataViewService.Delete( dataView, CurrentPersonId );
                    dataViewService.Save( dataView, CurrentPersonId );

                    // reload page, selecting the deleted data view's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != null )
                    {
                        qryParams["CategoryId"] = categoryId.ToString();
                    }

                    NavigateToPage( this.CurrentPage.Guid, qryParams );
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
            var entityTypeService = new EntityTypeService();

            ddlEntityType.Items.Clear();
            ddlEntityType.Items.Add( new ListItem( string.Empty, string.Empty ) );
            new EntityTypeService().GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );
        }

        public void BindDataTransformations()
        {
            ddlTransform.Items.Clear();
            int? entityTypeId = ddlEntityType.SelectedValueAsInt();
            if ( entityTypeId.HasValue )
            {
                var filteredEntityType = EntityTypeCache.Read( entityTypeId.Value );
                foreach ( var component in DataTransformContainer.GetComponentsByTransformedEntityName( filteredEntityType.Name ).OrderBy( c => c.Title ) )
                {
                    var transformEntityType = EntityTypeCache.Read( component.TypeName );
                    ListItem li = new ListItem( component.Title, transformEntityType.Id.ToString() );
                    ddlTransform.Items.Add( li );
                }
            }
            ddlTransform.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? parentCategoryId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "DataViewId" ) )
            {
                return;
            }

            var dataViewService = new DataViewService();
            DataView dataView = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                dataView = dataViewService.Get( itemKeyValue );
            }
            else
            {
                dataView = new DataView { Id = 0, IsSystem = false, CategoryId = parentCategoryId };
            }

            if ( dataView == null || !dataView.IsAuthorized( "View", CurrentPerson ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfDataViewId.Value = dataView.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !dataView.IsAuthorized( "Edit", CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( DataView.FriendlyTypeName );
            }

            if ( dataView.DataViewFilter != null && !dataView.DataViewFilter.IsAuthorized( "View", CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = "INFO: This Data View contains a filter that you do not have access to view.";
            }

            if ( dataView.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( DataView.FriendlyTypeName );
            }

            btnSecurity.Visible = dataView.IsAuthorized( "Administrate", CurrentPerson );
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
                btnDelete.Visible = dataViewService.CanDelete( dataView, out errorMessage );
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
                lActionTitle.Text = ActionTitle.Edit( DataView.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( DataView.FriendlyTypeName );
            }

            SetEditMode( true );
            LoadDropDowns( dataView );

            if ( dataView.DataViewFilter == null || dataView.DataViewFilter.ExpressionType == FilterExpressionType.Filter )
            {
                dataView.DataViewFilter = new DataViewFilter();
                dataView.DataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            }

            tbName.Text = dataView.Name;
            tbDescription.Text = dataView.Description;
            ddlEntityType.SetValue( dataView.EntityTypeId );
            cpCategory.SetValue( dataView.CategoryId );

            BindDataTransformations();
            ddlTransform.SetValue( dataView.TransformEntityTypeId ?? 0 );

            CreateFilterControl( dataView.EntityTypeId, dataView.DataViewFilter, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void ShowReadonlyDetails( DataView dataView )
        {
            SetEditMode( false );
            hfDataViewId.SetValue( dataView.Id );
            lReadOnlyTitle.Text = dataView.Name;

            DescriptionList descriptionList = new DescriptionList();

            if ( dataView.EntityType != null )
            {
                descriptionList.Add( "Applies To", dataView.EntityType.FriendlyName );
            }

            if ( dataView.Category != null )
            {
                descriptionList.Add( "Category", dataView.Category.Name );
            }

            descriptionList.Add( "Description", dataView.Description );

            if ( dataView.DataViewFilter != null && dataView.EntityTypeId.HasValue )
            {
                descriptionList.Add( "Filter", dataView.DataViewFilter.ToString( EntityTypeCache.Read( dataView.EntityTypeId.Value ).GetEntityType() ) );
            }

            if ( dataView.TransformEntityType != null )
            {
                descriptionList.Add( "Post-filter Transformation", dataView.TransformEntityType.FriendlyName );
            }

            lblMainDetails.Text = descriptionList.Html;

            ShowReport( dataView );
        }

        private void ShowReport( DataView dataView )
        {
            if ( dataView.EntityTypeId.HasValue && dataView.DataViewFilter != null && dataView.DataViewFilter.IsAuthorized( "View", CurrentPerson ) )
            {

                bool isPersonDataSet = dataView.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                if ( isPersonDataSet )
                {
                    gReport.PersonIdField = "Id";
                }
                else
                {
                    gReport.PersonIdField = null;
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
            if ( BindGrid( gPreview, dataView ) )
            {
                modalPreview.Show();
            }
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

        private bool BindGrid( Grid grid, DataView dataView )
        {
            var errors = new List<string>();
            grid.DataSource = dataView.BindGrid( grid, out errors, true );
            if ( grid.DataSource != null )
            {
                if ( errors.Any() )
                {
                    nbEditModeMessage.Text = "INFO: There was a problem with one or more of the filters for this data view...<br/><br/> " + errors.AsDelimited( "<br/>" );
                }

                grid.DataBind();
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
            var service = new DataViewService();
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
            dv.EntityTypeId = ddlEntityType.SelectedValueAsInt();
            dv.DataViewFilter = GetFilterControl();
            ShowPreview( dv );
        }

        void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "{0}_ff_{1}", groupControl.ID, groupControl.Controls.Count );
            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;
        }

        void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup();
            groupControl.Controls.Add( childGroupControl );
            childGroupControl.ID = string.Format( "{0}_fg_{1}", groupControl.ID, groupControl.Controls.Count );
            childGroupControl.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            childGroupControl.FilterType = FilterExpressionType.GroupAll;
        }

        void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );
        }

        void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );
        }

        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service )
        {
            if ( dataViewFilter != null )
            {
                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service );
                }
                service.Delete( dataViewFilter, CurrentPersonId );
            }
        }

        private void CreateFilterControl( int? filteredEntityTypeId, DataViewFilter filter, bool setSelection )
        {
            phFilters.Controls.Clear();
            if ( filter != null && filteredEntityTypeId.HasValue )
            {
                var filteredEntityType = EntityTypeCache.Read( filteredEntityTypeId.Value );
                CreateFilterControl( phFilters, filter, filteredEntityType.Name, setSelection );
            }
        }

        private void CreateFilterControl( Control parentControl, DataViewFilter filter, string filteredEntityTypeName, bool setSelection )
        {
            if ( filter.ExpressionType == FilterExpressionType.Filter )
            {
                var filterControl = new FilterField();
                parentControl.Controls.Add( filterControl );
                filterControl.ID = string.Format( "{0}_ff_{1}", parentControl.ID, parentControl.Controls.Count );
                filterControl.FilteredEntityTypeName = filteredEntityTypeName;
                if ( filter.EntityTypeId.HasValue )
                {
                    var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( filter.EntityTypeId.Value );
                    if ( entityTypeCache != null )
                    {
                        filterControl.FilterEntityTypeName = entityTypeCache.Name;
                    }
                }
                filterControl.Expanded = filter.Expanded;
                if ( setSelection )
                {
                    filterControl.Selection = filter.Selection;
                }
                filterControl.DeleteClick += filterControl_DeleteClick;
            }
            else
            {
                var groupControl = new FilterGroup();
                parentControl.Controls.Add( groupControl );
                groupControl.ID = string.Format( "{0}_fg_{1}", parentControl.ID, parentControl.Controls.Count );
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
                    CreateFilterControl( groupControl, childFilter, filteredEntityTypeName, setSelection );
                }
            }
        }

        private DataViewFilter GetFilterControl()
        {
            if ( phFilters.Controls.Count > 0 )
            {
                return GetFilterControl( phFilters.Controls[0] );
            }

            return null;
        }

        private DataViewFilter GetFilterControl( Control control )
        {
            FilterGroup groupControl = control as FilterGroup;
            if ( groupControl != null )
            {
                return GetFilterGroupControl( groupControl );
            }

            FilterField filterControl = control as FilterField;
            if ( filterControl != null )
            {
                return GetFilterFieldControl( filterControl );
            }

            return null;
        }

        private DataViewFilter GetFilterGroupControl( FilterGroup filterGroup )
        {
            DataViewFilter filter = new DataViewFilter();
            filter.ExpressionType = filterGroup.FilterType;
            foreach ( Control control in filterGroup.Controls )
            {
                DataViewFilter childFilter = GetFilterControl( control );
                if ( childFilter != null )
                {
                    filter.ChildFilters.Add( childFilter );
                }
            }
            return filter;
        }

        private DataViewFilter GetFilterFieldControl( FilterField filterField )
        {
            DataViewFilter filter = new DataViewFilter();
            filter.ExpressionType = FilterExpressionType.Filter;
            filter.Expanded = filterField.Expanded;
            if ( filterField.FilterEntityTypeName != null )
            {
                filter.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( filterField.FilterEntityTypeName ).Id;
                filter.Selection = filterField.Selection;
            }

            return filter;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var dataViewFilter = new DataViewFilter();
            dataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            BindDataTransformations();
            CreateFilterControl( ddlEntityType.SelectedValueAsInt(), dataViewFilter, false );
        }

        #endregion

    }
}