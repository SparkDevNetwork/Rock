using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Controls
{
    [
    AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal),
    DefaultProperty("GridColumns"),
    ParseChildren(true, "GridColumns"),
    ToolboxData("<{0}:Grid runat=\"server\"> </{0}:Grid>")
    ]
    public class Grid : DataBoundControl
    {
        private List<GridColumn> columns;

        [
        Category( "Behavior" ),
        DefaultValue( false ),
        Description( "Can grid be edited" )
        ]
        public virtual bool EnableEdit
        {
            get
            {
                bool? b = ViewState["EnableEdit"] as bool?;
                return ( b == null ) ? false : b.Value;
            }
            set
            {
                ViewState["EnableEdit"] = value;
            }
        }

        [
        Category( "Behavior" ),
        DefaultValue( false ),
        Description( "" )
        ]
        public virtual bool AsyncEditorLoading
        {
            get
            {
                bool? b = ViewState["AsyncEditorLoading"] as bool?;
                return ( b == null ) ? false : b.Value;
            }
            set
            {
                ViewState["AsyncEditorLoading"] = value;
            }
        }

        [
        Category("Behavior"),
        Description("The column collection"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        //Editor(typeof(GridColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public List<GridColumn> GridColumns
        {
            get
            {
                if ( columns == null )
                    columns = new List<GridColumn>();
                return columns;
            }
        }

        protected override void PerformSelect()
        {
            if ( !IsBoundUsingDataSourceID )
                OnDataBinding( EventArgs.Empty );

            GetData().Select( CreateDataSourceSelectArguments(), this.OnDataSourceViewSelectCallback );

            RequiresDataBinding = false;
            MarkAsDataBound();

            OnDataBound( EventArgs.Empty );
        }

        private void OnDataSourceViewSelectCallback( IEnumerable retrieveData )
        {
            if ( IsBoundUsingDataSourceID )
                OnDataBinding( EventArgs.Empty );

            PerformDataBinding( retrieveData );
        }

        protected override void PerformDataBinding( IEnumerable retrievedData )
        {
            base.PerformDataBinding( retrievedData );

            Rock.Cms.CmsPage.AddCSSLink( Page, "~/Scripts/slickgrid/slick.grid.css" );

            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.event.drag-2.0.min.js" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/slick.core.js" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/slick.grid.js" );
            
            if ( retrievedData != null )
            {
                // Add the Grid HTML container
                HtmlGenericControl grid = new HtmlGenericControl( "div" );
                grid.Attributes.Add( "class", "data-grid" );
                grid.Attributes.Add( "style", "display:none;" );
                grid.ID = this.ID;
                this.Controls.Add( grid );

                // Create column Definitions
                List<string> columnDefs = new List<string>();
                foreach ( GridColumn gridColumn in GridColumns )
                    columnDefs.Add( "{" + string.Join( ", ", gridColumn.ColumnParameters.ToArray() ) + "}" );

                // Create formatter and/or editor script functions
                Dictionary<string, string> functions = new Dictionary<string, string>();
                foreach ( GridColumn gridColumn in GridColumns )
                   gridColumn.AddScriptFunctions( functions, Page );

                // Create row Definitions
                List<string> rowDefs = new List<string>();
                foreach ( object dataItem in retrievedData )
                {
                    List<string> rowColValues = new List<string>();
                    foreach ( GridColumn gridColumn in GridColumns )
                        rowColValues.Add( gridColumn.RowParameter( dataItem ) );
                    rowDefs.Add( string.Format( "data[{0}] = {{{1}}};", rowDefs.Count, String.Join( ",", rowColValues.ToArray() ) ) );
                }

                // Create and add SlickGrid script
                string script = string.Format( @"
<script>
    var {1}_grid;

    var {1}_options = {{
        {2}
    }};

    {3}

    var {1}_columns = [
        {4}
    ];

    $(function () {{
        var data = [];
        {5}
        {1}_grid = new Slick.Grid(""#{0}"", data, {1}_columns, {1}_options);
        $(""#{0}"").show;

        {1}_grid.onCellChange.subscribe(function(e,args) {{
              alert(""Cell Changed"");
        }});
    }})
</script>",
                    grid.ClientID,
                    grid.ClientID.Replace( "-", "_" ),
                    String.Join( ",\n\t", OptionValues().ToArray() ),
                    String.Join( "\n\n\t", functions.Values.ToArray() ),
                    String.Join( ",\n\t", columnDefs.ToArray() ),
                    String.Join( "\n\t", rowDefs.ToArray() ) );

                this.Controls.Add( new LiteralControl( script ) );
            }
        }

        private List<string> OptionValues()
        {
            List<string> options = new List<string>();

            if ( EnableEdit )
                options.Add( "editable: true" );
            
            //temporary hard-coded options
            options.Add( "enableCellNavigation: true" );
            options.Add( "enableColumnReorder: false" );
            options.Add( "forceFitColumns: true" );

            return options;
        }

    }
}
