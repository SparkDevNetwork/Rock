//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class PagePicker : ItemPicker, ILabeledControl
    {
        private Label label;
        private HiddenField _hfPageRouteId;
        private HyperLink _btnShowPageRoutePicker;
        private LabeledRadioButtonList _rblSelectPageRoute;
        private LinkButton _btnSelectPageRoute;
        private HyperLink _btnCancelPageRoute;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get
            {
                EnsureChildControls();
                return label.Text;
            }

            set
            {
                EnsureChildControls();
                label.Text = value;
                RequiredErrorMessage = string.IsNullOrWhiteSpace( value ) ? "Page value is required" : value + " is required";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPicker" /> class.
        /// </summary>
        public PagePicker()
            : base()
        {
            this.PromptForPageRoute = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [prompt for page route].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prompt for page route]; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "true" ),
        Description( "If the Page has Page Routes, should the Page Routes be presented for selection?" )
        ]
        public bool PromptForPageRoute
        {
            get
            {
                if ( ViewState["PromptForPageRoute"] != null )
                {
                    return (bool)ViewState["PromptForPageRoute"];
                }

                // default to true
                return true;
            }

            set
            {
                ViewState["PromptForPageRoute"] = value;
                if ( value )
                {
                    this.AllowMultiSelect = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            ItemRestUrlExtraParams = string.Empty;

            string scriptFormat = @"

                $('#{0}').click(function () {{
                    $('#page-route-picker_{3}').toggle();
                }});

                $('#{1}').click(function () {{
                    $(this).parent().slideUp();
                }});

                $('#{2}').click(function () {{
                    $(this).parent().slideUp();
                }});";

            string script = string.Format( scriptFormat, _btnShowPageRoutePicker.ClientID, _btnSelectPageRoute.ClientID, _btnCancelPageRoute.ClientID, this.ClientID );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "page-route-picker-script_" + this.ID, script, true );

            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelectPageRoute );
            }
        }

        /// <summary>
        /// Gets the page unique identifier.
        /// </summary>
        /// <value>
        /// The page unique identifier.
        /// </value>
        public int? PageId
        {
            get
            {
                return this.ItemId.AsInteger();
            }
        }
        
        /// <summary>
        /// Gets or sets the page route unique identifier.
        /// </summary>
        /// <value>
        /// The page route unique identifier.
        /// </value>
        public int? PageRouteId
        {
            get
            {
                EnsureChildControls();
                return _hfPageRouteId.Value.AsInteger();
            }
            
            set
            {
                EnsureChildControls();
                _hfPageRouteId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Determines whether [is page route].
        /// </summary>
        /// <returns></returns>
        public bool IsPageRoute
        {
            get
            {
                return ( PageRouteId ?? 0 ) > Rock.Constants.None.Id;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="pageRoute">The page route.</param>
        public void SetValue( Rock.Model.PageRoute pageRoute )
        {
            if ( pageRoute != null )
            {
                Rock.Model.Page page = pageRoute.Page;
                PageRouteId = pageRoute.Id;
                ItemId = page.Id.ToString();

                string parentPageIds = string.Empty;
                var parentPage = page.ParentPage;
                while ( parentPage != null )
                {
                    parentPageIds = parentPage.Id + "," + parentPageIds;
                    parentPage = parentPage.ParentPage;
                }

                InitialItemParentIds = parentPageIds.TrimEnd( new char[] { ',' } );
                if ( pageRoute.Id != 0 )
                {
                    // PageRoute is selected, so show the Page and it's PageRoute and don't show the PageRoute picker
                    ItemName = page.Name + " (" + pageRoute.Route + ")";
                    
                    _rblSelectPageRoute.Visible = false;
                    _btnShowPageRoutePicker.Visible = false;
                }
                else
                {
                    // Only a Page is selected, so show PageRoutePicker button if it has page routes
                    ItemName = page.Name;
                    PageRouteId = null;

                    // Update PageRoutePicker control values
                    _rblSelectPageRoute.Items.Clear();
                    _rblSelectPageRoute.Visible = page.PageRoutes.Any();

                    if ( page.PageRoutes.Count > 0 )
                    {
                        foreach ( var item in page.PageRoutes )
                        {
                            _rblSelectPageRoute.Items.Add( new ListItem( item.Route, item.Id.ToString() ) );
                        }
                    }

                    _btnShowPageRoutePicker.Visible = _rblSelectPageRoute.Items.Count > 0;
                    if ( _rblSelectPageRoute.Items.Count == 1 )
                    {
                        _btnShowPageRoutePicker.Text = "( 1 route exists )";
                    }
                    else
                    {
                        _btnShowPageRoutePicker.Text = "(" + _rblSelectPageRoute.Items.Count + " routes exist )";
                    }
                }
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
                PageRouteId = null;
                _rblSelectPageRoute.Visible = false;
                _btnShowPageRoutePicker.Visible = false;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="page">The page.</param>
        public void SetValue( Rock.Model.Page page )
        {
            if ( page != null )
            {
                SetValue( new PageRoute { Page = page } );
            }
            else
            {
                SetValue( null as PageRoute );
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="pages">The pages.</param>
        public void SetValues( IEnumerable<Rock.Model.Page> pages )
        {
            var thePages = pages.ToList();

            if ( thePages.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentPageIds = string.Empty;

                foreach ( var page in thePages )
                {
                    if ( page != null )
                    {
                        ids.Add( page.Id.ToString() );
                        names.Add( page.Name );
                        var parentPage = page.ParentPage;

                        while ( parentPage != null )
                        {
                            parentPageIds += parentPage.Id.ToString() + ",";
                            parentPage = parentPage.ParentPage;
                        }
                    }
                }

                InitialItemParentIds = parentPageIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void SetValueOnSelect()
        {
            var page = new PageService().Get( int.Parse( ItemId ) );

            this.SetValue( page );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var pages = new PageService().Queryable().Where( p => ItemIds.Contains( p.Id.ToString() ) );
            this.SetValues( pages );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/pages/getchildren/"; }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            label = new Label();

            _hfPageRouteId = new HiddenField();
            _hfPageRouteId.ClientIDMode = ClientIDMode.Static;
            _hfPageRouteId.ID = string.Format( "hfPageRouteId_{0}", this.ID );

            _btnShowPageRoutePicker = new HyperLink();
            _btnShowPageRoutePicker.CssClass = "btn btn-mini";
            _btnShowPageRoutePicker.ID = string.Format( "btnShowPageRoutePicker_{0}", this.ID );
            _btnShowPageRoutePicker.Text = "Pick Route";
            _btnShowPageRoutePicker.Visible = false;

            _rblSelectPageRoute = new LabeledRadioButtonList();
            _rblSelectPageRoute.ID = "rblSelectPageRoute_" + this.ID;
            _rblSelectPageRoute.Visible = false;
            _rblSelectPageRoute.EnableViewState = true;

            _btnSelectPageRoute = new LinkButton();
            _btnSelectPageRoute.CssClass = "btn btn-mini btn-primary";
            _btnSelectPageRoute.ID = string.Format( "btnSelectPageRoute_{0}", this.ID );
            _btnSelectPageRoute.Text = "Select";
            _btnSelectPageRoute.CausesValidation = false;
            _btnSelectPageRoute.Click += _btnSelectPageRoute_Click;

            _btnCancelPageRoute = new HyperLink();
            _btnCancelPageRoute.CssClass = "btn btn-mini";
            _btnCancelPageRoute.ID = string.Format( "btnCancelPageRoute_{0}", this.ID );
            _btnCancelPageRoute.Text = "Cancel";

            Controls.Add( label );
            Controls.Add( _hfPageRouteId );
            Controls.Add( _rblSelectPageRoute );
            Controls.Add( _btnShowPageRoutePicker );
            Controls.Add( _btnSelectPageRoute );
            Controls.Add( _btnCancelPageRoute );
        }

        /// <summary>
        /// Handles the Click event of the _btnSelectPageRoute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _btnSelectPageRoute_Click( object sender, EventArgs e )
        {
            _rblSelectPageRoute.Visible = false;
            
            // pluck the selectedValueId of the Page Params in case the ViewState is shut off
            int selectedValueId = this.Page.Request.Params[_rblSelectPageRoute.UniqueID].AsInteger() ?? 0;
            PageRoute pageRoute = new PageRouteService().Get(selectedValueId);
            SetValue( pageRoute );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( string.IsNullOrEmpty( LabelText ) )
            {
                base.Render( writer );

                RenderPageRoutePicker( writer );
            }
            else
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                label.AddCssClass( "control-label" );

                label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                base.Render( writer );

                RenderPageRoutePicker( writer );

                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Renders the page route picker.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderPageRoutePicker( HtmlTextWriter writer )
        {
            // don't show the PageRoutePicker if this control is not enabled (readonly)
            if ( this.Enabled )
            {
                // this might be a PagePicker where we don't want them to choose a PageRoute (for example, the PageRoute detail block)
                if ( PromptForPageRoute )
                {
                    _hfPageRouteId.RenderControl( writer );

                    _btnShowPageRoutePicker.RenderControl( writer );

                    writer.Write( string.Format( @"<div id='page-route-picker_{0}' class='dropdown-menu rock-picker page-route-picker'>", this.ClientID ) );

                    _rblSelectPageRoute.RenderControl( writer );

                    writer.Write( @"<hr />" );

                    _btnSelectPageRoute.RenderControl( writer );
                    writer.WriteLine();
                    _btnCancelPageRoute.RenderControl( writer );
                    writer.Write( @"</div>" );
                }
            }
        }
    }
}