//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using AjaxControlToolkit;
using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;  
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A Modal Popup Dialog Window
    /// </summary>
    [ToolboxData( "<{0}:ModalDialog runat=server></{0}:ModalDialog>" )]
    public class ModalIFrameDialog : ModalPopupExtender, INamingContainer
    {
        private Button _dfltShowButton;
        private Panel _dialogPanel;

        private Panel _contentPanel;
        private HtmlGenericControl _iFrame;
        
        /// <summary>
        /// Raises the <see cref="E:Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BackgroundCssClass = "modal-backdrop";
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            base.Controls.Clear();

            _dfltShowButton = new Button();
            base.Controls.Add(_dfltShowButton);
            _dfltShowButton.ID = "default";
            _dfltShowButton.Attributes.Add("style","display:none");

            _dialogPanel = new Panel();
            base.Controls.Add( _dialogPanel );
            _dialogPanel.ID = "panel";
            _dialogPanel.CssClass = "modal";
            _dialogPanel.Attributes.Add("style","display:none");

            _contentPanel = new Panel();
            _dialogPanel.Controls.Add( _contentPanel );
            _contentPanel.ID = "contentPanel";
            _contentPanel.CssClass = "iframe";

            _iFrame = new HtmlGenericControl( "iframe" );
            _iFrame.ID = "iframe";
            _iFrame.Attributes.Add( "scrolling", "no" );
            _contentPanel.Controls.Add( _iFrame );

            this.PopupControlID = _dialogPanel.ID;
        }

        /// <summary>
        /// Raises the <see cref="E:PreRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            // If no target control has been defined, use a hidden default button.
            if ( this.TargetControlID == string.Empty )
                this.TargetControlID = _dfltShowButton.ID;

            base.OnPreRender( e );
        }

    }

}