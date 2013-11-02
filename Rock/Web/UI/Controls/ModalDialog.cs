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
    public class ModalDialog : ModalPopupExtender, INamingContainer
    {
        private Button _dfltShowButton;
        private Panel _dialogPanel;

        private Panel _headerPanel;
        private HtmlGenericControl _closeLink;
        private HtmlGenericControl _titleH3;
        private LiteralControl _title;

        private Panel _contentPanel;

        private Panel _footerPanel;
        private HtmlAnchor _serverSaveLink;
        private HtmlAnchor _saveLink;
        private HtmlAnchor _cancelLink;

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
        /// Gets or sets the save button text.
        /// </summary>
        /// <value>The save button text.</value>
        public string SaveButtonText
        {
            get { return ViewState["SaveButtonText"] as string ?? "Save"; }
            set { ViewState["SaveButtonText"] = value; }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }


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
            base.Controls.Add( _dfltShowButton );
            _dfltShowButton.ID = "default";
            _dfltShowButton.Attributes.Add( "style", "display:none" );

            _dialogPanel = new Panel();
            base.Controls.Add( _dialogPanel );
            _dialogPanel.ID = "panel";
            _dialogPanel.CssClass = "rock-modal rock-modal-frame";
            _dialogPanel.Attributes.Add( "style", "display:none" );

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

            _closeLink = new HtmlGenericControl( "A" );
            _headerPanel.Controls.Add( _closeLink );
            _closeLink.ID = "closeLink";
            _closeLink.Attributes.Add( "HRef", "#" );
            _closeLink.Attributes.Add( "class", "close" );
            _closeLink.InnerHtml = "&times;";

            _titleH3 = new HtmlGenericControl( "h3" );
            _titleH3.AddCssClass( "modal-title" );
            _headerPanel.Controls.Add( _titleH3 );

            _title = new LiteralControl();
            _title.Text = string.Empty;
            _titleH3.Controls.Add( _title );

            _cancelLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _cancelLink );
            _cancelLink.ID = "cancelLink";
            _cancelLink.Attributes.Add( "class", "btn" );
            _cancelLink.InnerText = "Cancel";

            _serverSaveLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _serverSaveLink );
            _serverSaveLink.ID = "serverSaveLink";
            _serverSaveLink.Attributes.Add( "class", "btn btn-primary" );
            _serverSaveLink.ValidationGroup = this.ValidationGroup;
            _serverSaveLink.ServerClick += SaveLink_ServerClick;

            _saveLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _saveLink );
            _saveLink.ID = "saveLink";
            _saveLink.Attributes.Add( "class", "btn btn-primary modaldialog-save-button" );
            _saveLink.ValidationGroup = this.ValidationGroup;

            this.PopupControlID = _dialogPanel.ID;
            this.CancelControlID = _cancelLink.ID;
        }

        /// <summary>
        /// Raises the <see cref="E:PreRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            _closeLink.Attributes["onclick"] = string.Format(
                "{0} $find('{1}').hide();return false;", this.OnCancelScript, this.BehaviorID );

            _serverSaveLink.Visible = SaveClick != null;
            _serverSaveLink.InnerText = SaveButtonText;

            _saveLink.Visible = SaveClick == null && !( string.IsNullOrWhiteSpace( OnOkScript ) );
            _saveLink.InnerText = SaveButtonText;

            if ( !_serverSaveLink.Visible )
            {
                if ( _saveLink.Visible )
                {
                    this.OkControlID = _saveLink.ID;
                }
                else
                {
                    _cancelLink.InnerText = "Ok";
                }
            }

            // If no target control has been defined, use a hidden default button.
            if ( this.TargetControlID == string.Empty )
                this.TargetControlID = _dfltShowButton.ID;

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the ServerClick event of the SaveLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void SaveLink_ServerClick( object sender, EventArgs e )
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