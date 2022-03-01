using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "System Communication Preview" )]
    [Category( "Communication" )]
    [Description( "Create a preview and send a test message for the given system communication using the selected date and target person." )]

    #region Block Attributes

    [SystemCommunicationField( "System Communication",
        Description = "The system communication to use when previewing the message. When set as a block setting, it will not allow overriding by the query string",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.SystemCommunication )]

    [DaysOfWeekField( "Send Day of the Week",
        Description = "Used to determine which dates to list in the Message Date drop down. <i><strong>Note:</strong>If no day is selected the Message Date drop down control will not be shown</i>",
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

    #endregion Block Attributes

    public partial class CommunicationJobPreview : RockBlock
    {
        #region Variables

        private static class Variables
        {
            public static SystemCommunication SystemCommunicationRecord { get; set; }

            public static RockEmailMessage RockEmailMessage { get; set; }

            public const string SystemCommunicationSourceReplacementKey = "[SOURCE_REPLACEMENT]";

            public static bool HasSystemCommunication => SystemCommunicationRecord != null;

            public static Person TargetPerson { get; set; }

            public static bool HasTargetPerson => TargetPerson != null;

            public static bool HasSendDate { get; set; }

            public static string DropDownSeparator => new string( '-', 30 );

            public static ListItem SelectedDate { get; set; }

            public const string LavaDebugCommand = "{{ 'Lava' | Debug }}";

            public const string EmailContainerHtml = @"<span class='card-text'>
                                <div id='divEmailPreview'
                                    class='email-preview js-email-preview center-block device-browser' style='position: relative; height: 720px;'>
                                    <iframe name='emailPreview' src='javascript: window.frameElement.getAttribute('srcdoc');'
                                        id='ifEmailPreview' name='emailpreview-iframe'
                                        class='emaileditor-iframe js-emailpreview-iframe email-wrapper email-content-desktop' frameborder='0' border='0' cellspacing='0'
                                        scrolling='yes' srcdoc='[SOURCE_REPLACEMENT]''></iframe>
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
                                </div>
                               </span>";
        }

        #endregion Variables

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
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
        }

        #endregion Attribute Keys

        #region Merge Field Keys

        private static class MergeFieldKey
        {
            public const string SendDateTime = "SendDateTime";
            public const string Person = "Person";
        }

        #endregion Merge Field Keys

        #region Page Events

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var ddlMessageDateJs = @"$(document).on('change','#ddlMessageDate',function (e) {
                    let thisIndex = $('#ddlMessageDate').prop('selectedIndex');
                    if ( this.value.startsWith('Separator') )
                    {
                        $('#ddlMessageDate').prop('selectedIndex', thisIndex + 1 );
                    }
                });";

            Page.ClientScript.RegisterStartupScript( Page.GetType(), "ddlMessageDate", ddlMessageDateJs, true );

            LoadSystemCommunication();
        }

        #endregion Page Events

        #region Control Events
        
        /// <summary>
        /// Handles the Click event of the lbUpdate control.
        /// </summary>
        protected void lbUpdate_Click( object sender, EventArgs e )
        {
            var mergeFields = ProcessTemplateMergeFields();

            var source = Variables.RockEmailMessage.Message.ResolveMergeFields( mergeFields ).ConvertHtmlStylesToInlineAttributes().EncodeHtml();
            lContent.Text = Variables.EmailContainerHtml.Replace( Variables.SystemCommunicationSourceReplacementKey, source );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMessageDate control.
        /// </summary>
        protected void ddlMessageDate_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlMessageDate.SelectedItem.Value.StartsWith( "Separator" ) )
            {
                var selectedIndex = ddlMessageDate.SelectedIndex + 1;
                if ( ddlMessageDate.Items.Count - 1 >= selectedIndex )
                {
                    ddlMessageDate.Items[selectedIndex].Selected = true;
                    Variables.SelectedDate = ddlMessageDate.Items[selectedIndex];
                }
            }
            else
            {
                Variables.SelectedDate = ddlMessageDate.SelectedItem;
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppTargetPerson control.
        /// </summary>
        protected void ppTargetPerson_SelectPerson( object sender, EventArgs e )
        {
            var person = new PersonService( new RockContext() ).Get( ppTargetPerson.ID.AsInteger() );
            Variables.TargetPerson = person;
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
            string currentEmail = CurrentPerson.Email;
            var rockContext = new RockContext();
            try
            {
                if ( Variables.RockEmailMessage == null )
                {
                    throw new Exception( $"A valid system communication was not selected." );
                }

                if ( Variables.RockEmailMessage.SystemCommunicationId.GetValueOrDefault( 0 ) == 0 )
                {
                    throw new Exception( $"The system communication specified is not valid." );
                }

                var currentPerson = new PersonService( rockContext ).Get( CurrentPerson.Id );

                var mergeFields = ProcessTemplateMergeFields();

                var fromName = Variables.SystemCommunicationRecord.FromName.IsNotNullOrWhiteSpace() ? Variables.SystemCommunicationRecord.FromName : CurrentPerson.FullName;
                var fromEmail = Variables.SystemCommunicationRecord.From.IsNotNullOrWhiteSpace() ? Variables.SystemCommunicationRecord.From : CurrentPerson.Email;

                var emailMessage = new RockEmailMessage( Variables.SystemCommunicationRecord )
                {
                    AdditionalMergeFields = mergeFields.ToDictionary( k => k.Key, v => ( object ) v.Value ),
                    CurrentPerson = currentPerson,
                    FromName = fromName,
                    FromEmail = fromEmail,
                    AppRoot = ResolveRockUrl( "~/" ),
                    ThemeRoot = ResolveRockUrl( "~~/" ),
                    Message = Variables.RockEmailMessage.Message = Variables.RockEmailMessage.Message.Replace( Variables.LavaDebugCommand, string.Empty )
                };

                var recipient = new RockEmailMessageRecipient( currentPerson, mergeFields );
                var sendErrorMessages = new List<string>();

                currentPerson.Email = ebSendTest.Text;
                rockContext.SaveChanges();

                emailMessage.AddRecipient( recipient );
                emailMessage.Send( out sendErrorMessages );

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
            }
            catch ( Exception ex )
            {
                nbSendTest.Text = ex.Message;
                nbSendTest.NotificationBoxType = NotificationBoxType.Danger;
                nbSendTest.Visible = true;
            }
            finally
            {
                var person = new PersonService( rockContext ).Get( CurrentPerson.Id );
                person.Email = currentEmail;
                rockContext.SaveChanges();
            }
        }

        private Dictionary<string, object> ProcessTemplateMergeFields()
        {
            var mergeFields = Variables.SystemCommunicationRecord.LavaFields.ToDictionary( k => k.Key, v => ( object ) v.Value );

            if ( Variables.HasSendDate && Variables.SelectedDate != null )
            {
                if ( !mergeFields.ContainsKey( MergeFieldKey.SendDateTime ) )
                {
                    mergeFields.Add( MergeFieldKey.SendDateTime, Variables.SelectedDate.Text );
                }
                else
                {
                    mergeFields[MergeFieldKey.SendDateTime] = Variables.SelectedDate.Text;
                }
            }

            var mergeFieldOptions = new Rock.Lava.CommonMergeFieldsOptions { GetCurrentPerson = true };
            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson, mergeFieldOptions );

            mergeFields.AddOrReplace( MergeFieldKey.Person, Variables.TargetPerson );

            mergeFields = mergeFields
                .Concat( commonMergeFields.Where( kvp => !mergeFields.ContainsKey( kvp.Key ) ) )
                .ToDictionary( k => k.Key, v => v.Value );

            Variables.SystemCommunicationRecord.LavaFields = mergeFields.ToDictionary( k => k.Key, v => v.Value?.ToString() );

            if ( Variables.RockEmailMessage.Message.Contains( Variables.LavaDebugCommand ) )
            {
                lavaDebug.Visible = true;
                lavaDebug.Text = mergeFields.lavaDebugInfo();

                Variables.RockEmailMessage.Message = Variables.RockEmailMessage.Message.Replace( Variables.LavaDebugCommand, string.Empty );
            }
            else
            {
                lavaDebug.Visible = false;
                lavaDebug.Text = string.Empty;
            }

            return mergeFields;
        }

        #endregion Control Events

        #region Methods
        
        /// <summary>
        /// Loads the system communication.
        /// </summary>
        private void LoadSystemCommunication()
        {
            var systemCommunicationPreviewInfo = SetSystemCommunication();

            nbMessage.Visible = !Variables.HasSystemCommunication;

            if ( !Variables.HasSystemCommunication )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                var communicationSource = systemCommunicationPreviewInfo.ParameterSettingType == SystemCommunicationPreviewInfo.ParameterSettingTypeEnum.BlockSetting ? "was not specified in the block setting or is invalid" : $"was not specified in the block setting or using the [{PageParameterKey.SystemCommunicationId}] url parameter";
                nbMessage.Text = $"A communication template {communicationSource}.";

                EnableControls( false );
            }
            else
            {
                EnableControls();

                SetTargetPerson();

                CreateDateDropDown();

                var mergeFields = ProcessTemplateMergeFields();

                var fromName = Variables.SystemCommunicationRecord.FromName.IsNotNullOrWhiteSpace() ? Variables.SystemCommunicationRecord.FromName : CurrentPerson.FullName;
                var fromEmail = Variables.SystemCommunicationRecord.From.IsNotNullOrWhiteSpace() ? Variables.SystemCommunicationRecord.From : CurrentPerson.Email;

                lTitle.Text = $"<span class='panel-title'>{Variables.SystemCommunicationRecord.Title} Message Preview</small>";
                lNavTitle.Text = $"<span class='text-muted'>{Variables.SystemCommunicationRecord.Title}</span>";
                lFrom.Text = $"<small><span class='text-semibold'>{fromName}</span> <span class='text-muted'>&lt;{fromEmail}&gt;</span></small>";
                lSubject.Text = $"<small class='text-semibold'>{Variables.SystemCommunicationRecord.Title} | {Variables.SystemCommunicationRecord.Subject}</small>";
                lDate.Text = $"<small class='text-semibold'>{RockDateTime.Now:MMMM d, yyyy}</small>";

                string source = Variables.RockEmailMessage.Message.ResolveMergeFields( mergeFields ).ConvertHtmlStylesToInlineAttributes().EncodeHtml();

                lContent.Text = Variables.EmailContainerHtml.Replace( Variables.SystemCommunicationSourceReplacementKey, source );
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
        /// Creates the date drop down.
        /// </summary>
        private void CreateDateDropDown()
        {
            var dayOfWeeks = GetAttributeValues( AttributeKey.SendDaysOfTheWeek )?.Select( dow => ( DayOfWeek ) Enum.Parse( typeof( DayOfWeek ), dow ) );
            var previousWeeks = GetAttributeValue( AttributeKey.PreviousWeeksToShow ).ToIntSafe();
            var futureWeeks = GetAttributeValue( AttributeKey.PreviousWeeksToShow ).ToIntSafe();

            Variables.HasSendDate = Variables.RockEmailMessage.Message.Contains( $"{{ {MergeFieldKey.SendDateTime} }}" );

            navMessageDate.Visible = Variables.HasSendDate && dayOfWeeks.Count() > 0;

            if ( navMessageDate.Visible )
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
                            if ( previousMonth != dt.Month && previousMonth > 0 )
                            {
                                ddlMessageDate.Items.Add( new ListItem { Text = Variables.DropDownSeparator, Value = $"Separator_{previousMonth}" } );
                            }

                            ddlMessageDate.Items.Add( new ListItem { Text = dt.ToString( "MMMM d, yyyy" ), Value = $"{dt:MMddyyyy}" } );

                            previousMonth = dt.Month;
                        }
                    }

                    ddlMessageDate.SelectedIndex = 0;

                    // Set the date from the querystring param
                    var publicationDate = PageParameter( PageParameterKey.PublicationDate ).AsDateTime();
                    if ( publicationDate.HasValue )
                    {
                        var publicationDateValue = publicationDate.Value.ToString( "MMddyyyy" );
                        var incomingDateItem = ddlMessageDate.Items.FindByValue( publicationDateValue );
                        if ( incomingDateItem != null )
                        {
                            ddlMessageDate.SelectedValue = incomingDateItem.Value;
                        }
                        else
                        {
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

                                var inputDate = publicationDate.Value;

                                var closestDate = inputDate >= allDates.Last()
                                    ? allDates.Last()
                                    : inputDate <= allDates.First()
                                        ? allDates.First()
                                        : allDates.First( d => d >= inputDate );

                                ddlMessageDate.SelectedValue = ddlMessageDate.Items.FindByValue( closestDate.ToString( "MMddyyyy" ) ).Value;
                            }
                            else
                            {
                                ddlMessageDate.SelectedIndex = 0;
                            }
                        }
                    }

                    Variables.SelectedDate = ddlMessageDate.SelectedItem;
                }
            }
            else
            {
                ddlMessageDate.Required = false;
                var rockNow = RockDateTime.Now;
                Variables.SelectedDate = new ListItem { Text = rockNow.ToString( "MMMM d, yyyy" ), Value = rockNow.Ticks.ToString() };
            }
        }

        /// <summary>
        /// Sets the system communication.
        /// </summary>
        /// <returns>JobPreviewInfo.</returns>
        private SystemCommunicationPreviewInfo SetSystemCommunication()
        {
            var previewInfo = new SystemCommunicationPreviewInfo();

            var communicationService = new SystemCommunicationService( new RockContext() );
            var systemCommunicationGuid = GetAttributeValue( AttributeKey.SystemCommunication ).AsGuid();

            if ( !systemCommunicationGuid.IsEmpty() )
            {
                previewInfo.ParameterSettingType = SystemCommunicationPreviewInfo.ParameterSettingTypeEnum.BlockSetting;
                Variables.SystemCommunicationRecord = communicationService.Get( systemCommunicationGuid );
            }
            else
            {
                previewInfo.ParameterSettingType = SystemCommunicationPreviewInfo.ParameterSettingTypeEnum.QueryStringParameter;
                var systemCommunicationId = PageParameter( PageParameterKey.SystemCommunicationId ).AsInteger();
                if ( systemCommunicationId > 0 )
                {
                    Variables.SystemCommunicationRecord = communicationService.Get( systemCommunicationId );
                }
            }

            if ( Variables.SystemCommunicationRecord != null && Variables.SystemCommunicationRecord.Guid != Guid.Empty )
            {
                Variables.RockEmailMessage = new RockEmailMessage( Variables.SystemCommunicationRecord.Guid );
            }

            return previewInfo;
        }

        /// <summary>
        /// Sets the target person.
        /// </summary>
        private void SetTargetPerson()
        {
            var targetPersonId = PageParameter( PageParameterKey.TargetPersonId ).AsInteger();
            var personService = new PersonService( new RockContext() );
            if ( targetPersonId > 0 )
            {
                Variables.TargetPerson = personService.Get( targetPersonId );
            }
            else
            {
                Variables.TargetPerson = CurrentPerson;
            }

            if ( Variables.HasTargetPerson )
            {
                ppTargetPerson.SetValue( Variables.TargetPerson );
            }
        }

        #endregion Methods

        #region Classes
        
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