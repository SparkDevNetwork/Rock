using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DynamicReporting : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gResults.DataKeyNames = new string[] { "id" };
            gResults.Actions.IsAddEnabled = false;
            gResults.Actions.IsExcelExportEnabled = true;
            gResults.GridRebind += gResults_GridRebind;

            if ( !Page.IsPostBack )
            {
                lstColumns.Items.Clear();
                foreach ( var c in GetEntityProperties( typeof( Person ) ) )
                {
                    ListItem listItem = new ListItem( c, c );
                    listItem.Selected = true;
                    lstColumns.Items.Add( listItem );
                }

                PopulateGridColumns();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        /// <summary>
        /// Populates the grid columns.
        /// </summary>
        private void PopulateGridColumns()
        {
            gResults.Columns.Clear();
            foreach ( var c in lstColumns.SelectedValues )
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
            string whereClause = tbWhereClause.Text;
            var qry = personService.Queryable();
            if (!string.IsNullOrWhiteSpace(whereClause))
            { 
                qry = qry.Where<Rock.Model.Person>( whereClause, null );
            }
            //var selectList = lstColumns.SelectedValues.ToArray();

            //var qrySelect = qry.Select( selectList, null );

            //var n = new string[] { "a", "b" };

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

       

        /// <summary>
        /// Handles the Click event of the btnGo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnGo_Click( object sender, EventArgs e )
        {
            RunQuery();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the lstColumns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lstColumns_SelectedIndexChanged( object sender, EventArgs e )
        {
            //PopulateGridColumns();
        }
}
}