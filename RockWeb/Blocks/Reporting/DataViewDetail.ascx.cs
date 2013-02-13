//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DataViewDetail : RockBlock, IDetailBlock
    {
        int? itemId;
        private DataView _dataView;

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


        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int id;
            if ( int.TryParse( PageParameter( "DataViewId" ), out id ) )
            {
                itemId = id;
            }

            if ( !Page.IsPostBack )
            {
                if (itemId.HasValue)
                {
                    ShowView( itemId.Value );
                }
                else
                {
                    ShowEdit( 0 );
                }
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _dataView = DataView.FromJson( ViewState["DataView"].ToString() );
            CreateFilterControl(false);
        }

        protected override object SaveViewState()
        {
            _dataView.DataViewFilter = GetFilterControl();

            ViewState["DataView"] = _dataView.ToJson();
            return base.SaveViewState();
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

        protected void btnEdit_Click( object sender, EventArgs e )
        {
            if ( itemId.HasValue )
            {
                ShowEdit( itemId.Value );
            }
        }

        protected void btnBack_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            DataView dataView = null;
            int? dataViewFilterId = null;

            DataViewService dataViewService = new DataViewService();

            if (_dataView.Id != 0)
            {
                dataView = dataViewService.Get(_dataView.Id);
                dataViewFilterId = dataView.DataViewFilterId;
            }
            else
            {
                dataView = new DataView();
                dataViewService.Add( dataView, CurrentPersonId );
            }

            dataView.EntityTypeId = int.Parse( ddlEntityType.SelectedValue );
            dataView.Name = tbName.Text;
            dataView.Description = tbDescription.Text;
            dataView.DataViewFilter = GetFilterControl();
            dataViewService.Save( dataView, CurrentPersonId );

            // Delete old report filter
            if ( dataViewFilterId.HasValue )
            {
                DataViewFilterService dataViewFilterService = new DataViewFilterService();
                DataViewFilter dataViewFilter = dataViewFilterService.Get( dataViewFilterId.Value );
                DeleteDataViewFilter( dataViewFilter, dataViewFilterService );
                dataViewFilterService.Save( dataViewFilter, CurrentPersonId );
            }

            ShowView(dataView.Id);
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( itemId.HasValue )
            {
                ShowView( itemId.Value );
            }
            else
            {
                NavigateToParentPage();
            }
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

        public void ShowView( int id )
        {
            _dataView = new DataViewService().Get( id );
            if ( _dataView != null )
            {
                lName.Text = _dataView.Name;
                ltAppliedTo.Text = _dataView.EntityType.FriendlyName ?? _dataView.EntityType.Name;
                ltDescription.Text = _dataView.Description;
                ltFilter.Text = _dataView.DataViewFilter.ToString().TrimStart( '(' ).TrimEnd( ')' );

                pnlEdit.Visible = false;
                pnlView.Visible = true;
            }
            else
            {
                ShowEdit( 0 );
            }
        }

        public void ShowEdit( int id )
        {
            // Only entity types that are entities, have a friendly name, and actually have existing
            // DataFilter componenets will be displayed in the drop down list
            var entityTypeNames = Rock.DataFilters.DataFilterContainer.GetAvailableFilteredEntityTypeNames();
            var entityTypeService = new EntityTypeService();
            ddlEntityType.DataSource = entityTypeService
                .Queryable()
                .Where( e => 
                    e.IsEntity && 
                    e.FriendlyName != null &&
                    entityTypeNames.Contains(e.Name))
                .OrderBy( e => e.FriendlyName )
                .ToList();
            ddlEntityType.DataBind();

            _dataView = new DataViewService().Get( id );
            if ( _dataView == null )
            {
                _dataView = new DataView();
            }

            if ( _dataView.DataViewFilter == null || _dataView.DataViewFilter.ExpressionType == FilterExpressionType.Filter )
            {
                _dataView.DataViewFilter = new DataViewFilter();
                _dataView.DataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            }

            if ( _dataView.EntityTypeId.HasValue )
            {
                ddlEntityType.SelectedValue = _dataView.EntityTypeId.Value.ToString();
            }

            tbName.Text = _dataView.Name;
            tbDescription.Text = _dataView.Description;

            CreateFilterControl( true );

            pnlView.Visible = false;
            pnlEdit.Visible = true;
        }

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "dataViewId" ) )
            {
                return;
            }

            itemId = itemKeyValue;

            if ( itemId == 0 )
            {
                ShowEdit( itemId.Value );
            }
            else
            {
                ShowView( itemId.Value );
            }
        }

        private void CreateFilterControl(bool setSelection)
        {
            phFilters.Controls.Clear();
            if ( _dataView.DataViewFilter != null )
            {
                CreateFilterControl( phFilters, _dataView.DataViewFilter, "Rock.Model.Person", setSelection );
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
                    if (entityTypeCache != null)
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
                foreach ( var childFilter in filter.ChildFilters)
                {
                    CreateFilterControl( groupControl, childFilter, filteredEntityTypeName, setSelection );
                }
            }
        }

        private DataViewFilter GetFilterControl()
        {
            if ( phFilters.Controls.Count > 0 )
            {
                return GetFilterControl(phFilters.Controls[0]);
            }

            return null;
        }

        private DataViewFilter GetFilterControl(Control control)
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

        private DataViewFilter GetFilterGroupControl(FilterGroup filterGroup)
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
    }
}