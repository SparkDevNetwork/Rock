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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "SMS Conversations" )]
    [Category( "Communication" )]
    [Description( "Block for having SMS Conversations between an SMS enabled phone and a Rock SMS Phone number that has 'Enable Mobile Conversations' set to false." )]
    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
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
        Description = "When enabled, only SMS Numbers that are not 'Assigned to a person' will be shown.",
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

    [NoteTypeField( "Note Types",
        Description = "Optional list of note types to limit the note editor to.",
        AllowMultiple = true,
        IsRequired = false,
        EntityType = typeof( Rock.Model.Person ),
        Order = 7,
        Key = AttributeKey.NoteTypes )]


    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 8 )]

    [Rock.SystemGuid.BlockTypeGuid( "3497603B-3BE6-4262-B7E9-EC01FC7140EB" )]
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
            public const string NoteTypes = "NoteTypes";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
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

            RockPage.AddCSSLink( "~/Styles/Blocks/Communication/SmsConversations.css" );
            RockPage.AddMetaTag( this.Page, preventPhoneMetaTag );

            this.BlockUpdated += Block_BlockUpdated;
            noteEditor.SaveButtonClick += noteEditor_SaveButtonClick;
            ConfigureNoteEditor();

            btnCreateNewMessage.Visible = this.GetAttributeValue( AttributeKey.EnableSmsSend ).AsBoolean();

            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            int databaseTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( spn => selectedNumberGuids.Contains( spn.Guid ) ).ToList();
            }

            // filter personal numbers (any that have a response recipient) if the hide personal option is enabled
            if ( GetAttributeValue( AttributeKey.HidePersonalSmsNumbers ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( spn => !spn.AssignedToPersonAliasId.HasValue ).ToList();
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( AttributeKey.ShowOnlyPersonalSmsNumber ).AsBoolean() && !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                smsNumbers = smsNumbers.Where( spn => CurrentPerson.Aliases.Any( a => a.Id == spn.AssignedToPersonAliasId ) ).ToList();
            }

            if ( smsNumbers.Any() )
            {
                var smsDetails = smsNumbers.Select( spn => new
                {
                    spn.Id,
                    Description = spn.Name
                } );

                ddlSmsNumbers.DataSource = smsDetails;
                ddlSmsNumbers.Visible = smsNumbers.Count() > 1;
                ddlSmsNumbers.DataValueField = "Id";
                ddlSmsNumbers.DataTextField = "Description";
                ddlSmsNumbers.DataBind();

                ddlMessageFilter.BindToEnum<CommunicationMessageFilter>();

                var preferences = GetBlockPersonPreferences();

                string smsNumberUserPref = preferences.GetValue( "smsNumber" );

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

                ddlMessageFilter.SelectedValue = preferences.GetValue( "messageFilter" ).IfEmpty( CommunicationMessageFilter.ShowUnreadReplies.ToString() );
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
            hfSelectedConversationKey.Value = string.Empty;
            tbNewMessage.Visible = false;
            btnSend.Visible = false;
            btnEditNote.Visible = false;
            lbShowImagePicker.Visible = false;
            noteEditor.Visible = false;

            var smsSystemPhoneNumberId = hfSmsNumber.ValueAsInt();
            if ( smsSystemPhoneNumberId == 0 )
            {
                return;
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

                    var communicationResponseService = new CommunicationResponseService( rockContext );

                    int months = GetAttributeValue( AttributeKey.ShowConversationsFromMonthsAgo ).AsInteger();

                    var startDateTime = RockDateTime.Now.AddMonths( -months );

                    var maxConversations = this.GetAttributeValue( AttributeKey.MaxConversations ).AsIntegerOrNull() ?? 1000;
                    var messageFilterOption = ddlMessageFilter.SelectedValue.ConvertToEnum<CommunicationMessageFilter>();

                    var responseListItems = communicationResponseService.GetCommunicationAndResponseRecipients( smsSystemPhoneNumberId, startDateTime, maxConversations, messageFilterOption, personId );

                    // don't display conversations if we're rebinding the recipient list
                    rptConversation.Visible = false;
                    gRecipients.DataSource = responseListItems;
                    gRecipients.DataBind();
                }
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                if ( sqlTimeoutException != null )
                {
                    nbError.NotificationBoxType = NotificationBoxType.Warning;
                    nbError.Text = "Unable to load SMS responses in a timely manner. You can try again or adjust the timeout setting of this block.";
                    nbError.Visible = true;
                    return;
                }
                else
                {
                    nbError.NotificationBoxType = NotificationBoxType.Danger;
                    nbError.Text = "An error occurred when loading SMS responses";
                    nbError.Details = ex.Message;
                    nbError.Visible = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Loads the responses for recipient.
        /// </summary>
        /// <param name="recipientPersonId">The recipient person identifier.</param>
        /// <returns></returns>
        private string LoadResponsesForRecipientPerson( int recipientPersonId )
        {
            var smsSystemPhoneNumberId = hfSmsNumber.ValueAsInt();
            var smsSystemPhoneNumber = smsSystemPhoneNumberId != 0
                ? SystemPhoneNumberCache.Get( smsSystemPhoneNumberId )
                : null;

            if ( smsSystemPhoneNumber == null )
            {
                return string.Empty;
            }

            try
            {
                var rockContext = new RockContext();
                rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
                var communicationResponseService = new CommunicationResponseService( rockContext );
                List<CommunicationRecipientResponse> responses = communicationResponseService.GetCommunicationConversationForPerson( recipientPersonId, smsSystemPhoneNumber );

                BindConversationRepeater( responses );

                if ( responses.Any() )
                {
                    var responseListItem = responses.Last();

                    if ( responseListItem.SMSMessage.IsNullOrWhiteSpace() && responseListItem.HasAttachments( rockContext ) )
                    {
                        return "Rock-Image-File";
                    }

                    return responses.Last().SMSMessage;
                }
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                var errorBox = nbError;

                if ( sqlTimeoutException != null )
                {
                    nbError.NotificationBoxType = NotificationBoxType.Warning;
                    nbError.Text = "Unable to load SMS responses for recipient in a timely manner. You can try again or adjust the timeout setting of this block.";
                    return string.Empty;
                }
                else
                {
                    errorBox.NotificationBoxType = NotificationBoxType.Danger;
                    nbError.Text = "An error occurred when loading SMS responses for recipient";
                    errorBox.Details = ex.Message;
                    errorBox.Visible = true;
                    return string.Empty;
                }
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
            int? recipientPersonAliasId = hfSelectedRecipientPersonAliasId.Value.AsIntegerOrNull();

            var hfPhoneNumber = ( HiddenField ) e.Row.FindControl( "hfPhoneNumber" );
            var lblName = ( Label ) e.Row.FindControl( "lblName" );
            string html = lblName.Text;
            string unknownPerson = " (Unknown Person)";
            var lava = GetAttributeValue( AttributeKey.PersonInfoLavaTemplate );

            if ( !recipientPersonAliasId.HasValue || recipientPersonAliasId.Value == -1 )
            {
                // We don't have a person to do the lava merge so just display the formatted phone number
                html = PhoneNumber.FormattedNumber( string.Empty, hfPhoneNumber.Value ) + unknownPerson;
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
            var preferences = GetBlockPersonPreferences();

            if ( ddlSmsNumbers.Visible )
            {
                preferences.SetValue( "smsNumber", ddlSmsNumbers.SelectedValue.ToString() );
                hfSmsNumber.SetValue( ddlSmsNumbers.SelectedValue.AsInteger() );
            }
            else
            {
                preferences.SetValue( "smsNumber", hfSmsNumber.Value.ToString() );
            }

            preferences.SetValue( "messageFilter", ddlMessageFilter.SelectedValue );

            preferences.Save();
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="toPersonId">To person identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="newMessage">if set to <c>true</c> [new message].</param>
        private void SendMessageToPerson( int toPersonId, string message, bool newMessage )
        {
            using ( var rockContext = new RockContext() )
            {
                // The sender is the logged in user.
                int fromPersonAliasId = CurrentUser.Person.PrimaryAliasId.Value;
                string fromPersonName = CurrentUser.Person.FullName;

                // The sending phone is the selected one
                var fromPhone = SystemPhoneNumberCache.Get( hfSmsNumber.ValueAsInt() );

                string responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( rockContext );

                BinaryFile binaryFile = null;
                List<BinaryFile> photos = null;

                if ( !newMessage && ImageUploaderConversation.BinaryFileId.IsNotNullOrZero() )
                {
                    // If this is a response using the conversation window and a photo file has been uploaded then add it
                    binaryFile = new BinaryFileService( rockContext ).Get( ImageUploaderConversation.BinaryFileId.Value );
                }
                else if ( newMessage && ImageUploaderModal.BinaryFileId.IsNotNullOrZero() )
                {
                    // If this is a new message using the modal and a photo file has been uploaded then add it
                    binaryFile = new BinaryFileService( rockContext ).Get( ImageUploaderModal.BinaryFileId.Value );
                }

                photos = binaryFile != null ? new List<BinaryFile> { binaryFile } : null;

                var toPrimaryAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( toPersonId );

                // Create and enqueue the communication
                Rock.Communication.Medium.Sms.CreateCommunicationMobile( CurrentUser.Person, toPrimaryAliasId, message, fromPhone, responseCode, photos, rockContext );
                ImageUploaderConversation.BinaryFileId = null;
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

                var conversationKeyHiddenField = ( HiddenFieldWithClass ) row.FindControl( "hfConversationKey" );
                if ( conversationKeyHiddenField.Value == hfSelectedConversationKey.Value )
                {
                    Literal literal = ( Literal ) row.FindControl( "litMessagePart" );

                    // This is our row, update the lit
                    if ( message == "Rock-Image-File" )
                    {
                        literal.Text = "Image";
                        row.AddCssClass( "latest-message-is-image" );
                    }
                    else
                    {
                        literal.Text = message;
                    }

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
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the lbLinkConversation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkConversation_Click( object sender, EventArgs e )
        {
            mdLinkToPerson.Title = string.Format( "Link Phone Number {0} to Person ", PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), hfSelectedPhoneNumber.Value, false ) );
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
        /// Handles the Message filter changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMessageFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            SaveSettings();
            LoadResponseListing();
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            string message = tbNewMessage.Text.Trim();

            if ( hfSelectedRecipientPersonAliasId.Value == string.Empty || ( message.Length == 0 && ImageUploaderConversation.BinaryFileId.IsNullOrZero() ) )
            {
                return;
            }

            int toPersonAliasId = hfSelectedRecipientPersonAliasId.ValueAsInt();

            int? toPersonId = new PersonAliasService( new RockContext() ).GetPersonId( toPersonAliasId );
            if ( !toPersonId.HasValue )
            {
                return;
            }

            SendMessageToPerson( toPersonId.Value, message, false );
            tbNewMessage.Text = string.Empty;
            LoadResponsesForRecipientPerson( toPersonId.Value );
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

            int toPersonId = ppRecipient.PersonId.Value;
            var personService = new PersonService( new RockContext() );
            var personHasSMSNumbers = personService.GetSelect( toPersonId, s => s.PhoneNumbers.Where( a => a.IsMessagingEnabled ).Any() );
            if ( !personHasSMSNumbers )
            {
                nbNoSms.Visible = true;
                return;
            }

            SendMessageToPerson( toPersonId, message, true );

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
            var senderClearButton = ( HtmlButton ) sender;
            if (senderClearButton != null && senderClearButton.ID == "btnSelectNone" )
            {
                // The PersonPicker clear button was clicked so no need to check for SMS numbers
                return;
            }

            if ( ppRecipient.PersonAliasId.HasValue )
            {
                int toPersonAliasId = ppRecipient.PersonAliasId.Value;
                var personAliasService = new PersonAliasService( new RockContext() );
                var toPerson = personAliasService.GetPerson( toPersonAliasId );
                if ( !toPerson.PhoneNumbers.Where( p => p.IsMessagingEnabled ).Any() )
                {
                    nbNoSms.Visible = true;
                }
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
            var hfConversationKey = ( HiddenField ) e.Row.FindControl( "hfConversationKey" );
            var hfPhoneNumber = ( HiddenField ) e.Row.FindControl( "hfPhoneNumber" );

            // Since we can get newer messages when a selected let's also update the message part on the response recipients grid.
            var litMessagePart = ( Literal ) e.Row.FindControl( "litMessagePart" );

            int? recipientPersonAliasId = hfRecipientPersonAliasId.Value.AsIntegerOrNull();

            hfSelectedRecipientPersonAliasId.Value = recipientPersonAliasId.ToString();
            hfSelectedConversationKey.Value = hfConversationKey.Value;
            hfSelectedPhoneNumber.Value = hfPhoneNumber.Value;

            var rockContext = new RockContext();

            Person recipientPerson = null;
            if ( recipientPersonAliasId.HasValue )
            {
                recipientPerson = new PersonAliasService( rockContext ).GetPerson( recipientPersonAliasId.Value );
            }

            if ( recipientPerson == null )
            {
                return;
            }

            noteEditor.Visible = false;
            var messagePart = LoadResponsesForRecipientPerson( recipientPerson.Id );
            if ( messagePart == "Rock-Image-File" )
            {
                litMessagePart.Text = "Image";
                e.Row.AddCssClass( "latest-message-is-image" );
            }
            else
            {
                litMessagePart.Text = messagePart;
            }

            var smsSystemPhoneNumberId = hfSmsNumber.Value.AsIntegerOrNull();
            var smsSystemPhoneNumber = smsSystemPhoneNumberId.HasValue
                ? SystemPhoneNumberCache.Get( smsSystemPhoneNumberId.Value )
                : null;

            if ( smsSystemPhoneNumber != null && recipientPersonAliasId.HasValue )
            {
                new CommunicationResponseService( rockContext ).UpdateReadPropertyByFromPersonId( recipientPerson.Id, smsSystemPhoneNumber );
            }

            tbNewMessage.Visible = true;
            btnSend.Visible = true;
            btnEditNote.Visible = true;
            lbShowImagePicker.Visible = true;

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
            var hfConversationKey = e.Row.FindControl( "hfConversationKey" ) as HiddenField;
            var hfPhoneNumber = e.Row.FindControl( "hfPhoneNumber" ) as HiddenField;
            var lblName = e.Row.FindControl( "lblName" ) as Label;
            var litDateTime = e.Row.FindControl( "litDateTime" ) as Literal;
            var litMessagePart = e.Row.FindControl( "litMessagePart" ) as Literal;

            var responseListItem = e.Row.DataItem as CommunicationRecipientResponse;
            hfRecipientPersonAliasId.Value = responseListItem.RecipientPersonAliasId.ToString();
            hfConversationKey.Value = responseListItem.ConversationKey;
            hfPhoneNumber.Value = responseListItem.ContactKey;
            if ( responseListItem.IsNamelessPerson )
            {
                lblName.Text = PhoneNumber.FormattedNumber( null, responseListItem.ContactKey );
            }
            else
            {
                lblName.Text = responseListItem.FullName;
            }

            litDateTime.Text = responseListItem.HumanizedCreatedDateTime;
            litMessagePart.Text = responseListItem.SMSMessage;

            if ( responseListItem.SMSMessage.IsNullOrWhiteSpace() && responseListItem.HasAttachments( new RockContext() ) )
            {
                litMessagePart.Text = "Image";
                e.Row.AddCssClass( "latest-message-is-image" );
            }

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
                var hfCommunicationRecipientPersonAliasId = ( HiddenFieldWithClass ) e.Item.FindControl( "hfCommunicationRecipientPersonAliasId" );
                hfCommunicationRecipientPersonAliasId.Value = communicationRecipientResponse.RecipientPersonAliasId.ToString();

                var hfCommunicationConversationKey = ( HiddenFieldWithClass ) e.Item.FindControl( "hfCommunicationConversationKey" );
                hfCommunicationConversationKey.Value = communicationRecipientResponse.ConversationKey;

                var lSMSMessage = ( Literal ) e.Item.FindControl( "lSMSMessage" );
                if ( communicationRecipientResponse.SMSMessage.IsNullOrWhiteSpace() )
                {
                    var divCommunicationBody = ( HtmlControl ) e.Item.FindControl( "divCommunicationBody" );
                    divCommunicationBody.Visible = false;
                }
                else
                {
                    lSMSMessage.Text = communicationRecipientResponse.SMSMessage;
                }

                var rockContext = new RockContext();

                if ( communicationRecipientResponse.HasAttachments( rockContext ) )
                {
                    var lSMSAttachments = ( Literal ) e.Item.FindControl( "lSMSAttachments" );
                    string applicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );

                    foreach ( var binaryFileGuid in communicationRecipientResponse.GetBinaryFileGuids( rockContext ) )
                    {
                        // Show the image thumbnail by appending the html to lSMSMessage.Text
                        string imageUrl = FileUrlHelper.GetImageUrl( binaryFileGuid, new GetImageUrlOptions{ Width = 200 } );
                        string imageElement = $"<a href='{imageUrl}' target='_blank' rel='noopener noreferrer'><img src='{imageUrl}' class='img-responsive sms-image'></a>";

                        // If there is a text portion or previous image then drop down a line before appending the image element
                        lSMSAttachments.Text += imageElement;
                    }
                }

                var lSenderName = ( Literal ) e.Item.FindControl( "lSenderName" );
                lSenderName.Text = communicationRecipientResponse.OutboundSenderFullName;

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

        #region Edit Note

        /// <summary>
        /// Configures the note editor.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        private void ConfigureNoteEditor()
        {
            var noteTypes = NoteTypeCache.GetByEntity( EntityTypeCache.GetId<Rock.Model.Person>(), string.Empty, string.Empty, true );

            // If block is configured to only allow certain note types, limit notes to those types.
            var configuredNoteTypes = GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();
            if ( configuredNoteTypes.Any() )
            {
                noteTypes = noteTypes.Where( n => configuredNoteTypes.Contains( n.Guid ) ).ToList();
            }

            NoteOptions noteOptions = new NoteOptions( this.ViewState )
            {
                NoteTypes = noteTypes.ToArray(),
                AddAlwaysVisible = true,
                DisplayType = NoteDisplayType.Full,
                ShowAlertCheckBox = true,
                ShowPrivateCheckBox = true,
                UsePersonIcon = true,
                ShowSecurityButton = false,
                ShowCreateDateInput = false,
            };

            noteEditor.SetNoteOptions( noteOptions );
            noteEditor.NoteTypeId = noteTypes.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Handles the Click event of the btnEditNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEditNote_Click( object sender, EventArgs e )
        {
            noteEditor.Style.Remove( "display" );
            noteEditor.Visible = true;
            noteEditor.ShowEditMode = true;

            var selectedPersonId = new PersonAliasService( new RockContext() ).GetPersonId( hfSelectedRecipientPersonAliasId.Value.AsInteger() );
            var note = new Note
            {
                EntityId = selectedPersonId,
                CreatedByPersonAlias = this.CurrentPersonAlias
            };

            noteEditor.SetNote( note );
        }

        /// <summary>
        /// Handles the SaveButtonClick event of the noteEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NoteEventArgs"/> instance containing the event data.</param>
        private void noteEditor_SaveButtonClick( object sender, NoteEventArgs e )
        {
            noteEditor.Visible = false;
            noteEditor.ShowEditMode = false;
        }

        #endregion Edit Note
    }
}