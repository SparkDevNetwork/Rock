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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "System Communication Preview" )]
    [Category( "Communication" )]
    [Description( "Create a preview and send a test message for the given system communication using the selected date and target person." )]

    #region Block Attributes

    [SystemCommunicationField( "System Communication",
        Description = "The system communication to use when previewing the message. When set as a block setting, it will not allow overriding by the query string.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.SystemCommunication )]

    [DaysOfWeekField( "Send Day of the Week",
        Description = "Used to determine which dates to list in the Message Date drop down. <i><strong>Note:</strong> If no day is selected the Message Date drop down will not be shown and the ‘SendDateTime’ Lava variable will be set to the current day.</i>",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.SendDaysOfTheWeek )]

    [IntegerField( "Number of Previous Weeks to Show",
        Description = "How many previous weeks to show in the drop down.",
        DefaultIntegerValue = 6,
        Order = 3,
        Key = AttributeKey.PreviousWeeksToShow )]

    [IntegerField( "Number of Future Weeks to Show",
        Description = "How many weeks ahead to show in the drop down.",
        DefaultIntegerValue = 1,
        Order = 4,
        Key = AttributeKey.FutureWeeksToShow )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled.",
        IsRequired = false,
        Key = AttributeKey.EnabledLavaCommands,
        Order = 5 )]

    [CodeEditorField( "Lava Template Append",
        Description = "This Lava will be appended to the system communication template to help setup any data that the template needs. This data would typically be passed to the template by a job or other means.",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.LavaTemplateAppend,
        Order = 6)]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "95366DA1-D878-4A9A-A26F-83160DBE784F" )]
    public partial class CommunicationJobPreview : RockBlock
    {
        #region Fields

        internal bool HasSendDate { get; set; }
        internal bool HasSystemCommunication = false;
        internal bool HasTargetPerson = false;
        #endregion

        #region Page Constants
        private static class PageConstants
        {
            public const string LavaDebugCommand = "{{ 'Lava' | Debug }}";

            public const string EmailContainerHtml = @"
                                <div id='divEmailPreview'
                                    class='email-preview js-email-preview overflow-auto' style='position: relative; height: 720px;'>
                                    <iframe name='emailPreview' src='javascript: window.frameElement.getAttribute('srcdoc');'
                                        id='ifEmailPreview' name='emailpreview-iframe'
                                        class='emaileditor-iframe inset-0 w-100 js-emailpreview-iframe email-wrapper email-content-desktop styled-scroll' frameborder='0' border='0' cellspacing='0'
                                        scrolling='no' srcdoc='[SOURCE_REPLACEMENT]''></iframe>
                                    <div class='resize-sensor'
                                        style='position: absolute; inset: 0px; overflow: scroll; z-index: -1; visibility: hidden;'>
                                        <div class='resize-sensor-expand'
                                            style='position: absolute; left: 0; top: 0; right: 0; bottom: 0; overflow: scroll; z-index: -1; visibility: hidden;'>
                                            <div style='position: absolute; left: 0px; top: 0px; width: 388px; height: 574px;'></div>
                                        </div>
                                        <div class='resize-sensor-shrink'
                                            style='position: absolute; left: 0; top: 0; right: 0; bottom: 0; overflow: scroll; z-index: -1; visibility: hidden;'>
                                            <div style='position: absolute; left: 0; top: 0; width: 200%; height: 200%'></div>
                                        </div>
                                    </div>
                                </div>";
            public const string SystemCommunicationSourceReplacementKey = "[SOURCE_REPLACEMENT]";
        }
        #endregion

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string SystemCommunicationId = "SystemCommunicationId";
            public const string PublicationDate = "PublicationDate";
            public const string TargetPersonId = "TargetPersonId";
        }

        #endregion Page Parameter Keys

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string SystemCommunication = "SystemCommunication";
            public const string SendDaysOfTheWeek = "SendDaysOfTheWeek";
            public const string PreviousWeeksToShow = "PreviousWeeksToShow";
            public const string FutureWeeksToShow = "FutureWeeksToShow";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string LavaTemplateAppend = "LavaTemplateAppend";
        }

        #endregion Attribute Keys

        #region Merge Field Keys

        private static class MergeFieldKey
        {
            public const string SendDateTime = "SendDateTime";
            public const string Person = "Person";
        }

        #endregion Merge Field Keys

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string SystemCommunicationGuid = "SystemCommunicationGuid";
            public const string TargetPersonId = "TargetPersonId";
            public const string SelectedDate = "SelectedDate";
        }

        #endregion ViewState Keys

        #region Page Events
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            BuildUI();
        }

        #endregion Page Events

        #region Control Events

        /// <summary>
        /// Handles the Click event of the lbUpdate control.
        /// </summary>
        protected void lbUpdate_Click( object sender, EventArgs e )
        {
            var mergeInfo = BuildSystemCommunication();

            var source = mergeInfo.RockEmailMessageRecord.Message
                .ResolveMergeFields( mergeInfo.MergeFields, null, EnabledLavaCommands )
                .EncodeHtml();

            lContent.Text = PageConstants.EmailContainerHtml.Replace( PageConstants.SystemCommunicationSourceReplacementKey, source );

            UpdateSendDateUrlParam();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMessageDate control.
        /// </summary>
        protected void ddlMessageDate_SelectedIndexChanged( object sender, EventArgs e )
        {
            ViewState[ViewStateKey.SelectedDate] = ddlMessageDate.SelectedIndex;
            HasSendDate = ViewState[ViewStateKey.SelectedDate].ToIntSafe() > 0;

            UpdateSendDateUrlParam();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppTargetPerson control.
        /// </summary>
        protected void ppTargetPerson_SelectPerson( object sender, EventArgs e )
        {
            var targetPersonValue = ppTargetPerson.SelectedValue.GetValueOrDefault( 0 );
            if ( targetPersonValue > 0 )
            {
                ViewState[ViewStateKey.TargetPersonId] = targetPersonValue;
            }

            UpdateTargetPersonUrlParam();
        }

        /// <summary>
        /// Handles the Click event of the btnSendEmail control.
        /// </summary>
        protected void btnSendEmail_Click( object sender, EventArgs e )
        {
            nbSendTest.Visible = false;
            mdSendTest.Show();
            ebSendTest.Text = CurrentPerson.Email;
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSendTest control.
        /// </summary>
        protected void mdSendTest_SaveClick( object sender, EventArgs e )
        {
            const bool DISABLE_PERSON_HISTORY = true;
            string currentEmail = CurrentPerson.Email;
            using ( var rockContext = new RockContext() )
            {
                rockContext.WrapTransactionIf( () =>
                {
                    try
                    {
                        var mergeInfo = BuildSystemCommunication();

                        var rockEmailMessage = mergeInfo.RockEmailMessageRecord;

                        if ( rockEmailMessage == null )
                        {
                            throw new Exception( $"A valid system communication was not selected." );
                        }

                        if ( rockEmailMessage.SystemCommunicationId.GetValueOrDefault( 0 ) == 0 )
                        {
                            throw new Exception( $"The system communication specified is not valid." );
                        }

                        var emailPerson = GetTargetPerson( rockContext );
                        var originalEmail = emailPerson.Email;

                        // Remove the lava debug command if it is specified in the message template.
                        var message = rockEmailMessage.Message.Replace( PageConstants.LavaDebugCommand, string.Empty );

                        rockEmailMessage.AdditionalMergeFields = mergeInfo.MergeFields.ToDictionary( k => k.Key, v => ( object ) v.Value );
                        rockEmailMessage.CurrentPerson = emailPerson;
                        rockEmailMessage.Message = message;

                        var sendErrorMessages = new List<string>();

                        // Set person email to the email specified in the dialog
                        emailPerson.Email = ebSendTest.Text;
                        rockContext.SaveChanges( disablePrePostProcessing: DISABLE_PERSON_HISTORY );

                        var recipient = new RockEmailMessageRecipient( emailPerson, mergeInfo.MergeFields );
                        rockEmailMessage.AddRecipient( recipient );
                        rockEmailMessage.Send( out sendErrorMessages );

                        if ( sendErrorMessages.Count == 0 )
                        {
                            nbSendTest.Text = $"Email submitted to <i>{recipient.EmailAddress}</i>";
                            nbSendTest.NotificationBoxType = NotificationBoxType.Info;
                            nbSendTest.Visible = true;
                        }
                        else
                        {
                            var errorString = $"<ul>[ERRORS]</ul>";
                            var sbError = new StringBuilder();

                            foreach ( var error in sendErrorMessages )
                            {
                                sbError.AppendLine( $"<li>{error}</li>" );
                            }

                            errorString = errorString.Replace( "[ERRORS]", sbError.ToString() );

                            nbSendTest.Text = errorString;
                            nbSendTest.NotificationBoxType = NotificationBoxType.Danger;
                            nbSendTest.Visible = true;
                        }

                        // Restore email to original email address
                        emailPerson.Email = originalEmail;
                        rockContext.SaveChanges( disablePrePostProcessing: DISABLE_PERSON_HISTORY );
                    }
                    catch ( Exception ex )
                    {
                        nbSendTest.Text = ex.Message;
                        nbSendTest.NotificationBoxType = NotificationBoxType.Danger;
                        nbSendTest.Visible = true;
                        return false;
                    }

                    return true;

                } ); // End transaction
            }
        }

        private SystemCommunicationMergeInfo BuildSystemCommunication()
        {
            var systemCommunication = GetSystemCommunication();
            var rockEmailMessage = GetRockEmailMessage();

            var targetPerson = GetTargetPerson();

            var mergeFields = systemCommunication.LavaFields
                .ToDictionary( k => k.Key, v => ( object ) v.Value );

            DateTime? messageDate = null;

            if ( HasSendDate )
            {
                messageDate = GetSendDateValue();
                mergeFields.AddOrReplace( MergeFieldKey.SendDateTime, $"{messageDate.Value:MMMM d, yyyy}" );
            }

            var mergeFieldOptions = new Rock.Lava.CommonMergeFieldsOptions { GetCurrentPerson = true };
            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson, mergeFieldOptions );

            mergeFields.AddOrReplace( MergeFieldKey.Person, targetPerson );

            mergeFields = mergeFields
                .Concat( commonMergeFields.Where( kvp => !mergeFields.ContainsKey( kvp.Key ) ) )
                .ToDictionary( k => k.Key, v => v.Value );

            if ( rockEmailMessage.Message.Contains( PageConstants.LavaDebugCommand ) )
            {
                lavaDebug.Visible = true;
                lavaDebug.Text = mergeFields.lavaDebugInfo();

                // Remove Lava Debug from the message content before it gets sent
                rockEmailMessage.Message = rockEmailMessage.Message.Replace( PageConstants.LavaDebugCommand, string.Empty );
            }
            else
            {
                lavaDebug.Visible = false;
                lavaDebug.Text = string.Empty;
            }

            // If certain fields are blank, then use the Global Attribute values to mimic the EmailTransportComponent.cs behavior.
            var globalAttributes = GlobalAttributesCache.Get();

            // Email - From Name
            if ( systemCommunication.FromName.IsNullOrWhiteSpace() )
            {
                systemCommunication.FromName = globalAttributes.GetValue( "OrganizationName" );
            }
            rockEmailMessage.FromName = ResolveText( systemCommunication.FromName, rockEmailMessage.CurrentPerson, rockEmailMessage.EnabledLavaCommands, mergeFields );

            // Email - From Address
            if ( systemCommunication.From.IsNullOrWhiteSpace() )
            {
                systemCommunication.From = globalAttributes.GetValue( "OrganizationEmail" );
            }
            rockEmailMessage.FromEmail = ResolveText( systemCommunication.From, rockEmailMessage.CurrentPerson, rockEmailMessage.EnabledLavaCommands, mergeFields )
                .Left( 255 );

            // Email - Subject - Max length - RFC 2822 is 998 characters
            rockEmailMessage.Subject = ResolveText( systemCommunication.Subject, rockEmailMessage.CurrentPerson, rockEmailMessage.EnabledLavaCommands, mergeFields )
                .Left( 998 );

            if ( rockEmailMessage.Subject.IsNotNullOrWhiteSpace() )
            {
                // Remove carriage returns and line feeds
                systemCommunication.Subject = Regex.Replace( rockEmailMessage.Subject, @"(\r?\n?)",
                    string.Empty );
            }

            systemCommunication.LavaFields = mergeFields.ToDictionary( k => k.Key, v => v.Value?.ToString() );

            return new SystemCommunicationMergeInfo
            {
                MergeFields = mergeFields,
                SystemCommunicationRecord = systemCommunication,
                RockEmailMessageRecord = rockEmailMessage
            };
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Builds the UI.
        /// </summary>
        private void BuildUI()
        {
            var previewInfo = SetSystemCommunication();

            nbMessage.Visible = !HasSystemCommunication;

            if ( !HasSystemCommunication )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                var communicationSource = previewInfo.ParameterSettingType == SystemCommunicationPreviewInfo.ParameterSettingTypeEnum.BlockSetting ? "was not specified in the block setting or is invalid" : $"was not specified in the block setting or using the [{PageParameterKey.SystemCommunicationId}] url parameter";
                nbMessage.Text = $"A communication template {communicationSource}.";

                EnableControls( false );
            }
            else
            {
                EnableControls();

                SetTargetPerson();

                BuildDateDropDown();

                var mergeInfo = BuildSystemCommunication();
                var systemCommunication = mergeInfo.SystemCommunicationRecord;
                var emailMessage = mergeInfo.RockEmailMessageRecord;

                var fromName = emailMessage.FromName.IsNotNullOrWhiteSpace() ? emailMessage.FromName : CurrentPerson.FullName;
                var fromEmail = emailMessage.FromEmail.IsNotNullOrWhiteSpace() ? emailMessage.FromEmail : CurrentPerson.Email;

                lTitle.Text = systemCommunication.Title;
                lNavTitle.Text = systemCommunication.Title;
                lFrom.Text = $"<span class='text-semibold'>{emailMessage.FromName}</span> <span class='text-muted'>&lt;{emailMessage.FromEmail}&gt;</span>";
                lSubject.Text = $"<span class='text-semibold'>{emailMessage.Subject}</small>";


                var messageDateTime = GetSendDateValue();

                lDate.Text = $"<span class='text-semibold'>{messageDateTime:MMMM d, yyyy}</span>";

                string source = mergeInfo.RockEmailMessageRecord.Message
                    .ResolveMergeFields( mergeInfo.MergeFields, null, EnabledLavaCommands )
                    .EncodeHtml();

                lContent.Text = PageConstants.EmailContainerHtml.Replace( PageConstants.SystemCommunicationSourceReplacementKey, source );
            }
        }

        /// <summary>
        /// Controls the state.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void EnableControls( bool enabled = true )
        {
            ddlMessageDate.Enabled = enabled;
            ppTargetPerson.Enabled = enabled;
            btnSendEmail.Enabled = enabled;
            btnDesktop.Disabled = !enabled;
            btnMobile.Disabled = !enabled;
            lbUpdate.Enabled = enabled;
        }

        /// <summary>
        /// Builds the date drop down.
        /// </summary>
        private void BuildDateDropDown()
        {
            var systemCommunication = GetSystemCommunication();

            var dayOfWeeks = GetAttributeValues( AttributeKey.SendDaysOfTheWeek )?
                .Select( dow => ( DayOfWeek ) Enum.Parse( typeof( DayOfWeek ), dow ) );

            var previousWeeks = GetAttributeValue( AttributeKey.PreviousWeeksToShow ).ToIntSafe();
            var futureWeeks = GetAttributeValue( AttributeKey.FutureWeeksToShow ).ToIntSafe();

            HasSendDate = systemCommunication.Body.Contains( $"{{ {MergeFieldKey.SendDateTime} }}" );

            ddlMessageDate.Visible = HasSendDate && dayOfWeeks.Count() > 0;

            if ( ddlMessageDate.Visible )
            {
                ddlMessageDate.Required = true;

                if ( !Page.IsPostBack )
                {
                    var startDate = RockDateTime.Now.AddDays( -( previousWeeks * 7 ) );
                    var endDate = RockDateTime.Now.AddDays( futureWeeks * 7 );
                    int previousMonth = 0;

                    for ( var dt = startDate; dt <= endDate; dt = dt.AddDays( 1 ) )
                    {
                        if ( dayOfWeeks.Contains( dt.DayOfWeek ) )
                        {
                            ddlMessageDate.Items.Add( new ListItem
                            {
                                Text = dt.ToString( "MMMM d, yyyy" ),
                                Value = $"{dt:MMddyyyy}"
                            } );

                            previousMonth = dt.Month;
                        }
                    }

                    ddlMessageDate.SelectedIndex = 0;

                    // Set the date from the query string param
                    var inputDate = DateTime.Now;
                    var publicationDate = PageParameter( PageParameterKey.PublicationDate ).AsDateTime();

                    if ( publicationDate.HasValue )
                    {
                        var publicationDateValue = publicationDate.Value.ToString( "MMddyyyy" );
                        var incomingDateItem = ddlMessageDate.Items.FindByValue( publicationDateValue );
                        if ( incomingDateItem != null )
                        {
                            ddlMessageDate.SelectedValue = incomingDateItem.Value;
                        }

                        inputDate = publicationDate.Value;
                    }


                    // Find the closest date
                    var allDates = new List<DateTime>();

                    foreach ( ListItem dateItem in ddlMessageDate.Items )
                    {
                        DateTime dateItemValue = DateTime.MinValue;

                        if ( DateTime.TryParse( dateItem.Text, out dateItemValue ) )
                        {
                            allDates.Add( dateItemValue );
                        }
                    }

                    if ( allDates != null )
                    {
                        allDates = allDates.OrderBy( d => d ).ToList();

                        var closestDate = inputDate >= allDates.Last()
                            ? allDates.Last()
                            : inputDate <= allDates.First()
                                ? allDates.First()
                                : allDates.First( d => d.ToDateKey() >= inputDate.ToDateKey() );

                        ddlMessageDate.SelectedValue = ddlMessageDate.Items.FindByValue( closestDate.ToString( "MMddyyyy" ) ).Value;
                    }
                    else
                    {
                        ddlMessageDate.SelectedIndex = 0;
                    }

                    ViewState[ViewStateKey.SelectedDate] = ddlMessageDate.SelectedIndex;
                }
            }
            else
            {
                ddlMessageDate.Required = false;
                ViewState[ViewStateKey.SelectedDate] = -1;
            }
        }

        /// <summary>
        /// Sets the system communication.
        /// </summary>
        /// <returns>JobPreviewInfo.</returns>
        private SystemCommunicationPreviewInfo SetSystemCommunication()
        {
            var previewInfo = new SystemCommunicationPreviewInfo();

            var systemCommunicationGuid = GetAttributeValue( AttributeKey.SystemCommunication ).AsGuid();

            if ( !systemCommunicationGuid.IsEmpty() )
            {
                previewInfo.ParameterSettingType = SystemCommunicationPreviewInfo.ParameterSettingTypeEnum.BlockSetting;
                ViewState[ViewStateKey.SystemCommunicationGuid] = systemCommunicationGuid;
            }
            else
            {
                previewInfo.ParameterSettingType = SystemCommunicationPreviewInfo.ParameterSettingTypeEnum.QueryStringParameter;
                var systemCommunicationId = PageParameter( PageParameterKey.SystemCommunicationId ).AsInteger();
                if ( systemCommunicationId > 0 )
                {
                    var systemCommunicationService = new SystemCommunicationService( new RockContext() );
                    var systemCommunication = systemCommunicationService.Get( systemCommunicationId );
                    if ( systemCommunication != null )
                    {
                        systemCommunicationGuid = systemCommunication.Guid;
                        ViewState[ViewStateKey.SystemCommunicationGuid] = systemCommunicationGuid;
                    }
                }
            }
            HasSystemCommunication = ViewState[ViewStateKey.SystemCommunicationGuid].ToStringSafe().Length > 0;
            return previewInfo;
        }

        private SystemCommunication GetSystemCommunication()
        {
            var systemCommunicationGuid = ViewState[ViewStateKey.SystemCommunicationGuid].ToStringSafe().AsGuid();
            if ( systemCommunicationGuid != Guid.Empty )
            {
                var communicationService = new SystemCommunicationService( new RockContext() );
                return communicationService.Get( systemCommunicationGuid );
            }

            return null;
        }

        private RockEmailMessage GetRockEmailMessage()
        {
            var systemCommunicationGuid = ViewState[ViewStateKey.SystemCommunicationGuid].ToStringSafe().AsGuid();
            if ( systemCommunicationGuid != Guid.Empty )
            {
                var rockMessage = new RockEmailMessage( systemCommunicationGuid )
                {
                    AppRoot = ResolveRockUrl( "~/" ),
                    ThemeRoot = ResolveRockUrl( "~~/" )
                };

                // Append the Lava from the block settings that will be used to setup any data that a job
                // or other code would typically provide.
                var appendTemplate = GetAttributeValue( AttributeKey.LavaTemplateAppend );
                if ( appendTemplate.IsNotNullOrWhiteSpace() )
                {
                    rockMessage.Message = appendTemplate + rockMessage.Message;
                }

                return rockMessage;
            }

            return null;
        }

        private Person GetTargetPerson( RockContext rockContext = null )
        {
            var personId = ViewState[ViewStateKey.TargetPersonId].ToIntSafe();
            if ( personId > 0 )
            {
                var personService = new PersonService( rockContext ?? new RockContext() );
                return personService.Get( personId );
            }

            return null;
        }

        private ListItem GetSelectedDate()
        {
            var selectedIndex = ViewState[ViewStateKey.SelectedDate].ToIntSafe();
            if ( selectedIndex > 0 )
            {
                return ddlMessageDate.Items[selectedIndex];
            }

            return null;
        }

        /// <summary>
        /// Sets the target person.
        /// </summary>
        private void SetTargetPerson()
        {
            var targetPersonId = PageParameter( PageParameterKey.TargetPersonId ).ToIntSafe();

            if ( targetPersonId > 0 )
            {
                ViewState[ViewStateKey.TargetPersonId] = targetPersonId;
            }
            else
            {
                var targetPersonValue = ppTargetPerson.SelectedValue.GetValueOrDefault( 0 );
                if ( targetPersonValue > 0 )
                {
                    ViewState[ViewStateKey.TargetPersonId] = targetPersonValue;
                }
                else
                {
                    ViewState[ViewStateKey.TargetPersonId] = CurrentPerson.Id;
                }
            }

            HasTargetPerson = ViewState[ViewStateKey.TargetPersonId].ToIntSafe() > 0;

            if ( HasTargetPerson )
            {
                targetPersonId = ViewState[ViewStateKey.TargetPersonId].ToIntSafe();
                var person = new PersonService( new RockContext() ).Get( targetPersonId );
                ppTargetPerson.SetValue( person );
            }
        }

        /// <summary>
        /// Used to mimic the behavior of the EmailTransportComponent.
        /// </summary>
        private string ResolveText( string content, Person person, string enabledLavaCommands, Dictionary<string, object> mergeFields )
        {
            if ( content.IsNullOrWhiteSpace() )
            {
                return content;
            }

            string value = content.ResolveMergeFields( mergeFields, person, enabledLavaCommands );

            return value;
        }

        /// <summary>
        /// Updates the target person URL parameter.
        /// </summary>
        /// <param name="targetPersonValue">The target person value.</param>
        private void UpdateTargetPersonUrlParam( bool redirect = true )
        {
            var pageParms = PageParameters();

            if ( pageParms != null )
            {
                pageParms.AddOrReplace( PageParameterKey.TargetPersonId, ViewState[ViewStateKey.TargetPersonId] );

                if ( redirect )
                {
                    NavigateToCurrentPage( pageParms.ToDictionary( kv => kv.Key, kv => kv.Value.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Updates the send date URL parameter.
        /// </summary>
        private void UpdateSendDateUrlParam(bool redirect=true)
        {
            var pageParms = PageParameters();

            if ( pageParms != null )
            {
                var messageDate = GetSendDateValue();

                pageParms.AddOrReplace( PageParameterKey.PublicationDate, messageDate.ToString( "MM-dd-yyyy" ) );

                lDate.Text = $"<span class='text-semibold'>{messageDate:MMMM d, yyyy}</span>";

                if ( redirect )
                {
                    NavigateToCurrentPage( pageParms.ToDictionary( kv => kv.Key, kv => kv.Value.ToString() ) );
                }
            }
        }
        private DateTime GetSendDateValue()
        {
            var selectedDate = GetSelectedDate();

            DateTime messageDate;
            if ( !DateTime.TryParseExact( selectedDate?.Value, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out messageDate ) )
            {
                messageDate = RockDateTime.Now;
            }
            else
            {
                return messageDate;
            }

            var publicationDate = PageParameter( PageParameterKey.PublicationDate ).AsDateTime();
            if ( publicationDate.HasValue )
            {
                messageDate = publicationDate.Value;
            }

            return messageDate;
        }
        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        protected string EnabledLavaCommands => GetAttributeValue( AttributeKey.EnabledLavaCommands );

        #endregion

        #region Classes

        /// <summary>
        /// Class SystemCommunicationMergeInfo.
        /// </summary>
        internal class SystemCommunicationMergeInfo
        {
            /// <summary>
            /// Gets or sets the merge fields.
            /// </summary>
            /// <value>The merge fields.</value>
            internal Dictionary<string, object> MergeFields { get; set; }

            /// <summary>
            /// Gets or sets the system communication record.
            /// </summary>
            /// <value>The system communication record.</value>
            internal SystemCommunication SystemCommunicationRecord { get; set; }

            /// <summary>
            /// Gets or sets the rock email message record.
            /// </summary>
            /// <value>The rock email message record.</value>
            internal RockEmailMessage RockEmailMessageRecord { get; set; }
        }

        /// <summary>
        /// Class SystemCommunicationPreviewInfo.
        /// </summary>
        internal class SystemCommunicationPreviewInfo
        {
            /// <summary>
            /// Enum ParameterSettingTypeEnum
            /// </summary>
            internal enum ParameterSettingTypeEnum
            {
                BlockSetting,
                QueryStringParameter
            }

            /// <summary>
            /// Gets or sets the type of the parameter setting.
            /// </summary>
            /// <value>The type of the parameter setting.</value>
            internal ParameterSettingTypeEnum ParameterSettingType { get; set; }
        }

        #endregion
    }
}