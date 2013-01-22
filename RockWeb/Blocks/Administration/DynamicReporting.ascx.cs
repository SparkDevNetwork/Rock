using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Model;

using Rock;
using Rock.Web.UI;
using Rock.Reporting;
using Rock.Reporting.PersonFilter;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DynamicReporting : RockBlock
    {
        private Report _report;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string reportId = PageParameter( "ReportId" );
            if ( !string.IsNullOrWhiteSpace( reportId ) )
            {
                _report = new ReportService().Get( Int32.Parse( reportId ) );
            }
            else
            {
                _report = new Report();
            }

            gResults.DataKeyNames = new string[] { "id" };
            gResults.Actions.IsAddEnabled = false;
            gResults.Actions.IsExcelExportEnabled = true;
            gResults.GridRebind += gResults_GridRebind;

            if ( !Page.IsPostBack )
            {
                foreach ( var serviceEntry in Rock.Reporting.FilterContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    ListItem li = new ListItem( component.Prompt, component.GetType().FullName );
                    ddlFilters.Items.Add(li);
                }

                ShowReport();
                PopulateGridColumns();
                RunQuery();
            }
        }

        private void ShowReport()
        {
            tbName.Text = _report.Name;
            tbDescription.Text = _report.Description;

            phFilters.Controls.Clear();

            if ( _report.ReportFilter != null )
            {
                var ul = new HtmlGenericControl( "ul" );
                phFilters.Controls.Add( ul );
                CreateFilterControl( ul, _report.ReportFilter );
            }
        }

        private void CreateFilterControl( System.Web.UI.Control parentControl, ReportFilter filter )
        {
            var li = new HtmlGenericControl( "li" );
            parentControl.Controls.Add( li );
            li.InnerText = filter.ToString();

            if ( filter.FilterType == FilterType.AndCollection || filter.FilterType == FilterType.OrCollection )
            {
                var ul = new HtmlGenericControl( "ul" );
                li.Controls.Add( ul );
                foreach ( var childFilter in filter.ReportFilters )
                {
                    CreateFilterControl( ul, childFilter );
                }
            }
        }

        /// <summary>
        /// Populates the grid columns.
        /// </summary>
        private void PopulateGridColumns()
        {
            gResults.Columns.Clear();
            foreach ( var c in GetEntityProperties( typeof( Person ) ) )
            {
                BoundField boundField = new BoundField();
                boundField.DataField = c;
                boundField.HeaderText = c.SplitCase();
                boundField.SortExpression = c;
                gResults.Columns.Add( boundField );
            }
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

        /// <summary>
        /// Runs the query.
        /// </summary>
        private void RunQuery()
        {
            PopulateGridColumns();

            PersonService personService = new PersonService();
            var qry = personService.Queryable();

            ParameterExpression item = Expression.Parameter( typeof( Person ), "p" );
            Expression expr = _report.GetExpression( item );
            if ( expr != null )
            {
                qry = qry.Where( Expression.Lambda<Func<Person, bool>>( expr, item ) );
            }

            var result = qry.ToList();
            gResults.DataSource = result;
            lblGridTitle.Text = string.Format( "Results ({0})", result.Count );
            gResults.DataBind();
        }

        /// <summary>
        /// Gets the entity properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private List<string> GetEntityProperties( Type type )
        {
            var properties = new List<string>();

            foreach ( var property in type.GetProperties() )
            {
                if ( !property.GetGetMethod().IsVirtual || property.Name == "Id" || property.Name == "Guid" || property.Name == "Order" )
                {
                    properties.Add( property.Name );
                }
            }

            return properties;
        }
    }
}