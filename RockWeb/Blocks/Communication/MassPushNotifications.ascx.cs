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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.UI.Controls.Communication;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// A block for creating and sending a mass push notification to recipients.
    /// </summary>
    [DisplayName( "Mass Push Notifications" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a mass push notification to recipients." )]

    #region Block Attributes

    [IntegerField( "Personal Device Active Duration",
        Key = AttributeKey.PersonalDeviceActiveDuration,
        Description = "The number of days that the device must have an interaction in order for it to be considered an active device.",
        IsRequired = true,
        DefaultIntegerValue = 365,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Order = 1 )]

    [BooleanField( "Send Immediately",
        Key = AttributeKey.SendImmediately,
        Description = "Should communication be sent right away (vs. just being queued for scheduled job to send)?",
        DefaultBooleanValue = true,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MASS_PUSH_NOTIFICATIONS )]
    public partial class MassPushNotifications : RockBlock
    {
        #region User Preference Keys

        /// <summary>
        /// User Preference key definitions for this block.
        /// </summary>
        private static class UserPreference
        {
            /// <summary>
            /// The category communication template preference key.
            /// </summary>
            public const string CategoryCommunicationTemplate = "CategoryCommunicationTemplate";
        }

        #endregion

        #region Attribute Keys

        /// <summary>
        /// Attribute key definitions for this block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The number of days that the device must have an interaction in order for it to be considered an active device.
            /// </summary>
            public const string PersonalDeviceActiveDuration = "PersonalDeviceActiveDuration";

            /// <summary>
            /// The Lava commands that should be enabled for this block.
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            /// <summary>
            /// Should communication be sent right away (vs. just being queued for scheduled job to send)?
            /// </summary>
            public const string SendImmediately = "SendImmediately";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The push medium identifier.
        /// </summary>
        private readonly int _pushMediumId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() ).Id;

        /// <summary>
        /// <c>true</c> if the push transport is enabled and configured.
        /// </summary>
        private readonly bool _pushTransportEnabled = MediumContainer.HasActivePushTransport();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the communication identifier.
        /// </summary>
        /// <value>
        /// The communication identifier.
        /// </value>
        protected int? CommunicationId
        {
            get
            {
                return hfCommunicationId.Value.AsIntegerOrNull();
            }
            set
            {
                hfCommunicationId.Value = value.ToStringSafe();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the template list is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the template list is empty; otherwise, <c>false</c>.
        /// </value>
        protected bool IsTemplateListEmpty
        {
            get
            {
                return ( bool? ) ViewState["IsTemplateListEmpty"] ?? false;
            }
            set
            {
                ViewState["IsTemplateListEmpty"] = value;
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

            var mediumControl = MediumControl.GetMediumControl( CommunicationType.PushNotification );

            mediumControl.ID = "mediumControl";
            mediumControl.IsTemplate = false;
            mediumControl.ValidationGroup = vsPushEditor.ValidationGroup;

            phPushControl.Controls.Add( mediumControl );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( !_pushTransportEnabled )
                {
                    nbNoMediumError.Visible = true;
                    base.OnLoad( e );
                    return;
                }

                ShowTemplateSelection();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets up a communication model with the user-entered values
        /// </summary>
        /// <returns>The communication for this session.</returns>
        private Rock.Model.Communication SetupCommunication()
        {
            var communication = new Rock.Model.Communication
            {
                Status = CommunicationStatus.Approved,
                ReviewedDateTime = RockDateTime.Now,
                ReviewerPersonAliasId = CurrentPersonAliasId,
                SenderPersonAliasId = CurrentPersonAliasId,
                CommunicationType = CommunicationType.PushNotification
            };

            communication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
            communication.CommunicationTemplateId = hfSelectedCommunicationTemplateId.Value.AsIntegerOrNull();

            var communicationData = new CommunicationDetails();
            var pushNotificationControl = phPushControl.Controls[0] as PushNotification;
            if ( pushNotificationControl != null )
            {
                pushNotificationControl.UpdateCommunication( communicationData );
            }

            CommunicationDetails.Copy( communicationData, communication );

            return communication;
        }

        /// <summary>
        /// Sends the test communication.
        /// </summary>
        /// <param name="nbResult">The <see cref="NotificationBox"/> that will contain the result of the test..</param>
        private void SendTestCommunication( NotificationBox nbResult )
        {
            nbResult.Visible = false;

            // Using a new context (so that changes in the UpdateCommunication() are not persisted )
            using ( var rockContext = new RockContext() )
            {
                var testCommunication = SetupCommunication();
                var pushData = testCommunication.PushData.FromJsonOrNull<PushData>();
                CommunicationService communicationService = null;

                try
                {
                    testCommunication.CreatedDateTime = RockDateTime.Now;
                    testCommunication.CreatedByPersonAliasId = this.CurrentPersonAliasId;
                    // removed the AsNoTracking() from the next line because otherwise the Person/PersonAlias is attempted (but fails) to be added as new.
                    testCommunication.CreatedByPersonAlias = new PersonAliasService( rockContext ).Queryable()
                        .Where( a => a.Id == this.CurrentPersonAliasId.Value )
                        .Include( a => a.Person )
                        .FirstOrDefault();

                    //
                    // Find all the active personal devices for the selected person.
                    //
                    var personalDeviceIds = new PersonalDeviceService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( a => a.PersonAlias.PersonId == CurrentPerson.Id && a.IsActive )
                        .Where( a => a.NotificationsEnabled && !string.IsNullOrEmpty( a.DeviceRegistrationId ) )
                        .Where( a => a.SiteId == pushData.MobileApplicationId )
                        .Select( a => a.Id )
                        .ToList();

                    if ( !personalDeviceIds.Any() )
                    {
                        nbResult.NotificationBoxType = NotificationBoxType.Danger;
                        nbResult.Text = "Test communication failed: No personal devices found.";
                        nbResult.Dismissable = true;
                        nbResult.Visible = true;

                        return;
                    }

                    //
                    // We are going to send a notification to each device.
                    //
                    PopulateCommunicationRecipients( testCommunication, personalDeviceIds );

                    //
                    // Temporarily save the communication so the send process doesn't bomb out.
                    //
                    communicationService = new CommunicationService( rockContext );
                    communicationService.Add( testCommunication );
                    rockContext.SaveChanges( disablePrePostProcessing: true );

                    foreach ( var medium in testCommunication.GetMediums() )
                    {
                        medium.Send( testCommunication );
                    }

                    //
                    // Check if any of the recipients sent.
                    //
                    var failedRecipient = new CommunicationRecipientService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r => r.CommunicationId == testCommunication.Id )
                        .Where( r => r.Status == CommunicationRecipientStatus.Failed )
                        .FirstOrDefault();

                    if ( failedRecipient == null )
                    {
                        nbResult.NotificationBoxType = NotificationBoxType.Success;
                        nbResult.Text = "Test communication has been sent.";
                    }
                    else
                    {
                        nbResult.NotificationBoxType = NotificationBoxType.Danger;
                        nbResult.Text = string.Format( "Test communication failed: {0}.", failedRecipient.StatusNote );
                    }

                    nbResult.Dismissable = true;
                    nbResult.Visible = true;
                }
                finally
                {
                    try
                    {
                        //
                        // We can't actually delete the test communication since if it is an
                        // action type of "Show Details" then they won't be able to view the
                        // communication on their device to see how it looks. Instead we switch
                        // the communication to be transient so the cleanup job will take care
                        // of it later.
                        //
                        if ( communicationService != null && testCommunication != null )
                        {
                            var testCommunicationId = testCommunication.Id;
                            testCommunication.Status = CommunicationStatus.Transient;
                            rockContext.SaveChanges( disablePrePostProcessing: true );

                            // Delete any Person History that was created for the Test Communication
                            using ( var historyContext = new RockContext() )
                            {
                                var categoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid() ).Id;
                                var communicationEntityTypeId = EntityTypeCache.Get<Rock.Model.Communication>().Id;
                                var historyService = new HistoryService( historyContext );

                                var communicationHistoryQuery = historyService.Queryable()
                                    .Where( a => a.CategoryId == categoryId && a.RelatedEntityTypeId == communicationEntityTypeId && a.RelatedEntityId == testCommunicationId );

                                foreach ( History communicationHistory in communicationHistoryQuery )
                                {
                                    historyService.Delete( communicationHistory );
                                }

                                historyContext.SaveChanges( disablePrePostProcessing: true );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // just log the exception, don't show it
                        ExceptionLogService.LogException( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Populates all the recipients and then sends the communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>This method should be called from a background task.</remarks>
        private void SendCommunication( Rock.Model.Communication communication, RockContext rockContext )
        {
            var pushData = communication.PushData.FromJsonOrNull<PushData>();
            var pageEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;
            var websiteMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ).Id;
            var filterDate = RockDateTime.Now.AddDays( -GetAttributeValue( AttributeKey.PersonalDeviceActiveDuration ).AsInteger() );

            //
            // Build a query for all the personal devices that have a recent interaction.
            // We go in through the Interaction table in the hopes that this query will
            // be faster than trying to query the PersonalDevice table first and then cross
            // join it to the Interaction table somehow. Hopefully SQL Server optimizes this well.
            //
            var personalDeviceIds = new InteractionService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == websiteMediumValueId )
                .Where( a => a.InteractionComponent.InteractionChannel.ChannelEntityId == pushData.MobileApplicationId )
                .Where( a => a.InteractionDateTime > filterDate )
                .Where( a => a.PersonalDevice.IsActive && a.PersonalDevice.NotificationsEnabled && !string.IsNullOrEmpty( a.PersonalDevice.DeviceRegistrationId ) )
                .Select( a => a.PersonalDeviceId.Value )
                .Distinct()
                .ToList();

            PopulateCommunicationRecipients( communication, personalDeviceIds );

            //
            // Update the communication status to mark it as ready to send.
            //
            communication.Status = CommunicationStatus.Approved;
            rockContext.SaveChanges();

            if ( GetAttributeValue( AttributeKey.SendImmediately ).AsBoolean() )
            {
                var processSendCommunicationMsg = new ProcessSendCommunication.Message
                {
                    CommunicationId = communication.Id,
                };
                processSendCommunicationMsg.Send();
            }
        }

        /// <summary>
        /// Populates the communication with recipients from the personal device identifiers.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="personalDeviceIds">The personal device ids.</param>
        private void PopulateCommunicationRecipients( Rock.Model.Communication communication, IEnumerable<int> personalDeviceIds )
        {
            foreach ( var personalDeviceId in personalDeviceIds )
            {
                var testRecipient = new CommunicationRecipient
                {
                    Status = CommunicationRecipientStatus.Pending,
                    PersonalDeviceId = personalDeviceId,
                    MediumEntityTypeId = _pushMediumId
                };

                communication.Recipients.Add( testRecipient );
            }
        }

        #endregion

        #region Template Selection

        /// <summary>
        /// Shows the template selection.
        /// </summary>
        private void ShowTemplateSelection()
        {
            var preferences = GetBlockPersonPreferences();

            cpCommunicationTemplate.SetValue( preferences.GetValue( UserPreference.CategoryCommunicationTemplate ).AsIntegerOrNull() );
            pnlTemplateSelection.Visible = true;

            if ( !BindTemplatePicker() )
            {
                pnlTemplateSelection.Visible = false;
                IsTemplateListEmpty = true;

                ShowPushEditor();
            }
        }

        /// <summary>
        /// Binds the template picker.
        /// </summary>
        /// <returns><c>true</c> if any templates were displayed.</returns>
        private bool BindTemplatePicker()
        {
            var rockContext = new RockContext();

            var templateQuery = new CommunicationTemplateService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive );

            int? categoryId = cpCommunicationTemplate.SelectedValue.AsIntegerOrNull();
            if ( categoryId.HasValue && categoryId > 0 )
            {
                templateQuery = templateQuery.Where( a => a.CategoryId == categoryId );
            }

            templateQuery = templateQuery.OrderBy( a => a.Name );

            // get list of push templates that the current user is authorized to View
            IEnumerable<CommunicationTemplate> templateList = templateQuery.AsNoTracking()
                .ToList()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                .Where( a => a.PushMessage.IsNotNullOrWhiteSpace() )
                .ToList();

            rptSelectTemplate.DataSource = templateList;
            rptSelectTemplate.DataBind();

            return templateList.Any();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSelectTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSelectTemplate_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            CommunicationTemplate communicationTemplate = e.Item.DataItem as CommunicationTemplate;

            if ( communicationTemplate != null )
            {
                Literal lTemplateImagePreview = e.Item.FindControl( "lTemplateImagePreview" ) as Literal;
                Literal lTemplateName = e.Item.FindControl( "lTemplateName" ) as Literal;
                Literal lTemplateDescription = e.Item.FindControl( "lTemplateDescription" ) as Literal;
                LinkButton btnSelectTemplate = e.Item.FindControl( "btnSelectTemplate" ) as LinkButton;

                if ( communicationTemplate.ImageFileId.HasValue )
                {
                    var imageUrl = FileUrlHelper.GetImageUrl( communicationTemplate.ImageFileId.Value );
                    lTemplateImagePreview.Text = string.Format( "<img src='{0}' width='100%'/>", this.ResolveRockUrl( imageUrl ) );
                }
                else
                {
                    lTemplateImagePreview.Text = string.Format( "<img src='{0}'/>", this.ResolveRockUrl( "~/Assets/Images/communication-template-default.svg" ) );
                }

                lTemplateName.Text = communicationTemplate.Name;
                lTemplateDescription.Text = communicationTemplate.Description;
                btnSelectTemplate.CommandName = "CommunicationTemplateId";
                btnSelectTemplate.CommandArgument = communicationTemplate.Id.ToString();

                if ( hfSelectedCommunicationTemplateId.Value == communicationTemplate.Id.ToString() )
                {
                    btnSelectTemplate.AddCssClass( "template-selected" );
                }
                else
                {
                    btnSelectTemplate.RemoveCssClass( "template-selected" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelectTemplate_Click( object sender, EventArgs e )
        {
            int communicationTemplateId = ( sender as LinkButton ).CommandArgument.AsInteger();

            InitializeFieldsFromCommunicationTemplate( communicationTemplateId );

            btnTemplateSelectionNext_Click( sender, e );
        }

        /// <summary>
        /// Initializes the fields from communication template.
        /// </summary>
        /// <param name="communicationTemplateId">The communication template identifier.</param>
        private void InitializeFieldsFromCommunicationTemplate( int communicationTemplateId )
        {
            hfSelectedCommunicationTemplateId.Value = communicationTemplateId.ToString();
            var communicationTemplate = new CommunicationTemplateService( new RockContext() ).Get( hfSelectedCommunicationTemplateId.Value.AsInteger() );

            var pushCommunication = new CommunicationDetails
            {
                PushData = communicationTemplate.PushData,
                PushImageBinaryFileId = communicationTemplate.PushImageBinaryFileId,
                PushMessage = communicationTemplate.PushMessage,
                PushTitle = communicationTemplate.PushTitle,
                PushOpenMessage = communicationTemplate.PushOpenMessage,
                PushOpenAction = communicationTemplate.PushOpenAction
            };

            var pushNotificationControl = phPushControl.Controls[0] as PushNotification;
            if ( pushNotificationControl != null )
            {
                pushNotificationControl.SetFromCommunication( pushCommunication );
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the cpCommunicationTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCommunicationTemplate_SelectItem( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreference.CategoryCommunicationTemplate, cpCommunicationTemplate.SelectedValue );
            preferences.Save();

            BindTemplatePicker();
        }

        /// <summary>
        /// Handles the Click event of the btnTemplateSelectionNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTemplateSelectionNext_Click( object sender, EventArgs e )
        {
            pnlTemplateSelection.Visible = false;

            // Show the next page...
            ShowPushEditor();
        }

        #endregion

        #region Push Section

        /// <summary>
        /// Shows the push editor.
        /// </summary>
        private void ShowPushEditor()
        {
            nbPushValidation.Visible = false;
            lbPushEditorPrevious.Visible = !IsTemplateListEmpty;

            pnlPushEditor.Visible = true;
        }

        /// <summary>
        /// Determines if the push settings are valid.
        /// </summary>
        /// <returns></returns>
        private bool VerifyPushSettingsAreValid( Rock.Model.Communication communication )
        {
            var pushData = communication.PushData.FromJsonOrNull<PushData>();

            if ( communication.PushMessage.IsNullOrWhiteSpace() )
            {
                nbPushValidation.Text = "Message is required.";
                nbPushValidation.Visible = true;

                return false;
            }

            if ( pushData == null || !pushData.MobileApplicationId.HasValue )
            {
                nbPushValidation.Text = "Please completely fill out the Open Action by selecting either a page or application.";
                nbPushValidation.Visible = true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the Click event of the lbPushEditorPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPushEditorPrevious_Click( object sender, EventArgs e )
        {
            pnlPushEditor.Visible = false;

            ShowTemplateSelection();
        }

        /// <summary>
        /// Handles the Click event of the lbPushEditorSendTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void lbPushEditorSendTest_Click( object sender, EventArgs e )
        {
            nbPushTestResult.Visible = false;

            var communication = SetupCommunication();
            nbPushValidation.Visible = false;
            if ( !VerifyPushSettingsAreValid( communication ) )
            {
                return;
            }

            mdPushSendTest.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbPushEditorSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void lbPushEditorSend_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );
            var communication = SetupCommunication();

            nbPushValidation.Visible = false;
            if ( !VerifyPushSettingsAreValid( communication ) )
            {
                return;
            }

            communicationService.Add( communication );

            //
            // Give the medium control a chance to do any final database work.
            //
            var pushNotificationControl = phPushControl.Controls[0] as PushNotification;
            if ( pushNotificationControl != null )
            {
                pushNotificationControl.OnCommunicationSave( rockContext );
            }

            //
            // Mark the communication as a transient so it doesn't get sent just yet
            // before we start populating the recipients. As long as it doesn't take
            // us more than 7 days to build the recipient list we are okay.
            //
            communication.Status = CommunicationStatus.Transient;
            rockContext.SaveChanges();

            //
            // Since we are sending to essentially ALL known personal devices, this query could
            // end up taking a LONG time. So kick it off to a background thread to perform
            // the heavy query processing of building that list of recipients.
            //
            System.Threading.Tasks.Task.Run( () => SendCommunication( communication, rockContext ) );

            pnlPushEditor.Visible = false;

            ShowResult( "Communication has been queued for sending." );
        }

        /// <summary>
        /// Handles the Click event of the lbPushSendTestMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPushSendTestMessage_Click( object sender, EventArgs e )
        {
            SendTestCommunication( nbPushTestResult );
        }

        #endregion

        #region Result Section

        /// <summary>
        /// Shows the result of the communication send.
        /// </summary>
        /// <param name="message">The message to display.</param>
        private void ShowResult( string message )
        {
            nbResult.Text = message;

            pnlResult.Visible = true;
        }

        #endregion
    }
}