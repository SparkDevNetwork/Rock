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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "WiFi Welcome" )]
    [Category( "Core" )]
    [Description( "Controls access to WiFi." )]
    // put block properties here to set visibility
    public partial class CaptivePortal : RockBlock
    {
        string macAddress;

        protected void Page_Load( object sender, EventArgs e )
        {
            macAddress = Request.Params["client_mac"];
            
            if ( !IsPostBack)
            {
                //Uri fpUrl = Request.UrlReferrer;
                //string frontPorchHost = fpUrl.Host;

                ViewState["FrontPorchUrl"] = Request.UrlReferrer.ToString();
                // If we are not going to show anything just forward the user on
                if ( ShowControls() )
                {
                    Response.Redirect( string.Format("{0}/client_mac={1}",ViewState["FrontPorchUrl"].ToString(), macAddress ) );
                }

                bool isNewDevice = CreateDeviceIfNew();
                int? rockUserId = Session["RockUserId"].ToString().AsIntegerOrNull();
                
                if ( rockUserId != null )
                {
                    LinkDeviceToPerson(rockUserId);
                    Prefill(rockUserId);
                    ExpireCookie();

                }
                else
                {

                }


            }

            


        }

        /// <summary>
        /// Creates the device if new.
        /// </summary>
        /// <returns>Returns true if the device was created, false it already existed</returns>
        private bool CreateDeviceIfNew()
        {
            // Check to see if the device exists

            return true;
        }

        private void LinkDeviceToPerson(int? rockUserId)
        {
            if ( rockUserId == null )
                return;

        }

        private void CreateDeviceCookie()
        {
            // If I don't know this device and person then create a cookie
            HttpCookie httpcookie = new HttpCookie("ROCK_PERSONALDEVICE_ADDRESS", macAddress);
            httpcookie.Expires = DateTime.MaxValue;
        }

        protected void Prefill(int? rockUserId)
        {
            // If they are logged into Rock then fill out the controls that are visible (use block properties)
            if ( rockUserId == null )
            {
                return;
            }

            Person person = new PersonService( new RockContext() ).GetByUserLoginId( rockUserId.Value );

            if (person == null)
            {
                return;
            }

            if (tbFirstName.Visible == true)
            {
                tbFirstName.Text = person.FirstName;
            }

            if (tbLastName.Visible)
            {
                tbLastName.Text = person.LastName;
            }

            if ( tbMobilePhone.Visible )
            {
                tbMobilePhone.Text = person.PhoneNumbers.Where( p => p.NumberTypeValueId == 13 ).Select( p => p.Number ).FirstOrDefault();
            }

            if (tbEmail.Visible == true )
            {
                tbEmail.Text = person.Email;
            }

            AttributeValueService a = new AttributeValueService( new RockContext());

        }

        private void ExpireCookie()
        {
            if ( Request.Cookies["ROCK_PERSONALDEVICE_ADDRESS"] != null )
            {
                Response.Cookies["ROCK_PERSONALDEVICE_ADDRESS"].Expires = DateTime.Now.AddDays( -1 );
            }
        }

        protected void btnConnect_Click( object sender, EventArgs e )
        {
            // Verify required fields

            // Send them back to Front Porch
            Response.Redirect( string.Format( "{0}/client_mac={1}", ViewState["FrontPorchUrl"].ToString(), macAddress ) );

        }

        protected bool ShowControls()
        {
            bool showNothing = false;
            // get the attributes and show the desired controls. If they are visible then they are required




            var legalNotice = System.IO.File.ReadAllText( Server.MapPath( "/Content/ExternalSite/WiFiLegalNotice.html" ) );
            iframeLegalNotice.Attributes["srcdoc"] = legalNotice.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage) );
            iframeLegalNotice.Src = "javascript: window.frameElement.getAttribute('srcdoc');";


            return showNothing;
        }
    }


}