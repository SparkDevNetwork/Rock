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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Obsidian.Blocks.Event
{
    /// <summary>
    /// Block used to register for a registration instance.
    /// </summary>

    [DisplayName( "Registration Entry" )]
    [Category( "Obsidian > Event" )]
    [Description( "Block used to register for a registration instance." )]
    [IconCssClass( "fa fa-users" )]

    #region BlockAttributes

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Web Prospect'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT,
        Order = 0 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 1 )]

    [DefinedValueField( "Source",
        Key = AttributeKey.Source,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        Description = "The Financial Source Type to use when creating transactions",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Order = 2 )]

    [TextField( "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch",
        IsRequired = false,
        DefaultValue = "Event Registration",
        Order = 3 )]

    [BooleanField( "Display Progress Bar",
        Key = AttributeKey.DisplayProgressBar,
        Description = "Display a progress bar for the registration.",
        DefaultBooleanValue = true,
        Order = 4 )]

    [BooleanField( "Allow InLine Digital Signature Documents",
        Key = AttributeKey.SignInline,
        Description = "Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline",
        DefaultBooleanValue = true,
        Order = 6 )]

    [SystemCommunicationField( "Confirm Account Template",
        Description = "Confirm Account Email Template",
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Order = 7,
        Key = AttributeKey.ConfirmAccountTemplate )]

    [TextField( "Family Term",
        Description = "The term to use for specifying which household or family a person is a member of.",
        IsRequired = true,
        DefaultValue = "immediate family",
        Order = 8,
        Key = AttributeKey.FamilyTerm )]

    [BooleanField( "Force Email Update",
        Description = "Force the email to be updated on the person's record.",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.ForceEmailUpdate )]

    [BooleanField( "Show Field Descriptions",
        Description = "Show the field description as help text",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.ShowFieldDescriptions )]

    [BooleanField( "Enabled Saved Account",
        Key = AttributeKey.EnableSavedAccount,
        Description = "Set this to false to disable the using Saved Account as a payment option, and to also disable the option to create saved account for future use.",
        DefaultBooleanValue = true,
        Order = 11 )]

    #endregion BlockAttributes

    public class RegistrationEntry : ObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string Source = "Source";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string DisplayProgressBar = "DisplayProgressBar";
            public const string SignInline = "SignInline";
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";
            public const string FamilyTerm = "FamilyTerm";
            public const string ForceEmailUpdate = "ForceEmailUpdate";
            public const string ShowFieldDescriptions = "ShowFieldDescriptions";
            public const string EnableSavedAccount = "EnableSavedAccount";
        }

        #endregion Keys

        #region BlockActions

        /// <summary>
        /// Gets the start panel data.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="slug">The slug.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetStartPanelData( int? registrationInstanceId = null, string slug = null, int? registrationId = null )
        {
            if ( !registrationInstanceId.HasValue )
            {
                registrationInstanceId = GetRegistrationInstanceId( slug, registrationId );
            }

            if ( !registrationInstanceId.HasValue )
            {
                return new BlockActionResult( HttpStatusCode.NotFound );
            }

            var registrationInstance = GetRegistrationInstance( registrationInstanceId.Value );

            if ( registrationInstance == null )
            {
                return new BlockActionResult( HttpStatusCode.NotFound );
            }

            var currentPerson = GetCurrentPerson();
            var registrationTemplate = registrationInstance.RegistrationTemplate;
            var registrationInfo = new RegistrationInfo( currentPerson );

            return new BlockActionResult( HttpStatusCode.OK, new StartPanelData
            {
                InstructionsMarkup = GetInstructionsMarkup( registrationInstance ),
                MaxRegistrants = GetMaxRegistrants( registrationTemplate, registrationInfo ),
                SlotsAvailable = registrationInfo.SlotsAvailable ?? 0,
                WaitListEnabled = registrationTemplate.WaitListEnabled
            } );
        }

        #endregion BlockActions

        #region Helpers

        /// <summary>
        /// If the registration template allows multiple registrants per registration, returns the maximum allowed
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="registrationInfo">The registration information.</param>
        /// <returns></returns>
        private int GetMaxRegistrants( RegistrationTemplate registrationTemplate, RegistrationInfo registrationInfo )
        {
            // If this is an existing registration, max registrants is the number of registrants already
            // on registration ( don't allow adding new registrants )
            if ( registrationInfo != null && registrationInfo.RegistrationId.HasValue )
            {
                return registrationInfo.RegistrantCount;
            }

            // Otherwise if template allows multiple, set the max amount
            if ( registrationTemplate != null && registrationTemplate.AllowMultipleRegistrants )
            {
                if ( !registrationTemplate.MaxRegistrants.HasValue )
                {
                    return int.MaxValue;
                }

                return registrationTemplate.MaxRegistrants.Value;
            }

            // Default is a maximum of one
            return 1;
        }

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns></returns>
        private RegistrationInstance GetRegistrationInstance( int id )
        {
            using ( var rockContext = new RockContext() )
            {
                var dateTime = RockDateTime.Now;
                return new RegistrationInstanceService( rockContext )
                    .Queryable()
                    .Include( ri => ri.Account )
                    .Include( ri => ri.RegistrationTemplate.Fees )
                    .Include( ri => ri.RegistrationTemplate.Discounts )
                    .Include( "RegistrationTemplate.Forms.Fields.Attribute" )
                    .Include( ri => ri.RegistrationTemplate.FinancialGateway )
                    .Where( r =>
                        r.Id == id &&
                        r.IsActive &&
                        r.RegistrationTemplate != null &&
                        r.RegistrationTemplate.IsActive &&
                        ( !r.StartDateTime.HasValue || r.StartDateTime <= dateTime ) &&
                        ( !r.EndDateTime.HasValue || r.EndDateTime > dateTime ) )
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the registration instance identifier from URL params like slug or registrationId.
        /// </summary>
        /// <returns></returns>
        private int? GetRegistrationInstanceId( string slug, int? registrationId )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !slug.IsNullOrWhiteSpace() )
                {
                    var now = RockDateTime.Now;

                    return new EventItemOccurrenceGroupMapService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( l =>
                            l.UrlSlug == slug &&
                            l.RegistrationInstance != null &&
                            l.RegistrationInstance.IsActive &&
                            l.RegistrationInstance.RegistrationTemplate != null &&
                            l.RegistrationInstance.RegistrationTemplate.IsActive &&
                            ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= now ) &&
                            ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > now ) )
                        .Select( a => a.RegistrationInstanceId )
                        .FirstOrDefault();
                }

                if ( registrationId.HasValue )
                {
                    return new RegistrationService( rockContext ).GetSelect( registrationId.Value, s => s.RegistrationInstanceId );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the instructions.
        /// </summary>
        /// <returns></returns>
        private string GetInstructionsMarkup( RegistrationInstance registrationInstance )
        {
            if ( registrationInstance?.RegistrationTemplate == null )
            {
                return string.Empty;
            }

            // Sanitize for empty check catches things like empty paragraph tags
            var sanitizedInstanceInstructions = registrationInstance.RegistrationInstructions.SanitizeHtml();

            if ( !sanitizedInstanceInstructions.IsNullOrWhiteSpace() )
            {
                return registrationInstance.RegistrationInstructions;
            }

            var sanitizedTemplateInstructions = registrationInstance.RegistrationTemplate.RegistrationInstructions.SanitizeHtml();

            if ( !sanitizedTemplateInstructions.IsNullOrWhiteSpace() )
            {
                return registrationInstance.RegistrationTemplate.RegistrationInstructions;
            }

            return string.Empty;
        }

        #endregion Helpers

        #region ViewModels

        /// <summary>
        /// Start Panel Data
        /// </summary>
        public sealed class StartPanelData
        {
            /// <summary>
            /// Gets or sets the instructions.
            /// </summary>
            /// <value>
            /// The instructions.
            /// </value>
            public string InstructionsMarkup { get; set; }

            /// <summary>
            /// Gets or sets the maximum registrants.
            /// </summary>
            /// <value>
            /// The maximum registrants.
            /// </value>
            public int MaxRegistrants { get; set; }

            /// <summary>
            /// Gets or sets the slots available.
            /// </summary>
            /// <value>
            /// The waitlist slots available.
            /// </value>
            public int SlotsAvailable { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [wait list enabled].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [wait list enabled]; otherwise, <c>false</c>.
            /// </value>
            public bool WaitListEnabled { get; set; }
        }

        #endregion ViewModels
    }
}
