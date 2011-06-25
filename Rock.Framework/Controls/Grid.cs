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
    AspNetHostingPermission( SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal ),
    AspNetHostingPermission( SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal ),
    DefaultProperty( "GridColumns" ),
    ParseChildren( true, "GridColumns" ),
    ToolboxData( "<{0}:Grid runat=\"server\"> </{0}:Grid>" )
    ]
    public class Grid : DataBoundControl, ICallbackEventHandler
    {
        private string returnValue;

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

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
        Description( "Should Paging options be displayed" )
        ]
        public virtual bool EnablePaging
        {
            get
            {
                bool? b = ViewState["EnablePaging"] as bool?;
                return ( b == null ) ? false : b.Value;
            }
            set
            {
                ViewState["EnablePaging"] = value;
            }
        }

        [
        Category( "Behavior" ),
        DefaultValue( false ),
        Description( "Enable row reordering" )
        ]
        public virtual bool EnableOrdering
        {
            get
            {
                bool? b = ViewState["EnableOrdering"] as bool?;
                return ( b == null ) ? false : b.Value;
            }
            set
            {
                ViewState["EnableOrdering"] = value;
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
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Unique identifier column name" )
        ]
        public virtual string IdColumnName
        {
            get
            {
                string s = ViewState["IdColumnName"] as string;
                return ( s == null ) ? "" : s;
            }
            set
            {
                ViewState["IdColumnName"] = value;
            }
        }

        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Title to display above grid" )
        ]
        public virtual string Title
        {
            get
            {
                string s = ViewState["Title"] as string;
                return ( s == null ) ? "" : s;
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        [
        Category( "Behavior" ),
        Description( "The column collection" ),
        DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
            //Editor(typeof(GridColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode( PersistenceMode.InnerDefaultProperty )
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

            string jsFriendlyClientId = this.ClientID.Replace( "-", "_" );

            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.event.drag-2.0.min.js" );
            Rock.Cms.CmsPage.AddCSSLink( Page, "~/Scripts/slickgrid/slick.grid.css" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/plugins/slick.rowselectionmodel.js" );

            if ( EnableOrdering )
            {
                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.event.drop-2.0.min.js" );
                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/plugins/slick.cellrangeselector.js" );
                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/plugins/slick.cellselectionmodel.js" );
                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/plugins/slick.rowmovemanager.js" );
            }

            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/slick.core.js" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/slick.grid.js" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/controls/slick.pager.js" );

            if ( EnablePaging )
            {
                Rock.Cms.CmsPage.AddCSSLink( Page, "~/Scripts/slickgrid/controls/slick.pager.css" );
                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/slick.dataview.js" );
            }

            ClientScriptManager cs = Page.ClientScript;
            Type type = this.GetType();

            string sendDataScript = string.Format( "{0}_SendServerData", jsFriendlyClientId );
            if ( !cs.IsClientScriptBlockRegistered( type, sendDataScript ) )
                cs.RegisterClientScriptBlock( type, sendDataScript, string.Format( @"
    function {0}_SendServerData(arg, context)
    {{
        {1}
    }}
",
                    jsFriendlyClientId,
                    cs.GetCallbackEventReference(
                        this, "arg", string.Format( "{0}_ReceiveServerData", jsFriendlyClientId ), "context" ) ), true );

            string receiveDataScript = string.Format( "{0}_ReceiveServerData", jsFriendlyClientId );
            if ( !cs.IsClientScriptBlockRegistered( type, receiveDataScript ) )
                cs.RegisterClientScriptBlock( type, receiveDataScript, string.Format( @"
    function {0}_ReceiveServerData(rValue)
    {{
    }}
",
                    jsFriendlyClientId ), true );

            string toggleControlRow = string.Format( "{0}_ToggleControlRow", jsFriendlyClientId );
            if ( !cs.IsClientScriptBlockRegistered( type, toggleControlRow ) )
                cs.RegisterClientScriptBlock( type, toggleControlRow, string.Format( @"
    function {0}_ToggleControlRow()
    {{
        if ($({0}_grid.getTopPanel()).is("":visible""))
            {0}_grid.hideTopPanel();
        else
            {0}_grid.showTopPanel();
        return false;
    }}
",
                    jsFriendlyClientId ), true );

            if ( retrievedData != null )
            {
                if ( Title != string.Empty )
                {
                    HtmlGenericControl title = new HtmlGenericControl( "div" );
                    title.Attributes.Add( "class", "data-grid-header" );
                    title.Attributes.Add( "style", "width:100%" );
                    this.Controls.Add( title );

                    HtmlGenericControl titleLabel = new HtmlGenericControl( "label" );
                    title.Controls.Add( titleLabel );
                    titleLabel.InnerText = Title;

                    HtmlGenericControl controlsIcon = new HtmlGenericControl( "a" );
                    controlsIcon.Attributes.Add( "style", "float:right" );
                    controlsIcon.Attributes.Add( "class", "icon-button attributes" );
                    controlsIcon.Attributes.Add( "title", "Toggle Controls" );
                    controlsIcon.Attributes.Add( "href", "#" );
                    controlsIcon.Attributes.Add( "onclick", string.Format("{0}_ToggleControlRow()", jsFriendlyClientId ) );
                    title.Controls.Add( controlsIcon );
                }

//<span style="float:right" class="ui-icon ui-icon-search" title="Toggle search panel" onclick="toggleFilterRow()"></span>                }

                // Add the Grid HTML container
                HtmlGenericControl grid = new HtmlGenericControl( "div" );
                grid.Attributes.Add( "class", "data-grid" );
                grid.Attributes.Add( "style", string.Format( "width:100%;height:{0};", this.Height.ToString() ) );
                grid.ID = this.ID + "_grid";
                this.Controls.Add( grid );

                HtmlGenericControl controlsGrid = new HtmlGenericControl( "div" );
                controlsGrid.ID = this.ID + "_controls";
                controlsGrid.Attributes.Add( "style", "display:none;background:#dddddd;padding:3px;color:black;height:25px" );
                controlsGrid.InnerText = "Control Icons will go here (i.e. excel export)";
                this.Parent.Controls.Add( controlsGrid );

                this.Height = Unit.Empty;

                HtmlGenericControl pager = new HtmlGenericControl( "div" );
                if ( EnablePaging )
                {
                    pager.Attributes.Add( "class", "data-grid-pager" );
                    pager.ID = this.ID + "_pager";
                    //pager.Attributes.Add( "style", string.Format( "width:{0};height:20px;", this.Width.ToString() ) );
                    this.Controls.Add( pager );
                }

                // Create column & function Definitions
                List<string> columnDefs = new List<string>();

                if ( EnableOrdering )
                    columnDefs.Add( "{id:\"#\", width:40, name:\"\", behavior:\"selectAndMove\", selectable:false, resizable:false, cssClass:\"data-grid-cell-reorder dnd\"}" );

                foreach ( GridColumn gridColumn in GridColumns )
                {
                    if (gridColumn.Visible)
                        columnDefs.Add( "{" + string.Join( ", ", gridColumn.ColumnParameters.ToArray() ) + "}" );

                    gridColumn.AddScriptFunctions( Page );
                }

                bool uniqueKey = IdColumnName != string.Empty;

                // If grid has a unique key, use a SlickGrid DataView object instead of just a data[] object
                if ( uniqueKey )
                    Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/slickgrid/slick.dataview.js" );

                // Create row Definitions
                List<string> rowDefs = new List<string>();
                foreach ( object dataItem in retrievedData )
                {
                    List<string> rowColValues = new List<string>();
                    foreach ( GridColumn gridColumn in GridColumns )
                    {
                        if ( IdColumnName == gridColumn.DataField && IdColumnName != "id" )
                            rowColValues.Add( string.Format( "id:\"{0}\"", DataBinder.GetPropertyValue( dataItem, gridColumn.DataField, null ) ) );

                        rowColValues.Add( gridColumn.RowParameter( dataItem ) );
                    }

                    rowDefs.Add( string.Format( "{0}_data[{1}] = {{{2}}};",
                        jsFriendlyClientId,
                        rowDefs.Count,
                        String.Join( ",", rowColValues.ToArray() ) ) );
                }

                // Create and add SlickGrid script
                StringBuilder script = new StringBuilder();
                script.Append( @"
<script>
" );
                if ( uniqueKey )
                    script.AppendFormat( @"
    var {0}_dataView;
    var {0}_selectedRowIds = [];
",
                        jsFriendlyClientId );

                script.AppendFormat( @"
    var {0}_grid;
    var {0}_data = [];

    var {0}_options = {{
        {1}
    }};

    var {0}_columns = [
        {2}
    ];

    $(function () {{

        {3}

",
                    jsFriendlyClientId,
                    String.Join( ",\n\t", OptionValues().ToArray() ),
                    String.Join( ",\n\t", columnDefs.ToArray() ),
                    String.Join( "\n\t", rowDefs.ToArray() ) );

                if ( uniqueKey )
                    script.AppendFormat( @"
        {0}_dataView = new Slick.Data.DataView();
",
                        jsFriendlyClientId );

                script.AppendFormat( @"
        {0}_grid = new Slick.Grid(""#{1}"", {0}_data{2}, {0}_columns, {0}_options);
        {0}_grid.setSelectionModel(new Slick.RowSelectionModel());
        
        $(""#{3}"")
            .appendTo({0}_grid.getTopPanel())
            .show();
",
                    jsFriendlyClientId,
                    grid.ClientID,
                    uniqueKey ? "View" : "",
                    controlsGrid.ClientID );

                if ( !uniqueKey )
                    script.AppendFormat( @"
        $(""#{0}"").show;
",
                    grid.ClientID );

                if ( EnablePaging )
                    script.AppendFormat( @"
        var testvar = $(""#{1}"");
        var {0}_pager = new Slick.Controls.Pager({0}_dataView, {0}_grid, $(""#{1}""));
",
                        jsFriendlyClientId,
                        pager.ClientID );

                if ( EnableOrdering )
                    script.AppendFormat( @"
        var {0}_moveRowsPlugin = new Slick.RowMoveManager();
        {0}_moveRowsPlugin.onBeforeMoveRows.subscribe(function(e,data) {{
            for (var i = 0; i < data.rows.length; i++) {{
                // no point in moving before or after itself
                if (data.rows[i] == data.insertBefore || data.rows[i] == data.insertBefore - 1) {{
                    e.stopPropagation();
                    return false;
                }}
            }}

            return true;
        }});

        {0}_moveRowsPlugin.onMoveRows.subscribe(function(e,args) {{
			var extractedRows = [], left, right;
            var rows = args.rows;
            var insertBefore = args.insertBefore;

            //alert (rows);
            //alert (insertBefore);

			left = {0}_data.slice(0,insertBefore);
			right = {0}_data.slice(insertBefore,{0}_data.length);

			for (var i=0; i<rows.length; i++) {{
				extractedRows.push({0}_data[rows[i]]);
			}}

			rows.sort().reverse();

			for (var i=0; i<rows.length; i++) {{
				var row = rows[i];
				if (row < insertBefore)
					left.splice(row,1);
				else
					right.splice(row-insertBefore,1);
			}}

			{0}_data = left.concat(extractedRows.concat(right));

			var selectedRows = [];
			for (var i=0; i<rows.length; i++)
				selectedRows.push(left.length+i);

            {0}_grid.resetActiveCell();
            {0}_dataView.setItems({0}_data);
			{0}_grid.setSelectedRows(selectedRows);
            {0}_grid.render();

            {0}_SendServerData('REORDER:' + rows + ';' + insertBefore);

        }});

        {0}_grid.registerPlugin({0}_moveRowsPlugin);

        {0}_grid.onDragInit.subscribe(function(e,dd) {{
            // prevent the grid from cancelling drag'n'drop by default
            e.stopImmediatePropagation();
        }});

        {0}_grid.onDragStart.subscribe(function(e,dd) {{
            var cell = {0}_grid.getCellFromEvent(e);
            if (!cell)
                return;

            dd.row = cell.row;
            if (!{0}_data[dd.row])
                return;

            if (Slick.GlobalEditorLock.isActive())
                return;

            e.stopImmediatePropagation();
            dd.mode = ""recycle"";

            var selectedRows = {0}_grid.getSelectedRows();

            if (!selectedRows.length || $.inArray(dd.row,selectedRows) == -1) {{
                selectedRows = [dd.row];
                {0}_grid.setSelectedRows(selectedRows);
            }}

            dd.rows = selectedRows;
            dd.count = selectedRows.length;

            var proxy = $(""<span></span>"")
                .css({{
                    position: ""absolute"",
                    display: ""inline-block"",
                    padding: ""4px 10px"",
                    background: ""#e0e0e0"",
                    border: ""1px solid gray"",
                    ""z-index"": 99999,
                    ""-moz-border-radius"": ""8px"",
                    ""-moz-box-shadow"": ""2px 2px 6px silver""
                    }})
                .appendTo(""body"");

            dd.helper = proxy;

            $(dd.available).css(""background"",""pink"");

            return proxy;
        }});

        {0}_grid.onDrag.subscribe(function(e,dd) {{
            if (dd.mode != ""recycle"") {{
                return;
            }}
            e.stopImmediatePropagation();
            dd.helper.css({{top: e.pageY + 5, left: e.pageX + 5}});
        }});

        {0}_grid.onDragEnd.subscribe(function(e,dd) {{
            if (dd.mode != ""recycle"") {{
                return;
            }}
            e.stopImmediatePropagation();
            dd.helper.remove();
            $(dd.available).css(""background"",""beige"");
        }});
",
                        jsFriendlyClientId,
                        pager.ClientID );

                if ( uniqueKey )
                    script.AppendFormat( @"

        {0}_grid.onCellChange.subscribe(function(e,args) {{
              //alert(""Cell Changed"");
        }});

		{0}_dataView.onRowsChanged.subscribe(function(e,args) {{
			{0}_grid.invalidateRows(args.rows);
			{0}_grid.render();

			if ({0}_selectedRowIds.length > 0)
			{{
				// since how the original data maps onto rows has changed,
				// the selected rows in the grid need to be updated
				var selRows = [];
				for (var i = 0; i < {0}_selectedRowIds.length; i++)
				{{
					var idx = {0}_dataView.getRowById({0}_selectedRowIds[i]);
					if (idx != undefined)
						selRows.push(idx);
				}}

				{0}_grid.setSelectedRows(selRows);
			}}
		}});

		{0}_dataView.onPagingInfoChanged.subscribe(function(e,pagingInfo) {{
            //alert('paging info changed');
			var isLastPage = pagingInfo.pageSize*(pagingInfo.pageNum+1)-1 >= pagingInfo.totalRows;
            var enableAddRow = isLastPage || pagingInfo.pageSize==0;
            var options = {0}_grid.getOptions();

            if (options.enableAddRow != enableAddRow)
    			{0}_grid.setOptions({{enableAddRow:enableAddRow}});
		}});

        {0}_dataView.onRowsChanged.subscribe(function(e,args) {{
			{0}_grid.invalidateRows(args.rows);
			{0}_grid.render();

			if ({0}_selectedRowIds.length > 0)
			{{
				var selRows = [];
				for (var i = 0; i < {0}_selectedRowIds.length; i++)
				{{
					var idx = {0}_dataView.getRowById({0}_selectedRowIds[i]);
					if (idx != undefined)
						selRows.push(idx);
				}}

				{0}_grid.setSelectedRows(selRows);
			}}
		}});

        {0}_dataView.beginUpdate();
        {0}_dataView.setItems({0}_data);
        {0}_dataView.endUpdate();
",
                    jsFriendlyClientId );

                script.Append( @"
    })
</script>" );

                this.Controls.Add( new LiteralControl( script.ToString() ) );
            }
        }

        private List<string> OptionValues()
        {
            List<string> options = new List<string>();

            if ( EnableEdit )
                options.Add( "editable: true" );

            if ( EnableOrdering )
                options.Add( "enableRowReordering: true" );

            //temporary hard-coded options
            options.Add( "enableCellNavigation: true" );
            options.Add( "enableColumnReorder: false" );
            options.Add( "forceFitColumns: true" );

            return options;
        }


        public string GetCallbackResult()
        {
            return returnValue;
        }

        public void RaiseCallbackEvent( string eventArgument )
        {
            if ( eventArgument.StartsWith( "REORDER:" ) )
            {
                string[] parms = eventArgument.Substring( 8 ).Split( ';' );

                int oldIndex = 0;
                Int32.TryParse( parms[0], out oldIndex );

                int newIndex = 0;
                Int32.TryParse( parms[1], out newIndex );

                GridReorderEventArgs args = new GridReorderEventArgs( oldIndex, newIndex );
                OnGridReorder( args );
            }

            returnValue = eventArgument;
        }

        public event GridReorderEventHandler GridReorder;
        protected virtual void OnGridReorder(GridReorderEventArgs e)
        {
            if ( GridReorder != null )
                GridReorder( this, e );
        }
    }

    public class GridReorderEventArgs : EventArgs
    {
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }

        public GridReorderEventArgs( int oldIndex, int newIndex )
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }
    public delegate void GridReorderEventHandler( object sender, GridReorderEventArgs e );
}
