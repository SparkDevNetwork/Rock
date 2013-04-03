//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [Description( "Block to execute a linq command and display the result (if any)." )]
    public partial class LinqGrid : RockBlock
    { 
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            gReport.GridRebind += gReport_GridRebind;
            RunCommand();
        }

        void gReport_GridRebind( object sender, EventArgs e )
        {
            RunCommand();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void RunCommand()
        {
            BuildGridColumns( gReport, typeof( Person ) );

            var service = new PersonService();
            var qry = service.Queryable().Where( p => p.LastName == "Turner" );

            gReport.DataSource = qry;
            gReport.DataBind();
        }

        private void BuildGridColumns( Grid grid, Type modelType )
        {
            grid.Columns.Clear();

            var displayColumns = new Dictionary<string, BoundField>();
            var allColumns = new Dictionary<string, BoundField>();

            foreach ( var property in modelType.GetProperties() )
            {
                if ( property.Name != "Id" )
                {
                    if ( property.GetCustomAttributes( typeof( Rock.Data.PreviewableAttribute ) ).Count() > 0 )
                    {
                        displayColumns.Add( property.Name, GetGridField( property ) );
                    }
                    else if ( displayColumns.Count == 0 && property.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 )
                    {
                        allColumns.Add( property.Name, GetGridField( property ) );
                    }
                }
            }

            // Always add hidden id column
            var idCol = new BoundField();
            idCol.DataField = "Id";
            idCol.Visible = false;
            grid.Columns.Add( idCol );

            Dictionary<string, BoundField> columns = displayColumns.Count > 0 ? displayColumns : allColumns;
            foreach ( var column in columns )
            {
                var bf = column.Value;
                bf.DataField = column.Key;
                bf.SortExpression = column.Key;
                bf.HeaderText = column.Key.SplitCase();
                grid.Columns.Add( bf );
            }
        }

        #endregion

    }
}