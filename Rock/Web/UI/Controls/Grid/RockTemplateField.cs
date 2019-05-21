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
using System.Linq;
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
            get
            {
                object t = ViewState["ExcelExportBehavior"];
                return ( t == null ) ? ExcelExportBehavior.IncludeIfVisible : (ExcelExportBehavior)t;
            }
            set { ViewState["ExcelExportBehavior"] = value; }
        }

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

        /// <summary>
        /// Gets or sets the template field identifier.
        /// </summary>
        /// <value>
        /// The template field identifier.
        /// </value>
        public string ID
        {
            get
            {
                object t = ViewState["ID"];
                return ( t == null ) ? string.Empty : ( string ) t;
            }
            set { ViewState["ID"] = value; }
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="dataControlFieldCell">The data control field cell.</param>
        /// <returns></returns>
        public virtual object GetExportValue( GridViewRow row, DataControlFieldCell dataControlFieldCell )
        {
            var textControls = dataControlFieldCell.ControlsOfTypeRecursive<Control>().OfType<ITextControl>();
            if ( textControls.Any() )
            {
                return textControls.Select( a => a.Text ).Where( t => !string.IsNullOrWhiteSpace( t ) ).ToList().AsDelimited( string.Empty ).ReverseCurrencyFormatting();
            }

            return null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the OnRowSelected event will be fired when a user clicks on this cell ( default is true)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on row selected enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool OnRowSelectedEnabled
        {
            get
            {
                return ViewState["OnRowSelectedEnabled"] as bool? ?? true;
            }

            set
            {
                ViewState["OnRowSelectedEnabled"] = true;
            }
        }
    }
}