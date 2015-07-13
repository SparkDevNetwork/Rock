// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class PagePicker : ItemPicker
    {
        private HiddenField _hfPageRouteId;
        private HyperLink _btnShowPageRoutePicker;
        private RockRadioButtonList _rblSelectPageRoute;
        private LinkButton _btnSelectPageRoute;
        private HyperLink _btnCancelPageRoute;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagePicker" /> class.
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
        /// Gets or sets the hidden page ids.
        /// </summary>
        /// <value>
        /// The hidden page ids.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        Description( "List of PageIds that should not be shown" )
        ]
        public int[] HiddenPageIds
        {
            get
            {
                int[] result = ViewState["HiddenPageIds"] as int[] ?? new int[0] { };
                return result;
            }

            set
            {
                ViewState["HiddenPageIds"] = value;
                if ( value != null && value.Length > 0 )
                {
                    this.ItemRestUrlExtraParams = "?hidePageIds=" + System.Web.HttpUtility.UrlEncode( value.ToList().AsDelimited( "," ) );
                }
                else
                {
                    this.ItemRestUrlExtraParams = string.Empty;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = string.Empty;
            base.OnInit( e );

            this.IconCssClass = "fa fa-file";

            string scriptFormat = @"

                $('#{0}').click(function () {{
                    $('#page-route-picker_{3}').find('.js-page-route-picker-menu').toggle(function () {{
                        Rock.dialogs.updateModalScrollBar('page-route-picker_{3}');
                    }});
                }});

                $('#{1}').click(function () {{
                    $(this).closest('.picker-menu').slideUp();
                }});

                $('#{2}').click(function () {{
                    $(this).closest('.picker-menu').slideUp();
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
                if ( this.ItemId == None.IdValue )
                {
                    return null;
                }
                else
                {
                    return this.ItemId.AsIntegerOrNull();
                }
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
                return _hfPageRouteId.Value.AsIntegerOrNull();
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
                    // PageRoute is selected, so show the Page and its PageRoute and don't show the PageRoute picker
                    ItemName = page.InternalName + " (" + pageRoute.Route + ")";

                    _rblSelectPageRoute.Visible = false;
                    _btnShowPageRoutePicker.Style[HtmlTextWriterStyle.Display] = "none";
                }
                else
                {
                    // Only a Page is selected, so show PageRoutePicker button if it has page routes
                    ItemName = page.InternalName;
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

                    if ( _rblSelectPageRoute.Items.Count > 0 )
                    {
                        _btnShowPageRoutePicker.Style[HtmlTextWriterStyle.Display] = "";
                    }
                    else
                    {
                        _btnShowPageRoutePicker.Style[HtmlTextWriterStyle.Display] = "none";
                    }

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
                _btnShowPageRoutePicker.Style[HtmlTextWriterStyle.Display] = "none";
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
                        names.Add( page.InternalName );
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
        protected override void SetValueOnSelect()
        {
            var page = new PageService( new RockContext() ).Get( int.Parse( ItemId ) );

            this.SetValue( page );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var pages = new PageService( new RockContext() ).Queryable().Where( p => ItemIds.Contains( p.Id.ToString() ) );
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

            _hfPageRouteId = new HiddenField();
            _hfPageRouteId.ID = string.Format( "hfPageRouteId_{0}", this.ID );

            _btnShowPageRoutePicker = new HyperLink();
            _btnShowPageRoutePicker.CssClass = "btn btn-xs btn-link js-hide-on-select-none";
            _btnShowPageRoutePicker.ID = string.Format( "btnShowPageRoutePicker_{0}", this.ID );
            _btnShowPageRoutePicker.Text = "Pick Route";
            _btnShowPageRoutePicker.Style[HtmlTextWriterStyle.Display] = "none";

            _rblSelectPageRoute = new RockRadioButtonList();
            _rblSelectPageRoute.ID = "rblSelectPageRoute_" + this.ID;
            _rblSelectPageRoute.Visible = false;
            _rblSelectPageRoute.EnableViewState = true;

            _btnSelectPageRoute = new LinkButton();
            _btnSelectPageRoute.CssClass = "btn btn-xs btn-primary";
            _btnSelectPageRoute.ID = string.Format( "btnSelectPageRoute_{0}", this.ID );
            _btnSelectPageRoute.Text = "Select";
            _btnSelectPageRoute.CausesValidation = false;
            _btnSelectPageRoute.Click += _btnSelectPageRoute_Click;

            _btnCancelPageRoute = new HyperLink();
            _btnCancelPageRoute.CssClass = "btn btn-link btn-xs";
            _btnCancelPageRoute.ID = string.Format( "btnCancelPageRoute_{0}", this.ID );
            _btnCancelPageRoute.Text = "Cancel";

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
            int selectedValueId = this.Page.Request.Params[_rblSelectPageRoute.UniqueID].AsInteger();
            PageRoute pageRoute = new PageRouteService( new RockContext() ).Get( selectedValueId );
            SetValue( pageRoute );
        }

        /// <summary>
        /// This is where you implment the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            base.RenderBaseControl( writer );

            // don't show the PageRoutePicker if this control is not enabled (readonly)
            if ( this.Enabled )
            {
                // this might be a PagePicker where we don't want them to choose a PageRoute (for example, the PageRoute detail block)
                if ( PromptForPageRoute )
                {
                    _hfPageRouteId.RenderControl( writer );

                    _btnShowPageRoutePicker.RenderControl( writer );

                    writer.Write( string.Format( @"<div id='page-route-picker_{0}' class='picker'>", this.ClientID ) );
                    writer.Write( @"<div class='picker-menu picker dropdown-menu js-page-route-picker-menu'>" );

                    _rblSelectPageRoute.RenderControl( writer );

                    writer.Write( @"<div class='picker-actions'>" );

                    _btnSelectPageRoute.RenderControl( writer );
                    writer.WriteLine();
                    _btnCancelPageRoute.RenderControl( writer );
                    writer.Write( "</div>" );
                    writer.Write( @"</div>" );
                    writer.Write( @"</div>" );
                }
            }
        }
    }
}