//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls.Report;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ReportDetail : RockBlock, IDetailBlock
    {
        private Report _report;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            gResults.DataKeyNames = new string[] { "id" };
            gResults.Actions.IsAddEnabled = false;
            gResults.Actions.IsExcelExportEnabled = true;
            gResults.GridRebind += gResults_GridRebind;

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "ReportId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "reportId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _report = Report.FromJson( ViewState["Report"].ToString() );
            CreateFilterControl();
        }

        protected override object SaveViewState()
        {
            ViewState["Report"] = _report.ToJson();
            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the GridRebind event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gResults_GridRebind( object sender, EventArgs e )
        {
            RunQuery();
        }

        void groupControl_AddFilterClick( object sender, AddFilterArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();
            filterField.EntityTypeName = e.EntityTypeName;
            groupControl.Controls.Add( filterField );

            _report.ReportFilter = GetFilterControl();

            RunQuery();
        }

        void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup();
            childGroupControl.FilterType = FilterType.And;
            groupControl.Controls.Add( childGroupControl );

            _report.ReportFilter = GetFilterControl();

            RunQuery();
        }

        void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );

            _report.ReportFilter = GetFilterControl();

            RunQuery();
        }

        void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );

            _report.ReportFilter = GetFilterControl();

            RunQuery();
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Report report = null;
            int? reportFilterId = null;

            ReportService reportService = new ReportService();

            if (_report.Id != 0)
            {
                report = reportService.Get(_report.Id);
                reportFilterId = report.ReportFilterId;
            }
            else
            {
                report = new Report();
                reportService.Add( report, CurrentPersonId );
            }

            report.Name = tbName.Text;
            report.Description = tbDescription.Text;
            report.ReportFilter = GetFilterControl();
            reportService.Save( report, CurrentPersonId );

            // Delete old report filter
            if ( reportFilterId.HasValue )
            {
                ReportFilterService reportFilterService = new ReportFilterService();
                ReportFilter filter = reportFilterService.Get( reportFilterId.Value );
                DeleteReportFilter( filter, reportFilterService );
                reportFilterService.Save( filter, CurrentPersonId );
            }

            _report = report;
            RunQuery();
        }

        private void DeleteReportFilter( ReportFilter filter, ReportFilterService service )
        {
            if ( filter != null )
            {
                foreach ( var childFilter in filter.ReportFilters.ToList() )
                {
                    DeleteReportFilter( childFilter, service );
                }
                service.Delete( filter, CurrentPersonId );
            }
        }

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "reportId" ) )
            {
                return;
            }

            _report = new ReportService().Get( itemKeyValue );
            if ( _report == null )
            {
                _report = new Report();
            }

            if ( _report.ReportFilter == null || _report.ReportFilter.FilterType == FilterType.Expression )
            {
                _report.ReportFilter = new ReportFilter();
                _report.ReportFilter.FilterType = FilterType.And;
            }

            tbName.Text = _report.Name;
            tbDescription.Text = _report.Description;
            CreateFilterControl();

            RunQuery();
        }

        private void CreateFilterControl()
        {
            phFilters.Controls.Clear();
            if ( _report.ReportFilter != null )
            {
                CreateFilterControl( phFilters, _report.ReportFilter );
            }
        }

        private void CreateFilterControl( Control parentControl, ReportFilter filter )
        {
            if ( filter.FilterType == FilterType.And || filter.FilterType == FilterType.Or )
            {
                var groupControl = new FilterGroup();
                parentControl.Controls.Add( groupControl );
                groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                groupControl.FilterType = filter.FilterType;
                groupControl.AddFilterClick += groupControl_AddFilterClick;
                groupControl.AddGroupClick += groupControl_AddGroupClick;
                groupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
                foreach ( var childFilter in filter.ReportFilters )
                {
                    CreateFilterControl( groupControl, childFilter );
                }
            }
            else
            {
                var filterControl = new FilterField();
                parentControl.Controls.Add( filterControl );
                filterControl.EntityTypeName = Rock.Web.Cache.EntityTypeCache.Read( filter.EntityTypeId.Value ).Name;
                filterControl.Selection = filter.Selection;
                filterControl.DeleteClick += filterControl_DeleteClick;
            }
        }

        private ReportFilter GetFilterControl()
        {
            if ( phFilters.Controls.Count > 0 )
            {
                return GetFilterControl(phFilters.Controls[0]);
            }

            return null;
        }

        private ReportFilter GetFilterControl(Control control)
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

        private ReportFilter GetFilterGroupControl(FilterGroup filterGroup)
        {
            ReportFilter filter = new ReportFilter();
            filter.FilterType = filterGroup.FilterType;
            foreach ( Control control in filterGroup.Controls )
            {
                ReportFilter childFilter = GetFilterControl( control );
                if ( childFilter != null )
                {
                    filter.ReportFilters.Add( childFilter );
                }
            }
            return filter;
        }

        private ReportFilter GetFilterFieldControl( FilterField filterField )
        {
            ReportFilter filter = new ReportFilter();
            filter.FilterType = FilterType.Expression;
            filter.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read(filterField.EntityTypeName).Id;
            filter.Selection = filterField.Selection;

            return filter;
        }

        /// <summary>
        /// Runs the query.
        /// </summary>
        private void RunQuery()
        {
            PersonService personService = new PersonService();
            var qry = personService.Queryable();

            ParameterExpression item = Expression.Parameter( typeof( Person ), "p" );
            Expression expr = _report.GetExpression( item );
            if ( expr != null )
            {
                var lambda = Expression.Lambda<Func<Person, bool>>( expr, item );
                qry = qry.Where( lambda );
            }

            var result = qry.ToList();
            gResults.DataSource = result;
            lblGridTitle.Text = string.Format( "Results ({0})", result.Count );
            gResults.DataBind();
        }
    }
}