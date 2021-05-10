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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "SMS Conversations" )]
    [Category( "Communication" )]
    [Description( "Block for having SMS Conversations between an SMS enabled phone and a Rock SMS Phone number that has 'Enable Mobile Conversations' set to false." )]
    [DefinedValueField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Order = 1 )]

    [BooleanField( "Show only personal SMS number",
        Key = AttributeKey.ShowOnlyPersonalSmsNumber,
        Description = "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        DefaultBooleanValue = false,
        Order = 2
         )]
    [BooleanField( "Hide personal SMS numbers",
        Key = AttributeKey.HidePersonalSmsNumbers,
        Description = "Only SMS Numbers that are not associated with a person. The numbers without a 'ResponseRecipient' attribute value.",
        DefaultBooleanValue = false,
        Order = 3
         )]
    [BooleanField( "Enable SMS Send",
        Key = AttributeKey.EnableSmsSend,
        Description = "Allow SMS messages to be sent from the block.",
        DefaultBooleanValue = true,
        Order = 4
         )]

    [IntegerField( "Show Conversations From Months Ago",
        Key = AttributeKey.ShowConversationsFromMonthsAgo,
        Description = "Limits the conversations shown in the left pane to those of X months ago or newer. This does not affect the actual messages shown on the right.",
        DefaultIntegerValue = 6,
        Order = 5
         )]

    [IntegerField( "Max Conversations",
        Key = AttributeKey.MaxConversations,
        Description = "Limits the number of conversations shown in the left pane. This does not affect the actual messages shown on the right.",
        DefaultIntegerValue = 100,
        Order = 5
         )]

    [CodeEditorField( "Person Info Lava Template",
        Key = AttributeKey.PersonInfoLavaTemplate,
        Description = "A Lava template to display person information about the selected Communication Recipient.",
        DefaultValue = "{{ Person.FullName }}",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 300,
        IsRequired = false,
        Order = 6
         )]

    // Start here to build the person description lit field after selecting recipient.
    public partial class SmsConversations : RockBlock
    {
        #region Attribute Keys
        protected static class AttributeKey
        {
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string ShowOnlyPersonalSmsNumber = "ShowOnlyPersonalSmsNumber";
            public const string HidePersonalSmsNumbers = "HidePersonalSmsNumbers";
            public const string EnableSmsSend = "EnableSmsSend";
            public const string ShowConversationsFromMonthsAgo = "ShowConversationsFromMonthsAgo";
            public const string MaxConversations = "MaxConversations";
            public const string PersonInfoLavaTemplate = "PersonInfoLavaTemplate";
        }

        #endregion Attribute Keys

        #region Control Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            newPersonEditor.ShowEmail = false;

            HtmlMeta preventPhoneMetaTag = new HtmlMeta
            {
                Name = "format-detection",
                Content = "telephone=no"
            };

            RockPage.AddMetaTag( this.Page, preventPhoneMetaTag );

            this.BlockUpdated += Block_BlockUpdated;

            btnCreateNewMessage.Visible = this.GetAttributeValue( AttributeKey.EnableSmsSend ).AsBoolean();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string postbackArgs = Request.Params["__EVENTARGUMENT"] ?? string.Empty;

            nbAddPerson.Visible = false;

            if ( ppPersonFilter.PersonId != null )
            {
                divPersonFilter.Style.Remove( "display" );
            }

            if ( !IsPostBack )
            {
                if ( LoadPhoneNumbers() )
                {
                    nbNoNumbers.Visible = false;
                    divMain.Visible = true;
                    LoadResponseListing();
                }
                else
                {
                    nbNoNumbers.Visible = true;
                    divMain.Visible = false;
                }
            }
        }

        #endregion Control Overrides

        #region private/protected Methods

        /// <summary>
        /// Loads the phone numbers.
        /// </summary>
        /// <returns></returns>
        private bool LoadPhoneNumbers()
        {
            // First load up all of the available numbers
            var smsNumbers = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() ).DefinedValues;

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) ).ToList();
            }

            // filter personal numbers (any that have a response recipient) if the hide personal option is enabled
            if ( GetAttributeValue( AttributeKey.HidePersonalSmsNumbers ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( v => v.GetAttributeValue( "ResponseRecipient" ).IsNullOrWhiteSpace() ).ToList();
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( AttributeKey.ShowOnlyPersonalSmsNumber ).AsBoolean() && !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                smsNumbers = smsNumbers.Where( v => CurrentPerson.Aliases.Any( a => a.Guid == v.GetAttributeValue( "ResponseRecipient" ).AsGuid() ) ).ToList();
            }

            if ( smsNumbers.Any() )
            {
                var smsDetails = smsNumbers.Select( v => new
                {
                    v.Id,
                    Description = string.IsNullOrWhiteSpace( v.Description )
                    ? PhoneNumber.FormattedNumber( string.Empty, v.Value.Replace( "+", string.Empty ) )
                    : v.Description.LeftWithEllipsis( 25 ),
                } );

                ddlSmsNumbers.DataSource = smsDetails;
                ddlSmsNumbers.Visible = smsNumbers.Count() > 1;
                ddlSmsNumbers.DataValueField = "Id";
                ddlSmsNumbers.DataTextField = "Description";
                ddlSmsNumbers.DataBind();

                string keyPrefix = string.Format( "sms-conversations-{0}-", this.BlockId );

                string smsNumberUserPref = this.GetUserPreference( keyPrefix + "smsNumber" ) ?? string.Empty;

                if ( smsNumberUserPref.IsNotNullOrWhiteSpace() )
                {
                    // Don't try to set the selected value unless you are sure it's in the list of items.
                    if ( ddlSmsNumbers.Items.FindByValue( smsNumberUserPref ) != null )
                    {
                        ddlSmsNumbers.SelectedValue = smsNumberUserPref;
                    }
                }

                hlSmsNumber.Visible = smsNumbers.Count() == 1;
                hlSmsNumber.Text = smsDetails.Select( v => v.Description ).FirstOrDefault();
                hfSmsNumber.SetValue( smsNumbers.Count() > 1 ? ddlSmsNumbers.SelectedValue.AsInteger() : smsDetails.Select( v => v.Id ).FirstOrDefault() );

                tglShowRead.Checked = this.GetUserPreference( keyPrefix + "showRead" ).AsBooleanOrNull() ?? true;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void LoadResponseListing()
        {
            LoadResponseListing( null );
        }

        /// <summary>
        /// Loads the response listing.
        /// </summary>
        private void LoadResponseListing( int? personId )
        {
            // NOTE: The FromPersonAliasId is the person who sent a text from a mobile device to Rock.
            // This person is also referred to as the Recipient because they are responding to a
            // communication from Rock. Restated the response is from the recipient of a communication.

            // This is the person lava field, we want to clear it because reloading this list will deselect the user.
            litSelectedRecipientDescription.Text = string.Empty;
            hfSelectedRecipientPersonAliasId.Value = string.Empty;
            hfSelectedMessageKey.Value = string.Empty;
            tbNewMessage.Visible = false;
            btnSend.Visible = false;

            int? smsPhoneDefinedValueId = hfSmsNumber.ValueAsInt();
            if ( smsPhoneDefinedValueId == default( int ) )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationResponseService = new CommunicationResponseService( rockContext );

                int months = GetAttributeValue( AttributeKey.ShowConversationsFromMonthsAgo ).AsInteger();

                var startDateTime = RockDateTime.Now.AddMonths( -months );
                bool showRead = tglShowRead.Checked;

                var maxConversations = this.GetAttributeValue( AttributeKey.MaxConversations ).AsIntegerOrNull() ?? 1000;

                var responseListItems = communicationResponseService.GetCommunicationResponseRecipients( smsPhoneDefinedValueId.Value, startDateTime, showRead, maxConversations, personId );

                // don't display conversations if we're rebinding the recipient list
                rptConversation.Visible = false;
                gRecipients.DataSource = responseListItems;
                gRecipients.DataBind();
            }
        }

        /// <summary>
        /// Loads the responses for recipient.
        /// </summary>
        /// <param name="recipientPersonAliasId">The recipient person alias identifier.</param>
        /// <returns></returns>
        private string LoadResponsesForRecipient( int recipientPersonAliasId )
        {
            int? smsPhoneDefinedValueId = hfSmsNumber.ValueAsInt();

            if ( smsPhoneDefinedValueId == default( int ) )
            {
                return string.Empty;
            }

            var communicationResponseService = new CommunicationResponseService( new RockContext() );
            List<CommunicationRecipientResponse> responses = communicationResponseService.GetCommunicationConversation( recipientPersonAliasId, smsPhoneDefinedValueId.Value );

            BindConversationRepeater( responses );

            if ( responses.Any() )
            {
                return responses.Last().SMSMessage;
            }

            return string.Empty;
        }

        /// <summary>
        /// Binds the conversation repeater.
        /// </summary>
        /// <param name="responses">The responses.</param>
        private void BindConversationRepeater( List<CommunicationRecipientResponse> responses )
        {
            rptConversation.Visible = true;
            rptConversation.DataSource = responses;
            rptConversation.DataBind();
        }

        /// <summary>
        /// Populates the person lava.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void PopulatePersonLava( RowEventArgs e )
        {
            var hfRecipientPersonAliasId = ( HiddenField ) e.Row.FindControl( "hfRecipientPersonAliasId" );
            int? recipientPersonAliasId = hfSelectedRecipientPersonAliasId.Value.AsIntegerOrNull();

            var hfMessageKey = ( HiddenField ) e.Row.FindControl( "hfMessageKey" );
            var lblName = ( Label ) e.Row.FindControl( "lblName" );
            string html = lblName.Text;
            string unknownPerson = " (Unknown Person)";
            var lava = GetAttributeValue( AttributeKey.PersonInfoLavaTemplate );

            if ( !recipientPersonAliasId.HasValue || recipientPersonAliasId.Value == -1 )
            {
                // We don't have a person to do the lava merge so just display the formatted phone number
                html = PhoneNumber.FormattedNumber( string.Empty, hfMessageKey.Value ) + unknownPerson;
                litSelectedRecipientDescription.Text = html;
            }
            else
            {
                // Merge the person and lava
                using ( var rockContext = new RockContext() )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var recipientPerson = personAliasService.GetPerson( recipientPersonAliasId.Value );
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                    mergeFields.Add( "Person", recipientPerson );

                    html = lava.ResolveMergeFields( mergeFields );
                }
            }

            litSelectedRecipientDescription.Text = string.Format( "<div class='header-lava pull-left'>{0}</div>", html );
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            string keyPrefix = string.Format( "sms-conversations-{0}-", this.BlockId );

            if ( ddlSmsNumbers.Visible )
            {
                this.SetUserPreference( keyPrefix + "smsNumber", ddlSmsNumbers.SelectedValue.ToString() );
                hfSmsNumber.SetValue( ddlSmsNumbers.SelectedValue.AsInteger() );
            }
            else
            {
                this.SetUserPreference( keyPrefix + "smsNumber", hfSmsNumber.Value.ToString() );
            }

            this.SetUserPreference( keyPrefix + "showRead", tglShowRead.Checked.ToString() );
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message.</param>
        private void SendMessage( int toPersonAliasId, string message )
        {
            using ( var rockContext = new RockContext() )
            {
                // The sender is the logged in user.
                int fromPersonAliasId = CurrentUser.Person.PrimaryAliasId.Value;
                string fromPersonName = CurrentUser.Person.FullName;

                // The sending phone is the selected one
                DefinedValueCache fromPhone = DefinedValueCache.Get( hfSmsNumber.ValueAsInt() );

                string responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( rockContext );

                // Create and enqueue the communication
                Rock.Communication.Medium.Sms.CreateCommunicationMobile( CurrentUser.Person, toPersonAliasId, message, fromPhone, responseCode, rockContext );
            }
        }

        /// <summary>
        /// Updates the message part for the grid row with the selected message key with the provided message string.
        /// </summary>
        /// <param name="message">The message.</param>
        private void UpdateMessagePart( string message )
        {
            foreach ( GridViewRow row in gRecipients.Rows )
            {
                if ( row.RowType != DataControlRowType.DataRow )
                {
                    continue;
                }

                var messageKeyHiddenField = ( HiddenFieldWithClass ) row.FindControl( "hfMessageKey" );
                if ( messageKeyHiddenField.Value == hfSelectedMessageKey.Value )
                {
                    // This is our row, update the lit
                    Literal literal = ( Literal ) row.FindControl( "litMessagePart" );
                    literal.Text = message;
                    break;
                }
            }
        }

        #endregion private/protected Methods

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( LoadPhoneNumbers() )
            {
                nbNoNumbers.Visible = false;
                divMain.Visible = true;
                LoadResponseListing();
            }
            else
            {
                nbNoNumbers.Visible = true;
                divMain.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbLinkConversation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkConversation_Click( object sender, EventArgs e )
        {
            mdLinkToPerson.Title = string.Format( "Link Phone Number {0} to Person ", PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), hfSelectedMessageKey.Value, false ) );
            ppPerson.SetValue( null );
            newPersonEditor.SetFromPerson( null );
            mdLinkToPerson.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSmsNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSmsNumbers_SelectedIndexChanged( object sender, EventArgs e )
        {
            SaveSettings();
            LoadResponseListing();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglShowRead control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglShowRead_CheckedChanged( object sender, EventArgs e )
        {
            SaveSettings();
            LoadResponseListing();
        }

        /// <summary>
        /// Handles the Click event of the btnCreateNewMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateNewMessage_Click( object sender, EventArgs e )
        {
            lblMdNewMessageSendingSMSNumber.Text = ddlSmsNumbers.SelectedItem.Text;
            mdNewMessage.Show();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPersonFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPersonFilter_SelectPerson( object sender, EventArgs e )
        {
            if ( ppPersonFilter.PersonId != null )
            {
                lbPersonFilter.AddCssClass( "bg-warning" );
            }
            else
            {
                lbPersonFilter.RemoveCssClass( "bg-warning" );
            }

            LoadResponseListing( ppPersonFilter.PersonId );
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            string message = tbNewMessage.Text.Trim();

            if ( message.Length == 0 || hfSelectedRecipientPersonAliasId.Value == string.Empty )
            {
                return;
            }

            int toPersonAliasId = hfSelectedRecipientPersonAliasId.ValueAsInt();
            SendMessage( toPersonAliasId, message );
            tbNewMessage.Text = string.Empty;
            LoadResponsesForRecipient( toPersonAliasId );
            UpdateMessagePart( message );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdNewMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdNewMessage_SaveClick( object sender, EventArgs e )
        {
            string message = tbSMSTextMessage.Text.Trim();
            if ( message.IsNullOrWhiteSpace() )
            {
                return;
            }

            nbNoSms.Visible = false;

            int toPersonAliasId = ppRecipient.PersonAliasId.Value;
            var personAliasService = new PersonAliasService( new RockContext() );
            var toPerson = personAliasService.GetPerson( toPersonAliasId );
            if ( !toPerson.PhoneNumbers.Where( p => p.IsMessagingEnabled ).Any() )
            {
                nbNoSms.Visible = true;
                return;
            }

            SendMessage( toPersonAliasId, message );

            mdNewMessage.Hide();
            LoadResponseListing();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppRecipient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppRecipient_SelectPerson( object sender, EventArgs e )
        {
            nbNoSms.Visible = false;

            int toPersonAliasId = ppRecipient.PersonAliasId.Value;
            var personAliasService = new PersonAliasService( new RockContext() );
            var toPerson = personAliasService.GetPerson( toPersonAliasId );
            if ( !toPerson.PhoneNumbers.Where( p => p.IsMessagingEnabled ).Any() )
            {
                nbNoSms.Visible = true;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRecipients_RowSelected( object sender, RowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var hfRecipientPersonAliasId = ( HiddenField ) e.Row.FindControl( "hfRecipientPersonAliasId" );
            var hfMessageKey = ( HiddenField ) e.Row.FindControl( "hfMessageKey" );

            // Since we can get newer messages when a selected let's also update the message part on the response recipients grid.
            var litMessagePart = ( Literal ) e.Row.FindControl( "litMessagePart" );

            int? recipientPersonAliasId = hfRecipientPersonAliasId.Value.AsIntegerOrNull();
            string messageKey = hfMessageKey.Value;

            hfSelectedRecipientPersonAliasId.Value = recipientPersonAliasId.ToString();
            hfSelectedMessageKey.Value = hfMessageKey.Value;

            var rockContext = new RockContext();

            Person recipientPerson = null;
            if ( recipientPersonAliasId.HasValue )
            {
                recipientPerson = new PersonAliasService( rockContext ).GetPerson( recipientPersonAliasId.Value );
            }

            litMessagePart.Text = LoadResponsesForRecipient( recipientPersonAliasId.Value );

            int? smsPhoneDefinedValueId = hfSmsNumber.Value.AsIntegerOrNull();

            if ( smsPhoneDefinedValueId.HasValue && recipientPersonAliasId.HasValue )
            {
                new CommunicationResponseService( rockContext ).UpdateReadPropertyByFromPersonAliasId( recipientPersonAliasId.Value, smsPhoneDefinedValueId.Value );
            }

            tbNewMessage.Visible = true;
            btnSend.Visible = true;

            upConversation.Attributes.Add( "class", "conversation-panel has-focus" );

            foreach ( GridViewRow row in gRecipients.Rows )
            {
                row.RemoveCssClass( "selected" );
            }

            e.Row.AddCssClass( "selected" );
            e.Row.RemoveCssClass( "unread" );

            // We're checking nameless person first because we don't need to worry about the rest for non-nameless people.
            var isRecipientPartOfMergeRequest = recipientPerson.IsNameless() && recipientPerson.IsPartOfMergeRequest();

            if ( recipientPerson == null || ( recipientPerson.IsNameless() && !isRecipientPartOfMergeRequest ) )
            {
                lbLinkConversation.Visible = true;
                lbViewMergeRequest.Visible = false;
            }
            else
            {
                lbLinkConversation.Visible = false;
                lbViewMergeRequest.Visible = isRecipientPartOfMergeRequest;
            }

            PopulatePersonLava( e );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRecipients_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var hfRecipientPersonAliasId = e.Row.FindControl( "hfRecipientPersonAliasId" ) as HiddenField;
            var hfMessageKey = e.Row.FindControl( "hfMessageKey" ) as HiddenField;
            var lblName = e.Row.FindControl( "lblName" ) as Label;
            var litDateTime = e.Row.FindControl( "litDateTime" ) as Literal;
            var litMessagePart = e.Row.FindControl( "litMessagePart" ) as Literal;

            var responseListItem = e.Row.DataItem as CommunicationRecipientResponse;
            hfRecipientPersonAliasId.Value = responseListItem.RecipientPersonAliasId.ToString();
            hfMessageKey.Value = responseListItem.MessageKey;
            if ( responseListItem.IsNamelessPerson )
            {
                lblName.Text = PhoneNumber.FormattedNumber( null, responseListItem.MessageKey );
            }
            else
            {
                lblName.Text = responseListItem.FullName;
            }

            litDateTime.Text = responseListItem.HumanizedCreatedDateTime;
            litMessagePart.Text = responseListItem.SMSMessage;

            if ( !responseListItem.IsRead )
            {
                e.Row.AddCssClass( "unread" );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptConversation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptConversation_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            CommunicationRecipientResponse communicationRecipientResponse = e.Item.DataItem as CommunicationRecipientResponse;

            if ( communicationRecipientResponse != null )
            {
                var hfCommunicationRecipientId = ( HiddenFieldWithClass ) e.Item.FindControl( "hfCommunicationRecipientId" );
                hfCommunicationRecipientId.Value = communicationRecipientResponse.RecipientPersonAliasId.ToString();

                var hfCommunicationMessageKey = ( HiddenFieldWithClass ) e.Item.FindControl( "hfCommunicationMessageKey" );
                hfCommunicationMessageKey.Value = communicationRecipientResponse.MessageKey;

                var lSMSMessage = ( Literal ) e.Item.FindControl( "lSMSMessage" );
                lSMSMessage.Text = communicationRecipientResponse.SMSMessage;

                var lSenderName = ( Literal ) e.Item.FindControl( "lSenderName" );
                lSenderName.Text = communicationRecipientResponse.FullName;

                var lblMessageDateTime = ( Label ) e.Item.FindControl( "lblMessageDateTime" );
                lblMessageDateTime.ToolTip = communicationRecipientResponse.CreatedDateTime.ToString();
                lblMessageDateTime.Text = communicationRecipientResponse.HumanizedCreatedDateTime.ToString();

                if ( communicationRecipientResponse.MessageStatus == CommunicationRecipientStatus.Pending )
                {
                    lblMessageDateTime.Text += " (Pending)";
                }

                var divCommunication = ( HtmlGenericControl ) e.Item.FindControl( "divCommunication" );

                if ( communicationRecipientResponse.IsOutbound )
                {
                    divCommunication.RemoveCssClass( "inbound" );
                    divCommunication.AddCssClass( "outbound" );
                }
                else
                {
                    divCommunication.RemoveCssClass( "outbound" );
                    divCommunication.AddCssClass( "inbound" );
                }
            }
        }

        #endregion Control Events

        #region Link Conversation Modal

        /// <summary>
        /// Handles the SaveClick event of the mdLinkConversation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdLinkToPerson_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var personService = new PersonService( rockContext );

                // Get the Person Record from the selected conversation. (It should be a 'NamelessPerson' record type)
                int namelessPersonAliasId = hfSelectedRecipientPersonAliasId.Value.AsInteger();
                var phoneNumberService = new PhoneNumberService( rockContext );
                Person namelessPerson = personAliasService.GetPerson( namelessPersonAliasId );

                if ( namelessPerson == null )
                {
                    // shouldn't happen, but just in case
                    return;
                }

                EntitySet mergeRequest = null;
                if ( pnlLinkToExistingPerson.Visible )
                {
                    var existingPersonId = ppPerson.PersonId;
                    if ( !existingPersonId.HasValue )
                    {
                        return;
                    }

                    var existingPerson = personService.Get( existingPersonId.Value );
                    mergeRequest = namelessPerson.CreateMergeRequest( existingPerson );
                    var entitySetService = new EntitySetService( rockContext );
                    entitySetService.Add( mergeRequest );

                    rockContext.SaveChanges();
                    hfSelectedRecipientPersonAliasId.Value = existingPerson.PrimaryAliasId.ToString();
                }
                else
                {
                    // new Person and new family
                    var newPerson = new Person();

                    newPersonEditor.UpdatePerson( newPerson, rockContext );

                    personService.Add( newPerson );
                    rockContext.SaveChanges();

                    mergeRequest = namelessPerson.CreateMergeRequest( newPerson );
                    var entitySetService = new EntitySetService( rockContext );
                    entitySetService.Add( mergeRequest );
                    rockContext.SaveChanges();

                    hfSelectedRecipientPersonAliasId.Value = newPerson.PrimaryAliasId.ToString();
                }

                RedirectToMergeRequest( mergeRequest );

            }

            mdLinkToPerson.Hide();
            LoadResponseListing();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglLinkPersonMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglLinkPersonMode_CheckedChanged( object sender, EventArgs e )
        {
            pnlLinkToExistingPerson.Visible = tglLinkPersonMode.Checked;
            pnlLinkToNewPerson.Visible = !tglLinkPersonMode.Checked;
        }

        #endregion Link Conversation Modal

        /// <summary>
        /// Handles the Click event of the lbViewMergeRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbViewMergeRequest_Click( object sender, EventArgs e )
        {
            var namelessPersonAliasId = hfSelectedRecipientPersonAliasId.Value.AsInteger();

            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var namelessPerson = personAliasService.GetPerson( namelessPersonAliasId );

                var mergeRequest = namelessPerson.GetMergeRequest( rockContext );

                RedirectToMergeRequest( mergeRequest );
            }
        }

        /// <summary>
        /// Redirects to merge request.
        /// </summary>
        /// <param name="mergeRequest">The merge request.</param>
        private void RedirectToMergeRequest( EntitySet mergeRequest )
        {
            if ( mergeRequest != null )
            {
                Page.Response.Redirect( string.Format( "~/PersonMerge/{0}", mergeRequest.Id ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}