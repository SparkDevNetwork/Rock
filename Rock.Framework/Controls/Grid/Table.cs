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

namespace Rock.Controls.Grid
{
    [
    AspNetHostingPermission( SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal ),
    AspNetHostingPermission( SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal ),
    DefaultProperty( "Columns" ),
    ParseChildren( true, "Columns" ),
    ToolboxData( "<{0}:Table runat=\"server\"> </{0}:Table>" )
    ]
    public class Table : DataBoundControl, ICallbackEventHandler
    {
        private string returnValue;

        string jsFriendlyClientId = string.Empty;

        List<List<string>> rowVals = new List<List<string>>();

        #region Properties

        [
        Category( "Behavior" ),
        DefaultValue( false ),
        Description( "Display icon for adding new rows to grid" )
        ]
        public virtual bool EnableAdd
        {
            get
            {
                bool? b = ViewState["EnableAdd"] as bool?;
                return ( b == null ) ? false : b.Value;
            }
            set
            {
                ViewState["EnableAdd"] = value;
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
        DefaultValue( "" ),
        Description( "Unique identifier column name" )
        ]
        public virtual string IdentityColumn
        {
            get
            {
                string s = ViewState["IdentityColumn"] as string;
                return ( s == null ) ? "" : s;
            }
            set
            {
                ViewState["IdentityColumn"] = value;
            }
        }

        [
        Category( "Behavior" ),
        Description( "The column collection" ),
        DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
            //Editor(typeof(GridColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode( PersistenceMode.InnerDefaultProperty )
        ]
        public List<Column> Columns
        {
            get
            {
                if ( columns == null )
                    columns = new List<Column>();
                return columns;
            }
        }
        private List<Column> columns;


        #endregion

        #region Events

        protected override void OnLoad( EventArgs e )
        {
            jsFriendlyClientId = this.ClientID.Replace( "-", "_" );

            Rock.Cms.CmsPage.AddCSSLink( Page, "~/CSS/grid.css" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.tablesorter.min.js" );
            Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.tablesorter.pager.js" );

            foreach ( Column column in Columns )
            {
                column.JsFriendlyClientId = jsFriendlyClientId;
                column.AddScriptFunctions( Page );
            }
        }

        protected override void Render( HtmlTextWriter writer )
        {
            writer.WriteBeginTag( "table" );
            writer.WriteAttribute( "id", jsFriendlyClientId );
            if ( this.Width != Unit.Empty )
                writer.WriteAttribute( "style", "width:" + this.Width.ToString() );
            if ( this.CssClass != string.Empty )
                writer.WriteAttribute( "class", this.CssClass );
            writer.Write( ">" );

            // Write header row
            writer.WriteFullBeginTag( "thead" );
            writer.WriteFullBeginTag( "tr" );
            foreach ( Column column in Columns )
            {
                writer.WriteFullBeginTag( "th" );
                writer.Write( column.HeaderText );
                writer.WriteEndTag( "th" );
            }
            writer.WriteEndTag( "tr" );
            writer.WriteEndTag( "thead" );

            writer.WriteFullBeginTag( "tbody" );

            foreach ( List<string> row in rowVals )
            {
                writer.WriteFullBeginTag( "tr" );

                foreach ( string cell in row )
                {
                    writer.WriteFullBeginTag( "td" );
                    writer.Write( cell );
                    writer.WriteEndTag( "td" );
                }

                writer.WriteEndTag( "tr" );
            }

            writer.WriteEndTag( "tbody" );
            writer.WriteEndTag( "table" );

            writer.WriteBeginTag( "div" );
            writer.WriteAttribute( "id", jsFriendlyClientId + "_pager" );
            writer.Write( ">" );
            writer.WriteEndTag( "div" );

            writer.Write( string.Format(@"
    <script type='text/javascript'>
        $(document).ready(function() 
            {{ 
                $('#{0}')
                    .tablesorter()
                    .tablesorterPager({{container: $('#{1}')}}); 
            }} 
        ); 
    </script>
", jsFriendlyClientId, jsFriendlyClientId + "_pager" ) );
        }

        #region Databinding Events

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

            rowVals = new List<List<string>>();

            foreach ( Column column in Columns )
                column.JsFriendlyClientId = this.ClientID.Replace( "-", "_" ); ;

            if ( retrievedData != null )
            {
                foreach ( object dataItem in retrievedData )
                {
                    List<string> vals = new List<string>();

                    foreach ( Column column in Columns )
                        vals.Add( column.FormatCell( dataItem, IdentityColumn.Trim() ) );

                    rowVals.Add(vals);
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #endregion

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

                int oldIndex = 0;
                Int32.TryParse( parms[0], out oldIndex );

                int newIndex = 0;
                Int32.TryParse( parms[1], out newIndex );

                GridReorderEventArgs args = new GridReorderEventArgs( oldIndex, newIndex );
                OnGridReorder( args );

                jsonResult = new JsonResult( "re-order", args.Cancel );
            }
            
            else if ( eventArgument.StartsWith( "add" ) )
            {
                GridRowEventArgs args = new GridRowEventArgs(0, 0);
                OnGridAdd( args );

                jsonResult = new JsonResult( "add", args.Cancel );
            }

            else if ( eventArgument.StartsWith( "delete:" ) )
            {
                string[] parms = eventArgument.Substring( 7 ).Split( ';' );

                int Index = 0;
                Int32.TryParse( parms[0], out Index );

                int Id = 0;
                Int32.TryParse( parms[1], out Id );

                GridRowEventArgs args = new GridRowEventArgs( Index, Id );
                OnGridDelete( args );

                jsonResult = new JsonResult( "delete", args.Cancel, Index );
            }

            returnValue = jsonResult.Serialize();
        }

        public event GridReorderEventHandler GridReorder;
        protected virtual void OnGridReorder(GridReorderEventArgs e)
        {
            if ( GridReorder != null )
                GridReorder( this, e );
        }

        public event GridAddEventHandler GridAdd;
        protected virtual void OnGridAdd( GridRowEventArgs e )
        {
            if ( GridAdd != null )
                GridAdd( this, e );
        }

        public event GridDeleteEventHandler GridDelete;
        protected virtual void OnGridDelete( GridRowEventArgs e )
        {
            if ( GridDelete != null )
                GridDelete( this, e );
        }

        #endregion

    }

    #region Event Classes

    public class GridRowEventArgs : EventArgs
    {
        public int Index { get; private set; }
        public int Id { get; private set; }

        private bool _cancel = false;
        public bool Cancel 
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
        
        public GridRowEventArgs( int index, int id )
        {
            Index = index;
            Id = id;
        }
    }

    public class GridReorderEventArgs : EventArgs
    {
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }

        private bool _cancel = false;
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public GridReorderEventArgs( int oldIndex, int newIndex )
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    public delegate void GridReorderEventHandler( object sender, GridReorderEventArgs e );
    public delegate void GridAddEventHandler( object sender, GridRowEventArgs e );
    public delegate void GridDeleteEventHandler( object sender, GridRowEventArgs e );

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

    #endregion

}
