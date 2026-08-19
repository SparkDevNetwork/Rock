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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson;

/*******************************************************************************************************************************
 * NOTE: The Security/AccountEdit block has very similar functionality.  If updating this block, make sure to check
 * that block also.  It may need the same updates.
 *******************************************************************************************************************************/

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Allows you to edit a person's full profile.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

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
        Description = "Whether the user is required to enter a year once a birth month and day are present.",
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
    public class EditPerson : RockBlockType
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
            var box = new EditPersonBox();

            var person = GetPerson();

            // Guard: no person to edit. Render the not-found state rather than an empty form.
            if ( person == null )
            {
                box.IsPersonFound = false;
                return box;
            }

            box.IsPersonFound = true;
            box.PersonIdKey = person.IdKey;
            box.IsEditAllowed = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.CancelUrl = RequestContext.ResolveRockUrl( $"~/Person/{person.IdKey}" );
            box.Options = GetOptions( person );
            box.Person = GetPersonBag( person );

            SetAccountProtectionProfileMessage( box, person );

            return box;
        }

        /// <summary>
        /// Gets the person to be edited, preferring the block context and falling back to the
        /// person identified by the page parameter.
        /// </summary>
        /// <returns>The <see cref="Person"/> to edit, or <c>null</c> if one could not be determined.</returns>
        private Person GetPerson()
        {
            var contextPerson = RequestContext.GetContextEntity<Person>();

            if ( contextPerson != null )
            {
                return contextPerson;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Builds the configuration, feature flags, and option sources for the form, applying the
        /// block settings and the current user's per-field permissions.
        /// </summary>
        /// <param name="person">The person being edited.</param>
        /// <returns>The populated <see cref="EditPersonOptionsBag"/>.</returns>
        private EditPersonOptionsBag GetOptions( Person person )
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

            /*
                8/19/26 - CLAUDE

                Boilerplate scaffold. The remaining option sources and feature flags still need wiring:
                  - IsChatVisible: ChatHelper.IsChatEnabled && person.HasChatAlias.
                  - IsEnvelopeNumberVisible: GlobalAttributesCache.Get().EnableGivingEnvelopeNumber && the giving envelope attribute exists.
                  - GivingGroups: PersonService.GetFamilies(person.Id) formatted like the WebForms GetFamilyNameWithFirstNames helper.
                  - SearchKeyTypes: the add-search-key dropdown options (GetValidSearchKeyTypes minus the Alternate-Id type).

                Reason: Options wiring pending.
            */

            return options;
        }

        /// <summary>
        /// Maps the person's current values onto the editable bag used to pre-fill the form.
        /// Mirrors the WebForms ShowDetails() flow.
        /// </summary>
        /// <param name="person">The person being edited.</param>
        /// <returns>The populated <see cref="EditPersonBag"/>.</returns>
        private EditPersonBag GetPersonBag( Person person )
        {
            var bag = new EditPersonBag
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                Gender = person.Gender,
                Email = person.Email,
                IsEmailActive = person.IsEmailActive,
                EmailPreference = person.EmailPreference,

                // Cast the model's Rock.Model.CommunicationType to the Rock.Enums.Communication.CommunicationType the bag exposes (same underlying values).
                CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) person.CommunicationPreference,
                GraduationYear = person.GraduationYear,
                InactiveReasonNote = person.InactiveReasonNote,
                IsLockedAsChild = person.IsLockedAsChild,
            };

            // Blank the nick name when it merely echoes the first name (matches WebForms behavior).
            bag.NickName = person.NickName.IsNotNullOrWhiteSpace() && !person.NickName.Equals( person.FirstName, System.StringComparison.OrdinalIgnoreCase )
                ? person.NickName
                : string.Empty;

            /*
                8/19/26 - CLAUDE

                Boilerplate scaffold. The following still need to be mapped from the person (see
                EditPerson.ascx.cs ShowDetails around lines 877-1049):
                  - Defined-value ListItemBags: Title, Suffix, MaritalStatus, ConnectionStatus,
                    RecordStatus, RecordStatusReason, RecordSource, Race, Ethnicity, Grade.
                  - Photo (ListItemBag), BirthDate (BirthdayPickerBag), AnniversaryDate, DeceasedDate.
                  - Chat tri-state values (only when chat is enabled and the person has a chat alias).
                  - GivingGroupGuid and GivingEnvelopeNumber (person attribute value).
                  - PhoneNumbers: one row per active phone number type, with the "default mobile SMS"
                    behavior applied to a blank mobile row.
                  - PreviousLastNames (person.GetPreviousNames()).
                  - AlternateIds and SearchKeys (person.GetPersonSearchKeys() split by the Alternate-Id type).

                Reason: Person value mapping pending.
            */

            return bag;
        }

        /// <summary>
        /// Sets the account protection profile warning on the box when the current user may view it
        /// and the person's profile is above Low. Mirrors the WebForms ShowDetails() banner logic.
        /// </summary>
        /// <param name="box">The initialization box to populate.</param>
        /// <param name="person">The person being edited.</param>
        private void SetAccountProtectionProfileMessage( EditPersonBox box, Person person )
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
                    box.AccountProtectionProfileMessage = $"Use care when editing this record as the individual has a login. {messageSuffix}";
                    box.AccountProtectionProfileAlertType = "Warning";
                    break;

                case Rock.Utility.Enums.AccountProtectionProfile.High:
                    box.AccountProtectionProfileMessage = $"Use care when editing this record as the individual has financial account information stored in Rock or is a member of a sensitive security role. {messageSuffix}";
                    box.AccountProtectionProfileAlertType = "Danger";
                    break;

                case Rock.Utility.Enums.AccountProtectionProfile.Extreme:
                    box.AccountProtectionProfileMessage = $"Use care when editing this record as the individual is in a sensitive security role. {messageSuffix}";
                    box.AccountProtectionProfileAlertType = "Danger";
                    break;
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the edited person. Ports the WebForms btnSave_Click transaction.
        /// </summary>
        /// <param name="bag">The save request.</param>
        /// <returns>The save result, including any validation warnings or a redirect URL on success.</returns>
        [BlockAction( "Save" )]
        public BlockActionResult Save( EditPersonSaveRequestBag bag )
        {
            // Re-authorize on the server; never trust the client's IsEditAllowed flag.
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this person." );
            }

            var personService = new PersonService( RockContext );
            var person = personService.Get( bag?.PersonIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( person == null )
            {
                return ActionNotFound( "The person to edit could not be found." );
            }

            /*
                8/19/26 - CLAUDE

                Boilerplate scaffold. The mutation logic still needs to be ported from
                EditPerson.ascx.cs btnSave_Click (lines 440-832), inside a RockContext transaction,
                re-checking each per-field security action before applying that group:
                  - Identity/demographics, status, contact, chat, giving, advanced field mapping.
                  - SMS single-select enforcement and RemoveEmptyAndDuplicatePhoneNumbers.
                  - Communication-preference-SMS-requires-number warning (soft failure).
                  - Deceased-date-before-birthday validation (soft failure). Also HIDE/CLEAR the
                    deceased date on the client when Record Status returns to Active (redesign bug fix).
                  - Alternate-identifier uniqueness validation (soft failure, inline).
                  - Envelope-number reassignment confirmation flow (EnvelopeNumberConfirmationMessage).
                  - Previous names / search keys diff against the database.
                  - Orphaned/cropped photo cleanup and family activate/inactivate re-evaluation.
                  - On success, return RedirectUrl = ~/Person/{person.Id}.

                Reason: Save mutation logic pending.
            */

            var response = new EditPersonSaveResponseBag
            {
                IsSuccess = false
            };

            return ActionOk( response );
        }

        #endregion Block Actions
    }
}
