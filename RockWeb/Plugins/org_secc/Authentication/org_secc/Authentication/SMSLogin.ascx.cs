// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_secc.Authentication
{
    /// <summary>
    /// User login using SMS for authentication.
    /// </summary>
    [DisplayName( "SMS Login" )]
    [Category( "SECC> Security" )]
    [Description( "User login using SMS for authentication." )]

    [CodeEditorField( "Prompt Message", "Message to show before logging in.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, true, @"
Please enter your cell phone number and we will text you a code to log in with. <br /><i>Text and data rates may apply</i>.
", "" )]

    [CodeEditorField( "No Number Message", "Message to show if the SMS number cannot be found in our system.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, true, @"
We are sorry, we could not find your phone number in our records.
", "" )]

    [LinkedPage( "Duplicate Number Page", "Page to navigate to create account.", true, "", "" )]
    [CodeEditorField( "Duplicate Message", "Message to show if duplicate phone records are found.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, true, defaultValue: @"
We are sorry, we dected more than one person with your number in our records.
        " )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "From", "The number to originate message from (configured under Admin Tools > General Settings > Defined Types > SMS From Values).", true, false, "", "" )]
    [TextField( "Message", "Message that will be sent along with the login code.", true, "Use {{ password }} to log in.  If you received this message by mistake please disregard." )]
    [LinkedPage( "Redirect Page", "Page to redirect user to upon successful login.", true, "", "")]
    public partial class SMSLogin : Rock.Web.UI.RockBlock
    {

        public string PhoneNumber
        {
            get
            {
                return Regex.Replace( tbPhoneNumber.Text, @"^(\+)|\D", "$1" );
            }
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
                if ( !Page.IsPostBack )
                {
                    lbPrompt.Text = GetAttributeValue( "PromptMessage" );
                }
            }
        }

        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            pnlPhoneNumber.Visible = false;
            RockContext rockContext = new RockContext();
            PhoneNumberService phoneNumberService = new PhoneNumberService( rockContext );
            var numberOwners = phoneNumberService.Queryable()
                .Where( pn => pn.Number == PhoneNumber )
                .Select( pn => pn.Person )
                .DistinctBy( p => p.Id )
                .ToList();

            if ( numberOwners.Count == 0 )
            {
                lbNoNumber.Text = GetAttributeValue( "NoNumberMessage" );
                pnlNoNumber.Visible = true;
                return;
            }

            if ( numberOwners.Count > 1 )
            {
                if ( GetAttributeValue( "DuplicateNumberPage" ).AsGuidOrNull() == null )
                {
                    btnResolution.Visible = false;
                }
                lbDuplicateNumber.Text = GetAttributeValue( "DuplicateMessage" );
                pnlDuplicateNumber.Visible = true;
                return;
            }

            var person = numberOwners.FirstOrDefault();

            UserLoginService userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.Queryable()
                .Where( u => u.UserName == ( "__PHONENUMBER__" + PhoneNumber ) )
                .FirstOrDefault();

            if ( userLogin == null )
            {
                var entityTypeId = EntityTypeCache.Read( "Avalanche.Security.Authentication.PhoneNumber" ).Id;

                userLogin = new UserLogin()
                {
                    UserName = "__PHONENUMBER__" + PhoneNumber,
                    EntityTypeId = entityTypeId,
                };
                userLoginService.Add( userLogin );
            }

            userLogin.PersonId = person.Id;
            userLogin.LastPasswordChangedDateTime = Rock.RockDateTime.Now;
            userLogin.FailedPasswordAttemptWindowStartDateTime = Rock.RockDateTime.Now;
            userLogin.FailedPasswordAttemptCount = 0;
            userLogin.IsConfirmed = true;
            userLogin.Password = new Random().Next( 100000, 999999 ).ToString();

            rockContext.SaveChanges();

            var recipients = new List<RecipientData>();
            recipients.Add( new RecipientData( PhoneNumber ) );

            var smsMessage = new RockSMSMessage();
            smsMessage.SetRecipients( recipients );

            // Get the From value
            Guid? fromGuid = GetAttributeValue( "From" ).AsGuidOrNull();
            if ( fromGuid.HasValue )
            {
                var fromValue = DefinedValueCache.Read( fromGuid.Value, rockContext );
                if ( fromValue != null )
                {
                    smsMessage.FromNumber = DefinedValueCache.Read( fromValue.Id );
                }
            }

            var mergeObjects = new Dictionary<string, object> { { "password", userLogin.Password } };
            var message = GetAttributeValue( "Message" ).ResolveMergeFields( mergeObjects, null );

            smsMessage.Message = message;

            var ipAddress = GetIpAddress();
            if ( SMSRecords.ReserveItems( ipAddress, PhoneNumber ) )
            {
                pnlCode.Visible = true;
                var delay = SMSRecords.GetDelay( ipAddress, PhoneNumber );
                Task.Run( () => { SendSMS( smsMessage, ipAddress, PhoneNumber, delay ); } );
            }
            else
            {
                LogException( new Exception( string.Format( "Unable to reserve for SMS message: IP: {0} PhoneNumber: {1}", ipAddress, PhoneNumber ) ) );
                pnlRateLimited.Visible = true;
            }
        }

        public async void SendSMS( RockSMSMessage smsMessage, string phoneNumber, string ipAddress, double delay )
        {
            await Task.Delay( ( int ) delay );
            try
            {
                smsMessage.Send();
            }
            catch
            {
                //Nom Nom Nom
            }
            SMSRecords.ReleaseItems( ipAddress, phoneNumber );
        }

        protected void btnLogin_Click( object sender, EventArgs e )
        {
            nbError.Visible = false;
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( "__PHONENUMBER__" + PhoneNumber );
                if ( userLogin != null && userLogin.EntityType != null )
                {
                    var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );
                    if ( component != null && component.IsActive && !component.RequiresRemoteAuthentication )
                    {
                        if ( component.Authenticate( userLogin, tbCode.Text ) )
                        {
                            CheckUser( userLogin, Request.QueryString["returnurl"], false );
                            return;
                        }
                    }
                }
            }
            nbError.Visible = true;
        }

        private void CheckUser( UserLogin userLogin, string returnUrl, bool rememberMe )
        {
            if ( userLogin != null )
            {
                if ( ( userLogin.IsConfirmed ?? true ) && !( userLogin.IsLockedOut ?? false ) )
                {
                    LoginUser( userLogin.UserName, returnUrl, rememberMe );
                }
                else
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

                    if ( userLogin.FailedPasswordAttemptCount > 5 )
                    {
                        pnlCode.Visible = false;
                        pnlPhoneNumber.Visible = true;
                    }
                    else
                    {
                        nbError.Visible = true;
                    }
                }
            }
        }

        private void LoginUser( string userName, string returnUrl, bool rememberMe )
        {
            string redirectUrlSetting = LinkedPageUrl( "RedirectPage" );

            UserLoginService.UpdateLastLogin( userName );

            Authorization.SetAuthCookie( userName, rememberMe, false );

            if ( !string.IsNullOrWhiteSpace( returnUrl ) )
            {
                string redirectUrl = Server.UrlDecode( returnUrl );
                Response.Redirect( redirectUrl );
                Context.ApplicationInstance.CompleteRequest();
            }
            else if ( !string.IsNullOrWhiteSpace( redirectUrlSetting ) )
            {
                Response.Redirect( redirectUrlSetting );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
        }
        public static string GetIpAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( !string.IsNullOrEmpty( ipAddress ) )
            {
                string[] addresses = ipAddress.Split( ',' );
                if ( addresses.Length != 0 )
                {
                    var forwardedIp = addresses[0];
                    var splitAddresss = forwardedIp.Split( ':' ); //Remove the occasional port
                    return splitAddresss[0];
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        protected void btnResolution_Click( object sender, EventArgs e )
        {
            NavigateToPage( GetAttributeValue( "DuplicateNumberPage" ).AsGuid(), new Dictionary<string, string>() );
        }

        protected void btnNoNmber_Click( object sender, EventArgs e )
        {
            Reset();
        }

        protected void btnDuplicateNumber_Click( object sender, EventArgs e )
        {
            Reset();
        }

        protected void btnRateLimited_Click( object sender, EventArgs e )
        {
            Reset();
        }

        private void Reset()
        {
            pnlDuplicateNumber.Visible = false;
            pnlRateLimited.Visible = false;
            pnlNoNumber.Visible = false;
            pnlPhoneNumber.Visible = true;
        }

    }

    public static class SMSRecords
    {
        public static object _delayLock = new object();
        public static object _reserveLock = new object();
        public static List<SMSRecord> Records { get; set; }
        public static List<string> ActiveItems { get; set; }
        static SMSRecords()
        {
            Records = new List<SMSRecord>();
            ActiveItems = new List<string>();
        }

        public static bool ReserveItems( string ip, string phoneNumber )
        {
            lock ( _reserveLock )
            {
                if ( ActiveItems.Contains( ip ) || ActiveItems.Contains( phoneNumber ) )
                {
                    //If the list already contains these two items
                    return false;
                }
                ActiveItems.Add( ip );
                ActiveItems.Add( phoneNumber );

                return true;
            }
        }

        public static void ReleaseItems( string ip, string phoneNumber )
        {
            lock ( _delayLock )
            {
                if ( ActiveItems.Contains( ip ) )
                {
                    ActiveItems.Remove( ip );
                }
                if ( ActiveItems.Contains( phoneNumber ) )
                {
                    ActiveItems.Remove( phoneNumber );
                }
            }
        }

        public static double GetDelay( string ip, string phoneNumber )
        {
            lock ( _delayLock )
            {
                SMSRecords.Records.Add( new SMSRecord( ip ) );
                SMSRecords.Records.Add( new SMSRecord( phoneNumber ) );
                var hourAgo = Rock.RockDateTime.Now.AddHours( -1 );
                var toRemove = new List<SMSRecord>();
                toRemove.AddRange( Records.Where( r => r.DateTime < hourAgo ).ToList() );
                foreach ( var item in toRemove )
                {
                    Records.Remove( item );
                }
                double delay = 2; //2ms
                //Slow down exponentially.
                delay = Math.Pow( delay, Records.Where( r => r.Value == ip || r.Value == phoneNumber ).Count() );

                return delay;
            }
        }
    }

    public class SMSRecord
    {
        public SMSRecord( string value )
        {
            Value = value;
            DateTime = Rock.RockDateTime.Now;
        }

        public string Value { get; set; }
        public DateTime DateTime { get; set; }
    }
}
