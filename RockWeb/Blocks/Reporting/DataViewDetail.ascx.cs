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
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

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

            RockPage.AddCSSLink( this.Page, "~/css/bootstrap-switch.css" );
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
            btnSecurity.EntityType = typeof( Rock.Model.DataView );
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
            CreateFilterControl( DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false );
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
            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

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

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
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

                dataView.EntityTypeId = int.Parse( ddlEntityType.SelectedValue );
                dataView.Name = tbName.Text;
                dataView.Description = tbDescription.Text;
                dataView.CategoryId = ddlCategory.SelectedValueAsInt();
                dataView.DataViewFilter = GetFilterControl();

                // check for duplicates within Category
                if ( service.Queryable()
                    .Where( g => ( g.CategoryId == dataView.CategoryId ) )
                    .Count( a => a.Name.Equals( dataView.Name, StringComparison.OrdinalIgnoreCase ) &&
                        !a.Id.Equals( dataView.Id ) ) > 0 )
                {
                    tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", DataView.FriendlyTypeName ) );
                    return;
                }

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
        /// Handles the Click event of the btnPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPreview_Click( object sender, EventArgs e )
        {
            ShowPreview( int.Parse(ddlEntityType.SelectedValue), GetFilterControl());
        }

        protected void btnPreview2_Click( object sender, EventArgs e )
        {
            int dataViewId = int.Parse( hfDataViewId.Value );

            var service = new DataViewService();
            var dataView = service.Get( dataViewId );

            if ( dataView != null && dataView.EntityTypeId.HasValue)
            {
                ShowPreview( dataView.EntityTypeId.Value, dataView.DataViewFilter );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add( new ListItem( None.Text, None.Id.ToString() ) );

            var service = new CategoryService();
            LoadChildCategories( service, null, Rock.Web.Cache.EntityTypeCache.GetId( "Rock.Model.DataView" ), 0 );

            // Only entity types that are entities, have a friendly name, and actually have existing
            // DataFilter componenets will be displayed in the drop down list
            var entityTypeNames = Rock.DataFilters.DataFilterContainer.GetAvailableFilteredEntityTypeNames();
            var entityTypeService = new EntityTypeService();
            ddlEntityType.DataSource = entityTypeService
                .Queryable()
                .Where( e =>
                    e.IsEntity &&
                    e.FriendlyName != null &&
                    entityTypeNames.Contains( e.Name ) )
                .OrderBy( e => e.FriendlyName )
                .ToList();
            ddlEntityType.DataBind();

        }

        private void LoadChildCategories( CategoryService service, int? parentId, int? entityTypeId, int level )
        {
            foreach ( var category in service.Get( parentId, entityTypeId ) )
            {
                ddlCategory.Items.Add( new ListItem( ( new string( '-', level ) ) + category.Name, category.Id.ToString() ) );
                LoadChildCategories( service, category.Id, entityTypeId, level + 1 );
            }
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

            btnPreview2.Visible = true;

            nbEditModeMessage.Text = string.Empty;
            if ( !dataView.IsAuthorized( "Edit", CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( DataView.FriendlyTypeName );
            }

            if ( dataView.DataViewFilter != null && !dataView.DataViewFilter.IsAuthorized( "View", CurrentPerson ) )
            {
                readOnly = true;
                btnPreview2.Visible = false;
                nbEditModeMessage.Text = "INFO: This Data View contains a filter that you do not have access to view.";
            }

            if ( dataView.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( DataView.FriendlyTypeName );
            }

            btnSecurity.Visible = dataView.IsAuthorized( "Administrate", CurrentPerson );
            btnSecurity.Title = "Secure " + dataView.Name;
            btnSecurity.EntityTypeId = dataView.Id;

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
            LoadDropDowns();

            if ( dataView.DataViewFilter == null || dataView.DataViewFilter.ExpressionType == FilterExpressionType.Filter )
            {
                dataView.DataViewFilter = new DataViewFilter();
                dataView.DataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            }

            tbName.Text = dataView.Name;
            tbDescription.Text = dataView.Description;
            ddlCategory.SetValue( dataView.CategoryId );
            ddlEntityType.SetValue( dataView.EntityTypeId );

            CreateFilterControl( dataView.DataViewFilter, true );
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

            string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
            lblMainDetails.Text = @"
<div>
    <dl>";

            if ( dataView.EntityType != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Applies To", dataView.EntityType.FriendlyName );
            }

            if ( dataView.Category != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Category", dataView.Category.Name );
            }

            lblMainDetails.Text += string.Format( descriptionFormat, "Description", dataView.Description );

            if ( dataView.DataViewFilter != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Filter", dataView.DataViewFilter.ToString() );
            }

            lblMainDetails.Text += @"
    </dl>
</div>";
        }

        /// <summary>
        /// Shows the preview.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="filter">The filter.</param>
        private void ShowPreview( int entityTypeId, DataViewFilter filter )
        {
            var cachedEntityType = EntityTypeCache.Read( entityTypeId );
            if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
            {
                Type entityType = cachedEntityType.GetEntityType();
                if ( entityType != null )
                {
                    BuildGridColumns( entityType );

                    Type[] modelType = { entityType };
                    Type genericServiceType = typeof( Rock.Data.Service<> );
                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );

                    object serviceInstance = Activator.CreateInstance( modelServiceType );

                    if ( serviceInstance != null )
                    {
                        MethodInfo getMethod = serviceInstance.GetType().GetMethod( "GetList", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );

                        if ( getMethod != null )
                        {
                            var paramExpression = serviceInstance.GetType().GetProperty( "ParameterExpression" ).GetValue( serviceInstance ) as ParameterExpression;
                            var whereExpression = filter != null ? filter.GetExpression( paramExpression ) : null;
                            gPreview.DataSource = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression } );
                            gPreview.DataBind();

                            modalPreview.Show();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the grid columns.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        private void BuildGridColumns( Type modelType )
        {
            gPreview.Columns.Clear();

            var previewColumns = new Dictionary<string, BoundField>();
            var allColumns = new Dictionary<string, BoundField>();

            foreach ( var property in modelType.GetProperties() )
            {
                if ( property.GetCustomAttributes( typeof( Rock.Data.PreviewableAttribute ) ).Count() > 0 )
                {
                    previewColumns.Add( property.Name, GetGridField( property ) );
                }
                else if ( previewColumns.Count == 0 && property.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 )
                {
                    allColumns.Add( property.Name, GetGridField( property ) );
                }
            }

            Dictionary<string, BoundField> columns = previewColumns.Count > 0 ? previewColumns : allColumns;

            foreach ( var column in columns )
            {
                var bf = column.Value;
                bf.DataField = column.Key;
                bf.SortExpression = column.Key;
                bf.HeaderText = column.Key.SplitCase();
                gPreview.Columns.Add( bf );
            }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private BoundField GetGridField( PropertyInfo property )
        {
            BoundField bf = new BoundField();

            if ( property.PropertyType == typeof( Boolean ) )
            {
                bf = new BoolField();
            }
            else if ( property.PropertyType == typeof( DateTime ) )
            {
                bf = new DateField();
            }
            else if ( property.PropertyType.IsEnum )
            {
                bf = new EnumField();
            }

            return bf;
        }

        #endregion

        #region Activities and Actions

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

        private void CreateFilterControl( DataViewFilter filter, bool setSelection )
        {
            phFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phFilters, filter, "Rock.Model.Person", setSelection );
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

        #endregion
    }
}