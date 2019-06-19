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
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
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
    [DefinedValueField( definedTypeGuid: Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
        name: "Allowed SMS Numbers",
        description: "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        required: false,
        allowMultiple: true,
        order: 1,
        key: "AllowedSMSNumbers" )]
    [BooleanField( "Show only personal SMS number",
        description: "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        defaultValue: false,
        order: 2,
        key: "ShowOnlyPersonalSmsNumber" )]
    [BooleanField( "Hide personal SMS numbers",
        description: "Only SMS Numbers that are not associated with a person. The numbers without a 'ResponseRecipient' attribute value.",
        defaultValue: false,
        order: 3,
        key: "HidePersonalSmsNumbers" )]
    [BooleanField( "Enable SMS Send",
        description: "Allow SMS messages to be sent from the block.",
        defaultValue: true,
        order: 4,
        key: "EnableSmsSend" )]
    [IntegerField( name: "Show Conversations From Months Ago",
        description: "Limits the conversations shown in the left pane to those of X months ago or newer. This does not affect the actual messages shown on the right.",
        defaultValue: 6,
        order: 5,
        key: "ShowConversationsFromMonthsAgo" )]
    [CodeEditorField( "Person Info Lava Template",
        description: "A Lava template to display person information about the selected Communication Recipient.",
        defaultValue: "{{ Person.FullName }}",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 300,
        required: false,
        order: 6,
        key: "PersonInfoLavaTemplate" )]

    // Start here to build the person description lit field after selecting recipient.
    public partial class SmsConversations : RockBlock
    {
        #region Control Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( mdLinkConversation.Visible )
            {
                string script = string.Format(
                    @"

    $('#{0}').on('click', function () {{

        // if Save was clicked, set the fields that should be validated based on what tab they are on
        if ($('#{9}').val() == 'Existing') {{
            enableRequiredField( '{1}', true )
            enableRequiredField( '{2}_rfv', false );
            enableRequiredField( '{3}_rfv', false );
            enableRequiredField( '{4}', false );
            enableRequiredField( '{5}', false );
            enableRequiredField( '{6}_rfv', false );
            enableRequiredField( '{10}_rfv', false );
        }} else {{
            enableRequiredField('{1}', false)
            enableRequiredField('{2}_rfv', true);
            enableRequiredField('{3}_rfv', true);
            enableRequiredField('{4}', true);
            enableRequiredField('{5}', true);
            enableRequiredField('{6}_rfv', true);
            enableRequiredField('{10}_rfv', true);
        }}

        // update the scrollbar since our validation box could show
        setTimeout( function ()
        {{
            Rock.dialogs.updateModalScrollBar( '{7}' );
        }});

    }})

    $('a[data-toggle=""pill""]').on('shown.bs.tab', function (e) {{

        var tabHref = $( e.target ).attr( 'href' );
        if ( tabHref == '#{8}' )
        {{
            $( '#{9}' ).val( 'Existing' );
        }} else {{
            $( '#{9}' ).val( 'New' );
        }}

        // if the validation error summary is shown, hide it when they switch tabs
        $( '#{7}' ).hide();
    }});
",
                    mdLinkConversation.ServerSaveLink.ClientID,                         // {0}
                    ppPerson.RequiredFieldValidator.ClientID,                       // {1}
                    tbNewPersonFirstName.ClientID,                                  // {2}
                    tbNewPersonLastName.ClientID,                                   // {3}
                    rblNewPersonRole.RequiredFieldValidator.ClientID,               // {4}
                    rblNewPersonGender.RequiredFieldValidator.ClientID,             // {5}
                    dvpNewPersonConnectionStatus.ClientID,                          // {6}
                    valSummaryAddPerson.ClientID,                                   // {7}
                    divExistingPerson.ClientID,                                     // {8}
                    hfActiveTab.ClientID,                                           // {9}
                    dpNewPersonBirthDate.ClientID );                                // {10}

                ScriptManager.RegisterStartupScript( mdLinkConversation, mdLinkConversation.GetType(), "modaldialog-validation", script, true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            HtmlMeta preventPhoneMetaTag = new HtmlMeta
            {
                Name = "format-detection",
                Content = "telephone=no"
            };
            RockPage.AddMetaTag( this.Page, preventPhoneMetaTag );

            this.BlockUpdated += Block_BlockUpdated;

            btnCreateNewMessage.Visible = this.GetAttributeValue( "EnableSmsSend" ).AsBoolean();
            dvpNewPersonTitle.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ).Id;
            dvpNewPersonSuffix.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).Id;
            dvpNewPersonMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
            dvpNewPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;

            var groupType = GroupTypeCache.GetFamilyGroupType();
            rblNewPersonRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
            rblNewPersonRole.DataBind();

            rblNewPersonGender.Items.Clear();
            rblNewPersonGender.Items.Add( new ListItem( Gender.Male.ConvertToString(), Gender.Male.ConvertToInt().ToString() ) );
            rblNewPersonGender.Items.Add( new ListItem( Gender.Female.ConvertToString(), Gender.Female.ConvertToInt().ToString() ) );
            rblNewPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
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
            else
            {
                if ( postbackArgs == "cancel" )
                {
                    HideDialog();
                }
                else
                {
                    ShowDialog();
                }
            }

            if ( !string.IsNullOrWhiteSpace( hfActiveTab.Value ) )
            {
                SetActiveTab();
                mdLinkConversation.Show();
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

            var selectedNumberGuids = GetAttributeValue( "AllowedSMSNumbers" ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) ).ToList();
            }

            // filter personal numbers (any that have a response recipient) if the hide personal option is enabled
            if ( GetAttributeValue( "HidePersonalSmsNumbers" ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( v => v.GetAttributeValue( "ResponseRecipient" ).IsNullOrWhiteSpace() ).ToList();
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( "ShowOnlyPersonalSmsNumber" ).AsBoolean() && !IsUserAuthorized( Authorization.ADMINISTRATE ) )
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

        /// <summary>
        /// Loads the response listing.
        /// </summary>
        private void LoadResponseListing()
        {
            // NOTE: The FromPersonAliasId is the person who sent a text from a mobile device to Rock.
            // This person is also referred to as the Recipient because they are responding to a
            // communication from Rock. Restated the response is from the recipient of a communication.

            // This is the person lava field, we want to clear it because reloading this list will deselect the user.
            litSelectedRecipientDescription.Text = string.Empty;
            hfSelectedRecipientId.Value = string.Empty;
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

                DataSet responses = null;
                int months = GetAttributeValue( "ShowConversationsFromMonthsAgo" ).AsInteger();

                if ( tglShowRead.Checked )
                {
                    responses = communicationResponseService.GetCommunicationsAndResponseRecipients( smsPhoneDefinedValueId.Value, months );
                }
                else
                {
                    // Since communications sent from Rock are always considered "Read" we don't need them included in the list if we are not showing "Read" messages.
                    responses = communicationResponseService.GetResponseRecipients( smsPhoneDefinedValueId.Value, false, months );
                }

                var responseListItems = responses.Tables[0].AsEnumerable()
                    .Select( r => new ResponseListItem
                    {
                        RecipientId = r.Field<int?>( "FromPersonAliasId" ),
                        MessageKey = r.Field<string>( "MessageKey" ),
                        FullName = r.Field<string>( "FullName" ),
                        CreatedDateTime = r.Field<DateTime>( "CreatedDateTime" ),
                        HumanizedCreatedDateTime = HumanizeDateTime( r.Field<DateTime>( "CreatedDateTime" ) ),
                        SMSMessage = r.Field<string>( "SMSMessage" ),
                        IsRead = r.Field<bool>( "IsRead" )
                    } )
                    .ToList();

                // don't display conversations if we're rebinding the recipient list
                rptConversation.Visible = false;
                gRecipients.DataSource = responseListItems;
                gRecipients.DataBind();
            }
        }

        /// <summary>
        /// Loads the responses for recipient.
        /// </summary>
        /// <param name="recipientId">The recipient identifier.</param>
        /// <returns></returns>
        private string LoadResponsesForRecipient( int recipientId )
        {
            int? smsPhoneDefinedValueId = hfSmsNumber.ValueAsInt();

            if ( smsPhoneDefinedValueId == default( int ) )
            {
                return string.Empty;
            }

            var communicationResponseService = new CommunicationResponseService( new RockContext() );
            var responses = communicationResponseService.GetConversation( recipientId, smsPhoneDefinedValueId.Value );

            BindConversationRepeater( responses );

            DataRow row = responses.Tables[0].AsEnumerable().Last();
            return row["SMSMessage"].ToString();
        }

        /// <summary>
        /// Loads the responses for recipient.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        /// <returns></returns>
        private string LoadResponsesForRecipient( string messageKey )
        {
            int? smsPhoneDefinedValueId = hfSmsNumber.ValueAsInt();

            if ( smsPhoneDefinedValueId == default( int ) )
            {
                return string.Empty;
            }

            var communicationResponseService = new CommunicationResponseService( new RockContext() );
            var responses = communicationResponseService.GetConversation( messageKey, smsPhoneDefinedValueId.Value );

            BindConversationRepeater( responses );

            DataRow row = responses.Tables[0].AsEnumerable().Last();
            return row["SMSMessage"].ToString();
        }

        /// <summary>
        /// Binds the conversation repeater.
        /// </summary>
        /// <param name="responses">The responses.</param>
        private void BindConversationRepeater( DataSet responses )
        {
            var communicationItems = responses.Tables[0].AsEnumerable()
                .Select( r => new ResponseListItem
                {
                    RecipientId = r.Field<int?>( "FromPersonAliasId" ),
                    MessageKey = r.Field<string>( "MessageKey" ),
                    FullName = r.Field<string>( "FullName" ),
                    CreatedDateTime = r.Field<DateTime>( "CreatedDateTime" ),
                    HumanizedCreatedDateTime = HumanizeDateTime( r.Field<DateTime>( "CreatedDateTime" ) ),
                    SMSMessage = r.Field<string>( "SMSMessage" ),
                    IsRead = r.Field<bool>( "IsRead" )
                } )
                .ToList();

            rptConversation.Visible = true;
            rptConversation.DataSource = communicationItems;
            rptConversation.DataBind();
        }

        /// <summary>
        /// Humanizes the date time to relative if not on the same day and short time if it is.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        private string HumanizeDateTime( DateTime? dateTime )
        {
            if ( dateTime == null )
            {
                return string.Empty;
            }

            DateTime dtCompare = RockDateTime.Now;

            if ( dtCompare.Date == dateTime.Value.Date )
            {
                return dateTime.Value.ToShortTimeString();
            }

            // Method Name "Truncate" collision between Humanizer and Rock ExtensionMethods so have to call as a static with full name.
            return Humanizer.DateHumanizeExtensions.Humanize( dateTime, true, dtCompare, null );
        }

        /// <summary>
        /// Populates the person lava.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void PopulatePersonLava( RowEventArgs e )
        {
            var recipientId = ( HiddenField ) e.Row.FindControl( "hfRecipientId" );
            var messageKey = ( HiddenField ) e.Row.FindControl( "hfMessageKey" );
            var fullName = ( Label ) e.Row.FindControl( "lblName" );
            string html = fullName.Text;
            string unknownPerson = " (Unknown Person)";
            var lava = GetAttributeValue( "PersonInfoLavaTemplate" );

            if ( recipientId.Value.IsNullOrWhiteSpace() || recipientId.Value == "-1" )
            {
                // We don't have a person to do the lava merge so just display the formatted phone number
                html = PhoneNumber.FormattedNumber( string.Empty, messageKey.Value ) + unknownPerson;
                litSelectedRecipientDescription.Text = html;
            }
            else
            {
                // Merge the person and lava
                using ( var rockContext = new RockContext() )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var recipientPerson = personAliasService.GetPerson( recipientId.ValueAsInt() );
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                    mergeFields.Add( "Person", recipientPerson );

                    html = lava.ResolveMergeFields( mergeFields );
                }
            }

            litSelectedRecipientDescription.Text = string.Format( "<div class='header-lava pull-left'>{0}</div>", html );
        }

        /// <summary>
        /// Updates the read property.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        private void UpdateReadProperty( string messageKey )
        {
            int? smsPhoneDefinedValueId = hfSmsNumber.ValueAsInt();

            if ( smsPhoneDefinedValueId == default( int ) )
            {
                return;
            }

            new CommunicationResponseService( new RockContext() ).UpdateReadPropertyByMessageKey( messageKey, smsPhoneDefinedValueId.Value );
        }

        /// <summary>
        /// POCO to store communication info
        /// </summary>
        protected class ResponseListItem
        {
            public int? RecipientId { get; set; }

            public string MessageKey { get; set; }

            public string FullName { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public string HumanizedCreatedDateTime { get; set; }

            public string SMSMessage { get; set; }

            public bool IsRead { get; set; }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MDNEWMESSAGE":
                    mdNewMessage.Show();
                    lblMdNewMessageSendingSMSNumber.Text = ddlSmsNumbers.SelectedItem.Text;
                    break;
                case "MDLINKCONVERSATION":
                    mdLinkConversation.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MDNEWMESSAGE":
                    ppRecipient.SetValue( null );
                    tbSMSTextMessage.Text = string.Empty;
                    nbNoSms.Visible = false;

                    mdNewMessage.Hide();
                    break;
                case "MDLINKCONVERSATION":
                    ppPerson.SetValue( null );
                    nbAddPerson.Visible = false;
                    dvpNewPersonTitle.ClearSelection();
                    tbNewPersonFirstName.Text = string.Empty;
                    tbNewPersonLastName.Text = string.Empty;
                    dvpNewPersonSuffix.ClearSelection();
                    dvpNewPersonConnectionStatus.ClearSelection();
                    rblNewPersonRole.ClearSelection();
                    rblNewPersonGender.ClearSelection();
                    dpNewPersonBirthDate.SelectedDate = null;
                    ddlGradePicker.ClearSelection();
                    dvpNewPersonMaritalStatus.ClearSelection();

                    mdLinkConversation.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            string keyPrefix = string.Format( "sms-conversations-{0}-", this.BlockId );

            if ( ddlSmsNumbers.Visible )
            {
                this.SetUserPreference( keyPrefix + "smsNumber", ddlSmsNumbers.SelectedValue.ToString(), false );
                hfSmsNumber.SetValue( ddlSmsNumbers.SelectedValue.AsInteger() );
            }
            else
            {
                this.SetUserPreference( keyPrefix + "smsNumber", hfSmsNumber.Value.ToString(), false );
            }
            this.SetUserPreference( keyPrefix + "showRead", tglShowRead.Checked.ToString(), false );
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
            ShowDialog( "mdLinkConversation" );
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
            ShowDialog( "mdNewMessage" );
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            string message = tbNewMessage.Text.Trim();

            if ( message.Length == 0 || hfSelectedRecipientId.Value == string.Empty )
            {
                return;
            }

            int toPersonAliasId = hfSelectedRecipientId.ValueAsInt();
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

            HideDialog();
            LoadResponseListing();
        }

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

            var recipientId = ( HiddenField ) e.Row.FindControl( "hfRecipientId" );
            var messageKey = ( HiddenField ) e.Row.FindControl( "hfMessageKey" );

            // Since we can get newer messages when a selected let's also update the message part on the response recipients grid.
            var litMessagePart = ( Literal ) e.Row.FindControl( "litMessagePart" );

            hfSelectedRecipientId.Value = recipientId.Value;
            hfSelectedMessageKey.Value = messageKey.Value;

            if ( recipientId.Value == "-1" )
            {
                litMessagePart.Text = LoadResponsesForRecipient( messageKey.Value );
            }
            else
            {
                litMessagePart.Text = LoadResponsesForRecipient( recipientId.ValueAsInt() );
            }

            UpdateReadProperty( messageKey.Value );
            tbNewMessage.Visible = true;
            btnSend.Visible = true;

            upConversation.Attributes.Add( "class", "conversation-panel has-focus" );

            foreach ( GridViewRow row in gRecipients.Rows )
            {
                row.RemoveCssClass( "selected" );
            }

            e.Row.AddCssClass( "selected" );
            e.Row.RemoveCssClass( "unread" );

            if ( recipientId.Value == "-1" )
            {
                lbLinkConversation.Visible = true;
            }
            else
            {
                lbLinkConversation.Visible = false;
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

            var dataItem = e.Row.DataItem;
            if ( !( bool ) dataItem.GetPropertyValue( "IsRead" ) )
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
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var messageKey = ( HiddenFieldWithClass ) e.Item.FindControl( "hfCommunicationMessageKey" );
                if ( messageKey.Value != string.Empty )
                {
                    var divCommunication = ( HtmlGenericControl ) e.Item.FindControl( "divCommunication" );
                    divCommunication.RemoveCssClass( "outbound" );
                    divCommunication.AddCssClass( "inbound" );
                }
            }
        }

        #endregion Control Events

        #region Link Conversation Modal
        /// <summary>
        /// Sets the active tab.
        /// </summary>
        private void SetActiveTab()
        {
            if ( hfActiveTab.Value == "Existing" )
            {
                liNewPerson.RemoveCssClass( "active" );
                divNewPerson.RemoveCssClass( "active" );
                liExistingPerson.AddCssClass( "active" );
                divExistingPerson.AddCssClass( "active" );
            }
            else
            {
                liNewPerson.AddCssClass( "active" );
                divNewPerson.AddCssClass( "active" );
                liExistingPerson.RemoveCssClass( "active" );
                divExistingPerson.RemoveCssClass( "active" );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdLinkConversation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdLinkConversation_SaveClick( object sender, EventArgs e )
        {
            // Do some validation on entering a new person/family first
            if ( hfActiveTab.Value != "Existing" )
            {
                var validationMessages = new List<string>();
                bool isValid = true;

                DateTime? birthdate = dpNewPersonBirthDate.SelectedDate;
                if ( !dpNewPersonBirthDate.IsValid )
                {
                    validationMessages.Add( "Birthdate is not valid." );
                    isValid = false;
                }

                if ( !isValid )
                {
                    if ( validationMessages.Any() )
                    {
                        nbAddPerson.Text = "<ul><li>" + validationMessages.AsDelimited( "</li><li>" ) + "</li></lu>";
                        nbAddPerson.Visible = true;
                    }

                    return;
                }
            }

            using ( var rockContext = new RockContext() )
            {
                int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                if ( hfActiveTab.Value == "Existing" )
                {
                    if ( ppPerson.PersonId.HasValue )
                    {
                        // All we need to do here is add the mobile phone number and save
                        var personService = new PersonService( rockContext );
                        var person = personService.Get( ppPerson.PersonId.Value );
                        bool hasSmsNumber = person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).Any();
                        var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneTypeId );

                        if ( phoneNumber == null )
                        {
                            phoneNumber = new PhoneNumber
                            {
                                NumberTypeValueId = mobilePhoneTypeId,
                                IsMessagingEnabled = !hasSmsNumber,
                                Number = hfSelectedMessageKey.Value
                            };

                            person.PhoneNumbers.Add( phoneNumber );
                        }
                        else
                        {
                            phoneNumber.Number = hfSelectedMessageKey.Value;
                            if ( !hasSmsNumber )
                            {
                                // If they don't have a number then use this one, otherwise don't do anything
                                phoneNumber.IsMessagingEnabled = true;
                            }
                        }

                        rockContext.SaveChanges();
                        hfSelectedRecipientId.Value = person.PrimaryAliasId.ToString();
                    }
                }
                else
                {
                    // new Person and new family
                    var person = new Person();

                    person.TitleValueId = dvpNewPersonTitle.SelectedValueAsId();
                    person.FirstName = tbNewPersonFirstName.Text;
                    person.NickName = tbNewPersonFirstName.Text;
                    person.LastName = tbNewPersonLastName.Text;
                    person.SuffixValueId = dvpNewPersonSuffix.SelectedValueAsId();
                    person.Gender = rblNewPersonGender.SelectedValueAsEnum<Gender>();
                    person.MaritalStatusValueId = dvpNewPersonMaritalStatus.SelectedValueAsInt();

                    person.PhoneNumbers = new List<PhoneNumber>();
                    var phoneNumber = new PhoneNumber
                    {
                        NumberTypeValueId = mobilePhoneTypeId,
                        IsMessagingEnabled = true,
                        Number = hfSelectedMessageKey.Value
                    };

                    person.PhoneNumbers.Add( phoneNumber );

                    var birthMonth = person.BirthMonth;
                    var birthDay = person.BirthDay;
                    var birthYear = person.BirthYear;

                    var birthday = dpNewPersonBirthDate.SelectedDate;
                    if ( birthday.HasValue )
                    {
                        person.BirthMonth = birthday.Value.Month;
                        person.BirthDay = birthday.Value.Day;
                        if ( birthday.Value.Year != DateTime.MinValue.Year )
                        {
                            person.BirthYear = birthday.Value.Year;
                        }
                        else
                        {
                            person.BirthYear = null;
                        }
                    }
                    else
                    {
                        person.SetBirthDate( null );
                    }

                    person.GradeOffset = ddlGradePicker.SelectedValueAsInt();
                    person.ConnectionStatusValueId = dvpNewPersonConnectionStatus.SelectedValueAsId();

                    var groupMember = new GroupMember();
                    groupMember.GroupRoleId = rblNewPersonRole.SelectedValueAsInt() ?? 0;
                    groupMember.Person = person;

                    var groupMembers = new List<GroupMember>();
                    groupMembers.Add( groupMember );

                    Group group = GroupService.SaveNewFamily( rockContext, groupMembers, null, true );
                    hfSelectedRecipientId.Value = person.PrimaryAliasId.ToString();
                }

                new CommunicationResponseService( rockContext ).UpdatePersonAliasByMessageKey( hfSelectedRecipientId.ValueAsInt(), hfSelectedMessageKey.Value, PersonAliasType.FromPersonAlias );
            }

            ppPerson.Required = false;
            tbNewPersonFirstName.Required = false;
            tbNewPersonLastName.Required = false;
            rblNewPersonRole.Required = false;
            rblNewPersonGender.Required = false;
            dvpNewPersonConnectionStatus.Required = false;

            hfActiveTab.Value = string.Empty;

            mdLinkConversation.Hide();
            HideDialog();
            LoadResponseListing();
        }

        #endregion Link Conversation Modal


    }
}