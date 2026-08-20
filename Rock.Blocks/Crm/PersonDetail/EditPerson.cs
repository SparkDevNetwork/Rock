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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Allows you to edit a person's full profile.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockEntityDetailBlockType{Person, EditPersonBag}" />

    [DisplayName( "Edit Person" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to edit a person." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Security Actions

    [SecurityAction( SecurityActionKey.EditFinancials, "The roles and/or users that can edit financial information for the selected person." )]
    [SecurityAction( SecurityActionKey.EditSMS, "The roles and/or users that can edit the SMS Enabled properties for the selected person." )]
    [SecurityAction( SecurityActionKey.EditConnectionStatus, "The roles and/or users that can edit the connection status for the selected person." )]
    [SecurityAction( SecurityActionKey.EditRecordStatus, "The roles and/or users that can edit the record status for the selected person." )]
    [SecurityAction( SecurityActionKey.ViewProtectionProfile, "The roles and/or users that can view the protection profile alert for the selected person." )]

    #endregion Security Actions

    #region Block Attributes

    [BooleanField(
        "Hide Grade",
        Key = AttributeKey.HideGrade,
        Description = "Whether the grade and graduation year fields are hidden. The two are always shown or hidden together.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [BooleanField(
        "Hide Anniversary Date",
        Key = AttributeKey.HideAnniversaryDate,
        Description = "Whether the anniversary date field is hidden. When hidden, it does not appear even for people whose marital status is Married.",
        DefaultBooleanValue = false,
        Order = 1 )]

    [CustomEnhancedListField(
        "Search Key Types",
        Key = AttributeKey.SearchKeyTypes,
        Description = "The search key types available in the Search Keys list. With no selection, all types are available.",
        ListSource = ListSource.SearchKeyTypes,
        IsRequired = false,
        Order = 2 )]

    [BooleanField(
        "Require Complete Birth Date",
        Key = AttributeKey.RequireCompleteBirthDate,
        Description = "Whether the year is required once a birth month and day are present.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [CustomDropdownListField(
        "Race Field",
        Key = AttributeKey.RaceOption,
        Description = "Whether the race field is hidden, optional, or required.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Order = 4 )]

    [CustomDropdownListField(
        "Ethnicity Field",
        Key = AttributeKey.EthnicityOption,
        Description = "Whether the ethnicity field is hidden, optional, or required.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Order = 5 )]

    [BooleanField(
        "Mobile SMS Enabled by Default",
        Key = AttributeKey.DefaultMobileSMSChecked,
        Description = "Whether SMS is enabled automatically on a new mobile number. Applies only when the mobile number is currently blank.",
        DefaultBooleanValue = true,
        Order = 6 )]

    #endregion Block Attributes

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "E295AD66-880C-4FB8-954B-36D11B8C2E92" )]
    [Rock.SystemGuid.BlockTypeGuid( "3A036FB2-366E-4F8A-AF7F-2044230CAADA" )]
    //[Rock.SystemGuid.BlockTypeGuid( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0" )]
    public class EditPerson : RockEntityDetailBlockType<Person, EditPersonBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string HideGrade = "HideGrade";
            public const string HideAnniversaryDate = "HideAnniversaryDate";
            public const string SearchKeyTypes = "SearchKeyTypes";
            public const string RequireCompleteBirthDate = "RequireCompleteBirthDate";
            public const string RaceOption = "RaceOption";
            public const string EthnicityOption = "EthnicityOption";
            public const string DefaultMobileSMSChecked = "DefaultMobileSMSChecked";
        }

        private static class SecurityActionKey
        {
            public const string EditFinancials = "EditFinancials";
            public const string EditSMS = "EditSMS";
            public const string EditConnectionStatus = "EditConnectionStatus";
            public const string EditRecordStatus = "EditRecordStatus";
            public const string ViewProtectionProfile = "ViewProtectionProfile";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Attribute List Sources

        private static class ListSource
        {
            /// <summary>
            /// Search key types that are user-selectable (their "UserSelectable" attribute is not False).
            /// </summary>
            public const string SearchKeyTypes = @"
        DECLARE @AttributeId int = (
	        SELECT [Id]
	        FROM [Attribute]
	        WHERE [Guid] = '15C419AA-76A9-4105-AB99-8384AB0E9B44'
        )
        SELECT
	        CAST( V.[Guid] as varchar(40) ) AS [Value],
	        V.[Value] AS [Text]
        FROM [DefinedType] T
        INNER JOIN [DefinedValue] V ON V.[DefinedTypeId] = T.[Id]
        LEFT OUTER JOIN [AttributeValue] AV
	        ON AV.[EntityId] = V.[Id]
	        AND AV.[AttributeId] = @AttributeId
	        AND AV.[Value] = 'False'
        WHERE T.[Guid] = '61BDD0E3-173D-45AB-9E8C-1FBB9FA8FDF3'
        AND AV.[Id] IS NULL
        ORDER BY V.[Order]
";

            public const string HIDE_OPTIONAL_REQUIRED = "Hide,Optional,Required";
        }

        #endregion Attribute List Sources

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<EditPersonBag, EditPersonOptionsBag>();

            var person = GetInitialEntity();

            SetBoxInitialEntityState( box, person );

            box.NavigationUrls = GetBoxNavigationUrls( person );
            box.Options = GetBoxOptions( person );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or ErrorMessage
        /// depending on the person and the current user's edit permission.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="person">The person being edited, or null when one could not be resolved.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<EditPersonBag, EditPersonOptionsBag> box, Person person )
        {
            if ( person == null )
            {
                box.ErrorMessage = $"The {Person.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( person );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Person.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the box options for rendering the form: block-setting flags, per-field security,
        /// header labels, and the account protection profile banner.
        /// </summary>
        /// <param name="person">The person being edited, or null.</param>
        /// <returns>The populated <see cref="EditPersonOptionsBag"/>.</returns>
        private EditPersonOptionsBag GetBoxOptions( Person person )
        {
            var canAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            var options = new EditPersonOptionsBag
            {
                IsGradeHidden = GetAttributeValue( AttributeKey.HideGrade ).AsBoolean(),
                IsAnniversaryDateHidden = GetAttributeValue( AttributeKey.HideAnniversaryDate ).AsBoolean(),
                RaceOption = GetAttributeValue( AttributeKey.RaceOption ),
                EthnicityOption = GetAttributeValue( AttributeKey.EthnicityOption ),
                IsCompleteBirthDateRequired = GetAttributeValue( AttributeKey.RequireCompleteBirthDate ).AsBoolean(),
                IsMobileSmsEnabledByDefault = GetAttributeValue( AttributeKey.DefaultMobileSMSChecked ).AsBoolean(),

                IsConnectionStatusEditable = canAdministrate || BlockCache.IsAuthorized( SecurityActionKey.EditConnectionStatus, RequestContext.CurrentPerson ),
                IsRecordStatusEditable = canAdministrate || BlockCache.IsAuthorized( SecurityActionKey.EditRecordStatus, RequestContext.CurrentPerson ),
                IsRecordSourceEditable = canAdministrate,
                IsGivingSectionVisible = canAdministrate || BlockCache.IsAuthorized( SecurityActionKey.EditFinancials, RequestContext.CurrentPerson ),
            };

            // Grade and Graduation Year are two views of the same stored value
            var currentGraduationDate = PersonService.GetCurrentGraduationDate();
            var gradeTransitionDate = new System.DateTime( RockDateTime.Now.Year, currentGraduationDate.Month, currentGraduationDate.Day );
            options.GradeTransitionYear = RockDateTime.Now.Year;
            options.GradeOffsetAdjustment = RockDateTime.Today < gradeTransitionDate ? 0 : 1;

            if ( person != null )
            {
                options.FamilyName = person.PrimaryFamily?.Name;
                options.CampusName = person.PrimaryCampus?.Name;
                options.NoPictureUrl = RequestContext.ResolveRockUrl( Person.GetPersonPhotoUrl( person, 400 ) );
                options.IsOnlyActiveFamilyMember = IsOnlyActiveFamilyMember( person );
                SetAccountProtectionProfileMessage( options, person );
            }

            /*
                8/19/26 - CLAUDE

                Remaining option sources still to wire as their sections come online:
                  - IsChatVisible, IsEnvelopeNumberVisible feature flags.
                  - GivingGroups and SearchKeyTypes option lists.

                Reason: Options grow with the client sections.
            */

            return options;
        }

        /// <inheritdoc/>
        protected override EditPersonBag GetEntityBagForView( Person entity )
        {
            return GetEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override EditPersonBag GetEntityBagForEdit( Person entity )
        {
            return GetEntityBag( entity );
        }

        /// <summary>
        /// Maps the person's current values onto the editable bag. Mirrors the WebForms ShowDetails() flow.
        /// </summary>
        /// <param name="person">The person being edited.</param>
        /// <returns>The populated <see cref="EditPersonBag"/>, or null.</returns>
        private EditPersonBag GetEntityBag( Person person )
        {
            if ( person == null )
            {
                return null;
            }

            var gradeFormatted = Person.GradeFormattedFromGradeOffset( person.GradeOffset );

            var bag = new EditPersonBag
            {
                IdKey = person.IdKey,
                Title = ToDefinedValueListItemBag( person.TitleValueId ),
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                Suffix = ToDefinedValueListItemBag( person.SuffixValueId ),
                Photo = person.Photo?.ToListItemBag( person.Photo.FileName ),
                ConnectionStatus = ToDefinedValueListItemBag( person.ConnectionStatusValueId ),
                RecordStatus = ToDefinedValueListItemBag( person.RecordStatusValueId ),
                RecordStatusReason = ToDefinedValueListItemBag( person.RecordStatusReasonValueId ),
                DeceasedDate = person.DeceasedDate?.ToString( "yyyy-MM-dd" ),
                RecordSource = ToDefinedValueListItemBag( person.RecordSourceValueId ),
                Gender = person.Gender,
                BirthDate = person.BirthDate != null
                    ? new BirthdayPickerBag
                    {
                        Day = person.BirthDate.Value.Day,
                        Month = person.BirthDate.Value.Month,
                        Year = person.BirthDate.Value.Year
                    }
                    : null,
                Grade = gradeFormatted.IsNotNullOrWhiteSpace()
                    ? new ListItemBag
                    {
                        // The GradePicker (with useGuidAsValue off) keys its items by grade offset.
                        Value = person.GradeOffset.Value.ToString(),
                        Text = gradeFormatted
                    }
                    : null,
                GraduationYear = person.GraduationYear,
                MaritalStatus = ToDefinedValueListItemBag( person.MaritalStatusValueId ),
                AnniversaryDate = person.AnniversaryDate?.ToString( "yyyy-MM-dd" ),
                Race = ToDefinedValueListItemBag( person.RaceValueId ),
                Ethnicity = ToDefinedValueListItemBag( person.EthnicityValueId ),
                Email = person.Email,
                IsEmailActive = person.IsEmailActive,
                EmailPreference = person.EmailPreference,

                // Cast the model's Rock.Model.CommunicationType to the Rock.Enums.Communication.CommunicationType the bag exposes (same underlying values).
                CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) person.CommunicationPreference,
                InactiveReasonNote = person.InactiveReasonNote,
                IsLockedAsChild = person.IsLockedAsChild,
            };

            // Blank the nick name when it merely echoes the first name (matches WebForms behavior).
            bag.NickName = person.NickName.IsNotNullOrWhiteSpace() && !person.NickName.Equals( person.FirstName, System.StringComparison.OrdinalIgnoreCase )
                ? person.NickName
                : string.Empty;

            /*
                8/19/26 - CLAUDE

                The following still need to be mapped from the person as their inputs are built
                (see EditPerson.ascx.cs ShowDetails around lines 877-1049):
                  - Chat tri-state values (when chat is enabled and the person has a chat alias).
                  - GivingGroupGuid and GivingEnvelopeNumber (person attribute value).
                  - PhoneNumbers, PreviousLastNames, AlternateIds, SearchKeys.

                Reason: Person value mapping grows with the client sections.
            */

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Person entity, ValidPropertiesBox<EditPersonBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // Per-field edit permissions are re-checked on the server; client visibility is never trusted.
            var canAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            var canEditConnectionStatus = canAdministrate || BlockCache.IsAuthorized( SecurityActionKey.EditConnectionStatus, RequestContext.CurrentPerson );
            var canEditRecordStatus = canAdministrate || BlockCache.IsAuthorized( SecurityActionKey.EditRecordStatus, RequestContext.CurrentPerson );
            var canEditRecordSource = canAdministrate;

            box.IfValidProperty( nameof( box.Bag.Title ),
                () => entity.TitleValueId = GetDefinedValueId( box.Bag.Title ) );

            box.IfValidProperty( nameof( box.Bag.FirstName ),
                () => entity.FirstName = box.Bag.FirstName );

            box.IfValidProperty( nameof( box.Bag.NickName ),
                () => entity.NickName = box.Bag.NickName );

            box.IfValidProperty( nameof( box.Bag.MiddleName ),
                () => entity.MiddleName = box.Bag.MiddleName );

            box.IfValidProperty( nameof( box.Bag.LastName ),
                () => entity.LastName = box.Bag.LastName );

            box.IfValidProperty( nameof( box.Bag.Suffix ),
                () => entity.SuffixValueId = GetDefinedValueId( box.Bag.Suffix ) );

            box.IfValidProperty( nameof( box.Bag.Photo ),
                () =>
                {
                    var newPhotoGuid = box.Bag.Photo?.Value.AsGuidOrNull();
                    var newPhotoBinaryFile = newPhotoGuid.HasValue ? new BinaryFileService( RockContext ).Get( newPhotoGuid.Value ) : null;
                    entity.PhotoId = newPhotoBinaryFile?.Id;

                    // A newly uploaded photo starts out temporary; keep it now that it is in use.
                    if ( newPhotoBinaryFile != null )
                    {
                        newPhotoBinaryFile.IsTemporary = false;
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.ConnectionStatus ),
                () =>
                {
                    if ( canEditConnectionStatus )
                    {
                        entity.ConnectionStatusValueId = GetDefinedValueId( box.Bag.ConnectionStatus );
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.RecordSource ),
                () =>
                {
                    if ( canEditRecordSource )
                    {
                        entity.RecordSourceValueId = GetDefinedValueId( box.Bag.RecordSource );
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.RecordStatus ),
                () =>
                {
                    if ( canEditRecordStatus )
                    {
                        entity.RecordStatusValueId = GetDefinedValueId( box.Bag.RecordStatus );
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.RecordStatusReason ),
                () =>
                {
                    if ( canEditRecordStatus )
                    {
                        var recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )?.Id;

                        // The reason only applies while the record is inactive.
                        entity.RecordStatusReasonValueId = entity.RecordStatusValueId == recordStatusInactiveId
                            ? GetDefinedValueId( box.Bag.RecordStatusReason )
                            : null;
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.DeceasedDate ),
                () =>
                {
                    if ( canEditRecordStatus )
                    {
                        var reasonDeceasedId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() )?.Id;

                        // The deceased date only applies when the inactive reason is deceased.
                        // Save() rejects a deceased date that precedes the birth date.
                        entity.DeceasedDate = entity.RecordStatusReasonValueId == reasonDeceasedId
                            ? box.Bag.DeceasedDate.AsDateTime()
                            : null;
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.InactiveReasonNote ),
                () =>
                {
                    if ( canEditRecordStatus )
                    {
                        entity.InactiveReasonNote = box.Bag.InactiveReasonNote?.Trim();
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.Gender ),
                () => entity.Gender = box.Bag.Gender );

            box.IfValidProperty( nameof( box.Bag.BirthDate ),
                () =>
                {
                    var birthDate = box.Bag.BirthDate;

                    // A birth date needs at least a month and day; the year is optional.
                    if ( birthDate != null && birthDate.Month > 0 && birthDate.Day > 0 )
                    {
                        entity.BirthMonth = birthDate.Month;
                        entity.BirthDay = birthDate.Day;
                        entity.BirthYear = birthDate.Year > 0 ? birthDate.Year : ( int? ) null;
                    }
                    else
                    {
                        entity.SetBirthDate( null );
                    }
                } );

            // Only Graduation Year is persisted; the client keeps the Grade picker in sync with it,
            // and Person.GradeOffset is derived from GraduationYear (matches WebForms).
            box.IfValidProperty( nameof( box.Bag.GraduationYear ),
                () => entity.GraduationYear = box.Bag.GraduationYear );

            box.IfValidProperty( nameof( box.Bag.MaritalStatus ),
                () => entity.MaritalStatusValueId = GetDefinedValueId( box.Bag.MaritalStatus ) );

            box.IfValidProperty( nameof( box.Bag.AnniversaryDate ),
                () =>
                {
                    var maritalStatusMarriedId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() )?.Id;

                    // The anniversary date only applies when the person is married.
                    entity.AnniversaryDate = entity.MaritalStatusValueId == maritalStatusMarriedId
                        ? box.Bag.AnniversaryDate.AsDateTime()
                        : null;
                } );

            box.IfValidProperty( nameof( box.Bag.Race ),
                () => entity.RaceValueId = GetDefinedValueId( box.Bag.Race ) );

            box.IfValidProperty( nameof( box.Bag.Ethnicity ),
                () => entity.EthnicityValueId = GetDefinedValueId( box.Bag.Ethnicity ) );

            box.IfValidProperty( nameof( box.Bag.IsLockedAsChild ),
                () => entity.IsLockedAsChild = box.Bag.IsLockedAsChild );

            /*
                8/19/26 - CLAUDE

                Fields without an IfValidProperty here (communication/email preference and every
                unbuilt section) are intentionally never written, so the server cannot clobber
                values the client did not edit. Each gets its IfValidProperty as its client input
                is built, along with the WebForms per-field security re-checks, soft validations,
                phone SMS single-select, and family re-evaluation.

                Reason: Mutation grows field-by-field with the client sections.
            */

            return true;
        }

        /// <inheritdoc/>
        protected override Person GetInitialEntity()
        {
            // Prefer the block context, falling back to the PersonId page parameter.
            var contextPerson = RequestContext.GetContextEntity<Person>();

            if ( contextPerson != null )
            {
                return contextPerson;
            }

            return GetInitialEntity<Person, PersonService>( RockContext, PageParameterKey.PersonId );
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Person entity, out BlockActionResult error )
        {
            var entityService = new PersonService( RockContext );
            error = null;

            entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Person.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {Person.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <param name="person">The person being edited, or null.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( Person person )
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = person != null ? RequestContext.ResolveRockUrl( $"~/Person/{person.IdKey}" ) : string.Empty
            };
        }

        /// <summary>
        /// Converts a defined value id to a <see cref="ListItemBag"/>, or null when no value is set.
        /// </summary>
        /// <param name="definedValueId">The defined value id.</param>
        /// <returns>The list item, or null.</returns>
        private static ListItemBag ToDefinedValueListItemBag( int? definedValueId )
        {
            return definedValueId.HasValue
                ? DefinedValueCache.Get( definedValueId.Value )?.ToListItemBag()
                : null;
        }

        /// <summary>
        /// Resolves a defined-value <see cref="ListItemBag"/> (whose value is a guid) back to its id.
        /// </summary>
        /// <param name="listItem">The list item selected on the client.</param>
        /// <returns>The defined value id, or null.</returns>
        private static int? GetDefinedValueId( ListItemBag listItem )
        {
            var guid = listItem?.Value.AsGuidOrNull();
            if ( !guid.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( guid.Value )?.Id;
        }

        /// <summary>
        /// Determines whether the person is the only active member of their primary family, so
        /// marking them inactive would leave no active members.
        /// </summary>
        /// <param name="person">The person being edited.</param>
        /// <returns><c>true</c> when the person is the only active member of their primary family.</returns>
        private bool IsOnlyActiveFamilyMember( Person person )
        {
            var primaryFamily = person.PrimaryFamily;
            if ( primaryFamily == null )
            {
                return false;
            }

            var recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )?.Id;

            return !primaryFamily.Members.Any( m => m.PersonId != person.Id && m.Person.RecordStatusValueId != recordStatusInactiveId );
        }

        /// <summary>
        /// Re-evaluates the active state of the person's families when their record status changed
        /// to or from inactive. Mirrors the WebForms btnSave_Click family activation logic.
        /// </summary>
        /// <param name="person">The saved person.</param>
        /// <param name="originalRecordStatusValueId">The record status value id before the save.</param>
        private void ReevaluateFamilyActiveState( Person person, int? originalRecordStatusValueId )
        {
            var recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )?.Id;

            var changedToOrFromInactive = originalRecordStatusValueId != person.RecordStatusValueId
                && ( originalRecordStatusValueId == recordStatusInactiveId || person.RecordStatusValueId == recordStatusInactiveId );

            if ( !changedToOrFromInactive )
            {
                return;
            }

            // A family stays active as long as it has at least one non-inactive member.
            foreach ( var family in new PersonService( RockContext ).GetFamilies( person.Id ) )
            {
                family.IsActive = family.Members.Any( m => m.Person.RecordStatusValueId != recordStatusInactiveId );
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Sets the account protection profile warning on the options when the current user may view it
        /// and the person's profile is above Low. Mirrors the WebForms ShowDetails() banner logic.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="person">The person being edited.</param>
        private void SetAccountProtectionProfileMessage( EditPersonOptionsBag options, Person person )
        {
            if ( !BlockCache.IsAuthorized( SecurityActionKey.ViewProtectionProfile, RequestContext.CurrentPerson ) )
            {
                return;
            }

            if ( person.AccountProtectionProfile <= Rock.Utility.Enums.AccountProtectionProfile.Low )
            {
                return;
            }

            const string messageSuffix = "Ensure you trust the source of the request to update their email address and/or mobile phone number as this could be used to grant access to their account.";

            switch ( person.AccountProtectionProfile )
            {
                case Rock.Utility.Enums.AccountProtectionProfile.Medium:
                    options.AccountProtectionProfileMessage = $"Use care when editing this record as the individual has a login. {messageSuffix}";
                    options.AccountProtectionProfileAlertType = "Warning";
                    break;

                case Rock.Utility.Enums.AccountProtectionProfile.High:
                    options.AccountProtectionProfileMessage = $"Use care when editing this record as the individual has financial account information stored in Rock or is a member of a sensitive security role. {messageSuffix}";
                    options.AccountProtectionProfileAlertType = "Danger";
                    break;

                case Rock.Utility.Enums.AccountProtectionProfile.Extreme:
                    options.AccountProtectionProfileMessage = $"Use care when editing this record as the individual is in a sensitive security role. {messageSuffix}";
                    options.AccountProtectionProfileAlertType = "Danger";
                    break;
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the edited person and returns the person profile URL to redirect to.
        /// </summary>
        /// <param name="box">The box that contains the edited values.</param>
        /// <returns>The URL to redirect to on success.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<EditPersonBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var originalPhotoId = entity.PhotoId;
            var originalRecordStatusValueId = entity.RecordStatusValueId;

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // A deceased date may not precede the birth date (matches WebForms). Only checked when a
            // complete birth date (with year) is present.
            if ( entity.DeceasedDate.HasValue && entity.BirthYear.HasValue && entity.BirthDate.HasValue
                && entity.DeceasedDate.Value < entity.BirthDate.Value )
            {
                return ActionBadRequest( "Deceased Date must be on or after the birth date." );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                // Flag the previous photo as temporary so it gets cleaned up later.
                if ( originalPhotoId.HasValue && originalPhotoId != entity.PhotoId )
                {
                    var orphanedBinaryFile = new BinaryFileService( RockContext ).Get( originalPhotoId.Value );
                    if ( orphanedBinaryFile != null )
                    {
                        orphanedBinaryFile.IsTemporary = true;
                        RockContext.SaveChanges();
                    }
                }

                ReevaluateFamilyActiveState( entity, originalRecordStatusValueId );
            } );

            return ActionOk( RequestContext.ResolveRockUrl( $"~/Person/{entity.IdKey}" ) );
        }

        #endregion Block Actions
    }
}
