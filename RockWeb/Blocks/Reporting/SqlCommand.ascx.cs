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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
    [DisplayName( "Sql Command" )]
    [Category( "Reporting" )]
    [Description( "Block to execute a sql command and display the result (if any)." )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, order: 1 )]
    public partial class SqlCommand : RockBlock
    {
        #region User Preference Keys

        /// <summary>
        /// Keys to use for User Preferences
        /// </summary>
        private static class UserPreferenceKey
        {
            public const string SqlCommandHistoryJSON = "SqlCommandHistoryJSON";
        }

        #endregion


        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            foreach( Grid gReport in rptGrids.ControlsOfTypeRecursive<Grid>())
            {
                gReport.GridRebind += gReport_GridRebind;
            }

            if ( !Page.IsPostBack )
            {
                List<SqlCommandHistoryItem> sqlCommandHistory = GetBlockUserPreference( UserPreferenceKey.SqlCommandHistoryJSON ).FromJsonOrNull<List<SqlCommandHistoryItem>>() ?? new List<SqlCommandHistoryItem>();
                if ( sqlCommandHistory.Any() )
                {
                    var lastSqlCommand = sqlCommandHistory.OrderByDescending( a => a.ExecuteDateTime ).FirstOrDefault();
                    tbQuery.Text = lastSqlCommand.SqlText;
                }
                else
                {
                    tbQuery.Text = @"
SELECT
    TOP 10 *
FROM
    [Person]
";
                }
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
        /// 
        /// </summary>
        public class SqlCommandHistoryItem
        {
            public string SqlText { get; set; }


            public DateTime ExecuteDateTime { get; set; }

            public override string ToString()
            {
                return string.Format( "[{1}] {0}", SqlText.Truncate( 50 ), ExecuteDateTime.ToElapsedString() );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void RunCommand()
        {
            nbSuccess.Visible = false;
            nbError.Visible = false;
            pQueryTime.Visible = false;
            rptGrids.Visible = false;

            string query = tbQuery.Text;
            if ( !string.IsNullOrWhiteSpace( query ) )

            {
                var sqlCommandHistory = GetBlockUserPreference( UserPreferenceKey.SqlCommandHistoryJSON ).FromJsonOrNull<List<SqlCommandHistoryItem>>() ?? new List<SqlCommandHistoryItem>();
                var lastCommand = sqlCommandHistory.OrderByDescending( a => a.ExecuteDateTime ).FirstOrDefault();
                if ( lastCommand != null && lastCommand.SqlText == query )
                {
                    lastCommand.ExecuteDateTime = RockDateTime.Now;
                }
                else
                {
                    sqlCommandHistory.Add( new SqlCommandHistoryItem { SqlText = query, ExecuteDateTime = RockDateTime.Now } );
                }

                if ( sqlCommandHistory.Count > 50 )
                {
                    sqlCommandHistory = sqlCommandHistory.OrderByDescending( a => a.ExecuteDateTime ).Take( 50 ).ToList();
                }

                SetBlockUserPreference( UserPreferenceKey.SqlCommandHistoryJSON, sqlCommandHistory.ToJson() );

                try
                {
                    if ( tQuery.Checked )
                    {
                        rptGrids.Visible = true;

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        DataSet dataSet = DbService.GetDataSet( query, CommandType.Text, null, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180 );

                        sw.Stop();

                        rptGrids.DataSource = dataSet.Tables.OfType<DataTable>().ToList();
                        rptGrids.DataBind();

                        pQueryTime.InnerText = string.Format( "{0} completed in {1:N0}ms", "Query".PluralizeIf(dataSet.Tables.Count != 1), sw.ElapsedMilliseconds );
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
        /// <param name="gReport">The g report.</param>
        private void AddGridColumns( DataTable dataTable, Grid gReport )
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
        /// <param name="gReport">The g report.</param>
        /// <returns></returns>
        private System.Data.DataView GetSortedView( DataTable dataTable, Grid gReport )
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

        /// <summary>
        /// Handles the ItemDataBound event of the rptGrids control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGrids_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var dataTable = e.Item.DataItem as DataTable;
            var lDataTableTitle = e.Item.FindControl( "lDataTableTitle" ) as Literal;
            if ( dataTable.DataSet.Tables.Count > 1 )
            {
                lDataTableTitle.Text = string.Format( "<label>Result {0}</label>", dataTable.DataSet.Tables.IndexOf( dataTable ) + 1 );
            }

            var gReport = e.Item.FindControl( "gReport" ) as Grid;

            AddGridColumns( dataTable, gReport );

            gReport.DataSource = GetSortedView( dataTable, gReport );
            gReport.DataBind();
        }
    }
}