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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PhotoSendRequest;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Block for selecting criteria to build a list of people who should receive a photo request.
    /// </summary>
    [DisplayName( "Send Photo Request" )]
    [Category( "CRM > PhotoRequest" )]
    [Description( "Block for selecting criteria to build a list of people who should receive a photo request." )]

    #region Block Attributes

    [CommunicationTemplateField(
        "Photo Request Template",
        Key = AttributeKey.PhotoRequestTemplate,
        Description = "The template to use with this block to send requests.",
        IsRequired = true,
        DefaultValue = "B9A0489C-A823-4C5C-A9F9-14A206EC3B88",
        Order = 0 )]

    [IntegerField(
        "Maximum Recipients",
        Key = AttributeKey.MaximumRecipients,
        Description = "The maximum number of recipients allowed before communication will need to be approved.",
        IsRequired = false,
        DefaultIntegerValue = 300,
        Order = 1 )]

    #endregion Block Attributes

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that can approve a photo request communication and send it without further review." )]

    [Rock.SystemGuid.EntityTypeGuid( "AFA0F397-037C-4572-ADF1-EB55C5C22AFC" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "AC5E21A3-03EE-4ADA-BC6D-75EEA431DE05" )]
    [Rock.SystemGuid.BlockTypeGuid( "DE1AF7AE-92A8-484F-B5F2-03D2D4B320EC" )]
    public class PhotoSendRequest : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PhotoRequestTemplate = "PhotoRequestTemplate";
            public const string MaximumRecipients = "MaximumRecipients";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The default value for the "Age is more than" criteria, in years.
        /// </summary>
        private const int DefaultAge = 16;

        /// <summary>
        /// The default value for the "Exclude people with a photo updated in the last (years)" criteria, in years.
        /// </summary>
        private const int DefaultPhotoUpdatedYears = 3;

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CustomBlockBox<PhotoSendRequestBag, PhotoSendRequestOptionsBag>
            {
                Bag = new PhotoSendRequestBag
                {
                    Age = DefaultAge,
                    PhotoUpdatedYears = DefaultPhotoUpdatedYears
                },
                Options = BuildOptionsBag()
            };
        }

        /// <summary>
        /// Builds the option lists and settings the criteria form needs to render.
        /// </summary>
        /// <returns>The populated options bag.</returns>
        private PhotoSendRequestOptionsBag BuildOptionsBag()
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var connectionStatusType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() );

            return new PhotoSendRequestOptionsBag
            {
                FamilyRoleOptions = familyGroupType.Roles.ToListItemBagList(),
                ConnectionStatusOptions = connectionStatusType?.DefinedValues
                    .Where( v => v.IsActive )
                    .ToListItemBagList() ?? new List<ListItemBag>(),
                MaximumRecipients = GetAttributeValue( AttributeKey.MaximumRecipients ).AsIntegerOrNull() ?? int.MaxValue,
                IsApproveAuthorized = BlockCache.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson )
            };
        }

        /// <summary>
        /// Builds the query of people who match the supplied criteria. An empty role or connection-status
        /// selection intentionally matches no one.
        /// </summary>
        /// <param name="criteria">The criteria selected on the form.</param>
        /// <returns>A queryable of the matching people.</returns>
        private IQueryable<Person> GetMatchingPeople( PhotoSendRequestCriteriaBag criteria )
        {
            var personService = new PersonService( RockContext );
            var photoRequestGroupGuid = Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid();
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();

            // Resolve the selected unique identifiers to integer ids via cache so the query
            // filters on indexed id columns rather than guids.
            var selectedRoleGuids = ( criteria.FamilyRoleGuids ?? new List<string>() )
                .Select( g => g.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();
            var selectedRoleIds = familyGroupType.Roles
                .Where( r => selectedRoleGuids.Contains( r.Guid ) )
                .Select( r => r.Id )
                .ToList();

            var connectionStatusType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() );
            var selectedStatusGuids = ( criteria.ConnectionStatusGuids ?? new List<string>() )
                .Select( g => g.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();
            var selectedStatusIds = connectionStatusType?.DefinedValues
                .Where( v => selectedStatusGuids.Contains( v.Guid ) )
                .Select( v => v.Id )
                .ToList() ?? new List<int>();

            var minimumAgeBirthDate = RockDateTime.Now.AddYears( -( criteria.Age ?? 0 ) );
            var photoUpdatedCutoff = RockDateTime.Now.AddYears( -( criteria.PhotoUpdatedYears ?? 0 ) );

            // People who are inactive or pending in the photo request group have opted out and are always excluded.
            var optedOutPeople = personService.Queryable( false, false )
                .Where( p => p.Members.Any( gm => gm.Group.Guid == photoRequestGroupGuid
                    && ( gm.GroupMemberStatus == GroupMemberStatus.Inactive || gm.GroupMemberStatus == GroupMemberStatus.Pending ) ) );

            var people = personService.Queryable( false, false )
                // Must have an email address to receive the request.
                .Where( p => p.Email != null && p.Email.Trim() != string.Empty )
                // Must match one of the selected connection statuses.
                .Where( p => selectedStatusIds.Contains( p.ConnectionStatusValueId ?? -1 ) )
                // Must be older than the requested age.
                .Where( p => p.BirthDate <= minimumAgeBirthDate )
                // Must belong to a family in one of the selected roles.
                .Where( p => p.Members.Any( gm => gm.Group.GroupTypeId == familyGroupType.Id
                    && selectedRoleIds.Contains( gm.GroupRoleId ) ) )
                // Must be missing a photo or have one older than the requested threshold.
                .Where( p => p.PhotoId == null || p.Photo.ModifiedDateTime == null || p.Photo.ModifiedDateTime <= photoUpdatedCutoff )
                .Except( optedOutPeople );

            return people;
        }

        /// <summary>
        /// Gets the configured communication template, returning a user-facing error message when it is
        /// missing or misconfigured.
        /// </summary>
        /// <param name="errorMessage">When this method returns <c>null</c>, contains the configuration error to display.</param>
        /// <returns>The communication template, or <c>null</c> if it is not configured.</returns>
        private CommunicationTemplate GetConfiguredTemplate( out string errorMessage )
        {
            errorMessage = null;

            var templateGuid = GetAttributeValue( AttributeKey.PhotoRequestTemplate ).AsGuidOrNull();
            if ( !templateGuid.HasValue )
            {
                errorMessage = "Missing communication template configuration.";
                return null;
            }

            var template = new CommunicationTemplateService( RockContext ).Get( templateGuid.Value );
            if ( template == null )
            {
                errorMessage = "The communication template appears to be missing.";
            }

            return template;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the number of people who match the supplied criteria, used to confirm a send before it happens.
        /// </summary>
        /// <param name="criteria">The criteria selected on the form.</param>
        /// <returns>The matching recipient count.</returns>
        [BlockAction]
        public BlockActionResult GetRecipientCount( PhotoSendRequestCriteriaBag criteria )
        {
            if ( criteria == null )
            {
                return ActionBadRequest( "No criteria were provided." );
            }

            var count = GetMatchingPeople( criteria ).Count();

            return ActionOk( count );
        }

        /// <summary>
        /// Builds and sends (or submits for approval) the photo request communication to everyone matching the criteria.
        /// </summary>
        /// <param name="criteria">The criteria selected on the form.</param>
        /// <returns>A message describing the result of the send.</returns>
        [BlockAction]
        public BlockActionResult SendCommunication( PhotoSendRequestCriteriaBag criteria )
        {
            var currentPerson = RequestContext.CurrentPerson;
            if ( currentPerson?.PrimaryAliasId == null )
            {
                return ActionUnauthorized();
            }

            if ( criteria == null )
            {
                return ActionBadRequest( "No criteria were provided." );
            }

            var template = GetConfiguredTemplate( out var configurationError );
            if ( template == null )
            {
                return ActionBadRequest( configurationError );
            }

            // Resolve recipients up front as primary alias ids to avoid loading each alias individually.
            var recipientAliasIds = GetMatchingPeople( criteria )
                .Where( p => p.PrimaryAliasId.HasValue )
                .Select( p => p.PrimaryAliasId.Value )
                .ToList();

            if ( recipientAliasIds.Count == 0 )
            {
                return ActionBadRequest( "That didn't match anyone. Try adjusting your criteria." );
            }

            var communicationService = new CommunicationService( RockContext );
            var emailMediumEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;

            var communication = new Rock.Model.Communication
            {
                Status = CommunicationStatus.Transient,
                SenderPersonAliasId = currentPerson.PrimaryAliasId,
                IsBulkCommunication = true,
                CommunicationType = CommunicationType.Email,
                FutureSendDateTime = null
            };
            communicationService.Add( communication );

            CommunicationDetails.Copy( template, communication );

            foreach ( var personAliasId in recipientAliasIds )
            {
                communication.Recipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = personAliasId,
                    MediumEntityTypeId = emailMediumEntityTypeId
                } );
            }

            var maximumRecipients = GetAttributeValue( AttributeKey.MaximumRecipients ).AsIntegerOrNull() ?? int.MaxValue;
            var isApprovalRequired = communication.Recipients.Count > maximumRecipients;
            var canApprove = BlockCache.IsAuthorized( Authorization.APPROVE, currentPerson );

            string message;
            if ( isApprovalRequired && !canApprove )
            {
                communication.Status = CommunicationStatus.PendingApproval;
                message = "Communication has been submitted for approval.";
            }
            else
            {
                communication.Status = CommunicationStatus.Approved;
                communication.ReviewedDateTime = RockDateTime.Now;
                communication.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;
                message = "Communication has been queued for sending.";
            }

            RockContext.SaveChanges();

            if ( communication.Status == CommunicationStatus.Approved )
            {
                new ProcessSendCommunication.Message
                {
                    CommunicationId = communication.Id
                }.Send();
            }

            return ActionOk( message );
        }

        /// <summary>
        /// Sends a test photo request communication to the current person so they can preview it.
        /// </summary>
        /// <returns>A message describing the result of the test send.</returns>
        [BlockAction]
        public BlockActionResult SendTestCommunication()
        {
            var currentPerson = RequestContext.CurrentPerson;
            if ( currentPerson?.PrimaryAliasId == null )
            {
                return ActionUnauthorized();
            }

            if ( currentPerson.Email.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A test email cannot be sent because you do not have an email address." );
            }

            var template = GetConfiguredTemplate( out var configurationError );
            if ( template == null )
            {
                return ActionBadRequest( configurationError );
            }

            var communicationService = new CommunicationService( RockContext );
            var emailMediumEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;

            var testCommunication = new Rock.Model.Communication
            {
                Status = CommunicationStatus.Approved,
                SenderPersonAliasId = currentPerson.PrimaryAliasId,
                IsBulkCommunication = true,
                CommunicationType = CommunicationType.Email,
                FutureSendDateTime = null,
                ReviewedDateTime = RockDateTime.Now,
                ReviewerPersonAliasId = currentPerson.PrimaryAliasId
            };

            CommunicationDetails.Copy( template, testCommunication );

            testCommunication.Recipients.Add( new CommunicationRecipient
            {
                Status = CommunicationRecipientStatus.Pending,
                PersonAliasId = currentPerson.PrimaryAliasId.Value,
                MediumEntityTypeId = emailMediumEntityTypeId
            } );

            // Persist so it can be sent, then remove the temporary test communication once delivery is queued.
            communicationService.Add( testCommunication );
            RockContext.SaveChanges();

            Rock.Model.Communication.Send( testCommunication );

            communicationService.Delete( testCommunication );
            RockContext.SaveChanges();

            return ActionOk( "Test communication has been sent." );
        }

        #endregion Block Actions
    }
}
