//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Rock Grid Control
    /// </summary>
    [ToolboxData( "<{0}:Grid runat=server></{0}:Grid>" )]
    public class Grid : System.Web.UI.WebControls.GridView, IPostBackEventHandler
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether client-side sorting should be enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if client-sid sorting should be enabled; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether adding should be enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if adding is enabled; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets the client script to be called when user clicks tha add icon
        /// </summary>
        /// <value>
        /// The client add script.
        /// </value>
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains event data.</param>
        protected override void OnInit( EventArgs e )
        {
            Rock.Web.UI.Page.AddCSSLink( Page, "~/CSS/grid.css" );

            this.CssClass = "grid-table";
            this.AutoGenerateColumns = false;
            this.RowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            this.HeaderStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            this.SelectedRowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;

            // hack to turn off style="border-collapse: collapse"
            this.GridLines =  GridLines.None;
            this.CellSpacing = -1;

            EmptyDataTemplate emptyDataTemplate = new EmptyDataTemplate();
            emptyDataTemplate.AddClick += gridPagerTemplate_AddClick;
            this.EmptyDataTemplate = emptyDataTemplate;

            // Paging Support
            this.AllowPaging = true;
            this.PageIndex = 1;
            this.PageSize = 20;

            PagerTemplate pagerTemplate = new PagerTemplate();
            pagerTemplate.AddClick += gridPagerTemplate_AddClick;
            pagerTemplate.PageClick += gridPagerTemplate_PageClick;
            this.PagerTemplate = pagerTemplate;

            this.ShowHeaderWhenEmpty = true;

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
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

                Rock.Web.UI.Page.AddScriptLink( Page, "~/Scripts/jquery.tablesorter.min.js" );

                string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('#{0}').tablesorter();
    }});
", this.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-sort-{0}-script", this.ClientID ), script, true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.BaseDataBoundControl.DataBound"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
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

                ddl = pagerRow.Cells[0].FindControl( "ddlPageSize" ) as DropDownList;
                if ( ddl != null )
                    ddl.SelectedValue = this.PageSize.ToString();
                
                // Set Action Controls
                HtmlGenericControl aAdd = pagerRow.Cells[0].FindControl( "aAdd" ) as HtmlGenericControl;
                if ( aAdd != null )
                {
                    aAdd.Visible = EnableAdd && ClientAddScript != string.Empty;
                    aAdd.Attributes["onclick"] = ClientAddScript;
                }

                LinkButton lbAdd = pagerRow.Cells[0].FindControl( "lbAdd" ) as LinkButton;
                if ( lbAdd != null )
                    lbAdd.Visible = EnableAdd && ClientAddScript == string.Empty;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowDataBound"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewRowEventArgs"/> that contains event data.</param>
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

        /// <summary>
        /// Handles the PageClick event of the gridPagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void gridPagerTemplate_PageClick( object sender, EventArgs e )
        {
            GridViewRow pagerRow = this.BottomPagerRow;
            if ( pagerRow != null )
            {
                // Set Paging Controls
                DropDownList ddl = pagerRow.Cells[0].FindControl( "ddlPageList" ) as DropDownList;
                if ( ddl != null )
                    this.PageIndex = ddl.SelectedIndex >= 0 ? ddl.SelectedIndex : 0;

                ddl = pagerRow.Cells[0].FindControl( "ddlPageSize" ) as DropDownList;
                if ( ddl != null )
                    this.PageSize = Int32.Parse(ddl.SelectedValue);

                EventArgs eventArgs = new EventArgs();
                OnGridRebind( eventArgs );
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gridPagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void gridPagerTemplate_AddClick( object sender, EventArgs e )
        {
            OnGridAdd( e );
        }

        #endregion

        #region Callback Methods/Events

        /// <summary>
        /// Raises the appropriate events for the <see cref="T:System.Web.UI.WebControls.GridView"/> control when it posts back to the server.
        /// </summary>
        /// <param name="eventArgument">The event argument from which to create a <see cref="T:System.Web.UI.WebControls.CommandEventArgs"/> for the event or events that are raised.</param>
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

        /// <summary>
        /// Occurs when [grid reorder].
        /// </summary>
        public event GridReorderEventHandler GridReorder;

        /// <summary>
        /// Raises the <see cref="E:GridReorder"/> event.
        /// </summary>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGridReorder( GridReorderEventArgs e )
        {
            if ( GridReorder != null )
                GridReorder( this, e );
        }

        /// <summary>
        /// Occurs when [grid add].
        /// </summary>
        public event GridAddEventHandler GridAdd;

        /// <summary>
        /// Raises the <see cref="E:GridAdd"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnGridAdd( EventArgs e )
        {
            if ( GridAdd != null )
                GridAdd( this, e );
        }

        /// <summary>
        /// Occurs when [grid rebind].
        /// </summary>
        public event GridRebindEventHandler GridRebind;

        /// <summary>
        /// Raises the <see cref="E:GridRebind"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnGridRebind( EventArgs e )
        {
            if ( GridRebind != null )
                GridRebind( this, e );
        }

        #endregion
    }

    #region Delegates

    /// <summary>
    /// Delegate used for raising the grid reorder event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
    public delegate void GridReorderEventHandler( object sender, GridReorderEventArgs e );

    /// <summary>
    /// Delegate used for raising the grid add event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public delegate void GridAddEventHandler( object sender, EventArgs e);

    /// <summary>
    /// Delegate used for raising the grid rebind event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public delegate void GridRebindEventHandler( object sender, EventArgs e );

    #endregion

    #region Event Handlers

    /// <summary>
    /// Grid Reorder Event Argument
    /// </summary>
    public class GridReorderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data key.
        /// </summary>
        public string DataKey { get; private set; }

        /// <summary>
        /// Gets the old index.
        /// </summary>
        public int OldIndex { get; private set; }

        /// <summary>
        /// Gets the new index.
        /// </summary>
        public int NewIndex { get; private set; }

        private bool _cancel = false;
        /// <summary>
        /// Gets or sets a value indicating whether the reorder event should be cancelled
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridReorderEventArgs"/> class.
        /// </summary>
        /// <param name="dataKey">The data key.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public GridReorderEventArgs( string dataKey, int oldIndex, int newIndex )
        {
            DataKey = dataKey;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// JSON Result  
    /// </summary>
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

    /// <summary>
    /// Template used for an empty data row in the <see cref="Grid"/> control
    /// </summary>
    internal class EmptyDataTemplate : ITemplate
    {
        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            HtmlGenericControl div = new HtmlGenericControl( "div" );
            div.Attributes.Add( "class", "paging" );
            container.Controls.Add( div );

            HtmlGenericControl divActions = new HtmlGenericControl( "div" );
            divActions.Attributes.Add( "class", "actions" );
            container.Controls.Add( divActions );

            HtmlGenericControl aAdd = new HtmlGenericControl( "a" );
            aAdd.ID = "aAdd";
            aAdd.Attributes.Add( "href", "#" );
            aAdd.Attributes.Add( "class", "add" );
            aAdd.InnerText = "Add";
            divActions.Controls.Add( aAdd );

            LinkButton lbAdd = new LinkButton();
            lbAdd.ID = "lbAdd";
            lbAdd.CssClass = "add";
            lbAdd.Text = "Add";
            lbAdd.Click += lbAdd_Click;
            lbAdd.CausesValidation = false;
            divActions.Controls.Add( lbAdd );
        }

        void lbAdd_Click( object sender, EventArgs e )
        {
            if ( AddClick != null )
                AddClick( sender, e );
        }

        internal event EventHandler AddClick;
    }

    /// <summary>
    /// Template used for the pager row in the <see cref="Grid"/> control
    /// </summary>
    internal class PagerTemplate : ITemplate
    {
        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void  InstantiateIn(Control container)
        {
            HtmlGenericControl div = new HtmlGenericControl( "div" );
            div.Attributes.Add( "class", "paging" );
            container.Controls.Add( div );

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlPageList";
            ddl.AutoPostBack = true;
            ddl.CausesValidation = false;
            ddl.SelectedIndexChanged += ddl_SelectedIndexChanged;

            Label lbl = new Label();
            lbl.Text = "Select Page:";
            lbl.AssociatedControlID = "ddlPageList";

            div.Controls.Add( lbl );
            div.Controls.Add( ddl );

            Label lblPages = new Label();
            lbl.ID = "lblPages";
            div.Controls.Add( lblPages );

            div.Controls.Add( new LiteralControl( "&nbsp;&nbsp;&nbsp;&nbsp;" ) );

            ddl = new DropDownList();
            ddl.ID = "ddlPageSize";
            ddl.AutoPostBack = true;
            ddl.CausesValidation = false;
            ddl.SelectedIndexChanged += ddl_SelectedIndexChanged;
            ddl.Items.Add( new ListItem( "5", "5" ) );
            ddl.Items.Add( new ListItem( "20", "20" ) );
            ddl.Items.Add( new ListItem( "100", "100" ) );
            ddl.Items.Add( new ListItem( "1000", "1000" ) );

            lbl = new Label();
            lbl.Text = "Page Size:";
            lbl.AssociatedControlID = "ddlPageSize";

            div.Controls.Add( lbl );
            div.Controls.Add( ddl );

            HtmlGenericControl divActions = new HtmlGenericControl( "div" );
            divActions.Attributes.Add( "class", "grid-actions" );
            container.Controls.Add( divActions );

            HtmlGenericControl aAdd = new HtmlGenericControl( "a" );
            aAdd.ID = "aAdd";
            aAdd.Attributes.Add( "href", "#" );
            aAdd.Attributes.Add( "class", "add" );
            aAdd.InnerText = "Add";
            divActions.Controls.Add( aAdd );

            LinkButton lbAdd = new LinkButton();
            lbAdd.ID = "lbAdd";
            lbAdd.CssClass = "add";
            lbAdd.Text = "Add";
            lbAdd.Click += lbAdd_Click;
            lbAdd.CausesValidation = false;
            divActions.Controls.Add( lbAdd );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void ddl_SelectedIndexChanged( object sender, EventArgs e )
        {
            if (PageClick != null)
                PageClick( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the lbAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void lbAdd_Click( object sender, EventArgs e )
        {
            if (AddClick != null)
                AddClick( sender, e );
        }

        /// <summary>
        /// Occurs when [page click].
        /// </summary>
        internal event EventHandler PageClick;

        /// <summary>
        /// Occurs when [add click].
        /// </summary>
        internal event EventHandler AddClick;
    }

    #endregion
}