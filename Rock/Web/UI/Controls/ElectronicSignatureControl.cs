// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for Drawing or Typing an Electronic Signature
    /// </summary>
    public class ElectronicSignatureControl : CompositeControl, INamingContainer
    {
        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string DocumentTerm = "DocumentTerm";
            public const string SignatureType = "SignatureType";
            public const string DrawnSignatureImageMimeType = "DrawnSignatureImageMimeType";
            public const string PromptForEmailAddress = "PromptForEmailAddress";
            public const string ShowNameOnCompletionStepWhenInTypedSignatureMode = "ShowNameOnCompletionStepWhenInTypedSignatureMode";
        }

        #endregion ViewState Keys

        #region Controls

        private HiddenFieldWithClass _hfSignatureImageDataUrl;
        private Panel _pnlSignatureEntry;
        private Panel _pnlSignatureEntryDrawn;
        private Literal _lSignaturePadCanvas;
        private Panel _pnlSignatureEntryTyped;
        private RockTextBox _tbSignatureTyped;
        private Literal _lSignatureSignDisclaimer;
        private BootstrapButton _btnSignSignature;
        private Literal _clearSignatureLink;
        private CustomValidator _signatureValidator;
        private ValidationSummary _signatureValidationSummary;

        private Panel _pnlSignatureComplete;
        private RockLiteral _lCompletionSignedName;
        private RockTextBox _tbLegalName;
        private EmailBox _ebEmailAddress;
        private BootstrapButton _btnCompleteSignature;
        private ValidationSummary _completionValidationSummary;

        #endregion Controls

        /// <inheritdoc cref="SignatureDocumentTemplate.SignatureType"/>
        public SignatureType SignatureType
        {
            get
            {
                return this.ViewState[ViewStateKey.SignatureType] as SignatureType? ?? SignatureType.Typed;
            }

            set
            {
                this.ViewState[ViewStateKey.SignatureType] = value;
                EnsureChildControls();
                UpdateUIControls();
            }
        }

        /// <summary>
        /// If this is true, the 'Signed Name' will be shown in the completion step
        /// </summary>
        /// <value><c>true</c> if [always prompt for legal name]; otherwise, <c>false</c>.</value>
        public bool ShowNameOnCompletionStepWhenInTypedSignatureMode
        {
            get => this.ViewState[ViewStateKey.ShowNameOnCompletionStepWhenInTypedSignatureMode] as bool? ?? false;
            set
            {
                this.ViewState[ViewStateKey.ShowNameOnCompletionStepWhenInTypedSignatureMode] = value;
                EnsureChildControls();
                UpdateUIControls();
            }
        }

        /// <inheritdoc cref="SignatureDocumentTemplate.DocumentTerm"/>
        public string DocumentTerm
        {
            get => this.ViewState[ViewStateKey.DocumentTerm] as string;

            set
            {
                this.ViewState[ViewStateKey.DocumentTerm] = value;
                EnsureChildControls();
                UpdateUIControls();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EmailAddressPromptType" />.
        /// You'll probably want to set this to <see cref="EmailAddressPromptType.CompletionEmail" /> if <see cref="SignatureDocumentTemplate.CompletionSystemCommunicationId" /> is set.
        /// </summary>
        /// <value><c>true</c> if [prompt for email address]; otherwise, <c>false</c>.</value>
        public EmailAddressPromptType EmailAddressPrompt
        {
            get => this.ViewState[ViewStateKey.PromptForEmailAddress] as EmailAddressPromptType? ?? EmailAddressPromptType.CompletionEmail;
            set
            {
                this.ViewState[ViewStateKey.PromptForEmailAddress] = value;
                EnsureChildControls();
                UpdateUIControls();
            }
        }

        /// <summary>
        /// Enum EmailAddressPromptType
        /// </summary>
        public enum EmailAddressPromptType
        {
            /// <summary>
            /// Email label will say that it'll be used to send a copy of the signed document to.
            /// </summary>
            CompletionEmail,

            /// <summary>
            /// Email label will just say "Please enter an email address"
            /// </summary>
            PersonEmail
        }

        /// <summary>
        /// Gets or sets the image mime type (<c>image/png, image/jpeg, image/svg+xml</c>) to do used capturing the signature image data. Defaults to <c>image/png</c>.
        /// See also <see cref="DrawnSignatureImageDataUrl"/>
        /// </summary>
        public string DrawnSignatureImageMimeType
        {
            get
            {
                return this.ViewState[ViewStateKey.DrawnSignatureImageMimeType] as string ?? "image/png";
            }

            set
            {
                this.ViewState[ViewStateKey.DrawnSignatureImageMimeType] = value;
            }
        }

        /// <summary>
        /// Gets the signature image data URL when in <see cref="SignatureType.Drawn"/> mode. After drawing a signature,
        /// this would be an img URL in Base64 format
        /// </summary>
        /// <value>The signature image data URL.</value>
        public string DrawnSignatureImageDataUrl
        {
            get
            {
                EnsureChildControls();
                return _hfSignatureImageDataUrl.Value;
            }
        }

        /// <summary>
        /// Gets the typed signature text when in <see cref="SignatureType.Typed" /> mode.
        /// Or the typed Legal Name when in in <see cref="SignatureType.Drawn" /> mode.
        /// </summary>
        /// <value>The typed signature text.</value>
        public string SignedName
        {
            get
            {
                EnsureChildControls();
                if ( SignatureType == SignatureType.Typed )
                {
                    return _tbSignatureTyped.Text;
                }
                else
                {
                    return _tbLegalName.Text;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Person's Legal Name which is shown on the Confirm step
        /// when in in <see cref="SignatureType.Drawn" /> mode.
        /// </summary>
        /// <value>The name of the legal.</value>
        public string LegalName
        {
            get
            {
                EnsureChildControls();
                return _tbLegalName.Text;
            }

            set
            {
                EnsureChildControls();
                _tbLegalName.Text = value;
            }
        }

        /// <summary>
        /// Gets/Sets the email address that the person entered when signing.
        /// </summary>
        /// <value>The email address.</value>
        public string SignedByEmail
        {
            get
            {
                EnsureChildControls();
                return _ebEmailAddress.Text;
            }

            set
            {
                EnsureChildControls();
                _ebEmailAddress.Text = value;
            }
        }

        /// <summary>
        /// Updates the UI controls based on configuration options
        /// </summary>
        private void UpdateUIControls()
        {
            var documentTermDisplay = DocumentTerm?.ToLower();
            if ( documentTermDisplay.IsNullOrWhiteSpace() )
            {
                documentTermDisplay = "document";
            }

            _lSignatureSignDisclaimer.Text = $"<div class='signature-entry-agreement'>By clicking the sign button below, I agree to the above {documentTermDisplay} and understand this is a legal representation of my signature.</div>";

            _ebEmailAddress.Label = $"Please enter an email address below where we can send a copy of the {documentTermDisplay} to.";

            _pnlSignatureEntryDrawn.Visible = SignatureType == SignatureType.Drawn;
            _pnlSignatureEntryTyped.Visible = SignatureType == SignatureType.Typed;

            // Always prompt for LegalName prompt when in Drawn mode.
            // When in Typed mode, we'll use the Typed Name (on _pnlSignatureEntryTyped panel)
            if ( SignatureType == SignatureType.Drawn )
            {
                _tbLegalName.Visible = true;
                _lCompletionSignedName.Visible = false;
            }
            else
            {
                _tbLegalName.Visible = false;
                _lCompletionSignedName.Visible = ShowNameOnCompletionStepWhenInTypedSignatureMode;
            }

            if ( EmailAddressPrompt == EmailAddressPromptType.CompletionEmail )
            {
                _ebEmailAddress.Label = $"Please enter an email address below where we can send a copy of the {documentTermDisplay} to.";
            }
            else if ( EmailAddressPrompt == EmailAddressPromptType.PersonEmail )
            {
                _ebEmailAddress.Label = "Please enter an email address below";
            }
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            if ( !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                RockPage.AddScriptLink( Page, "~/Scripts/signature_pad/signature_pad.umd.min.js" );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Have this control render as a div instead of a span
        /// </summary>
        /// <value>The tag key.</value>
        protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            var validationGroup = $"vgElectronicSignatureControl_{this.ID}";

            _hfSignatureImageDataUrl = new HiddenFieldWithClass();
            _hfSignatureImageDataUrl.ID = "_hfSignatureImageDataUrl";
            _hfSignatureImageDataUrl.CssClass = "js-signature-data";
            Controls.Add( _hfSignatureImageDataUrl );

            _pnlSignatureEntry = new Panel();
            _pnlSignatureEntry.ID = "_pnlSignatureEntry";
            _pnlSignatureEntry.CssClass = "signature-entry";

            Controls.Add( _pnlSignatureEntry );

            // this is what will display validation message from _signatureValidator for the signing step
            _signatureValidationSummary = new ValidationSummary();
            _signatureValidationSummary.ID = "_signatureValidationSummary";
            _signatureValidationSummary.ValidationGroup = validationGroup;
            _signatureValidationSummary.CssClass = "alert alert-validation";
            _pnlSignatureEntry.Controls.Add( _signatureValidationSummary );

            // Add custom validator that will check for blank drawn or typed signature
            _signatureValidator = new CustomValidator();
            _signatureValidator.ID = "_customValidator";
            _signatureValidator.ClientValidationFunction = "Rock.controls.electronicSignatureControl.clientValidate";
            _signatureValidator.ErrorMessage = "Please enter a signature";
            _signatureValidator.Enabled = true;
            _signatureValidator.Display = ValidatorDisplay.Dynamic;
            _signatureValidator.ValidationGroup = validationGroup;
            _pnlSignatureEntry.Controls.Add( _signatureValidator );

            /* Controls for Drawn Signature*/
            _pnlSignatureEntryDrawn = new Panel();
            _pnlSignatureEntryDrawn.ID = "_pnlSignatureEntryDrawn";
            _pnlSignatureEntryDrawn.CssClass = "signature-entry-drawn js-signature-entry-drawn";
            _pnlSignatureEntry.Controls.Add( _pnlSignatureEntryDrawn );

            var lUseMouseOrFinger = new Literal()
            {
                Text = "<small class='text-muted'>Use mouse or finger to sign below.</small>"
            };

            _pnlSignatureEntryDrawn.Controls.Add( lUseMouseOrFinger );

            var pnlSignSignatureDrawnRow = new Panel()
            {
                CssClass = "row"
            };

            _pnlSignatureEntryDrawn.Controls.Add( pnlSignSignatureDrawnRow );

            var pnlSignSignatureCanvasCol = new Panel()
            {
                CssClass = "col-md-10"
            };

            pnlSignSignatureDrawnRow.Controls.Add( pnlSignSignatureCanvasCol );

            // drawing canvas
            var pnlSignatureEntryDrawnCanvasDiv = new Panel() { CssClass = "signature-entry-drawn-canvas-container" };
            _pnlSignatureEntryDrawn.Controls.Add( pnlSignatureEntryDrawnCanvasDiv );

            _lSignaturePadCanvas = new Literal();
            _lSignaturePadCanvas.ID = "_lSignaturePadCanvas";
            _lSignaturePadCanvas.Text = "<canvas class='js-signature-pad-canvas e-signature-pad' style='border-bottom: 1px solid #c4c4c4;'></canvas>";
            pnlSignSignatureCanvasCol.Controls.Add( _lSignaturePadCanvas );

            // clear signature button
            var pnlSignSignatureClearButtonCol = new Panel()
            {
                CssClass = "col-md-2"
            };

            pnlSignSignatureDrawnRow.Controls.Add( pnlSignSignatureClearButtonCol );

            _clearSignatureLink = new Literal();
            _clearSignatureLink.ID = "_clearSignatureLink";
            _clearSignatureLink.Text = $@"<a class='btn btn-default js-clear-signature pull-right'><i class='fa fa-2x fa-undo'></i></a>";
            pnlSignSignatureClearButtonCol.Controls.Add( _clearSignatureLink );

            /* Controls for Typed Signature*/
            _pnlSignatureEntryTyped = new Panel();
            _pnlSignatureEntryTyped.ID = "_pnlSignatureEntryTyped";
            _pnlSignatureEntryTyped.CssClass = "signature-entry-typed";
            _pnlSignatureEntry.Controls.Add( _pnlSignatureEntryTyped );

            _tbSignatureTyped = new RockTextBox();
            _tbSignatureTyped.ID = "_tbSignatureTyped";
            _tbSignatureTyped.Placeholder = "Type Name";
            _tbSignatureTyped.CssClass = "js-signature-typed";
            _tbSignatureTyped.ValidationGroup = validationGroup;
            _pnlSignatureEntryTyped.Controls.Add( _tbSignatureTyped );

            /* Signing step actions */
            _lSignatureSignDisclaimer = new Literal();
            _lSignatureSignDisclaimer.ID = "_lSignatureSignDisclaimer";

            _pnlSignatureEntry.Controls.Add( _lSignatureSignDisclaimer );

            var pnlSignSignatureDivRow = new Panel
            {
                CssClass = "row"
            };

            var pnlSignSignatureDivCol = new Panel
            {
                CssClass = "col-md-12"
            };

            _pnlSignatureEntry.Controls.Add( pnlSignSignatureDivRow );
            pnlSignSignatureDivRow.Controls.Add( pnlSignSignatureDivCol );

            _btnSignSignature = new BootstrapButton();
            _btnSignSignature.ID = "_btnSignSignature";
            _btnSignSignature.CssClass = "btn btn-primary btn-xs js-save-signature pull-right";
            _btnSignSignature.Text = "Sign";
            _btnSignSignature.ValidationGroup = validationGroup;
            _btnSignSignature.Click += _btnSignSignature_Click;
            pnlSignSignatureDivCol.Controls.Add( _btnSignSignature );

            _pnlSignatureEntryDrawn.Visible = SignatureType == SignatureType.Drawn;
            _pnlSignatureEntryTyped.Visible = SignatureType == SignatureType.Typed;

            /* Controls for signature completion */
            _pnlSignatureComplete = new Panel();
            _pnlSignatureComplete.ID = "_pnlSignatureComplete";
            _pnlSignatureComplete.CssClass = "signature-entry-complete";
            this.Controls.Add( _pnlSignatureComplete );

            _completionValidationSummary = new ValidationSummary();
            _completionValidationSummary.ValidationGroup = validationGroup;
            _completionValidationSummary.CssClass = "alert alert-validation";
            _pnlSignatureComplete.Controls.Add( _completionValidationSummary );

            _tbLegalName = new RockTextBox();
            _tbLegalName.ID = "_tbLegalName";
            _tbLegalName.Label = "Please enter your legal name";
            _tbLegalName.Required = true;
            _tbLegalName.ValidationGroup = validationGroup;
            _tbLegalName.RequiredErrorMessage = "Legal Name is required.";
            _pnlSignatureComplete.Controls.Add( _tbLegalName );

            _lCompletionSignedName = new RockLiteral()
            {
                ID = "_lCompletionSignedName",
                Label = "Legal Name"
            };

            _pnlSignatureComplete.Controls.Add( _lCompletionSignedName );

            _ebEmailAddress = new EmailBox();
            _ebEmailAddress.ID = "_ebEmailAddress";

            _ebEmailAddress.Required = true;
            _ebEmailAddress.RequiredErrorMessage = "Email Address is required.";
            _ebEmailAddress.ValidationGroup = validationGroup;
            _pnlSignatureComplete.Controls.Add( _ebEmailAddress );

            var pnlCompleteSignatureDiv = new Panel
            {
                CssClass = "row"
            };

            _pnlSignatureComplete.Controls.Add( pnlCompleteSignatureDiv );

            _btnCompleteSignature = new BootstrapButton();
            _btnCompleteSignature.ID = "_btnCompleteSignature";
            _btnCompleteSignature.CssClass = "btn btn-primary btn-xs pull-right";
            _btnCompleteSignature.Text = "Complete";
            _btnCompleteSignature.ValidationGroup = validationGroup;
            _btnCompleteSignature.Click += _btnCompleteSignature_Click;
            pnlCompleteSignatureDiv.Controls.Add( _btnCompleteSignature );

            UpdateUIControls();

            _pnlSignatureEntry.Visible = true;
            _pnlSignatureComplete.Visible = false;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            this.AddCssClass( "js-electronic-signature-control" );
            base.RenderControl( writer );
            RegisterJavaScript();
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "signature_pad-include", ResolveUrl( "~/Scripts/signature_pad/signature_pad.umd.min.js" ) );
            }

            var electronicSignatureControlScript =
    $@"Rock.controls.electronicSignatureControl.initialize({{
    controlId: '{this.ClientID}',
    imageMimeType: '{this.DrawnSignatureImageMimeType}'
}})
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "electronicSignatureControl_script" + this.ClientID, electronicSignatureControlScript, true );
        }

        /// <summary>
        /// Handles the Click event of the _btnSignSignature control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _btnSignSignature_Click( object sender, EventArgs e )
        {
            EnsureChildControls();
            _pnlSignatureEntry.Visible = false;
            _pnlSignatureComplete.Visible = true;

            // If we are showing the SignedName on the completion page...
            _lCompletionSignedName.Text = _tbSignatureTyped.Text;
        }

        /// <summary>
        /// Handles the Click event of the _btnCompleteSignature control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _btnCompleteSignature_Click( object sender, EventArgs e )
        {
            CompleteSignatureClicked?.Invoke( sender, e );
        }

        /// <summary>
        /// Occurs when the 'Complete' button is clicked on the completion step when the completion step is complete.
        /// </summary>
        public event EventHandler CompleteSignatureClicked;

        #endregion Base Control Methods
    }
}
