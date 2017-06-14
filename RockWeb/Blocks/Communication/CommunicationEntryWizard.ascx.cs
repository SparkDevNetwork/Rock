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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Communication Entry Wizard" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communications such as email, SMS, etc. to recipients." )]
 
    public partial class CommunicationEntryWizard : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties
        public string BaseUrl { get; set; }
        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            BaseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd( '/' ) + "/";


            imgupImage.BinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();

            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/summernote/summernote.min.js" ), true );
            RockPage.AddScriptLink( "~/Scripts/Bundles/RockHtmlEditorPlugins", false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ifEmailDesigner.Attributes["srcdoc"] = sampleTemplate;
            }
        }

        #endregion

        #region Events

        #region Recipient Selection Events
        protected void lbRecipientSelectionNext_Click( object sender, EventArgs e )
        {
            pnlRecipientSelection.Visible = false;
            pnlEmailEditor.Visible = true;
        }
        #endregion

        #region Email Editor Events
        protected void imgupImage_ImageUploaded( object sender, ImageUploaderEventArgs e )
        {
            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "SaveAndCloseImageComponentUploaded",
                "saveAndCloseImageComponent(null);",
                true );
        }

        protected void imgupImage_ImageRemoved( object sender, ImageUploaderEventArgs e )
        {
            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "SaveAndCloseImageComponentRemoved",
                "saveAndCloseImageComponent(null);",
                true );
        }
        #endregion

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }
        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion



        // remove before flight
        string sampleTemplate = @"<!DOCTYPE html>
<html>
<head>
<title>A Responsive Email Template</title>

<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
<style type=""text/css"">
    /* CLIENT-SPECIFIC STYLES */
    body, table, td, a{-webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%;} /* Prevent WebKit and Windows mobile changing default text sizes */
    table, td{mso-table-lspace: 0pt; mso-table-rspace: 0pt;} /* Remove spacing between tables in Outlook 2007 and up */
    img{-ms-interpolation-mode: bicubic;} /* Allow smoother rendering of resized image in Internet Explorer */

    /* RESET STYLES */
    img{border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none;}
    table{border-collapse: collapse !important;}
    body{height: 100% !important; margin: 0 !important; padding: 0 !important; width: 100% !important;}

    /* iOS BLUE LINKS */
    a[x-apple-data-detectors] {
        color: inherit !important;
        text-decoration: none !important;
        font-size: inherit !important;
        font-family: inherit !important;
        font-weight: inherit !important;
        line-height: inherit !important;
    }

    /* MOBILE STYLES */
    @media screen and (max-width: 525px) {

        /* ALLOWS FOR FLUID TABLES */
        .wrapper {
          width: 100% !important;
        	max-width: 100% !important;
        }

        /* ADJUSTS LAYOUT OF LOGO IMAGE */
        .logo img {
          margin: 0 auto !important;
        }

        /* USE THESE CLASSES TO HIDE CONTENT ON MOBILE */
        .mobile-hide {
          display: none !important;
        }

        .img-max {
          max-width: 100% !important;
          width: 100% !important;
          height: auto !important;
        }

        /* FULL-WIDTH TABLES */
        .responsive-table {
          width: 100% !important;
        }

        /* UTILITY CLASSES FOR ADJUSTING PADDING ON MOBILE */
        .padding {
          padding: 10px 5% 15px 5% !important;
        }

        .padding-meta {
          padding: 30px 5% 0px 5% !important;
          text-align: center;
        }

        .padding-copy {
     		padding: 10px 5% 10px 5% !important;
          text-align: center;
        }

        .no-padding {
          padding: 0 !important;
        }

        .section-padding {
          padding: 50px 15px 50px 15px !important;
        }

        /* ADJUST BUTTONS ON MOBILE */
        .mobile-button-container {
            margin: 0 auto;
            width: 100% !important;
        }

        .mobile-button {
            padding: 15px !important;
            border: 0 !important;
            font-size: 16px !important;
            display: block !important;
        }

    }

    /* ANDROID CENTER FIX */
    div[style*=""margin: 16px 0;""] { margin: 0 !important; }
</style>
<!--[if gte mso 12]>
<style type=""text/css"">
.mso-right {
	padding-left: 20px;
}
</style>
<![endif]-->
</head>
<body style=""margin: 0 !important; padding: 0 !important;"">

<!-- HIDDEN PREHEADER TEXT -->
<div style=""display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;"">
    Entice the open with some amazing preheader text. Use a little mystery and get those subscribers to read through...
</div>

<!-- HEADER -->
<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
        <td bgcolor=""#333333"" align=""center"">
            <!--[if (gte mso 9)|(IE)]>
            <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
            <tr>
            <td align=""center"" valign=""top"" width=""500"">
            <![endif]-->
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 500px;"" class=""wrapper"">
                <tr>
                    <td align=""center"" valign=""top"" style=""padding: 15px 0;"" class=""logo"">
                        <a href=""http://litmus.com"" target=""_blank"">
                            <img alt=""Logo"" src=""http://www.minecartstudio.com/Content/Misc/logo-1.jpg"" width=""60"" height=""60"" style=""display: block; font-family: Helvetica, Arial, sans-serif; color: #ffffff; font-size: 16px;"" border=""0"">
                        </a>
                    </td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
    <tr>
        <td bgcolor=""#D8F1FF"" align=""center"" style=""padding: 70px 15px 70px 15px;"" class=""section-padding"">
            <!--[if (gte mso 9)|(IE)]>
            <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
            <tr>
            <td align=""center"" valign=""top"" width=""500"">
            <![endif]-->
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 500px;"" class=""responsive-table"">
                <tr>
                    <td style=""font-size: 25px; font-family: Helvetica, Arial, sans-serif; color: #333333; padding-top: 30px;"">
					
					
                        <div class=""dropzone"">

							<div class=""component component-text"" data-content=""<h1>Yo MTV Raps</h1>"" data-state=""component"">
								<h1>Hello There!</h1>
							</div>

						</div>
                    
					
					</td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
    <tr>
        <td bgcolor=""#ffffff"" align=""center"" style=""padding: 70px 15px 25px 15px;"" class=""section-padding"">
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""500"" style=""padding:0 0 20px 0;"" class=""responsive-table"">
                <tr>
                    <td align=""center"" height=""100%"" valign=""top"" width=""100%"" style=""padding-bottom: 35px;"">
                        <!--[if (gte mso 9)|(IE)]>
                        <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
                        <tr>
                        <td align=""center"" valign=""top"" width=""500"">
                        <![endif]-->
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width:500;"">
                            <tr>
                                <td align=""center"" valign=""top"">
                                    <!--[if (gte mso 9)|(IE)]>
                                    <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
                                    <tr>
                                    <td align=""left"" valign=""top"" width=""150"">
                                    <![endif]-->
                                    
									
									<div class=""dropzone""></div>

                                </td>
                            </tr>
                        </table>
                        <!--[if (gte mso 9)|(IE)]>
                        </td>
                        </tr>
                        </table>
                        <![endif]-->
                    </td>
                </tr>
                
            </table>
        </td>
    </tr>
    
        
    <tr>
        <td bgcolor=""#ffffff"" align=""center"" style=""padding: 20px 0px;"">
            <!--[if (gte mso 9)|(IE)]>
            <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
            <tr>
            <td align=""center"" valign=""top"" width=""500"">
            <![endif]-->
            <!-- UNSUBSCRIBE COPY -->
            <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""max-width: 500px;"" class=""responsive-table"">
                <tr>
                    <td align=""center"" style=""font-size: 12px; line-height: 18px; font-family: Helvetica, Arial, sans-serif; color:#666666;"">
                        1234 Main Street, Anywhere, MA 01234, USA
                        <br>
                        <a href=""http://litmus.com"" target=""_blank"" style=""color: #666666; text-decoration: none;"">Unsubscribe</a>
                        <span style=""font-family: Arial, sans-serif; font-size: 12px; color: #444444;"">&nbsp;&nbsp;|&nbsp;&nbsp;</span>
                        <a href=""http://litmus.com"" target=""_blank"" style=""color: #666666; text-decoration: none;"">View this email in your browser</a>
                    </td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
</table>
</body>
</html>
";
    }
}