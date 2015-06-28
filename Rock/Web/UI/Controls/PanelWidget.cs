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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:PanelWidget runat=server></{0}:PanelWidget>" )]
    public class PanelWidget : PlaceHolder, INamingContainer
    {
        #region Properties

        /// <summary>
        /// The hidden field for tracking expanded
        /// </summary>
        private HiddenField _hfExpanded;

        /// <summary>
        /// The title label
        /// </summary>
        private HiddenFieldWithClass _hfTitle;

        /// <summary>
        /// The hidden field to tell validator to disable vrm for _hfTitle
        /// </summary>
        private HiddenField _hfTitleDisableVrm;

        /// <summary>
        /// The delete button
        /// </summary>
        private LinkButton _lbDelete;

        /// <summary>
        /// Gets or sets the Title text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string Title
        {
            get
            {
                EnsureChildControls();
                return HttpUtility.HtmlDecode( _hfTitle.Value );
            }
            set
            {
                EnsureChildControls();
                _hfTitle.Value = HttpUtility.HtmlEncode( value );
            }
        }

        /// <summary>
        /// Gets or sets the title icon CSS class.
        /// </summary>
        /// <value>
        /// The title icon CSS class.
        /// </value>
        public string TitleIconCssClass
        {
            get
            {
                return ViewState["TitleIconCssClass"] as string ?? string.Empty;
            }

            set
            {
                ViewState["TitleIconCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show reorder icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show reorder icon]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowReorderIcon
        {
            get
            {
                return ViewState["ShowReorderIcon"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowReorderIcon"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show delete button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show delete button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDeleteButton
        {
            get
            {
                return ViewState["ShowDeleteButton"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowDeleteButton"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PanelWidget" /> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();

                bool expanded = false;
                if ( !bool.TryParse( _hfExpanded.Value, out expanded ) )
                {
                    expanded = false;
                }

                return expanded;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get { return ViewState["CssClass"] as string ?? string.Empty; }
            set { ViewState["CssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the header controls.
        /// </summary>
        /// <value>
        /// The header controls.
        /// </value>
        public Control[] HeaderControls { private get; set; }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();
            base.OnInit( e );

            string script = @"
// activity animation
$('.rock-panel-widget > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    if ( $(this).find('.js-header-controls').length ) {
        $(this).find('.js-header-title').slideToggle();
        $(this).find('.js-header-controls').slideToggle();
    }

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('a.view-state > i', this).toggleClass('fa-chevron-down');
    $('a.view-state > i', this).toggleClass('fa-chevron-up');
});

// fix so that certain controls will fire its event, but not the parent event 
$('.js-stop-immediate-propagation').click(function (event) {
    event.stopImmediatePropagation();
});

";
            ScriptManager.RegisterStartupScript( this, typeof( PanelWidget ), "RockPanelWidgetScript", script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfExpanded = new HiddenField();
            _hfExpanded.ID = "_hfExpanded";
            Controls.Add( _hfExpanded );
            _hfExpanded.Value = "False";

            _hfTitle = new HiddenFieldWithClass();
            _hfTitle.ValidateRequestMode = System.Web.UI.ValidateRequestMode.Disabled;
            _hfTitle.ID = "_hfTitle";
            _hfTitle.CssClass = "js-header-title-hidden";
            Controls.Add( _hfTitle );

            _hfTitleDisableVrm = new HiddenField();
            _hfTitleDisableVrm.ID = _hfTitle.ID + "_dvrm";
            _hfTitleDisableVrm.Value = "True";
            Controls.Add( _hfTitleDisableVrm ); 

            _lbDelete = new LinkButton();
            _lbDelete.ID = "_lbDelete";
            Controls.Add( _lbDelete );
            _lbDelete.CausesValidation = false;
            _lbDelete.CssClass = "btn btn-xs btn-danger js-stop-immediate-propagation";
            _lbDelete.Click += lbDelete_Click;
            _lbDelete.Controls.Add( new LiteralControl { Text = "<i class='fa fa fa-times'></i>" } );
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, System.EventArgs e )
        {
            if ( DeleteClick != null )
            {
                DeleteClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete click].
        /// </summary>
        public event EventHandler DeleteClick;

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                // Section
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget rock-panel-widget " + CssClass );
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
                writer.RenderBeginTag( "section" );

                // Header
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                writer.RenderBeginTag( "header" );

                // Hidden Field to track expansion
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
                _hfExpanded.RenderControl( writer );

                /* Begin - Title and header controls */
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Hidden Field to track Title
                _hfTitle.RenderControl( writer );
                _hfTitleDisableVrm.RenderControl( writer );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "js-header-title" );
                if ( Expanded )
                {
                    // if the panel is expanded and there are special HeaderControls to show instead of the Title, hide the title (and the header controls will be shown instead)
                    if ( this.HeaderControls != null )
                    {
                        writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                    }
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Title
                if ( !string.IsNullOrWhiteSpace( TitleIconCssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, TitleIconCssClass );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    writer.Write( " " );
                }

                // also write out the title (also stored in _hfTitle.value)
                writer.Write( Title );

                writer.RenderEndTag();

                // Header Controls
                if ( this.HeaderControls != null )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-item-select js-header-controls" );
                    if ( !Expanded )
                    {
                        writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                    }

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    foreach ( var control in this.HeaderControls )
                    {
                        if ( !this.Controls.Contains( control ) )
                        {
                            this.Controls.Add( control );
                        }

                        if ( control is WebControl )
                        {
                            ( control as WebControl ).AddCssClass( "js-stop-immediate-propagation" );
                        }

                        control.RenderControl( writer );
                    }

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();

                /* End - Title and Header Controls */

                // Panel Controls
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                RenderLabels( writer );

                if ( ShowReorderIcon )
                {
                    // Reorder Icon
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-link btn-xs panel-widget-reorder js-stop-immediate-propagation" );
                    writer.RenderBeginTag( HtmlTextWriterTag.A );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-bars" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                // Chevron up/down Button
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-link btn-xs view-state" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();

                if ( ShowDeleteButton )
                {
                    _lbDelete.Visible = true;
                    _lbDelete.RenderControl( writer );
                }
                else
                {
                    _lbDelete.Visible = false;
                }

                writer.RenderEndTag(); // pull-right

                writer.RenderEndTag(); // Header                

                // Body
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
                if ( !Expanded )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                RenderChildControls( writer );

                writer.RenderEndTag();

                writer.RenderEndTag();  // Section
            }
        }

        /// <summary>
        /// Renders the child controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderChildControls( HtmlTextWriter writer )
        {
            // Render placeholder's child controls
            if ( this.Controls != null )
            {
                List<Control> alreadyRenderedControls = new List<Control>();
                alreadyRenderedControls.Add( _hfExpanded );
                alreadyRenderedControls.Add( _hfTitle );
                alreadyRenderedControls.Add( _hfTitleDisableVrm );
                alreadyRenderedControls.Add( _lbDelete );
                if ( this.HeaderControls != null )
                {
                    alreadyRenderedControls.AddRange( HeaderControls );
                }

                foreach ( Control child in this.Controls )
                {
                    if ( !alreadyRenderedControls.Contains( child ) )
                    {
                        child.RenderControl( writer );
                    }
                }
            }
        }

        /// <summary>
        /// Renders the labels.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderLabels( HtmlTextWriter writer)
        {
        }
    }
}