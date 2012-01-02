//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
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
        const int ALL_ITEMS_SIZE = 1000000;

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
            this.PageIndex = 0;
            this.PageSize = 25;

            PagerTemplate pagerTemplate = new PagerTemplate();
            pagerTemplate.NavigateClick += pagerTemplate_NavigateClick;
            pagerTemplate.ItemsPerPageClick += pagerTemplate_ItemsPerPageClick;
            pagerTemplate.AddClick += gridPagerTemplate_AddClick;
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

            PagerTemplate pagerTemplate = this.PagerTemplate as PagerTemplate;
            if ( PagerTemplate != null )
            {
                pagerTemplate.SetNavigation( this.PageCount, this.PageIndex, this.PageSize );
                SetActionProperties( pagerTemplate.Actions );
            }

            EmptyDataTemplate emptyDataTemplate = this.EmptyDataTemplate as EmptyDataTemplate;
            if (emptyDataTemplate != null)
                SetActionProperties( emptyDataTemplate.Actions );
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

        #region Private Methods

        private void SetActionProperties( GridActions actions )
        {
            actions.EnableAdd = this.EnableAdd;
            actions.ClientAddScript = this.ClientAddScript;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handles the ItemsPerPageClick event of the pagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
        void pagerTemplate_ItemsPerPageClick( object sender, NumericalEventArgs e )
        {
            this.PageSize = e.Number;
            OnGridRebind( e );
        }

        /// <summary>
        /// Handles the NavigateClick event of the pagerTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
        void pagerTemplate_NavigateClick( object sender, NumericalEventArgs e )
        {
            this.PageIndex = e.Number;
            OnGridRebind( e );
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

    /// <summary>
    /// Delegate used for raising the grid items per page changed event
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="Rock.Web.UI.Controls.NumericalEventArgs"/> instance containing the event data.</param>
    internal delegate void PageNavigationEventHandler( object sender, NumericalEventArgs e );

    #endregion

    #region Event Handlers

    /// <summary>
    /// Items Per Page Event Argument
    /// </summary>
    internal class NumericalEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the items per page.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericalEventArgs"/> class.
        /// </summary>
        /// <param name="itemsPerPage">The items per page.</param>
        public NumericalEventArgs( int number )
        {
            Number = number;
        }
    }

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
        GridActions gridActions;

        /// <summary>
        /// Gets the actions.
        /// </summary>
        public GridActions Actions
        {
            get { return gridActions; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyDataTemplate"/> class.
        /// </summary>
        public EmptyDataTemplate()
        {
            gridActions = new GridActions();
            gridActions.AddClick += new EventHandler( actions_AddClick );
        }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            container.Controls.Add( gridActions );
        }

        void actions_AddClick( object sender, EventArgs e )
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
        const int ALL_ITEMS_SIZE = 1000000;

        Literal lStatus;

        HtmlGenericControl NavigationPanel;

        HtmlGenericContainer[] PageLinkListItem = new HtmlGenericContainer[12];
        LinkButton[] PageLink = new LinkButton[12];

        HtmlGenericContainer[] ItemLinkListItem = new HtmlGenericContainer[4];
        LinkButton[] ItemLink = new LinkButton[4];
        
        GridActions gridActions;

        /// <summary>
        /// Gets the actions.
        /// </summary>
        public GridActions Actions
        {
            get { return gridActions; }
        }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn(Control container)
        {
            HtmlGenericControl divPagination = new HtmlGenericControl( "div" );
            divPagination.Attributes.Add( "class", "pagination" );
            container.Controls.Add( divPagination );

            // Page Status
            HtmlGenericControl divStatus = new HtmlGenericControl( "div" );
            divStatus.Attributes.Add( "class", "page-status" );
            divPagination.Controls.Add( divStatus );

            lStatus = new Literal(); 
            divStatus.Controls.Add( lStatus );

            // Pagination
            NavigationPanel = new HtmlGenericControl( "div" );
            NavigationPanel.Attributes.Add( "class", "page-navigation" );
            divPagination.Controls.Add( NavigationPanel );

            HtmlGenericControl ulNavigation = new HtmlGenericControl( "ul" );
            NavigationPanel.Controls.Add( ulNavigation );

            for ( var i = 0; i < PageLinkListItem.Length; i++ )
            {
                PageLinkListItem[i] = new HtmlGenericContainer( "li" );
                ulNavigation.Controls.Add( PageLinkListItem[i] );

                PageLink[i] = new LinkButton();
                PageLinkListItem[i].Controls.Add( PageLink[i] );
                PageLink[i].Click += new EventHandler( lbPage_Click );
            }

            PageLink[0].Text = "&larr; Previous";
            PageLink[PageLinkListItem.Length - 1].Text = "Next &rarr;";

            // Items Per Page
            HtmlGenericControl divSize = new HtmlGenericControl( "div" );
            divSize.Attributes.Add( "class", "page-size" );
            divPagination.Controls.Add( divSize );

            Label lblPageSize = new Label();
            lblPageSize.Text = "Items per page:";
            divSize.Controls.Add( lblPageSize );

            HtmlGenericControl divSizeOptions = new HtmlGenericControl( "div" );
            divSizeOptions.Attributes.Add( "class", "page-size-options" );
            divSize.Controls.Add( divSizeOptions );

            HtmlGenericControl ulSizeOptions = new HtmlGenericControl( "ul" );
            divSizeOptions.Controls.Add( ulSizeOptions );

            for ( int i = 0; i < ItemLinkListItem.Length; i++ )
            {
                ItemLinkListItem[i] = new HtmlGenericContainer( "li" );
                ulSizeOptions.Controls.Add( ItemLinkListItem[i] );

                ItemLink[i] = new LinkButton();
                ItemLinkListItem[i].Controls.Add( ItemLink[i] );
                ItemLink[i].Click += new EventHandler( lbItems_Click );
            }

            ItemLink[0].Text = "25";
            ItemLink[1].Text = "100";
            ItemLink[2].Text = "1,000";
            ItemLink[3].Text = "All";


            // Actions
            gridActions = new GridActions();
            container.Controls.Add( gridActions );
            gridActions.AddClick += new EventHandler( actions_AddClick );

        }

        public void SetNavigation( int pageCount, int pageIndex, int pageSize )
        {
            // Set status
            lStatus.Text = string.Format( "Page {0:N0} of {1:N0}", pageIndex+1, pageCount );

            // Set navigation controls
            if (pageCount > 1)
            {
                int pageNumber = ( int )( pageIndex / 10 );

                if ( pageNumber <= 0 )
                {
                    PageLinkListItem[0].Attributes["class"] = "prev disabled";
                    PageLink[0].Attributes["page-index"] = "0";
                    PageLink[0].Enabled = false;
                }
                else
                {
                    PageLinkListItem[0].Attributes["class"] = "prev";
                    PageLink[0].Attributes["page-index"] = ( pageNumber - 1 ).ToString();
                    PageLink[0].Enabled = true;
                }

                if ( pageNumber + 9 >= pageCount )
                {
                    PageLinkListItem[PageLinkListItem.Length - 1].Attributes["class"] = "next disabled";
                    PageLink[PageLinkListItem.Length - 1].Attributes["page-index"] = pageIndex.ToString();
                    PageLink[PageLinkListItem.Length - 1].Enabled = false;
                }
                else
                {
                    PageLinkListItem[PageLinkListItem.Length - 1].Attributes["class"] = "next";
                    PageLink[PageLinkListItem.Length - 1].Attributes["page-index"] = (pageNumber + 10).ToString();
                    PageLink[PageLinkListItem.Length - 1].Enabled = true;
                }


                NavigationPanel.Visible = true;
                for ( int i = 1; i < PageLink.Length - 1; i++ )
                {
                    int currentPage = pageNumber + (i - 1);

                    HtmlGenericControl li = PageLinkListItem[i];
                    LinkButton lb = PageLink[i];

                    if ( currentPage < pageCount )
                    {
                        li.Attributes["class"] = currentPage == pageIndex ? "active" : "";
                        li.Visible = true;
                        
                        lb.Text = ( currentPage + 1 ).ToString( "N0" );
                        lb.Attributes["page-index"] = currentPage.ToString();
                        lb.Visible = true;
                    }
                    else
                    {
                        li.Visible = false;
                        lb.Visible = false;
                    }
                }
            }
            else
            {
                NavigationPanel.Visible = false;
            }

            // Set page size controls
            string pageSizeValue = pageSize == ALL_ITEMS_SIZE ? "All" : pageSize.ToString( "N0" );
            for ( int i = 0; i < ItemLinkListItem.Length; i++ )
                ItemLinkListItem[i].Attributes["class"] = ItemLink[i].Text == pageSizeValue ? "active" : "";
        }

        void lbPage_Click( object sender, EventArgs e )
        {
            if ( NavigateClick != null )
            {
                LinkButton lbPage = sender as LinkButton;
                if ( lbPage != null )
                {
                    int pageIndex = 0;
                    if ( Int32.TryParse( lbPage.Attributes["page-index"], out pageIndex ) )
                    {
                        NumericalEventArgs eventArgs = new NumericalEventArgs( pageIndex );
                        NavigateClick( sender, eventArgs );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPageSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void lbItems_Click( object sender, EventArgs e )
        {
            LinkButton lbItems = sender as LinkButton;
            if ( lbItems != null && ItemsPerPageClick != null)
            {
                int itemsPerPage = ALL_ITEMS_SIZE;

                switch ( lbItems.Text )
                {
                    case "25":
                        itemsPerPage = 25;
                        break;
                    case "100":
                        itemsPerPage = 100;
                        break;
                    case "1,000":
                        itemsPerPage = 1000;
                        break;
                }

                NumericalEventArgs eventArgs = new NumericalEventArgs( itemsPerPage );

                ItemsPerPageClick( sender, eventArgs );
            }
        }

        /// <summary>
        /// Handles the AddClick event of the actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void actions_AddClick( object sender, EventArgs e )
        {
            if (AddClick != null)
                AddClick( sender, e );
        }

        /// <summary>
        /// Occurs when [navigate click].
        /// </summary>
        internal event PageNavigationEventHandler NavigateClick;

        /// <summary>
        /// Occurs when [page click].
        /// </summary>
        internal event PageNavigationEventHandler ItemsPerPageClick;

        /// <summary>
        /// Occurs when [add click].
        /// </summary>
        internal event EventHandler AddClick;
    }

    #endregion
}