using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for reordering rows in a grid
    /// </summary>
    [ToolboxData( "<{0}:ReorderField runat=server></{0}:ReorderField>" )]
    public class ReorderField : TemplateField
    {
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
                if (grid.AllowSorting || grid.EnableClientSorting)
                    throw new ArgumentException( "Cannot use ReorderField with grid AllowSorting or EnableClientSorting" );

                string script = @"
    var fixHelper = function(e, ui) {
        ui.children().each(function() {
            $(this).width($(this).width());
        });
        return ui;
    };
";
                grid.Page.ClientScript.RegisterStartupScript( grid.Page.GetType(), "grid-sortable-helper-script", script, true );

                script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('#{0} tbody').sortable({{
            helper: fixHelper,
            handle: '.grid-icon-cell.reorder',
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

                grid.Page.ClientScript.RegisterStartupScript( this.GetType(),
                    string.Format( "grid-sort-{0}-script", grid.ClientID ), script, true );

                this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                this.ItemStyle.CssClass = "grid-icon-cell reorder";
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
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                HtmlGenericControl a = new HtmlGenericControl( "a" );
                a.Attributes.Add( "href", "#" );
                a.InnerText = "Reorder";
                cell.Controls.Add( a );
            }
        }
    }
}