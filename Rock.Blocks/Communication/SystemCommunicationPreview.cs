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
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.SystemCommunicationPreview;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    [DisplayName( "System Communication Preview" )]
    [Category( "Communication" )]
    [Description( "Create a preview and send a test message for the given system communication using the selected date and target person." )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

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
         Order = 6 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9" )]
    [Rock.SystemGuid.EntityTypeGuid( "D61A57A2-C067-435F-99F6-7B6BB9534058")]
    public class SystemCommunicationPreview : RockBlockType
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

        #region Properties

        protected string EnabledLavaCommands => GetAttributeValue( AttributeKey.EnabledLavaCommands );

        #endregion

        #region Methods

        public override object GetObsidianBlockInitialization()
        {
            var rockContext = new RockContext();
            var globalAttributes = GlobalAttributesCache.Get();

            // Get System Communication Guid from Block Settings or QueryString.
            Guid? systemCommunicationGuid = GetAttributeValue( AttributeKey.SystemCommunication ).AsGuidOrNull();

            if ( systemCommunicationGuid == null )
            {
                var systemCommunicationId = RequestContext.GetPageParameter( PageParameterKey.SystemCommunicationId ).AsIntegerOrNull();
                if ( systemCommunicationId.HasValue )
                {
                    systemCommunicationGuid = new SystemCommunicationService( rockContext ).GetGuid( systemCommunicationId.Value );
                }
            }

            var systemCommunicationService = new SystemCommunicationService( rockContext );
            var systemCommunication = systemCommunicationService.Get( systemCommunicationGuid.Value );

            if ( systemCommunication != null )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                string bodyHtml = systemCommunication.Body.ResolveMergeFields( mergeFields );
                string subjectHtml = systemCommunication.Subject.ResolveMergeFields( mergeFields );
                var hasSendDate = systemCommunication.Body.Contains( "{{ SendDateTime }}" );

                DateTime publicationDate = DateTime.Now;

                var systemCommunicationPreviewInitializationBox = new SystemCommunicationPreviewInitializationBox
                {

                    Title = systemCommunication.Title,
                    From = systemCommunication.From.IsNullOrWhiteSpace() ? globalAttributes.GetValue( "OganizationEmail" ) : systemCommunication.From,
                    FromName = systemCommunication.FromName.IsNullOrWhiteSpace() ? globalAttributes.GetValue( "OrganizationName" ) : systemCommunication.FromName,
                    Subject = subjectHtml,
                    Body = bodyHtml,
                    Date = publicationDate.ToString( "MMMM d, yyy" ),
                    HasSendDate = hasSendDate,
                };

                return systemCommunicationPreviewInitializationBox;
            }

            return null;
        }

        private void SetEmailFromDetails( RockEmailMessage rockEmailMessage, SystemCommunication systemCommunication )
        {
            var globalAttributes = GlobalAttributesCache.Get();

            // Email - From Name
            if ( string.IsNullOrWhiteSpace( systemCommunication.FromName ) )
            {
                systemCommunication.FromName = globalAttributes.GetValue( "OrganizationName" );
            }

            rockEmailMessage.FromName = systemCommunication.FromName;

            // Email - From Address
            if ( string.IsNullOrWhiteSpace( systemCommunication.From ) )
            {
                systemCommunication.From = globalAttributes.GetValue( "OrganizationEmail" );
            }

            rockEmailMessage.FromEmail = systemCommunication.From;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult SetSystemCommunication( SystemCommunicationPreviewInitializationBox box )
        {
            var rockContext = new RockContext();
            var systemCommunicationService = new SystemCommunicationService( rockContext );
            SystemCommunication systemCommunication = null;

            var systemCommunicationGuid = GetAttributeValue( AttributeKey.SystemCommunication ).AsGuid();
            if ( !systemCommunicationGuid.IsEmpty() )
            {
                systemCommunication = systemCommunicationService.Get( systemCommunicationGuid );
            }
            else if ( box.Id > 0 )
            {
                systemCommunication = systemCommunicationService.Get( box.Id );
            }

            if ( systemCommunication != null )
            {
                var rockEmailMessage = new RockEmailMessage( systemCommunicationGuid );
                var appendTemplate = GetAttributeValue( AttributeKey.LavaTemplateAppend );

                if ( !string.IsNullOrWhiteSpace( appendTemplate ) )
                {
                    rockEmailMessage.Message = appendTemplate + rockEmailMessage.Message;
                }

                if ( !DateTime.TryParse( box.PublicationDate, out DateTime publicationDate ) )
                {
                    publicationDate = DateTime.Now;
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );

                if ( box.TargetPersonId > 0 )
                {
                    var person = new PersonService( rockContext ).Get( box.TargetPersonId );
                    if ( person == null )
                    {
                        return ActionBadRequest( "Target person not found." );
                    }
                    else
                    {
                        mergeFields.Add( "Person", person );
                    }
                }

                string bodyHtml = systemCommunication.Body.ResolveMergeFields( mergeFields );
                string subjectHtml = systemCommunication.Subject.ResolveMergeFields( mergeFields );

                var systemCommunicationPreviewInitializationBox = new SystemCommunicationPreviewInitializationBox
                {
                    Id = systemCommunication.Id,
                    Title = systemCommunication.Title,
                    From = systemCommunication.From,
                    FromName = systemCommunication.FromName.IsNullOrWhiteSpace() ? GlobalAttributesCache.Get().GetValue( "OrganizationName" ) : systemCommunication.FromName,
                    Subject = subjectHtml,
                    Body = bodyHtml,
                    Date = publicationDate.ToString( "MMMM d, yyyy" )
                };

                return ActionOk( systemCommunicationPreviewInitializationBox );
            }

            return ActionNotFound( "System Communication not found." );
        }

        [BlockAction]
        public BlockActionResult GetDateOptions()
        {
            var dayOfWeeks = GetAttributeValues( AttributeKey.SendDaysOfTheWeek )
                .Select( dow => ( DayOfWeek ) Enum.Parse( typeof( DayOfWeek ), dow ) ).ToList();

            var previousWeeks = GetAttributeValue( AttributeKey.PreviousWeeksToShow ).AsIntegerOrNull() ?? 6;
            var futureWeeks = GetAttributeValue( AttributeKey.FutureWeeksToShow ).AsIntegerOrNull() ?? 1;

            var dateOptions = new List<ListItemBag>();
            var startDate = RockDateTime.Today.AddDays( -( previousWeeks * 7 ) );
            var endDate = RockDateTime.Today.AddDays( futureWeeks * 7 );

            for ( var dt = startDate; dt <= endDate; dt = dt.AddDays( 1 ) )
            {
                if ( dayOfWeeks.Contains( dt.DayOfWeek ) )
                {
                    dateOptions.Add( new ListItemBag
                    {
                        Text = dt.ToString( "MMMM d, yyyy" ),
                        Value = dt.ToString( "yyyyMMdd" )
                    } );
                }
            }

            return ActionOk( dateOptions );
        }

        [BlockAction]
        public BlockActionResult GetTargetPerson( GetTargetPersonRequestBag bag )
        {
            var rockContext = new RockContext();
            if ( Guid.TryParse( bag.PersonAliasGuid, out Guid guid ) )
            {
                var personAlias = new PersonAliasService( rockContext ).Get( guid );
                if ( personAlias != null )
                {
                    var personId = personAlias.PersonId;
                    return ActionOk( new { Id = personId } );
                }

                return ActionNotFound( $"Person not found for alias GUID: { bag.PersonAliasGuid }" );
            }

            return ActionBadRequest( "Invalid person GUID for person alias or person not found." );
        }

        [BlockAction]
        public BlockActionResult SendTestEmail( SystemCommunicationPreviewInitializationBox box )
        {
            using ( var rockContext = new RockContext() )
            {
                var systemCommunicationService = new SystemCommunicationService( rockContext );
                var personService = new PersonService( rockContext );

                // Fetch the system communication
                var systemCommunication = systemCommunicationService.Get( box.Id );
                if ( systemCommunication == null )
                {
                    return ActionNotFound( "System Communication not found" );
                }

                // Fetch the target person
                var targetPerson = personService.Get( box.TargetPersonId );
                if ( targetPerson == null )
                {
                    return ActionNotFound( "Target person not found." );
                }

                // Temporarily change the person's email address
                string originalEmail = targetPerson.Email;
                targetPerson.Email = box.Email;
                rockContext.SaveChanges();

                try
                {
                    // Prepare the email
                    var rockEmailMessage = new RockEmailMessage( systemCommunication.Guid );

                    // Append Lava Template if any
                    var lavaTemplateAppend = GetAttributeValue( AttributeKey.LavaTemplateAppend );
                    if ( !string.IsNullOrWhiteSpace( lavaTemplateAppend ) )
                    {
                        rockEmailMessage.Message = lavaTemplateAppend + rockEmailMessage.Message;
                    }

                    // Remove Lava Debug command
                    rockEmailMessage.Message = rockEmailMessage.Message.Replace( PageConstants.LavaDebugCommand, string.Empty );

                    // Prepare merge fields
                    var mergeFields = new Dictionary<string, object> { { MergeFieldKey.Person, targetPerson } };

                    // Attempt to get the send data from the URL params
                    string sendDateParam = RequestContext.GetPageParameter( "PublicationDate" );
                    DateTime sendDate;
                    if ( DateTime.TryParse( sendDateParam, out sendDate ) )
                    {
                        // Add the "SendDateTime" merge field if a valid date is provided
                        mergeFields.Add( MergeFieldKey.SendDateTime, sendDate.ToString( "MMMM d, yyyy" ) );
                    }

                    // Set the recipient with merge fields
                    rockEmailMessage.AddRecipient( new RockEmailMessageRecipient( targetPerson, mergeFields ) );
                    rockEmailMessage.CreateCommunicationRecord = false;

                    // Set the From Name and From Email based on System Communication or Global Attributes
                    SetEmailFromDetails( rockEmailMessage, systemCommunication );

                    // Prepare the subject, removing carriage returns and line feeds, as well as enforcing max length
                    rockEmailMessage.Subject = Regex.Replace( rockEmailMessage.Subject, @"\r\n?|\n", string.Empty ).Left( 998 );

                    // Send the email
                    var errors = new List<string>();
                    try
                    {
                        rockEmailMessage.Send( out errors );
                    }
                    catch ( Exception ex )
                    {
                        errors.Add( ex.Message );
                    }
                    if ( errors.Any() )
                    {
                        // Revert the target person's email to it's original value if sending fails
                        targetPerson.Email = originalEmail;
                        rockContext.SaveChanges();

                        string errorMessage = string.Join( ", ", errors );
                        return ActionBadRequest( $"Failed to send test email: {errorMessage}" );
                    }
                }
                finally
                {
                    // Revert the target person's email to it's original value
                    targetPerson.Email = originalEmail;
                    rockContext.SaveChanges();
                }

                return ActionOk( "Test email sent successfully." );
            }
        }

        #endregion

        #region Helper Classes

        public class GetTargetPersonRequestBag
        {
            public string PersonAliasGuid { get; set; }
        }

        #endregion
    }
}
