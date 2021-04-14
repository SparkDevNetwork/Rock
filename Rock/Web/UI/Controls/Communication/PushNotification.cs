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

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Communication;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    ///  Push Notification Medium Control
    /// </summary>
    public class PushNotification : MediumControl
    {
        #region UI Controls
        private RockControlWrapper rcwMessage;
        private MergeFieldPicker mfpMessage;
        private Label lblCount;
        private RockTextBox tbMessage;
        private RockTextBox tbTitle;

        private ImageUploader iupPushImage;
        private RockRadioButtonList rbOpenAction;
        private HtmlEditor htmlAdditionalDetails;
        private RockDropDownList ddlMobileApplications;
        private KeyValueList kvlQuerystring;
        private PagePicker ppMobilePage;
        private UrlLinkBox urlLink;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the transport.
        /// </summary>
        /// <value>
        /// The transport.
        /// </value>
        public TransportComponent Transport { get; set; }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public override string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return tbMessage.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                mfpMessage.ValidationGroup = value;
                tbMessage.ValidationGroup = value;
                tbTitle.ValidationGroup = value;
            }
        }
        #endregion

        #region Base Control Methods
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();

            // add the bootstrap-limit so that we can have a countdown of characters when entering push text
            string script = $"$('#{tbMessage.ClientID}').limit({{maxChars: {1024}, counter:'#{lblCount.ClientID}', normalClass:'badge', warningClass:'badge-warning', overLimitClass: 'badge-danger'}});";
            ScriptManager.RegisterStartupScript( this, GetType(), $"limit-{ClientID}", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // These values need to be reset here because viewstate retains them if their hidden.
            var pushOpenAction = GetSelectedOpenActionOrDefault();
            if ( pushOpenAction != PushOpenAction.ShowDetails )
            {
                htmlAdditionalDetails.Text = null;
                ddlMobileApplications.SelectedValue = null;
            }

            if ( pushOpenAction != PushOpenAction.LinkToMobilePage )
            {
                kvlQuerystring.Value = null;
                ppMobilePage.SetValue( ( int? ) null );
            }

            if ( pushOpenAction != PushOpenAction.LinkToUrl )
            {
                urlLink.Text = null;
            }
        }

        #endregion

        #region CompositeControl Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            tbTitle = new RockTextBox
            {
                ID = string.Format( "tbTextTitle_{0}", this.ID ),
                TextMode = TextBoxMode.SingleLine,
                Required = false,
                Label = "Title",
                MaxLength = 100
            };
            Controls.Add( tbTitle );

            rcwMessage = new RockControlWrapper
            {
                ID = string.Format( "rcwMessage_{0}", this.ID ),
                Label = "Message",
                Help = "<span class='tip tip-lava'></span>"
            };
            Controls.Add( rcwMessage );

            mfpMessage = new MergeFieldPicker
            {
                ID = string.Format( "mfpMergeFields_{0}", this.ID )
            };
            mfpMessage.MergeFields.Clear();
            mfpMessage.MergeFields.Add( "GlobalAttribute" );
            mfpMessage.MergeFields.Add( "Rock.Model.Person" );
            mfpMessage.CssClass += " pull-right margin-b-sm";
            mfpMessage.SelectItem += mfpMergeFields_SelectItem;
            rcwMessage.Controls.Add( mfpMessage );

            lblCount = new Label
            {
                CssClass = "badge margin-all-sm pull-right",
                ID = $"{nameof( lblCount )}_{ID}"
            };
            rcwMessage.Controls.Add( lblCount );

            tbMessage = new RockTextBox
            {
                ID = string.Format( "tbTextMessage_{0}", this.ID ),
                TextMode = TextBoxMode.MultiLine,
                Rows = 3
            };
            rcwMessage.Controls.Add( tbMessage );

            iupPushImage = new ImageUploader
            {
                ID = $"{nameof( iupPushImage )}_{ID}",
                Label = "Image",
                Help = "We recommend an image size of 1038x520.",
                Visible = false // Images aren't used during push yet, so don't show.
            };

            rcwMessage.Controls.Add( iupPushImage );

            rbOpenAction = CreateOpenActionRadioList();
            rcwMessage.Controls.Add( rbOpenAction );

            ddlMobileApplications = CreateMobileApplicationDropDownList();
            rcwMessage.Controls.Add( ddlMobileApplications );

            htmlAdditionalDetails = new HtmlEditor
            {
                ID = $"{nameof( htmlAdditionalDetails )}_{ID}",
                Label = "Additional Details",
                Height = 300
            };

            rcwMessage.Controls.Add( htmlAdditionalDetails );

            ppMobilePage = new PagePicker
            {
                ID = $"{nameof( ppMobilePage )}_{ID}",
                Label = "Mobile Page"
            };

            rcwMessage.Controls.Add( ppMobilePage );

            kvlQuerystring = new KeyValueList
            {
                ID = $"{nameof( kvlQuerystring )}_{ID}",
                Label = "Mobile Page Query String",
                KeyPrompt = "Key",
                ValuePrompt = "Value"
            };

            rcwMessage.Controls.Add( kvlQuerystring );

            urlLink = new UrlLinkBox
            {
                ID = $"{nameof( urlLink )}_{ID}",
                Label = "URL"
            };

            rcwMessage.Controls.Add( urlLink );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( rbOpenAction.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                var openAction = ( PushOpenAction ) rbOpenAction.SelectedValue.AsInteger();

                urlLink.Visible = openAction == PushOpenAction.LinkToUrl;
                kvlQuerystring.Visible = openAction == PushOpenAction.LinkToMobilePage;
                ppMobilePage.Visible = openAction == PushOpenAction.LinkToMobilePage;
                ddlMobileApplications.Visible = openAction == PushOpenAction.ShowDetails;
                htmlAdditionalDetails.Visible = openAction == PushOpenAction.ShowDetails;
            }
            else
            {
                urlLink.Visible = false;
                kvlQuerystring.Visible = false;
                ppMobilePage.Visible = false;
                ddlMobileApplications.Visible = false;
                htmlAdditionalDetails.Visible = false;
            }

            // Setting this here because the control clears out this in the OnInit function.
            ppMobilePage.SiteType = SiteType.Mobile;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            rcwMessage.RenderControl( writer );
        }

        #endregion

        #region MediumControl Methods
        /// <summary>
        /// Sets control values from a communication record.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void SetFromCommunication( CommunicationDetails communication )
        {
            EnsureChildControls();
            tbTitle.Text = communication.PushTitle;
            tbMessage.Text = communication.PushMessage;

            iupPushImage.BinaryFileId = communication.PushImageBinaryFileId;
            if ( communication.PushOpenAction != null )
            {
                rbOpenAction.SelectedValue = communication.PushOpenAction.ConvertToInt().ToString();
            }

            var pushData = new PushData();
            if ( communication.PushData.IsNotNullOrWhiteSpace() )
            {
                pushData = Newtonsoft.Json.JsonConvert.DeserializeObject<PushData>( communication.PushData );
            }

            ddlMobileApplications.SelectedValue = null;
            htmlAdditionalDetails.Text = null;
            if ( communication.PushOpenAction == PushOpenAction.ShowDetails )
            {
                ddlMobileApplications.SelectedValue = pushData.MobileApplicationId.ToStringSafe();
                htmlAdditionalDetails.Text = communication.PushOpenMessage;
            }

            kvlQuerystring.Value = null;
            if ( communication.PushOpenAction == PushOpenAction.LinkToMobilePage )
            {
                ppMobilePage.SetValue( pushData.MobilePageId );

                if ( pushData.MobilePageQueryString != null )
                {
                    kvlQuerystring.Value = pushData.MobilePageQueryString.Select( a => string.Format( "{0}^{1}", a.Key, a.Value ) ).ToList().AsDelimited( "|" );
                }
            }

            urlLink.Text = null;
            if ( communication.PushOpenAction == PushOpenAction.LinkToUrl )
            {
                urlLink.Text = pushData.Url;
            }
        }

        /// <summary>
        /// Updates the a communication record from control values.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void UpdateCommunication( CommunicationDetails communication )
        {
            EnsureChildControls();
            communication.PushTitle = tbTitle.Text;
            communication.PushMessage = tbMessage.Text;
            communication.PushImageBinaryFileId = iupPushImage.BinaryFileId;
            communication.PushOpenAction = GetSelectedOpenActionOrDefault();

            var pushData = new PushData();

            if ( communication.PushOpenAction == PushOpenAction.ShowDetails )
            {
                communication.PushOpenMessage = htmlAdditionalDetails.Text;
                pushData.MobileApplicationId = ddlMobileApplications.SelectedValue.AsIntegerOrNull();
            }

            if ( communication.PushOpenAction == PushOpenAction.LinkToMobilePage )
            {
                pushData.MobilePageQueryString = kvlQuerystring.Value.AsDictionaryOrNull();
                pushData.MobilePageId = ppMobilePage.SelectedValue.AsIntegerOrNull();
                pushData.MobileApplicationId = pushData.MobilePageId.HasValue ? PageCache.Get( pushData.MobilePageId.Value )?.SiteId : null;
            }

            if ( communication.PushOpenAction == PushOpenAction.LinkToUrl )
            {
                pushData.Url = urlLink.Text;
            }

            communication.PushData = pushData.ToJson();
        }

        /// <summary>
        /// On new communication, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void InitializeFromSender( Person sender )
        {
        }
        #endregion

        #region Control Events

        void mfpMergeFields_SelectItem( object sender, EventArgs e )
        {
            EnsureChildControls();
            tbMessage.Text += mfpMessage.SelectedMergeField;
            mfpMessage.SetValue( string.Empty );
        }
        #endregion

        #region Private Helper Methods
        private RockDropDownList CreateMobileApplicationDropDownList()
        {
            var mobileSites = new SiteService( new Data.RockContext() ).Queryable().Where( s => s.SiteType == SiteType.Mobile ).Select( s => new { s.Id, s.Name } ).ToList();

            ddlMobileApplications = new RockDropDownList
            {
                ID = $"{nameof( ddlMobileApplications )}_{ID}",
                Label = "Application"
            };

            ddlMobileApplications.Items.Add( new ListItem() );

            for ( int i = 0; i < mobileSites.Count; i++ )
            {
                var site = mobileSites[i];
                ddlMobileApplications.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
            }

            return ddlMobileApplications;
        }

        private RockRadioButtonList CreateOpenActionRadioList()
        {
            rbOpenAction = new RockRadioButtonList
            {
                ID = $"{nameof( rbOpenAction )}_{ID}",
                Label = "Open Action",
                RepeatDirection = RepeatDirection.Horizontal,
                Help = "Defines the open action for the message.",
                AutoPostBack = true
            };

            if ( Transport is IRockMobilePush )
            {
                rbOpenAction.Items.AddRange( new ListItem[] {
                    new ListItem
                    {
                        Text = "Link to Mobile Page",
                        Value = PushOpenAction.LinkToMobilePage.ConvertToInt().ToString()
                    },
                    new ListItem
                    {
                        Text = "Show Details",
                        Value = PushOpenAction.ShowDetails.ConvertToInt().ToString()
                    }
                } );
            }
            else
            {
                rbOpenAction.Items.AddRange( new ListItem[] {
                    new ListItem {
                        Text = "No Action",
                        Value = PushOpenAction.NoAction.ConvertToInt().ToString()
                    },
                    new ListItem
                    {
                        Text = "Link to URL",
                        Value = PushOpenAction.LinkToUrl.ConvertToInt().ToString()
                    }
                } );
            }

            rbOpenAction.SelectedValue = PushOpenAction.NoAction.ConvertToInt().ToString();

            return rbOpenAction;
        }

        private PushOpenAction? GetSelectedOpenActionOrDefault()
        {
            var pushOpenAction = rbOpenAction.SelectedValue.AsIntegerOrNull();
            if ( pushOpenAction == null )
            {
                return null;
            }
            return ( PushOpenAction ) pushOpenAction;
        }
        #endregion
    }
}
