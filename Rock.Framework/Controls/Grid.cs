using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Rock.Controls
{
    [ToolboxData( "<{0}:Grid runat=server></{0}:Grid>" )]
    public class Grid : System.Web.UI.WebControls.GridView, IPostBackEventHandler
    {
        #region Properties

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

        [
        Category( "Behavior" ),
        DefaultValue( false ),
        Description( "Enable Add" )
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
        Description( "Client Add Script" )
        ]
        public virtual string ClientAddScript
        {
            get
            {
                if ( ViewState["ClientAddScript"] == null )
                    ViewState["ClientAddScript"] = string.Empty;
                return ViewState["ClientAddScript"].ToString();
            }
            set
            {
                ViewState["ClientAddScript"] = value;
            }
        }

        #endregion

        #region Overridden Methods

        protected override void OnInit( EventArgs e )
        {
            Rock.Cms.CmsPage.AddCSSLink( Page, "~/CSS/grid.css" );

            this.CssClass = "grid-table";
            this.AutoGenerateColumns = false;
            this.RowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            this.HeaderStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            this.SelectedRowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            this.PagerStyle.CssClass = "grid-footer";

            // Paging Support
            this.AllowPaging = true;

            GridPagerTemplate gridPagerTemplate = new GridPagerTemplate();
            gridPagerTemplate.AddClick += new EventHandler( gridPagerTemplate_AddClick );
            gridPagerTemplate.PageClick += new EventHandler( gridPagerTemplate_PageClick );
            this.PagerTemplate = gridPagerTemplate;

            this.ShowHeaderWhenEmpty = true;
            this.EmptyDataText = "No Results";

            base.OnInit( e );
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            UseAccessibleHeader = true;

            if ( HeaderRow != null )
                HeaderRow.TableSection = TableRowSection.TableHeader;

            if ( FooterRow != null )
                FooterRow.TableSection = TableRowSection.TableFooter;

            if ( TopPagerRow != null )
                TopPagerRow.TableSection = TableRowSection.TableHeader;

            if ( BottomPagerRow != null )
            {
                BottomPagerRow.TableSection = TableRowSection.TableFooter;
                if ( !BottomPagerRow.Visible )
                    BottomPagerRow.Visible = true;
            }

            if ( this.EnableClientSorting )
            {
                if ( this.AllowSorting )
                    throw new ArgumentException( "Cannot use EnableClientSorting with AllowSorting" );

                Rock.Cms.CmsPage.AddScriptLink( Page, "~/Scripts/jquery.tablesorter.min.js" );

                string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('#{0}').tablesorter();
    }});
", this.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-sort-{0}-script", this.ClientID ), script, true );
            }
        }

        protected override void OnDataBound( EventArgs e )
        {
            base.OnDataBound( e );

            GridViewRow pagerRow = this.BottomPagerRow;
            if ( pagerRow != null )
            {
                // Set Paging Controls
                DropDownList ddl = pagerRow.Cells[0].FindControl( "ddlPageList" ) as DropDownList;
                if ( ddl != null )
                {
                    for ( int i = 0; i < this.PageCount; i++ )
                    {
                        int pageNumber = i + 1;
                        ListItem li = new ListItem( pageNumber.ToString(), i.ToString() );
                        if ( i == this.PageIndex )
                            li.Selected = true;
                        ddl.Items.Add( li );
                    }
                }
                
                // Set Action Controls
                LinkButton lbAdd = pagerRow.Cells[0].FindControl( "lbAdd" ) as LinkButton;
                if ( lbAdd != null )
                {
                    lbAdd.Visible = EnableAdd;
                    if ( ClientAddScript != string.Empty )
                        lbAdd.OnClientClick = ClientAddScript;
                }
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
        }

        #endregion

        #region Internal Methods

        void gridPagerTemplate_PageClick( object sender, EventArgs e )
        {
            GridViewRow pagerRow = this.BottomPagerRow;
            if ( pagerRow != null )
            {
                // Set Paging Controls
                DropDownList ddl = pagerRow.Cells[0].FindControl( "ddlPageList" ) as DropDownList;
                if ( ddl != null )
                {
                    this.PageIndex = ddl.SelectedIndex;
                    EventArgs eventArgs = new EventArgs();
                    OnGridRebind( eventArgs );
                }
            }
        }

        void gridPagerTemplate_AddClick( object sender, EventArgs e )
        {
            OnGridAdd( e );
        }

        #endregion

        #region Callback Methods/Events

        void IPostBackEventHandler.RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument.StartsWith( "re-order:" ) )
            {
                string[] parms = eventArgument.Substring( 9 ).Split( ';' );

                string dataKey = parms[0];

                int oldIndex = 0;
                if ( !Int32.TryParse( parms[1], out oldIndex ) )
                    oldIndex = 0;

                int newIndex = 0;
                if ( !Int32.TryParse( parms[2], out newIndex ) )
                    newIndex = 0;

                int pageFactor = this.PageIndex * this.PageSize;
                oldIndex += pageFactor;
                newIndex += pageFactor;

                GridReorderEventArgs args = new GridReorderEventArgs( dataKey, oldIndex, newIndex );
                OnGridReorder( args );
            }
        }
        
        #endregion

        #region Events

        public event GridReorderEventHandler GridReorder;
        protected virtual void OnGridReorder( GridReorderEventArgs e )
        {
            if ( GridReorder != null )
                GridReorder( this, e );
        }

        public event GridAddEventHandler GridAdd;
        protected virtual void OnGridAdd( EventArgs e )
        {
            if ( GridAdd != null )
                GridAdd( this, e );
        }

        public event GridRebindEventHandler GridRebind;
        protected virtual void OnGridRebind( EventArgs e )
        {
            if ( GridRebind != null )
                GridRebind( this, e );
        }

        #endregion
    }

    #region Delegates

    public delegate void GridReorderEventHandler( object sender, GridReorderEventArgs e );
    public delegate void GridAddEventHandler( object sender, EventArgs e);
    public delegate void GridRebindEventHandler( object sender, EventArgs e );

    #endregion

    #region Event Handlers

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

    #region Helper Classes

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

    #region Templates

    internal class GridPagerTemplate : ITemplate
    {
        public void  InstantiateIn(Control container)
        {
            HtmlGenericControl divPaging = new HtmlGenericControl( "div" );
            divPaging.Attributes.Add( "class", "paging" );
            container.Controls.Add( divPaging );

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlPageList";
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += new EventHandler( ddl_SelectedIndexChanged );

            Label lbl = new Label();
            lbl.Text = "Select a page:";
            lbl.AssociatedControlID = "ddlPageList";

            divPaging.Controls.Add( lbl );
            divPaging.Controls.Add( ddl );

            HtmlGenericControl divActions = new HtmlGenericControl( "div" );
            divActions.Attributes.Add( "class", "actions" );
            container.Controls.Add( divActions );

            LinkButton lbAdd = new LinkButton();
            lbAdd.ID = "lbAdd";
            lbAdd.CssClass = "add";
            lbAdd.Text = "Add";
            lbAdd.Click += new EventHandler( lbAdd_Click );
            divActions.Controls.Add( lbAdd );
        }

        void ddl_SelectedIndexChanged( object sender, EventArgs e )
        {
            if (PageClick != null)
                PageClick( sender, e );
        }

        void lbAdd_Click( object sender, EventArgs e )
        {
            if (AddClick != null)
                AddClick( sender, e );
        }

        internal event EventHandler PageClick;
        internal event EventHandler AddClick;
    }

    #endregion
}