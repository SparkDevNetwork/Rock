//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    public class ReorderField : TemplateField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReorderField" /> class.
        /// </summary>
        public ReorderField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-columncommand";
            this.ItemStyle.CssClass = "grid-columncommand";
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
            handle: '.icon-reorder',
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
                buttonIcon.Attributes.Add( "class", "icon-reorder" );
                a.Controls.Add( buttonIcon );

                cell.Controls.Add( a );
            }
        }
    }
}