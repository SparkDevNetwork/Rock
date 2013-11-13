//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:WidgetPanel runat=server></{0}:WidgetPanel>" )]
    public class WidgetPanel : PlaceHolder
    {
        #region Properties

        /// <summary>
        /// The hidden field for tracking expanded
        /// </summary>
        protected HiddenField hfExpanded;

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
        /// Gets or sets a value indicating whether this <see cref="WidgetPanel" /> is expanded.
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
                if ( !bool.TryParse( hfExpanded.Value, out expanded ) )
                    expanded = false;
                return expanded;
            }
            set
            {
                EnsureChildControls();
                hfExpanded.Value = value.ToString();
            }
        }

        #endregion

        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// activity animation
$('.rock-widget-panel > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('a.view-state > i', this).toggleClass('icon-chevron-down');
    $('a.view-state > i', this).toggleClass('icon-chevron-up');
});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "RockWidgetPanelScript", script, true );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            hfExpanded = new HiddenField();
            Controls.Add( hfExpanded );
            hfExpanded.ID = this.ID + "_hfExpanded";
            hfExpanded.Value = "False";
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                // Section
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget rock-widget-panel" );
                writer.RenderBeginTag( "section" );

                // Header
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                writer.RenderBeginTag( "header" );

                // Hidden Field to track expansion
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
                hfExpanded.RenderControl( writer );

                // Title
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( Title );
                writer.RenderEndTag();
                writer.RenderEndTag();

                // Chevron
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-xs view-state" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, Expanded ? "icon-chevron-up" : "icon-chevron-down" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();  // Header                
                
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
                    foreach ( Control child in this.Controls )
                    {
                        if ( child != hfExpanded )
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