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
using Rock;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace org.lakepointe.Checkin.Controls
{
    public class LPCPreRegistrationChildRow : PreRegistrationChildRow
    {
        protected int _index;
        protected LinkButton _lbTakePhoto;
        protected EmailBox _ebEmail;

        public HiddenField hfImage { get; set; }

        public ImageEditor ieUploadedImage;

        public event EventHandler<LpcChildClickEventArgs> Click;

        /// <summary>
        /// Gets or sets a value indicating whether [show birth date].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show birth date]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowImage
        {
            get
            {
                EnsureChildControls();
                return _lbTakePhoto.Visible;
            }
            set
            {
                EnsureChildControls();
                _lbTakePhoto.Visible = value;
            }
        }

        public bool ShowUploadImage
        {
            get
            {
                EnsureChildControls();
                return ieUploadedImage.Visible;
            }
            set
            {
                EnsureChildControls();
                ieUploadedImage.Visible = value;
            }
        }

        public bool ShowEmail
        {
            get
            {
                EnsureChildControls();
                return _ebEmail.Visible;
            }
            set
            {
                EnsureChildControls();
                _ebEmail.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require birth date].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require birth date]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireEmail
        {
            get
            {
                EnsureChildControls();
                return _ebEmail.Required;
            }
            set
            {
                EnsureChildControls();
                _ebEmail.Required = value;
            }
        }

        public string Email
        {
            get
            {
                EnsureChildControls();
                return _ebEmail.Text;
            }

            set
            {
                EnsureChildControls();
                _ebEmail.Text = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGroupMembersRow" /> class.
        /// </summary>
        public LPCPreRegistrationChildRow( int index, string path = "" )
            : base(path)
        {
            _index = index;

            _ebEmail = new EmailBox();
            _lbTakePhoto = new LinkButton();
            ieUploadedImage = new ImageEditor();
            hfImage = new HiddenField();

            _lbTakePhoto.Click += _lbTakePhoto_Click;
        }

        private void _lbTakePhoto_Click( object sender, EventArgs e )
        {
            Click?.Invoke( sender, new LpcChildClickEventArgs( hfImage.ClientID, hfImage.ClientID.Replace( "hfImage", "canvas" ) ) );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _ebEmail.ID = string.Format( "ebEmail{0}", _index );
            _ebEmail.Label = Translation.GetValueOrDefault( "LabelEmail", "Email" );

            _lbTakePhoto.ID = string.Format( "lbCamera{0}", _index );
            ieUploadedImage.ID = string.Format("uploadChildPhoto{0}", _index);
            hfImage.ID = string.Format( "hfImage{0}", _index );

            Controls.Add( _ebEmail );
            Controls.Add( _lbTakePhoto );
            Controls.Add(ieUploadedImage);
            Controls.Add( hfImage );

            //_ebEmail.CssClass = "form-control";
            _lbTakePhoto.CssClass = "btn btn-primary";
            _lbTakePhoto.CausesValidation = false;
            _lbTakePhoto.Text = $@"<i class=""fal fa-camera""></i> {Translation.GetValueOrDefault( "LabelTakePhoto", "Take Photo" )}&nbsp&nbsp";

            ieUploadedImage.Label = Translation.GetValueOrDefault( "LabelPhoto", "Photo" );
            ieUploadedImage.ButtonText = @"<i class=""fal fa-pencil-alt""></i>";
            ieUploadedImage.BinaryFileTypeGuid = "03BD8476-8A9F-4078-B628-5B538F967AFC".AsGuid();
            ieUploadedImage.CssClass = "form-control";
            ieUploadedImage.Required = false;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl(writer);
            if ( this.Visible )
            {
                if ( ShowImage )
                {
                    writer.Write( @"<div class=""row"">" );
                    writer.Write( @"<div id=""pnlCPhoto{0}"" class=""col-sm-12"">", _index );
                        hfImage.RenderControl( writer );
                        writer.Write( @"<div class=""row""><div class=""col-sm-6"">" );
                            _lbTakePhoto.RenderControl( writer );
                        writer.Write( @"</div><div class=""col-sm-6"">" );
                            // Beware: javascript in (LPC)FamilyPreRegistration.ascx requires this relationship between hfImage and canvas
                            writer.Write( @"<canvas id=""{0}"" width=320 height=320></canvas>", hfImage.ClientID.Replace( "hfImage", "canvas" ) );
                        writer.Write( @"</div></div>" );
                    writer.Write( @"</div>" );
                writer.Write( @"</div>" );
                }

                if ( ShowUploadImage )
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "row");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-sm-6");
                    ieUploadedImage.RenderControl(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
            }
        }
    }

    /// <summary>
    /// Helper Class for serializing child data in viewstate
    /// </summary>
    [Serializable]
    public class LPCPreRegistrationChild : PreRegistrationChild
    {
        public string TakenImage { get; set; }

        public int? UploadedImage { get; set; }

        public int? PreCropBinaryFileId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreRegistrationChild"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public LPCPreRegistrationChild( Person person )
            : base( person )
        {
            UploadedImage = person.PhotoId;
        }
    }

    public class LpcChildClickEventArgs : EventArgs
    {
        public string HiddenField { get; }
        public string Canvas { get; }
        public LpcChildClickEventArgs( string hiddenFieldId, string canvasId )
        {
            HiddenField = hiddenFieldId;
            Canvas = canvasId;
        }
    }
}