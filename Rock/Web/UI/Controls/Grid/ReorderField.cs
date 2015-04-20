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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for reordering rows in a grid
    /// </summary>
    [ToolboxData( "<{0}:ReorderField runat=server></{0}:ReorderField>" )]
    public class ReorderField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReorderField" /> class.
        /// </summary>
        public ReorderField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-columnreorder";
            this.ItemStyle.CssClass = "grid-columnreorder";
        }

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            Grid grid = control as Grid;
            if ( grid != null )
            {
                if (grid.AllowSorting)
                    throw new ArgumentException( "Cannot use ReorderField with grid AllowSorting" );

                string script = @"
    var fixHelper = function(e, ui) {
        ui.children().each(function() {
            $(this).width($(this).width());
        });
        return ui;
    };
";
                ScriptManager.RegisterStartupScript( grid, grid.GetType(), "grid-sortable-helper-script", script, true );

                script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('#{0} tbody').sortable({{
            helper: fixHelper,
            handle: '.fa-bars',
            start: function(event, ui) {{
                var start_pos = ui.item.index();
                ui.item.data('start_pos', start_pos);
            }},
            update: function(event, ui) {{
                __doPostBack('{1}', 're-order:' + ui.item.attr('datakey') + ';' + ui.item.data('start_pos') + ';' + ui.item.index());
            }}
        }}).disableSelection();
    }});
", grid.ClientID, grid.UniqueID );

                ScriptManager.RegisterStartupScript( grid, grid.GetType(), string.Format( "grid-sort-{0}-script", grid.ClientID ), script, true );

                this.ItemTemplate = new ReorderFieldTemplate();
            }

            return base.Initialize( sortingEnabled, control );
        }
    }

    /// <summary>
    /// Template used by the <see cref="ReorderField"/> control
    /// </summary>
    public class ReorderFieldTemplate : ITemplate
    {
        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                HtmlGenericControl a = new HtmlGenericControl( "a" );
                a.Attributes.Add( "href", "#" );
                a.AddCssClass( "minimal" );
                
                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add( "class", "fa fa-bars" );
                a.Controls.Add( buttonIcon );

                cell.Controls.Add( a );
            }
        }
    }
}