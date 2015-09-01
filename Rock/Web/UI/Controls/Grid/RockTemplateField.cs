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

    }

    /// <summary>
    /// A HeaderTemplate for RockTemplateFields that ensures that the HeaderCell uses the TemplateField's HeaderStyle.CssClass
    /// </summary>
    public class RockTemplateFieldHeaderTemplate : ITemplate
    {
        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            var cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                var templateField = cell.ContainingField as RockTemplateField;
                if ( templateField != null )
                {
                    // make sure the header cell gets the HeaderStyle.CssClass
                    cell.AddCssClass( templateField.HeaderStyle.CssClass );
                }
            }
        }
    }
}