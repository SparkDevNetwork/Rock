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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column to display a boolean value.
    /// </summary>
    [ToolboxData( "<{0}:RockTemplateField runat=server></{0}:RockTemplateField>" )]
    public class RockTemplateField : TemplateField, IPriorityColumn, IRockGridField
    {
        /// <summary>
        /// Gets or sets the column priority.
        /// </summary>
        /// <value>
        /// The priority of the column.
        /// </value>
        public ColumnPriority ColumnPriority
        {
            get {
                object t = ViewState["ColumnPriority"];
                return (t == null) ? ColumnPriority.AlwaysVisible : (ColumnPriority)t; 
            }
            set { ViewState["ColumnPriority"] = value; }
        }

        /// <summary>
        /// When exporting a grid with an Export source of ColumnOutput, this property controls whether a column is included
        /// in the export or not
        /// </summary>
        public virtual ExcelExportBehavior ExcelExportBehavior
        {
            get { return _excelExportBehavior; }
            set { _excelExportBehavior = value; }
        }
        private ExcelExportBehavior _excelExportBehavior = ExcelExportBehavior.IncludeIfVisible;

        /// <summary>
        /// Adds text or controls to a cell's controls collection.
        /// </summary>
        /// <param name="cell">A <see cref="T:System.Web.UI.WebControls.DataControlFieldCell" /> that contains the text or controls of the <see cref="T:System.Web.UI.WebControls.DataControlField" />.</param>
        /// <param name="cellType">One of the <see cref="T:System.Web.UI.WebControls.DataControlCellType" /> values.</param>
        /// <param name="rowState">One of the <see cref="T:System.Web.UI.WebControls.DataControlRowState" /> values, specifying the state of the row that contains the <see cref="T:System.Web.UI.WebControls.DataControlFieldCell" />.</param>
        /// <param name="rowIndex">The index of the row that the <see cref="T:System.Web.UI.WebControls.DataControlFieldCell" /> is contained in.</param>
        public override void InitializeCell( DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex )
        {
            if ( cellType == DataControlCellType.Header )
            {
                if ( !string.IsNullOrEmpty( this.HeaderStyle.CssClass ) )
                {
                    // make sure the header cell sets the CssClass                
                    cell.AddCssClass( this.HeaderStyle.CssClass );
                }
            }
            
            base.InitializeCell( cell, cellType, rowState, rowIndex );
        }
    }
}