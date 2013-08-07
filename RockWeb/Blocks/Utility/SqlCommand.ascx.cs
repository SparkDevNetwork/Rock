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
    [Description( "Block to execute a sql command and display the result (if any)." )]
    public partial class SqlCommand : RockBlock
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
        }

        void gReport_GridRebind( object sender, EventArgs e )
        {
            RunCommand();
        }

        protected void btnExec_Click( object sender, EventArgs e )
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
            nbSuccess.Visible = false;
            nbError.Visible = false;
            gReport.Visible = false;

            string query = tbQuery.Text;
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    if ( tQuery.Checked )
                    {

                        gReport.Visible = true;

                        DataTable dataTable = new Service().GetDataTable( query, CommandType.Text, null );

                        AddGridColumns( dataTable );

                        gReport.DataSource = GetSortedView( dataTable ); ;
                        gReport.DataBind();
                    }
                    else
                    {
                        int rows = new Service().ExecuteCommand( query, new object[] { } );
                        if ( rows < 0 )
                        {
                            rows = 0;
                        }
                        nbSuccess.Text = string.Format( "Row(s) affected: {0}", rows );
                        nbSuccess.Visible = true;
                    }

                }
                catch ( System.Exception ex )
                {
                    nbError.Text = ex.Message;
                    nbError.Visible = true;
                }
            }
        }

        private void AddGridColumns(DataTable dataTable)
        {
            int rowsToEval = 10;
            if ( dataTable.Rows.Count < 10 )
            {
                rowsToEval = dataTable.Rows.Count;
            }

            gReport.Columns.Clear();
            foreach(DataColumn dtColumn in dataTable.Columns)
            {
                BoundField bf = new BoundField();

                if ( dtColumn.DataType == typeof( Boolean ) )
                {
                    bf = new BoolField();
                }

                if ( dtColumn.DataType == typeof( DateTime ) )
                {
                    bf = new DateField();

                    for ( int i = 0; i < rowsToEval; i++ )
                    {
                        object dateObj = dataTable.Rows[i][dtColumn];
                        if ( dateObj is DateTime )
                        {
                            DateTime dateTime = (DateTime)dateObj;
                            if ( dateTime.TimeOfDay.Seconds != 0 )
                            {
                                bf = new DateTimeField();
                                break;
                            }
                        }
                    }
                }

                bf.DataField = dtColumn.ColumnName;
                bf.SortExpression = dtColumn.ColumnName;
                bf.HeaderText = dtColumn.ColumnName.SplitCase();
                gReport.Columns.Add( bf );
            }
        }

        private System.Data.DataView GetSortedView( DataTable dataTable )
        {
            System.Data.DataView dataView = dataTable.DefaultView;

            SortProperty sortProperty = gReport.SortProperty;
            if ( sortProperty != null )
            {
                dataView.Sort = string.Format( "{0} {1}",
                    sortProperty.Property,
                    sortProperty.DirectionString );
            }

            return dataView;
        }

        #endregion

    }
}