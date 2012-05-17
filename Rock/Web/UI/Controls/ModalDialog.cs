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
    public class ModalDialog : ModalPopupExtender
    {
        private Button _dfltShowButton;
        private Panel _dialogPanel;

        private Panel _headerPanel;
        private HtmlGenericControl _titleH3;
        private LiteralControl _title;

        private Panel _contentPanel;
        
        private Panel _footerPanel;
        private Button _okButton;
        private Button _cancelButton;

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
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [
            Category( "Appearance" ),
            DefaultValue( "" ),
            Description( "The title of the dialog." )
        ]
        public string Title
        {
            get
            {
                EnsureChildControls();
                return _title.Text;
            }
            set
            {
                EnsureChildControls();
                _title.Text = value;
            }
        }


        /// <summary>
        /// The content of the popup.
        /// </summary>
        [
            DesignerSerializationVisibility( DesignerSerializationVisibility.Content ), 
            NotifyParentProperty( true ), 
            PersistenceMode( PersistenceMode.InnerProperty ),
            Category( "Data" ),
            DefaultValue( typeof( Panel ), "" ),
            Description( "The Contents of the dialog." )
        ]
        public Panel Content
        {
            get 
            {
                EnsureChildControls();
                return _contentPanel; 
            }
        
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
            _dfltShowButton.ID = "dfltButton";
            _dfltShowButton.Attributes.Add("style","display:none");

            _dialogPanel = new Panel();
            base.Controls.Add( _dialogPanel );
            _dialogPanel.ID = "dialogPanel";
            _dialogPanel.CssClass = "modal2";

            _headerPanel = new Panel();
            _dialogPanel.Controls.Add( _headerPanel );
            _headerPanel.ID = "headerPanel";
            _headerPanel.CssClass = "modal-header";

            _contentPanel = new Panel();
            _dialogPanel.Controls.Add( _contentPanel );
            _contentPanel.ID = "contentPanel";
            _contentPanel.CssClass = "modal-body";

            _footerPanel = new Panel();
            _dialogPanel.Controls.Add( _footerPanel );
            _footerPanel.ID = "footerPanel";
            _footerPanel.CssClass = "modal-footer";

            _titleH3 = new HtmlGenericControl( "h3" );
            _headerPanel.Controls.Add( _titleH3 );

            _title = new LiteralControl();
            _titleH3.Controls.Add( _title );

            _okButton = new Button();
            _footerPanel.Controls.Add( _okButton );
            _okButton.ID = "okButton";
            _okButton.CssClass = "btn primary";
            _okButton.Text = "Save";
            _okButton.Click += new EventHandler( SaveButton_Click );

            _cancelButton = new Button();
            _footerPanel.Controls.Add( _cancelButton );
            _cancelButton.ID = "cancelButton";
            _cancelButton.CssClass = "btn secondary";
            _cancelButton.Text = "Cancel";

            this.PopupControlID = _dialogPanel.ID;
            this.CancelControlID = _cancelButton.ID;
        }

        /// <summary>
        /// Raises the <see cref="E:PreRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            // If no event handler has been defined for the save button, use the default client behavior
            if ( SaveClick == null )
                this.OkControlID = _okButton.ID;

            // If no target control has been defined, use a hidden default button.
            if ( this.TargetControlID == string.Empty )
                this.TargetControlID = _dfltShowButton.ID;

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void SaveButton_Click( object sender, EventArgs e )
        {
            if ( SaveClick != null )
                SaveClick( sender, e );
        }

        /// <summary>
        /// Occurs when the save button is clicked.
        /// </summary>
        public event EventHandler SaveClick;

    }

}