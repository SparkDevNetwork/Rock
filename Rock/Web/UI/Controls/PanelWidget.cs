//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:PanelWidget runat=server></{0}:PanelWidget>" )]
    public class PanelWidget : PlaceHolder
    {
        #region Properties

        /// <summary>
        /// The hidden field for tracking expanded
        /// </summary>
        private HiddenField _hfExpanded;

        private LinkButton _lbDelete;

        /// <summary>
        /// Gets or sets the Title text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string Title
        {
            get { return ViewState["Title"] as string ?? string.Empty; }
            set { ViewState["Title"] = value; }
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

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('a.view-state > i', this).toggleClass('fa-chevron-down');
    $('a.view-state > i', this).toggleClass('fa-chevron-up');
});


// fix so that the Remove button will fire its event, but not the parent event 
$('.rock-panel-widget a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.rock-panel-widget a.panel-widget-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "RockPanelWidgetScript", script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfExpanded = new HiddenField();
            Controls.Add( _hfExpanded );
            _hfExpanded.ID = this.ID + "_hfExpanded";
            _hfExpanded.Value = "False";

            _lbDelete = new LinkButton();
            _lbDelete.CausesValidation = false;
            _lbDelete.ID = this.ID + "_lbDelete";
            _lbDelete.CssClass = "btn btn-xs btn-danger";
            _lbDelete.Click += lbDelete_Click;
            _lbDelete.Controls.Add( new LiteralControl { Text = "<i class='icon-remove'></i>" } );

            Controls.Add( _lbDelete );
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget rock-panel-widget" );
                writer.RenderBeginTag( "section" );

                // Header
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                writer.RenderBeginTag( "header" );

                // Hidden Field to track expansion
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
                _hfExpanded.RenderControl( writer );

                // Title
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( Title );
                writer.RenderEndTag();
                writer.RenderEndTag();

                // Panel Controls
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( ShowReorderIcon )
                {
                    // Reorder Icon
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-xs panel-widget-reorder" );
                    writer.RenderBeginTag( HtmlTextWriterTag.A );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-reorder" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                // Chevron up/down Button
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-xs view-state" );
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

                // Render placeholder's child controls
                if ( this.Controls != null )
                {
                    var builtInControls = new Control[] { _hfExpanded, _lbDelete };
                    foreach ( Control child in this.Controls )
                    {
                        if ( !builtInControls.Contains( child ) )
                        {
                            child.RenderControl( writer );
                        }
                    }
                }

                writer.RenderEndTag();

                writer.RenderEndTag();  // Section
            }
        }
    }
}