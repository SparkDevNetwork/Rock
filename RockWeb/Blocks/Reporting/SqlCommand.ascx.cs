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
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [DisplayName( "SQL Command" )]
    [Category( "Reporting" )]
    [Description( "Block to execute a SQL command and display the result (if any)." )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, order: 1 )]
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

            if ( !Page.IsPostBack )
            {
                tbQuery.Text = @"
SELECT
    TOP 10 *
FROM
    [Person]
";
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            RunCommand();
        }

        /// <summary>
        /// Handles the Click event of the btnExec control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
            pQueryTime.Visible = false;

            string query = tbQuery.Text;
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    if ( tQuery.Checked )
                    {
                        gReport.Visible = true;

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        DataSet dataSet = DbService.GetDataSet( query, CommandType.Text, null, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );
                        sw.Stop();

                        if ( dataSet.Tables.Count > 0 )
                        {
                            var dataTable = dataSet.Tables[0];

                            AddGridColumns( dataTable );

                            gReport.DataSource = GetSortedView( dataTable );
                            gReport.DataBind();
                        }

                        pQueryTime.InnerText = string.Format( "Query completed in {0:N0}ms", sw.ElapsedMilliseconds );
                        pQueryTime.Visible = true;
                    }
                    else
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        int rows = DbService.ExecuteCommand( query, commandTimeout: GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );
                        sw.Stop();

                        if ( rows < 0 )
                        {
                            rows = 0;
                        }

                        nbSuccess.Title = string.Format( "Command completed successfully in {0:N0}ms.", sw.ElapsedMilliseconds );
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

        /// <summary>
        /// Adds the grid columns.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        private void AddGridColumns( DataTable dataTable )
        {
            int rowsToEval = 10;
            if ( dataTable.Rows.Count < 10 )
            {
                rowsToEval = dataTable.Rows.Count;
            }

            gReport.Columns.Clear();
            foreach ( DataColumn dataTableColumn in dataTable.Columns )
            {
                BoundField bf = new BoundField();

                if ( dataTableColumn.DataType == typeof( bool ) )
                {
                    bf = new BoolField();
                }

                if ( dataTableColumn.DataType == typeof( DateTime ) )
                {
                    bf = new DateField();

                    for ( int i = 0; i < rowsToEval; i++ )
                    {
                        object dateObj = dataTable.Rows[i][dataTableColumn];
                        if ( dateObj is DateTime )
                        {
                            DateTime dateTime = ( DateTime ) dateObj;
                            if ( dateTime.TimeOfDay.Seconds != 0 )
                            {
                                bf = new DateTimeField();
                                break;
                            }
                        }
                    }
                }

                bf.DataField = dataTableColumn.ColumnName;
                bf.SortExpression = dataTableColumn.ColumnName;
                bf.HeaderText = dataTableColumn.ColumnName.SplitCase();
                gReport.Columns.Add( bf );
            }
        }

        /// <summary>
        /// Gets the sorted view.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        private System.Data.DataView GetSortedView( DataTable dataTable )
        {
            System.Data.DataView dataView = dataTable.DefaultView;

            SortProperty sortProperty = gReport.SortProperty;
            if ( sortProperty != null )
            {
                dataView.Sort = string.Format( "{0} {1}", sortProperty.Property, sortProperty.DirectionString );
            }

            return dataView;
        }

        #endregion
    }
}