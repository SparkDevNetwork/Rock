using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Controls
{
    [ToolboxData( "<{0}:Grid runat=server></{0}:Grid>" )]
    public class Grid : System.Web.UI.WebControls.GridView, ICallbackEventHandler
    {
        private string returnValue;
        string jsFriendlyClientId = string.Empty;

        [
        Category( "Behavior" ),
        DefaultValue( false ),
        Description( "Enable Ordering" )
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
        Description( "Enable Client-side Sorting" )
        ]
        public virtual bool EnableClientSorting
        {
            get
            {
                bool? b = ViewState["EnableClientSorting"] as bool?;
                return ( b == null ) ? false : b.Value;
            }
            set
            {
                ViewState["EnableClientSorting"] = value;
            }
        }

        protected override void OnInit( EventArgs e )
        {
            Rock.Cms.CmsPage.AddCSSLink( Page, "~/CSS/grid.css" );

            this.CssClass = "data-grid";
            this.AutoGenerateColumns = false;
            this.AllowPaging = true;

            jsFriendlyClientId = this.ClientID.Replace( "-", "_" );

            ClientScriptManager cs = Page.ClientScript;
            Type type = this.GetType();

            string gridScriptKey = string.Format( "{0}_gridScripts", jsFriendlyClientId );
            if ( !cs.IsClientScriptBlockRegistered( type, gridScriptKey ) )
            {
                string gridScript = string.Format( @"

    function {0}_SendServerData(arg, context)
    {{
        {1}
    }}

    function {0}_ReceiveServerData(rValue)
    {{
        var jsonObject = JSON.parse(rValue);
        if (!jsonObject.Cancel)
        {{
            switch (jsonObject.Action)
            {{
                case 'delete': 
                    var item = {0}_dataView.getItem(jsonObject.Result);
                    {0}_dataView.deleteItem(item.id);
                    {0}_grid.invalidate();
                    {0}_grid.render();
                    break;
            }}
        }}
    }}
",
                    jsFriendlyClientId,
                    cs.GetCallbackEventReference(
                        this, "arg", string.Format( "{0}_ReceiveServerData", jsFriendlyClientId ), "context" ) );

                cs.RegisterClientScriptBlock( type, gridScriptKey, gridScript, true );
            }

            base.OnInit( e );
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( Rows.Count > 0 )
            {
                UseAccessibleHeader = true;
                HeaderRow.TableSection = TableRowSection.TableHeader;
                FooterRow.TableSection = TableRowSection.TableFooter;
            }

            if ( TopPagerRow != null )
                TopPagerRow.TableSection = TableRowSection.TableHeader;

            if ( BottomPagerRow != null )
                BottomPagerRow.TableSection = TableRowSection.TableFooter;

            if ( this.EnableClientSorting )
            {
                if ( this.AllowSorting || this.EnableOrdering )
                    throw new ArgumentException( "Cannot use EnableClientSorting with AllowSorting or EnableOrdering" );

                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.tablesorter.min.js" );

                string script = string.Format( @"
    $(document).ready(function() {{
        $('#{0}').tablesorter();
    }});
", this.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-sort-{0}-script", this.ClientID ), script, true );
            }

            if ( this.EnableOrdering )
            {
                if ( this.AllowSorting || this.EnableClientSorting )
                    throw new ArgumentException( "Cannot use EnableOrdering with AllowSorting or EnableClientSorting" );

                string script = @"
    var fixHelper = function(e, ui) {
        ui.children().each(function() {
            $(this).width($(this).width());
        });
        return ui;
    };
";
                this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), 
                    "grid-sort-helper-script", script, true );

                script = string.Format( @"
    $(document).ready(function() {{
        $('#{0} tbody').sortable({{
            helper: fixHelper,
            start: function(event, ui) {{
                var start_pos = ui.item.index();
                ui.item.data('start_pos', start_pos);
            }},
            update: function(event, ui) {{
                {0}_SendServerData('re-order:' + ui.item.attr('datakey') + ';' + ui.item.data('start_pos') + ';' + ui.item.index());
            }}
        }}).disableSelection();
    }});
", jsFriendlyClientId );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), 
                    string.Format( "grid-sort-{0}-script", jsFriendlyClientId ), script, true );
            }
        }

        protected override void OnRowDataBound( GridViewRowEventArgs e )
        {
            base.OnRowDataBound( e );

            if ( e.Row.DataItem != null && this.DataKeys != null && this.DataKeys.Count > 0 )
            {
                object dataKey = this.DataKeys[e.Row.RowIndex].Value as object;
                if ( dataKey != null )
                {
                    string key = dataKey.ToString();
                    e.Row.Attributes.Add("datakey",key);
                }
            }

            else if (e.Row.RowType == DataControlRowType.Pager)
            {
                int pgrCells = e.Row.Cells.Count;
            }
        }

        #region Callback Methods/Events

        public string GetCallbackResult()
        {
            return returnValue;
        }

        public void RaiseCallbackEvent( string eventArgument )
        {
            JsonResult jsonResult = null;

            if ( eventArgument.StartsWith( "re-order:" ) )
            {
                string[] parms = eventArgument.Substring( 9 ).Split( ';' );

                string dataKey = parms[0];

                int oldIndex = 0;
                Int32.TryParse( parms[1], out oldIndex );

                int newIndex = 0;
                Int32.TryParse( parms[2], out newIndex );

                GridReorderEventArgs args = new GridReorderEventArgs( dataKey, oldIndex, newIndex );
                OnGridReorder( args );

                jsonResult = new JsonResult( "re-order", args.Cancel );
            }

            returnValue = jsonResult.Serialize();

        }

        public event GridReorderEventHandler GridReorder;
        protected virtual void OnGridReorder( GridReorderEventArgs e )
        {
            if ( GridReorder != null )
                GridReorder( this, e );
        }

        #endregion
    }

    #region Event Classes

    public class GridReorderEventArgs : EventArgs
    {
        public string DataKey { get; private set; }
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }

        private bool _cancel = false;
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public GridReorderEventArgs( string dataKey, int oldIndex, int newIndex )
        {
            DataKey = dataKey;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    #endregion
    
    public delegate void GridReorderEventHandler( object sender, GridReorderEventArgs e );

    internal class JsonResult
    {
        public string Action { get; set; }
        public bool Cancel { get; set; }
        public object Result { get; set; }

        public JsonResult( string action, bool cancel )
        {
            Action = action;
            Cancel = cancel;
            Result = null;
        }

        public JsonResult( string action, bool cancel, object result )
        {
            Action = action;
            Cancel = cancel;
            Result = result;
        }

        public string Serialize()
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();

            StringBuilder sb = new StringBuilder();

            serializer.Serialize( this, sb );

            return sb.ToString();
        }


    }
}