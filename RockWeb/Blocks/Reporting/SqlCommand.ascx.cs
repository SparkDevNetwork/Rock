// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [DisplayName( "Sql Command" )]
    [Category( "Reporting" )]
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