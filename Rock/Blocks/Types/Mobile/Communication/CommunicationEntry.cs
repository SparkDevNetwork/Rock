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

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Communication.CommunicationEntry;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Communication
{
    /// <summary>
    /// The mobile adaptation of the web CommunicationEntry block.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    #region Block Attributes

    [BooleanField( "Enable Email",
        Description = "When enabled, show email as a selectable communication transport.",
        DefaultBooleanValue = true,
        Key = AttributeKey.EnableEmail,
        Order = 0 )]

    [BooleanField( "Enable SMS",
        Description = "When enabled, show SMS as a selectable communication transport.",
        DefaultBooleanValue = true,
        Key = AttributeKey.EnableSms,
        Order = 1 )]

    [BooleanField( "Show From Name",
        Description = "When enabled, a field will be shown to input From Name (email transport only).",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowFromName,
        Order = 2 )]

    [BooleanField( "Show Reply To",
        Description = "When enabled, a field will be shown to input Reply To (email transport only).",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowReplyTo,
        Order = 3 )]

    [BooleanField( "Show Send To Parents",
        Description = "When enabled, a toggle will show to enable an individual with the Age Classification of 'Child' to have the communication sent to their parents as well.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowSendToParents,
        Order = 4 )]

    [BooleanField( "Is Bulk",
        Description = "When enabled, the communication will be flagged as a bulk communication.",
        DefaultBooleanValue = false,
        Key = AttributeKey.IsBulk,
        Order = 5 )]

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKey.AllowedSmsNumbers,
        Order = 6 )]

    [BooleanField( "Show only personal SMS number",
        Description = "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowOnlyPersonalSmsNumber,
        Order = 7 )]

    [BooleanField( "Hide personal SMS numbers",
        Description = "Only SMS Numbers that are not associated with a person. The numbers without an Assigned To Person value.",
        DefaultBooleanValue = false,
        Key = AttributeKey.HidePersonalSmsNumbers,
        Order = 8 )]

    [IntegerField( "SMS Character Limit",
        Description = "The number of characters to limit the SMS communication body to. Set to 0 to disable.",
        IsRequired = false,
        Key = AttributeKey.SmsCharacterLimit,
        DefaultIntegerValue = 0,
        Order = 9 )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page to link to when user taps on a person listed in the 'Failed to Deliver' section. PersonGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.PersonProfilePage,
        Order = 10 )]

    [BooleanField( "Show Additional Email Recipients",
        Key = AttributeKey.ShowAdditionalEmailRecipients,
        Description = "Allow additional email recipients to be entered for email communications?",
        DefaultBooleanValue = false,
        Order = 11 )]

    #endregion

    [DisplayName( "Communication Entry" )]
    [Category( "Mobile > Communication" )]
    [Description( "Allows you to send communications to a set of recipients." )]
    [IconCssClass( "ti ti-message" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    // This block uses a unique security action that determines whether someone can send out communications immediately or has
    // to submit them for approval.
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_COMMUNICATION_COMMUNICATIONENTRY_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "B0182DA2-82F7-4798-A48E-88EBE61F2109" )]
    public class CommunicationEntry : RockBlockType
    {
        #region Attribute Keys

        /// <summary>
        /// The block setting attribute keys for the Communication Entry mobile block.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The enable email attribute key.
            /// </summary>
            public const string EnableEmail = "EnableEmail";

            /// <summary>
            /// The enable SMS attribute key.
            /// </summary>
            public const string EnableSms = "EnableSms";

            /// <summary>
            /// The enable parent communication attribute key.
            /// </summary>
            public const string ShowSendToParents = "ShowSendToParents";

            /// <summary>
            /// The show from name attribute key.
            /// </summary>
            public const string ShowFromName = "ShowFromName";

            /// <summary>
            /// The show reply to attribute key.
            /// </summary>
            public const string ShowReplyTo = "ShowReplyTo";

            /// <summary>
            /// The is bulk attribute key.
            /// </summary>
            public const string IsBulk = "IsBulk";

            /// <summary>
            /// The allowed SMS numbers attribute key.
            /// </summary>
            public const string AllowedSmsNumbers = "AllowedSMSNumbers";

            /// <summary>
            /// The show only personal SMS number attribute key.
            /// </summary>
            public const string ShowOnlyPersonalSmsNumber = "ShowOnlyPersonalSmsNumber";

            /// <summary>
            /// The hide personal SMS numbers attribute key.
            /// </summary>
            public const string HidePersonalSmsNumbers = "HidePersonalSmsNumbers";

            /// <summary>
            /// The number of characters to limit the SMS communication body to.
            /// </summary>
            public const string SmsCharacterLimit = "SmsCharacterLimit";

            /// <summary>
            /// The person profile page attribute key.
            /// </summary>
            public const string PersonProfilePage = "PersonProfilePage";

            /// <summary>
            /// Whether to show additional email recipients.
            /// </summary>
            public const string ShowAdditionalEmailRecipients = "ShowAdditionalEmailRecipients";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether email should be displayed as an option of communication medium.
        /// </summary>
        /// <value><c>true</c> if [enable email]; otherwise, <c>false</c>.</value>
        public bool EnableEmail => GetAttributeValue( AttributeKey.EnableEmail ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether SMS should be displayed as an option of communication medium.
        /// </summary>
        /// <value><c>true</c> if [enable SMS]; otherwise, <c>false</c>.</value>
        public bool EnableSms => GetAttributeValue( AttributeKey.EnableSms ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether we should show a toggle, used to send a communication to the parent of any children in the recipients.
        /// </summary>
        /// <value><c>true</c> if [enable parent communication]; otherwise, <c>false</c>.</value>
        public bool ShowSendToParents => GetAttributeValue( AttributeKey.ShowSendToParents ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether this instance is bulk.
        /// </summary>
        /// <value><c>true</c> if this instance is bulk; otherwise, <c>false</c>.</value>
        public bool IsBulk => GetAttributeValue( AttributeKey.IsBulk ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether we should show the from name field.
        /// </summary>
        /// <value><c>true</c> if [show from name]; otherwise, <c>false</c>.</value>
        public bool ShowFromName => GetAttributeValue( AttributeKey.ShowFromName ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether we should show the reply to field.
        /// </summary>
        /// <value><c>true</c> if [show from name]; otherwise, <c>false</c>.</value>
        public bool ShowReplyTo => GetAttributeValue( AttributeKey.ShowReplyTo ).AsBoolean();

        /// <summary>
        /// Gets the max number of characters allowed in an SMS message.
        /// </summary>
        public int SmsCharacterLimit => GetAttributeValue( AttributeKey.SmsCharacterLimit ).AsIntegerOrNull() ?? 0;

        /// <summary>
        /// Gets the person profile page unique identifier.
        /// </summary>
        /// <value>
        /// The person profile page unique identifier.
        /// </value>
        protected Guid? PersonProfilePageGuid => GetAttributeValue( AttributeKey.PersonProfilePage ).AsGuidOrNull();

        /// <summary>
        /// Whether or not to allow additional email recipients to be entered for email communications.
        /// </summary>
        protected bool ShowAdditionalEmailRecipients => GetAttributeValue( AttributeKey.ShowAdditionalEmailRecipients ).AsBoolean();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>A collection of string/object pairs.</returns>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Communication.CommunicationEntry.Configuration
            {
                EnableEmail = EnableEmail,
                EnableSms = EnableSms,
                ShowSendToParents = ShowSendToParents,
                IsBulk = IsBulk,
                ShowFromName = ShowFromName,
                ShowReplyTo = ShowReplyTo,
                SmsCharacterLimit = SmsCharacterLimit,
                PersonProfilePageGuid = PersonProfilePageGuid,
                AreAdditionalEmailRecipientsAllowed = ShowAdditionalEmailRecipients
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets all phone numbers available to the currently logged in person.
        /// </summary>
        /// <returns>A collection of phone number objects or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult GetPhoneNumbers()
        {
            return ActionOk( LoadPhoneNumbers() );
        }

        /// <summary>
        /// Gets the recipients.
        /// </summary>
        /// <param name="entitySetGuid">The entity set unique identifier.</param>
        /// <returns>A BlockActionResult containing the list of recipients.</returns>
        [BlockAction]
        public BlockActionResult GetRecipients( Guid entitySetGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // First load the entity set from the entity set guid.
                var entitySet = GetCurrentPersonEntitySet( rockContext, entitySetGuid );

                if ( entitySet == null )
                {
                    return ActionNotFound( "Could not load the recipients for this communication." );
                }

                var personService = new PersonService( rockContext ).Queryable();

                // Load the actual "recipients".
                var entitySetItems = new EntitySetItemService( rockContext )
                    .GetByEntitySetId( entitySet.Id )
                    .Select( esi => new
                    {
                        esi.Guid,
                        esi.EntityId
                    } )
                    // We need to take our entity set item and retrieve the corresponding person from the EntityId.
                    .Join(
                        personService,
                        esi => esi.EntityId,
                        p => p.Id,
                        ( esi, p ) => new
                        {
                            p.NickName,
                            p.LastName,
                            p.Email,
                            p.PhoneNumbers,
                            PersonGuid = p.Guid,
                            p.PhotoId,
                            EntitySetItemGuid = esi.Guid,
                        }
                    )
                    .AsNoTracking()
                    .ToList();

                var recipientBag = entitySetItems.Select( a => new CommunicationRecipientResponseBag
                {
                    NickName = a.NickName,
                    LastName = a.LastName,
                    Email = a.Email,
                    PersonGuid = a.PersonGuid,
                    EntitySetItemGuid = a.EntitySetItemGuid,
                    PhotoUrl = a.PhotoId != null ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( a.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) ) : string.Empty,
                    SmsNumber = a.PhoneNumbers.GetFirstSmsNumber(),
                } ).ToList();

                return ActionOk( recipientBag );
            }
        }

        /// <summary>
        /// Deletes the recipient from entity set.
        /// </summary>
        /// <param name="entitySetGuid">The entity set unique identifier.</param>
        /// <param name="entitySetItemGuid">The entity set item unique identifier.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult DeleteRecipientFromEntitySet( Guid entitySetGuid, Guid entitySetItemGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var entitySet = GetCurrentPersonEntitySet( rockContext, entitySetGuid );

                if ( entitySet == null )
                {
                    return ActionNotFound();
                }

                // Load our recipient.
                var entitySetItemService = new EntitySetItemService( rockContext );
                var entitySetItem = entitySetItemService.Queryable()
                    .Where( esi => esi.Guid == entitySetItemGuid && esi.EntitySetId == entitySet.Id )
                    .FirstOrDefault();

                if ( entitySetItem == null )
                {
                    return ActionNotFound();
                }

                entitySetItemService.Delete( entitySetItem );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Sends the communication.
        /// </summary>
        /// <param name="sendCommunicationBag">The send communication bag.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult SendCommunication( SendCommunicationRequestBag sendCommunicationBag )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( RequestContext.CurrentPerson == null )
                {
                    return ActionForbidden();
                }

                if ( sendCommunicationBag == null )
                {
                    return ActionBadRequest();
                }

                var entitySet = GetCurrentPersonEntitySet( RockContext, sendCommunicationBag.EntitySetGuid );

                if ( entitySet == null )
                {
                    return ActionNotFound( "Could not load the recipients for this communication." );
                }

                if ( entitySet.IsExpired )
                {
                    return ActionBadRequest( "The message session has expired. Please try again." );
                }

                /*
                    10/18/2026 - MSE

                    The attachment guids come from the client. Mobile uploads are saved as
                    permanent files, so only a file the current person uploaded is accepted.
                    Otherwise any file in the system, including secured files, could be
                    attached and emailed to the sender.

                    Reason: Prevent sending files the person is not allowed to see.
                */
                if ( !IsAttachmentAllowed( sendCommunicationBag.FileAttachmentGuid )
                     || !IsAttachmentAllowed( sendCommunicationBag.ImageAttachmentGuid ) )
                {
                    return ActionBadRequest( "The attachment is not valid." );
                }

                /*
                    10/19/2026 - MSE

                    The from number comes from the client. Only a number the phone
                    number picker offers is accepted, which applies number security,
                    SMS enabled, the Allowed SMS Numbers setting and the personal
                    number settings. Otherwise a text could be sent from another
                    person's personal number or a number this block does not allow.

                    Reason: Enforce the block's SMS number settings on the server.
                */
                if ( sendCommunicationBag.CommunicationType.ToNative() == CommunicationType.SMS
                     && !LoadPhoneNumbers().Any( n => n.Guid == sendCommunicationBag.FromNumberGuid ) )
                {
                    return ActionBadRequest( "Invalid From Phone." );
                }

                return ActionOk( SendCommunicationInternal( sendCommunicationBag, entitySet ) );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the phone numbers that the current person is allowed to access.
        /// </summary>
        /// <returns>A collection of objects representing the SMS phone numbers.</returns>
        private IEnumerable<PhoneNumberBag> LoadPhoneNumbers()
        {
            // First load up all of the available numbers.
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .Where( spn => spn.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) && spn.IsSmsEnabled );

            // Filter to the numbers that are specifically configured for use with this block.
            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSmsNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) );
            }

            // Filter personal numbers (any that have a response recipient) if the hide personal option is enabled.
            if ( GetAttributeValue( AttributeKey.HidePersonalSmsNumbers ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( spn => !spn.AssignedToPersonAliasId.HasValue );
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( AttributeKey.ShowOnlyPersonalSmsNumber ).AsBoolean() && !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                smsNumbers = smsNumbers.Where( spn => RequestContext.CurrentPerson.Aliases.Any( a => a.Id == spn.AssignedToPersonAliasId ) );
            }

            return smsNumbers
                .Select( n => new PhoneNumberBag
                {
                    Guid = n.Guid,
                    PhoneNumber = n.Number,
                    Description = n.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the person entity set if it was created by the current person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entitySetGuid">The entity set unique identifier from the client.</param>
        /// <returns>The <see cref="EntitySet"/>, or <c>null</c> if it was not found, is not a person entity set or was not created by the current person.</returns>
        private EntitySet GetCurrentPersonEntitySet( RockContext rockContext, Guid entitySetGuid )
        {
            /*
                10/19/2026 - MSE

                The entity set Guid comes from the client. The mobile app creates
                the set with api/EntitySets/CreateFromItems while the person is
                signed in, so the creator is the person who should use it. The
                person Id is compared instead of the alias Id so a merged person
                still matches. Sets with no creator, or that are not person sets,
                are rejected.

                Reason: Prevent reading, editing or messaging another person's recipient list.
            */
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( !currentPersonId.HasValue )
            {
                return null;
            }

            var entitySet = new EntitySetService( rockContext ).Get( entitySetGuid );
            var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;

            if ( entitySet == null
                 || !entitySet.CreatedByPersonAliasId.HasValue
                 || entitySet.EntityTypeId != personEntityTypeId )
            {
                return null;
            }

            var ownerPersonId = new PersonAliasService( rockContext ).GetPersonId( entitySet.CreatedByPersonAliasId.Value );

            return ownerPersonId.HasValue && ownerPersonId.Value == currentPersonId.Value ? entitySet : null;
        }

        /// <summary>
        /// Determines whether the binary file can be attached to the
        /// communication by the current person.
        /// </summary>
        /// <param name="binaryFileGuid">The unique identifier of the binary file from the client.</param>
        /// <returns><c>true</c> if the file can be attached; otherwise <c>false</c>.</returns>
        private bool IsAttachmentAllowed( Guid? binaryFileGuid )
        {
            if ( !binaryFileGuid.HasValue || binaryFileGuid.Value == Guid.Empty )
            {
                return true;
            }

            var guid = binaryFileGuid.Value;
            var file = new BinaryFileService( RockContext ).Queryable()
                .Where( b => b.Guid == guid )
                .Select( b => new
                {
                    CreatedByPersonId = ( int? ) b.CreatedByPersonAlias.PersonId
                } )
                .FirstOrDefault();

            // An unknown file is ignored when the communication is built.
            if ( file == null )
            {
                return true;
            }

            var currentPersonId = RequestContext.CurrentPerson?.Id;

            return currentPersonId.HasValue && file.CreatedByPersonId == currentPersonId.Value;
        }

        /// <summary>
        /// Creates a new communication and sends it based off the information in the <see cref="SendCommunication(SendCommunicationRequestBag)" />.
        /// </summary>
        /// <param name="bag">The bag containing the communication information.</param>
        /// <param name="entityset">The EntitySet of the recipients.</param>
        /// <returns></returns>
        private string SendCommunicationInternal( SendCommunicationRequestBag bag, EntitySet entityset )
        {
            var communicationService = new CommunicationService( RockContext );
            var entitySetItemService = new EntitySetItemService( RockContext );
            var personService = new PersonService( RockContext );
            var personAliasService = new PersonAliasService( RockContext );
            var binaryFileService = new BinaryFileService( RockContext );
            var groupMemberService = new GroupMemberService( RockContext );
            var communicationType = bag.CommunicationType.ToNative();

            // Create the initial communication.
            var communication = CreateNewCommunication( communicationType, bag.IsBulk );
            communicationService.Add( communication );

            // Load the recipients from the entity set.
            var recipientPeopleAliasIds = GetPeopleFromEntitySet( entityset, entitySetItemService, personService );
            AddRecipientsToCommunication( communication, recipientPeopleAliasIds );

            if ( bag.SendToParents )
            {
                AddParentsToCommunication( communication, recipientPeopleAliasIds.Where( id => id.HasValue ).Select( id => id.Value ), personAliasService, groupMemberService );
            }

            // Structure the communication based on the communication type.
            if ( communicationType == CommunicationType.Email )
            {
                /*
                    10/18/2026 - MSE

                    Show Additional Email Recipients was only sent to the client as a UI
                    flag. A crafted request could still add any email address as a
                    recipient, so the setting is now enforced on the server.

                    Reason: Only honor additional recipients when the block allows them.
                */
                var additionalRecipients = ShowAdditionalEmailRecipients ? bag.AdditionalEmailRecipients : null;

                StructureEmailCommunication( communication, bag.Subject, bag.Message, bag.FromEmail, bag.FromName, bag.ReplyTo, true, bag.FileAttachmentGuid, additionalRecipients, personService, binaryFileService );
            }
            else if ( communicationType == CommunicationType.SMS )
            {
                StructureSmsCommunication( communication, bag.FromNumberGuid, bag.Message, bag.ImageAttachmentGuid, binaryFileService );
            }

            // Save the communication prior to checking recipients.
            communication.Status = CommunicationStatus.Draft;
            RockContext.SaveChanges();

            var successMessage = DetermineCommunicationStatus( communication, bag.MaxRecipients );
            RockContext.SaveChanges();

            // Send the approval email (if needed), now that we have a communication id.
            if ( communication.Status == CommunicationStatus.PendingApproval )
            {
                SendApprovalEmail( communication.Id );
            }

            // If the communication is already approved, queue it up to send.
            if ( communication.Status == CommunicationStatus.Approved &&
                        ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now ) )
            {
                SendCommunication( communication.Id );
            }

            RockContext.SaveChanges();

            return successMessage;
        }

        /// <summary>
        /// Adds the parents of the children in the communication to the communication.
        /// </summary>
        /// <param name="communication"></param>
        /// <param name="recipientPersonAliasIds"></param>
        /// <param name="personAliasService"></param>
        /// <param name="groupMemberService"></param>
        private void AddParentsToCommunication( Rock.Model.Communication communication, IEnumerable<int> recipientPersonAliasIds, PersonAliasService personAliasService, GroupMemberService groupMemberService )
        {
            // This is knowingly obscure. We are actually just going to include all adult family members in the family, since there
            // is no great way to distinguish a child's father/mother in Rock today. In other words, Uncle Joe will also
            // receive the communication.
            var parentAliasIds = GetParentsForChildren( recipientPersonAliasIds, personAliasService, groupMemberService );

            foreach ( var parentAliasId in parentAliasIds )
            {
                if ( !parentAliasId.HasValue )
                {
                    continue;
                }

                // Checking for duplicates.
                if ( communication.Recipients.Any( cr => cr.PersonAliasId == parentAliasId ) )
                {
                    continue;
                };

                communication.Recipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = parentAliasId
                } );
            }
        }

        /// <summary>
        /// This function will take a person, and if they're a child return a queryable of all
        /// of the adults in their primary family. The term 'Parents' is iffy, we know.
        /// </summary>
        /// <param name="personAliasIds">The people.</param>
        /// <param name="groupMemberService">The group member service.</param>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="filterByGender">The filter by gender.</param>
        /// <returns>IQueryable&lt;Person&gt;.</returns>
        private IEnumerable<int?> GetParentsForChildren( IEnumerable<int> personAliasIds, PersonAliasService personAliasService, GroupMemberService groupMemberService, Gender? filterByGender = null )
        {
            // This is knowingly obscure. We are actually just going to include all adult family members in the family, since there
            // is no great way to distinguish a child's father/mother in Rock today. In other words, Uncle Joe could also
            // receive the communication.
            var personFamilyIds = personAliasService
                .Queryable()
                .Where( pa => personAliasIds.Contains( pa.Id ) && pa.Person.AgeClassification == AgeClassification.Child )
                .Select( pa => pa.Person.PrimaryFamilyId )
                .ToList();

            var parentsQry = groupMemberService
                .Queryable()
                .Where( gm => personFamilyIds.Contains( gm.GroupId )
                    && gm.Person.AgeClassification == AgeClassification.Adult
                    && gm.GroupMemberStatus == GroupMemberStatus.Active );

            if ( filterByGender != null )
            {
                parentsQry = parentsQry.Where( x => x.Person.Gender == filterByGender );
            }

            return parentsQry.Select( gm => gm.Person.PrimaryAliasId );
        }

        /// <summary>
        /// Sends a communication.
        /// </summary>
        /// <param name="communicationId"></param>
        private void SendCommunication( int communicationId )
        {
            var transactionMsg = new ProcessSendCommunication.Message()
            {
                CommunicationId = communicationId
            };
            transactionMsg.Send();
        }

        /// <summary>
        /// Sends an approval email for a communication.
        /// </summary>
        /// <param name="communicationId"></param>
        private void SendApprovalEmail( int communicationId )
        {
            var approvalTransactionMsg = new ProcessSendCommunicationApprovalEmail.Message()
            {
                CommunicationId = communicationId
            };
            approvalTransactionMsg.Send();
        }

        /// <summary>
        /// Determines whether a communication requires approval or marks it as approved.
        /// </summary>
        /// <param name="communication"></param>
        /// <param name="maxRecipients"></param>
        /// <returns></returns>
        private string DetermineCommunicationStatus( Rock.Model.Communication communication, int? maxRecipients )
        {
            // This block uses a unique 'Approve' security verb to either submit a communication for approval or
            // instantly queue a communication. Only required if the request exceeds the maximum number of recipients.
            if ( RequiresApproval( communication.Recipients.Count(), maxRecipients ) )
            {
                communication.Status = CommunicationStatus.PendingApproval;
                return $"Your message to {communication.Recipients.Count} {"recipient".PluralizeIf( communication.Recipients.Count > 0 )} has been submitted for approval.";
            }
            else
            {
                communication.Status = CommunicationStatus.Approved;
                communication.ReviewedDateTime = RockDateTime.Now;
                communication.ReviewerPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;

                if ( communication.FutureSendDateTime.HasValue &&
                    communication.FutureSendDateTime > RockDateTime.Now )
                {
                    return string.Format( "Communication will be sent {0}.",
                        communication.FutureSendDateTime.Value.ToRelativeDateString( 0 ) );
                }
                else
                {
                    return $"Your message to {communication.Recipients.Count} {"recipient".PluralizeIf( communication.Recipients.Count > 0 )} has been queued for sending and should be delivered in the next few minutes.";
                }
            }
        }

        /// <summary>
        /// Adds the recipients to a communication from their person alias ids.
        /// </summary>
        /// <param name="communication"></param>
        /// <param name="recipientPersonAliasIds"></param>
        private void AddRecipientsToCommunication( Rock.Model.Communication communication, IEnumerable<int?> recipientPersonAliasIds )
        {
            foreach ( var recipientPersonAliasId in recipientPersonAliasIds )
            {
                if ( !recipientPersonAliasId.HasValue )
                {
                    continue;
                }

                // Checking for duplicates.
                if ( communication.Recipients.Any( cr => cr.PersonAliasId == recipientPersonAliasId ) )
                {
                    continue;
                };

                communication.Recipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = recipientPersonAliasId.Value
                } );
            }
        }

        /// <summary>
        /// Create a generic communication with a Communication Status of transient.
        /// </summary>
        /// <returns></returns>
        private Rock.Model.Communication CreateNewCommunication( CommunicationType communicationType, bool isBulk )
        {
            var communication = new Rock.Model.Communication
            {
                Status = CommunicationStatus.Transient,
                SenderPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId,
                CommunicationType = communicationType,
                IsBulkCommunication = isBulk
            };

            return communication;
        }

        /// <summary>
        /// Structures the communication as an email.
        /// </summary>
        /// <param name="communication"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="fromEmail"></param>
        /// <param name="fromName"></param>
        /// <param name="replyTo"></param>
        /// <param name="sanitize"></param>
        /// <param name="additionalRecipients"></param>
        /// <param name="fileAttachmentGuid"></param>
        /// <param name="personService"></param>
        /// <param name="binaryFileService"></param>
        private void StructureEmailCommunication( Rock.Model.Communication communication, string subject, string body, string fromEmail, string fromName, string replyTo, bool sanitize, Guid? fileAttachmentGuid, List<string> additionalRecipients, PersonService personService, BinaryFileService binaryFileService )
        {
            var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;
            foreach ( var recipient in communication.Recipients )
            {
                recipient.MediumEntityTypeId = emailMediumEntityTypeId;
            }

            // Add the additional email recipients as "nameless" recipients.
            if ( additionalRecipients != null )
            {
                foreach ( var emailRecipient in additionalRecipients )
                {
                    var emailPerson = personService.GetPersonFromEmailAddress( emailRecipient, true );
                    if ( emailPerson != null )
                    {
                        var newRecipient = new CommunicationRecipient
                        {
                            PersonAliasId = emailPerson.PrimaryAliasId,
                            MediumEntityTypeId = emailMediumEntityTypeId
                        };

                        // Checking for duplicates.
                        if ( communication.Recipients.Any( cr => cr.PersonAliasId == newRecipient.PersonAliasId ) )
                        {
                            continue;
                        };

                        communication.Recipients.Add( newRecipient );
                    }
                }
            }

            // Structuring the communication.
            communication.Subject = subject;

            // Sanitize the message by encoding HTML &
            // converting newlines properly.
            var message = body;
            if ( sanitize )
            {
                message = message.EncodeHtml().ConvertCrLfToHtmlBr();
            }

            communication.Message = message;
            communication.FromEmail = fromEmail;
            communication.FromName = fromName;
            communication.ReplyToEmail = replyTo;

            // If there was a file attachment, add it to the communication.
            if ( fileAttachmentGuid.HasValue )
            {
                var binaryFileId = binaryFileService.Get( fileAttachmentGuid.Value )?.Id;

                if ( binaryFileId.HasValue )
                {
                    communication.AddAttachment( new CommunicationAttachment
                    {
                        BinaryFileId = binaryFileId.Value
                    }, CommunicationType.Email );
                }
            }
        }

        /// <summary>
        /// Structures the communication as an SMS.
        /// </summary>
        /// <param name="communication"></param>
        /// <param name="fromNumberGuid"></param>
        /// <param name="message"></param>
        /// <param name="imageAttachmentGuid"></param>
        /// <param name="binaryFileService"></param>
        private void StructureSmsCommunication( Rock.Model.Communication communication, Guid fromNumberGuid, string message, Guid? imageAttachmentGuid, BinaryFileService binaryFileService )
        {
            var smsMediumTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Sms>().Id;
            foreach ( var recipient in communication.Recipients )
            {
                recipient.MediumEntityTypeId = smsMediumTypeId;
            }

            // Structuring the communication.
            communication.CommunicationType = CommunicationType.SMS;
            communication.SmsFromSystemPhoneNumberId = SystemPhoneNumberCache.Get( fromNumberGuid )?.Id;
            communication.SMSMessage = message;

            // If there was an image attachment, add it to the communication.
            if ( imageAttachmentGuid.HasValue )
            {
                var binaryFileId = binaryFileService.Get( imageAttachmentGuid.Value )?.Id;

                if ( binaryFileId != null )
                {
                    communication.AddAttachment( new CommunicationAttachment
                    {
                        BinaryFileId = binaryFileId.Value
                    }, CommunicationType.SMS );
                }
            }
        }

        /// <summary>
        /// Gets the people from the entity set.
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="entitySetItemService"></param>
        /// <param name="personService"></param>
        /// <returns></returns>
        private IEnumerable<int?> GetPeopleFromEntitySet( EntitySet entitySet, EntitySetItemService entitySetItemService, PersonService personService )
        {
            // Convert our recipients to CommunicationRecipients.
            var recipientPeople = entitySetItemService
                .GetByEntitySetId( entitySet.Id )
                .Select( esi => new
                {
                    esi.EntityId
                } )
                // Join the person that is in reference to the EntityId of the EntitySetItem.
                .Join( personService.Queryable(),
                    e => e.EntityId,
                    p => p.Id,
                    ( e, p ) => p
                );

            return recipientPeople.Select( p => p.PrimaryAliasId );
        }

        /// <summary>
        /// Determines whether or not the communication (based on count) requires approval before
        /// being sent out. If they have permission via the "Approve" security verb, this gets
        /// overridden.
        /// </summary>
        /// <param name="communicationRecipientCount">The communication recipient count.</param>
        /// <param name="maxRecipients">The maximum recipients.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool RequiresApproval( int communicationRecipientCount, int? maxRecipients )
        {
            if ( maxRecipients.HasValue && communicationRecipientCount > maxRecipients.Value && !BlockCache.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson ) )
            {
                return true;
            }

            return false;
        }

        #endregion

    }
}
