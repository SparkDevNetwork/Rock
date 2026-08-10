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
using System.Text.RegularExpressions;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.CheckIn.Manager.PersonLeft;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Displays a checked-in person's profile card inside Check-in Manager,
    /// including contact information, family, related people, and a
    /// streamlined SMS send experience for volunteers.
    /// </summary>

    [DisplayName( "Person Profile" )]
    [Category( "Check-in > Manager" )]
    [Description( "Displays person details for a checked-in person." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Show Related People",
        Key = AttributeKey.ShowRelatedPeople,
        Description = "Should anyone who is allowed to check-in the current person also be displayed with the family members?",
        IsRequired = false,
        Order = 1 )]

    [SystemPhoneNumberField(
        "Send SMS From",
        Key = AttributeKey.SmsFrom,
        Description = "The phone number SMS messages should be sent from.",
        IsRequired = false,
        AllowMultiple = false,
        Order = 2 )]

    [AttributeCategoryField(
        "Child Attribute Category",
        Key = AttributeKey.ChildAttributeCategory,
        Description = "The children Attribute Category to display attributes from.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 3 )]

    [AttributeCategoryField(
        "Adult Attribute Category",
        Key = AttributeKey.AdultAttributeCategory,
        Description = "The adult Attribute Category to display attributes from.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 4 )]

    [BooleanField(
        "Show Share Person Button",
        Key = AttributeKey.ShowSharePersonButton,
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 5 )]

    [LinkedPage(
        "Share Person Page",
        Key = AttributeKey.SharePersonPage,
        Description = "The page whose URL is offered to the browser's share sheet when the share button is used.",
        DefaultValue = Rock.SystemGuid.Page.EDIT_PERSON + "," + Rock.SystemGuid.PageRoute.EDIT_PERSON_ROUTE,
        IsRequired = false,
        Order = 6 )]

    [LinkedPage(
        "Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "The page to go to when a family member of the attendee is clicked.",
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER,
        IsRequired = false,
        Order = 7 )]

    [CategoryField(
        "Snippet Category",
        Key = AttributeKey.SnippetCategory,
        Description = "The category to show SMS Snippets for (leave blank for all categories).",
        EntityType = typeof( Snippet ),
        IsRequired = false,
        Order = 8 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "D5B00F4A-DBF6-4532-BA01-210FE0F62D5C" )]
    [Rock.SystemGuid.BlockTypeGuid( "D54909DB-8A5D-4665-97ED-E2C8577E3C64" )]
    public class PersonLeft : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowRelatedPeople = "ShowRelatedPeople";
            public const string SmsFrom = "SMSFrom";
            public const string ChildAttributeCategory = "ChildAttributeCategory";
            public const string AdultAttributeCategory = "AdultAttributeCategory";
            public const string ShowSharePersonButton = "ShowSharePersonButton";
            public const string SharePersonPage = "SharePersonPage";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string SnippetCategory = "SnippetCategory";
        }

        private static class PageParameterKey
        {
            /// <summary>
            /// A page-parameter that may arrive as an integer Id, an IdKey, or
            /// a Guid. Resolves via the IdKey-aware Get overload.
            /// </summary>
            public const string PersonId = "PersonId";

            /// <summary>
            /// A legacy Guid-only page-parameter carried forward from the
            /// WebForms block so existing links keep working.
            /// </summary>
            public const string Person = "Person";

            /// <summary>
            /// When only an AttendanceId is supplied, the block resolves the
            /// underlying person and redirects to add PersonId to the URL so
            /// sibling blocks on the page can see it.
            /// </summary>
            public const string AttendanceId = "AttendanceId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Compiled regex used to mask the digits of unlisted phone numbers
        /// with asterisks while preserving formatting characters (parens,
        /// spaces, dashes).
        /// </summary>
        private static readonly Regex DigitMaskRegex = new Regex( @"\d", RegexOptions.Compiled );

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            if ( !BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return new PersonLeftInitializationBox { IsVisible = false };
            }

            if ( TryRedirectFromAttendance() )
            {
                return new PersonLeftInitializationBox { IsVisible = false };
            }

            var person = ResolvePerson();

            if ( person == null )
            {
                return new PersonLeftInitializationBox { IsVisible = false };
            }

            person.LoadAttributes( RockContext );

            var box = new PersonLeftInitializationBox
            {
                IsVisible = true,
                PersonIdKey = person.IdKey,
                FullName = person.FullName,
                PhotoImageTag = Person.GetPersonPhotoImageTag( person, 200, 200 ),
                HasPhoto = person.PhotoId.HasValue,
                PhotoUrl = person.PhotoId.HasValue ? person.PhotoUrl : null,
                CampusName = person.GetCampus()?.Name,
                SharePersonUrl = BuildSharePersonUrl( person ),
                EmailTagHtml = BuildEmailTagHtml( person ),
                PhoneNumbers = BuildPhoneNumberBags( person, out var isSmsAvailable ),
                AdultAttributes = BuildAttributeBags( person, isAdultSection: true ),
                ChildAttributes = BuildAttributeBags( person, isAdultSection: false ),
                FamilyMembers = BuildFamilyBags( person ),
                RelatedPeople = BuildRelatedPeopleBags( person ),
                IsSmsAvailable = isSmsAvailable
            };

            return box;
        }

        /// <summary>
        /// Resolves the target person from the page parameters. Accepts the
        /// modern IdKey/Id form via <c>PersonId</c> as well as the legacy
        /// Guid-only <c>Person</c> parameter that older Check-in Manager
        /// links still use.
        /// </summary>
        /// <returns>The resolved <see cref="Person"/>, or <c>null</c> when
        /// no matching person was found.</returns>
        private Person ResolvePerson()
        {
            var personService = new PersonService( RockContext );
            int? personId = null;

            var personKey = PageParameter( PageParameterKey.PersonId );
            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                personId = personService.Get( personKey, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
            }

            if ( !personId.HasValue )
            {
                var personGuid = PageParameter( PageParameterKey.Person ).AsGuidOrNull();
                if ( personGuid.HasValue )
                {
                    personId = personService.GetId( personGuid.Value );
                }
            }

            if ( !personId.HasValue )
            {
                return null;
            }

            return personService.Queryable( true, true )
                .Include( a => a.PhoneNumbers )
                .Include( a => a.RecordStatusValue )
                .FirstOrDefault( a => a.Id == personId.Value );
        }

        /// <summary>
        /// When the URL supplies an <c>AttendanceId</c> without a
        /// <c>PersonId</c>, resolves the corresponding person and issues a
        /// redirect that adds <c>PersonId</c> to the URL so any other blocks
        /// on the page can see it.
        /// </summary>
        /// <returns><c>true</c> when a redirect was initiated (and the caller
        /// should short-circuit the rest of the render).</returns>
        private bool TryRedirectFromAttendance()
        {
            if ( PageParameter( PageParameterKey.PersonId ).IsNotNullOrWhiteSpace()
                || PageParameter( PageParameterKey.Person ).IsNotNullOrWhiteSpace() )
            {
                return false;
            }

            var attendanceId = PageParameter( PageParameterKey.AttendanceId ).AsIntegerOrNull();
            if ( !attendanceId.HasValue )
            {
                return false;
            }

            // Fetch the raw PersonId in SQL (translatable), then compute the
            // IdKey in memory. Selecting Person.IdKey directly does not work:
            // IdKey is a [NotMapped] property implemented in C# via IdHasher,
            // so EF cannot translate it and silently returns the underlying
            // scalar Id column, producing ?PersonId=7 instead of a hashed key.
            var personId = new AttendanceService( RockContext )
                .GetSelect( attendanceId.Value, a => ( int? ) a.PersonAlias.PersonId );

            if ( !personId.HasValue )
            {
                return false;
            }

            var personIdKey = Rock.Utility.IdHasher.Instance.GetHash( personId.Value );
            if ( personIdKey.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var currentUrl = this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.PersonId] = personIdKey
            } );

            RequestContext.Response.RedirectToUrl( currentUrl );
            return true;
        }

        /// <summary>
        /// Builds the pre-rendered email tag HTML for the person, or
        /// <c>null</c> when the person has no email address.
        /// </summary>
        private string BuildEmailTagHtml( Person person )
        {
            if ( person.Email.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var rootUrl = RequestContext.ResolveRockUrl( "/" );
            return person.GetEmailTag( rootUrl, "text-color" );
        }

        /// <summary>
        /// Builds the absolute URL that the browser's Web Share API should
        /// offer when the share button is used. Returns <c>null</c> when the
        /// share button is disabled or no share page is configured.
        /// </summary>
        private string BuildSharePersonUrl( Person person )
        {
            if ( !GetAttributeValue( AttributeKey.ShowSharePersonButton ).AsBoolean() )
            {
                return null;
            }

            // The share URL intentionally passes PersonId=<Guid> to match the
            // parameter the EDIT_PERSON page and route already understand.
            var relativeUrl = this.GetLinkedPageUrl( AttributeKey.SharePersonPage, new Dictionary<string, string>
            {
                [PageParameterKey.PersonId] = person.Guid.ToString()
            } );

            if ( relativeUrl.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return ResolveRockUrlIncludeRoot( relativeUrl );
        }

        /// <summary>
        /// Resolves the supplied URL relative to the site theme and prepends
        /// the request's scheme and host so the result is an absolute URL
        /// suitable for the Web Share API.
        /// </summary>
        private string ResolveRockUrlIncludeRoot( string url )
        {
            var virtualPath = RequestContext.ResolveRockUrl( url );

            if ( !virtualPath.StartsWith( "/" ) )
            {
                return virtualPath;
            }

            if ( RequestContext.RootUrlPath.IsNotNullOrWhiteSpace() )
            {
                return $"{RequestContext.RootUrlPath}{virtualPath}";
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
        }

        /// <summary>
        /// Projects the person's phone numbers to the display bag used by
        /// the frontend. Unlisted numbers are masked before leaving the
        /// server so raw digits never reach the DOM.
        /// </summary>
        /// <param name="person">The person whose phones should be rendered.</param>
        /// <param name="isSmsAvailable">Set to <c>true</c> when the block is
        /// SMS-configured and the person has an eligible phone number.</param>
        private List<PersonLeftPhoneNumberBag> BuildPhoneNumberBags( Person person, out bool isSmsAvailable )
        {
            isSmsAvailable = false;

            var smsCapablePhone = GetSmsCapableMobilePhoneNumber( person );
            var isSmsConfigured = GetAttributeValue( AttributeKey.SmsFrom ).AsGuidOrNull().HasValue;
            var smsCapablePhoneId = isSmsConfigured && smsCapablePhone != null
                ? ( int? ) smsCapablePhone.Id
                : null;

            isSmsAvailable = smsCapablePhoneId.HasValue;

            return person.PhoneNumbers
                .Select( p => new PersonLeftPhoneNumberBag
                {
                    NumberFormatted = p.IsUnlisted ? MaskDigits( p.NumberFormatted ) : p.NumberFormatted,
                    RawNumber = p.IsUnlisted ? MaskDigits( p.Number ) : p.Number,
                    NumberType = p.NumberTypeValue?.Value,
                    IsUnlisted = p.IsUnlisted,
                    CanSendSms = smsCapablePhoneId.HasValue && p.Id == smsCapablePhoneId.Value
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the adult or child attribute list for the profile card,
        /// filtered to the configured category and to attributes the current
        /// user is authorized to view.
        /// </summary>
        /// <param name="person">The person whose attributes are being displayed.</param>
        /// <param name="isAdultSection"><c>true</c> to build the adult
        /// section (uses <see cref="AttributeKey.AdultAttributeCategory"/>),
        /// <c>false</c> to build the child section.</param>
        private List<PersonLeftAttributeBag> BuildAttributeBags( Person person, bool isAdultSection )
        {
            var appliesToAdults = person.AgeClassification == AgeClassification.Adult || person.AgeClassification == AgeClassification.Unknown;
            var appliesToChildren = person.AgeClassification == AgeClassification.Child || person.AgeClassification == AgeClassification.Unknown;

            if ( isAdultSection && !appliesToAdults )
            {
                return new List<PersonLeftAttributeBag>();
            }

            if ( !isAdultSection && !appliesToChildren )
            {
                return new List<PersonLeftAttributeBag>();
            }

            var categoryGuid = GetAttributeValue( isAdultSection ? AttributeKey.AdultAttributeCategory : AttributeKey.ChildAttributeCategory ).AsGuidOrNull();
            if ( !categoryGuid.HasValue )
            {
                return new List<PersonLeftAttributeBag>();
            }

            var category = CategoryCache.Get( categoryGuid.Value );
            if ( category == null )
            {
                return new List<PersonLeftAttributeBag>();
            }

            var currentPerson = RequestContext.CurrentPerson;

            var attributes = person.Attributes.Values
                .Where( a => a.CategoryIds.Contains( category.Id ) )
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            var bags = new List<PersonLeftAttributeBag>();

            foreach ( var attribute in attributes )
            {
                var rawValue = person.GetAttributeValue( attribute.Key );
                if ( rawValue.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                // Use the field type's HTML-formatting method rather than
                // PublicAttributeHelper.GetPublicValueForView. The public-view
                // pipeline returns the raw private value (e.g. "True") on the
                // assumption a Vue field component will render it; we're
                // rendering plain HTML, so we need the WebForms-style
                // "Yes"/"No" (and equivalent) output.
                var formattedValue = attribute.FieldType.Field.FormatValueAsHtml( null, attribute.EntityTypeId, person.Id, rawValue, attribute.QualifierValues );
                if ( formattedValue.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                bags.Add( new PersonLeftAttributeBag
                {
                    Name = attribute.Name,
                    Key = attribute.Key,
                    FormattedValueHtml = formattedValue
                } );
            }

            return bags;
        }

        /// <summary>
        /// Builds the family-member tiles shown under the profile card. Other
        /// family members are ordered by their family group and then by
        /// their date of birth to match the WebForms layout.
        /// </summary>
        private List<PersonLeftRelatedPersonBag> BuildFamilyBags( Person person )
        {
            var personService = new PersonService( RockContext );

            var familyMembers = personService.GetFamilyMembers( person.Id, includeSelf: true ).ToList();

            var otherFamilyMembers = familyMembers
                .Where( m => m.PersonId != person.Id )
                .OrderBy( m => m.GroupId )
                .ThenBy( m => m.Person.BirthDate )
                .ToList();

            return otherFamilyMembers
                .Select( m => new PersonLeftRelatedPersonBag
                {
                    NickName = m.Person.NickName,
                    PhotoImageTag = Person.GetPersonPhotoImageTag( m.Person, 64, 64, className: "d-block mb-spacing-tiny" ),
                    Url = BuildProfileUrlForRelatedPerson( m.Person.Guid ),
                    RelationshipName = null
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the related-people tiles shown under the family panel. The
        /// tiles show people who are the inverse of a known-relationship
        /// role that has the CanCheckin attribute set, matching the WebForms
        /// behavior.
        /// </summary>
        private List<PersonLeftRelatedPersonBag> BuildRelatedPeopleBags( Person person )
        {
            if ( !GetAttributeValue( AttributeKey.ShowRelatedPeople ).AsBoolean() )
            {
                return new List<PersonLeftRelatedPersonBag>();
            }

            var knownRelationshipsGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            if ( !knownRelationshipsGroupTypeId.HasValue )
            {
                return new List<PersonLeftRelatedPersonBag>();
            }

            var knownRelationshipRoles = new GroupTypeRoleService( RockContext )
                .Queryable().AsNoTracking()
                .Where( r => r.GroupTypeId == knownRelationshipsGroupTypeId.Value )
                .ToList();

            var checkInInverseRoleIds = new List<int>();
            foreach ( var role in knownRelationshipRoles )
            {
                // Known-relationship roles are a small set (typically fewer
                // than 20). Loading each role's attributes here is preferable
                // to a bulk lift for readability.
                role.LoadAttributes( RockContext );

                if ( !role.GetAttributeValue( "CanCheckin" ).AsBoolean() )
                {
                    continue;
                }

                if ( !role.Attributes.ContainsKey( "InverseRelationship" ) )
                {
                    continue;
                }

                var inverseRoleGuid = role.GetAttributeValue( "InverseRelationship" ).AsGuidOrNull();
                if ( !inverseRoleGuid.HasValue )
                {
                    continue;
                }

                var inverseRole = knownRelationshipRoles.FirstOrDefault( r => r.Guid == inverseRoleGuid.Value );
                if ( inverseRole != null )
                {
                    checkInInverseRoleIds.Add( inverseRole.Id );
                }
            }

            if ( !checkInInverseRoleIds.Any() )
            {
                return new List<PersonLeftRelatedPersonBag>();
            }

            var personService = new PersonService( RockContext );

            var relatedMembers = personService
                .GetRelatedPeople( new List<int> { person.Id }, checkInInverseRoleIds )
                .OrderBy( m => m.Person.LastName )
                .ThenBy( m => m.Person.NickName )
                .ToList();

            return relatedMembers
                .Select( m => new PersonLeftRelatedPersonBag
                {
                    NickName = m.Person.NickName,
                    PhotoImageTag = Person.GetPersonPhotoImageTag( m.Person, 50, 50, className: "rounded" ),
                    Url = BuildProfileUrlForRelatedPerson( m.Person.Guid ),
                    RelationshipName = m.GroupRole.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the Person Profile page URL used by both the family and
        /// related-people tiles. Uses the legacy <c>Person</c> Guid query
        /// parameter so downstream blocks on the profile page continue to
        /// resolve the person.
        /// </summary>
        private string BuildProfileUrlForRelatedPerson( Guid relatedPersonGuid )
        {
            return this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, new Dictionary<string, string>
            {
                [PageParameterKey.Person] = relatedPersonGuid.ToString()
            } );
        }

        /// <summary>
        /// Returns the phone number Check-in Manager should target when
        /// sending an SMS. Prefers a phone that is already SMS-enabled and
        /// falls back to any Mobile-typed number with a non-empty value -
        /// even if it is currently opted out - so the SMS icon surfaces for
        /// people whose mobile hasn't been flagged as SMS-enabled.
        /// <see cref="EnsureRecipientCanReceiveSms"/> flips the necessary
        /// flags at send time.
        /// </summary>
        private static PhoneNumber GetSmsCapableMobilePhoneNumber( Person person )
        {
            if ( person == null )
            {
                return null;
            }

            var smsEnabledPhone = person.PhoneNumbers.FirstOrDefault( n => n.IsMessagingEnabled && n.Number.IsNotNullOrWhiteSpace() );
            if ( smsEnabledPhone != null )
            {
                return smsEnabledPhone;
            }

            var mobilePhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( !mobilePhoneTypeId.HasValue )
            {
                return null;
            }

            return person.PhoneNumbers.FirstOrDefault( n =>
                n.NumberTypeValueId == mobilePhoneTypeId.Value
                && n.Number.IsNotNullOrWhiteSpace() );
        }

        /// <summary>
        /// Replaces every digit in <paramref name="value"/> with an asterisk,
        /// preserving formatting characters such as parentheses, spaces, and
        /// dashes. Used to render unlisted phone numbers so staff can see
        /// the number's type and layout without seeing the actual digits.
        /// </summary>
        private static string MaskDigits( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return value;
            }

            return DigitMaskRegex.Replace( value, "*" );
        }

        #endregion Methods

        #region SMS Send Helpers

        /// <summary>
        /// Adjusts the supplied phone number and person so a Check-in Manager
        /// SMS will actually be sent: enables SMS messaging on the phone if
        /// disabled, clears an SMS opt-out if present, and activates the
        /// person if their record status is currently something other than
        /// Active (unless the person is marked Deceased). Saves any changes
        /// and returns a summary describing which adjustments were made so
        /// the caller can log an explanatory history entry.
        /// </summary>
        private static SmsRecipientPrepResult EnsureRecipientCanReceiveSms( RockContext rockContext, Person person, PhoneNumber phoneNumber )
        {
            var result = new SmsRecipientPrepResult();

            if ( phoneNumber != null && !phoneNumber.IsMessagingEnabled )
            {
                phoneNumber.IsMessagingEnabled = true;
                result.EnabledMessaging = true;
            }

            if ( phoneNumber != null && phoneNumber.IsMessagingOptedOut )
            {
                phoneNumber.IsMessagingOptedOut = false;
                phoneNumber.MessagingOptedOutDateTime = null;
                result.ClearedOptOut = true;
            }

            var activeRecordStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var deceasedRecordStatusReasonValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );
            var isDeceased = deceasedRecordStatusReasonValueId.HasValue
                && person != null
                && person.RecordStatusReasonValueId == deceasedRecordStatusReasonValueId.Value;

            if ( activeRecordStatusValueId.HasValue
                && person != null
                && person.RecordStatusValueId != activeRecordStatusValueId.Value
                && !isDeceased )
            {
                person.RecordStatusValueId = activeRecordStatusValueId.Value;
                person.RecordStatusReasonValueId = null;
                result.ReactivatedPerson = true;
            }

            if ( result.HasAnyChange )
            {
                rockContext.SaveChanges();
            }

            return result;
        }

        /// <summary>
        /// Writes a custom person-history entry describing the fields
        /// Check-in Manager auto-adjusted so the SMS could be sent. Rock's
        /// SaveHooks already log the raw property changes for RecordStatus
        /// and IsMessagingEnabled, so this entry supplies the "why" and
        /// covers IsMessagingOptedOut (which the SaveHook does not track).
        /// Does nothing when no adjustments were made.
        /// </summary>
        private static void WriteHistoryLogForSmsAutoAdjustments( RockContext rockContext, Person person, PhoneNumber phoneNumber, SmsRecipientPrepResult prepResult )
        {
            if ( person == null || prepResult == null || !prepResult.HasAnyChange )
            {
                return;
            }

            var numberLabel = phoneNumber != null
                ? $"{phoneNumber.NumberTypeValue?.Value} phone ({phoneNumber.NumberFormatted})".Trim()
                : "phone number";

            var adjustments = new List<string>();
            if ( prepResult.EnabledMessaging )
            {
                adjustments.Add( $"Enabled SMS on {numberLabel}" );
            }

            if ( prepResult.ClearedOptOut )
            {
                adjustments.Add( $"Cleared SMS opt-out on {numberLabel}" );
            }

            if ( prepResult.ReactivatedPerson )
            {
                adjustments.Add( "Set Record Status to Active" );
            }

            var caption = adjustments.AsDelimited( "; " );

            var historyChanges = new History.HistoryChangeList();
            var historyChange = historyChanges.AddCustom( History.HistoryVerb.Sent.ConvertToString().ToUpper(), History.HistoryChangeType.Record.ToString(), "SMS from Check-in Manager" );
            historyChange.Caption = caption;

            HistoryService.SaveChanges(
                rockContext,
                typeof( Person ),
                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                person.Id,
                historyChanges );
        }

        /// <summary>
        /// Summary of the recipient adjustments made by
        /// <see cref="EnsureRecipientCanReceiveSms"/> so the caller can
        /// compose a matching person-history entry.
        /// </summary>
        private sealed class SmsRecipientPrepResult
        {
            /// <summary>
            /// <c>true</c> when <see cref="PhoneNumber.IsMessagingEnabled"/>
            /// was flipped from <c>false</c> to <c>true</c>.
            /// </summary>
            public bool EnabledMessaging { get; set; }

            /// <summary>
            /// <c>true</c> when <see cref="PhoneNumber.IsMessagingOptedOut"/>
            /// was flipped from <c>true</c> to <c>false</c> (and
            /// <see cref="PhoneNumber.MessagingOptedOutDateTime"/> was
            /// cleared).
            /// </summary>
            public bool ClearedOptOut { get; set; }

            /// <summary>
            /// <c>true</c> when the person's
            /// <see cref="Person.RecordStatusValueId"/> was changed to
            /// Active.
            /// </summary>
            public bool ReactivatedPerson { get; set; }

            /// <summary>
            /// <c>true</c> when at least one adjustment was made.
            /// </summary>
            public bool HasAnyChange => EnabledMessaging || ClearedOptOut || ReactivatedPerson;
        }

        #endregion SMS Send Helpers

        #region Block Actions

        /// <summary>
        /// Returns the SMS snippets the current user can insert into the
        /// message, filtered by the optional Snippet Category block setting
        /// and either the current user's personal snippets or the shared
        /// snippets they are authorized to view.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetSnippets( PersonLeftSnippetListRequestBag bag )
        {
            var smsSnippetTypeGuid = Rock.SystemGuid.SnippetType.SMS.AsGuid();
            var smsSnippetTypeId = new SnippetTypeService( RockContext )
                .Queryable()
                .Where( st => st.Guid == smsSnippetTypeGuid )
                .Select( st => ( int? ) st.Id )
                .FirstOrDefault();

            if ( !smsSnippetTypeId.HasValue )
            {
                return ActionOk( new List<PersonLeftSnippetBag>() );
            }

            var snippetCategoryGuid = GetAttributeValue( AttributeKey.SnippetCategory ).AsGuidOrNull();
            var snippetCategoryId = snippetCategoryGuid.HasValue
                ? CategoryCache.GetId( snippetCategoryGuid.Value )
                : null;

            var currentPerson = RequestContext.CurrentPerson;
            var currentPersonId = currentPerson?.Id;
            var usePersonal = bag?.UsePersonal ?? false;

            // Two disjoint queries. Personal shows just the current user's
            // owned snippets; shared restricts to ownerless snippets and
            // relies on GetAuthorizedSnippets to apply the VIEW check.
            // Branching in C# rather than inside the Expression avoids EF
            // silently miscompiling a ternary over a closure variable.
            var snippetService = new SnippetService( RockContext );

            var authorized = usePersonal
                ? snippetService.GetAuthorizedSnippets( currentPerson,
                    s => s.SnippetTypeId == smsSnippetTypeId.Value
                        && ( !snippetCategoryId.HasValue || s.CategoryId == snippetCategoryId.Value )
                        && s.OwnerPersonAlias.PersonId == currentPersonId )
                : snippetService.GetAuthorizedSnippets( currentPerson,
                    s => s.SnippetTypeId == smsSnippetTypeId.Value
                        && ( !snippetCategoryId.HasValue || s.CategoryId == snippetCategoryId.Value )
                        && s.OwnerPersonAliasId == null );

            var snippets = authorized
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .ThenBy( s => s.Id )
                .Select( s => new PersonLeftSnippetBag
                {
                    IdKey = s.IdKey,
                    Name = s.Name
                } )
                .ToList();

            return ActionOk( snippets );
        }

        /// <summary>
        /// Resolves the Lava content of the selected snippet against the
        /// current person and target person, and returns the ready-to-paste
        /// text so it can be inserted into the SMS message box.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetSnippetContent( PersonLeftSnippetContentRequestBag bag )
        {
            if ( bag == null || bag.SnippetIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The snippet could not be loaded." );
            }

            var snippet = new SnippetService( RockContext ).Get( bag.SnippetIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( snippet == null )
            {
                return ActionBadRequest( "The snippet could not be loaded." );
            }

            var person = ResolvePerson();

            var mergeFields = RequestContext.GetCommonMergeFields();
            if ( person != null )
            {
                mergeFields.Add( "Person", person );
            }

            var content = snippet.Content.ResolveMergeFields( mergeFields );

            return ActionOk( new PersonLeftSnippetContentBag
            {
                Content = content
            } );
        }

        /// <summary>
        /// Sends an SMS to the person the block is currently displaying,
        /// applying the auto-adjustments required to unblock the send and
        /// logging a person-history entry describing any adjustments made.
        /// </summary>
        [BlockAction]
        public BlockActionResult SendSms( PersonLeftSendSmsRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Please enter a valid message to send." );
            }

            var message = bag.Message?.Trim();
            if ( message.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Please enter a valid message to send." );
            }

            var systemPhoneNumberGuid = GetAttributeValue( AttributeKey.SmsFrom ).AsGuidOrNull();
            if ( !systemPhoneNumberGuid.HasValue )
            {
                ExceptionLogService.LogException( new Exception( $"While trying to send an SMS from the Check-in Manager, the following error occurred: There is a misconfiguration with the {AttributeKey.SmsFrom} setting." ) );
                return ActionBadRequest( "Error sending message. Please try again or contact an administrator if the error continues." );
            }

            var smsFromNumber = SystemPhoneNumberCache.Get( systemPhoneNumberGuid.Value );
            if ( smsFromNumber == null )
            {
                ExceptionLogService.LogException( new Exception( $"While trying to send an SMS from the Check-in Manager, the following error occurred: The configured System Phone Number ({systemPhoneNumberGuid}) does not exist." ) );
                return ActionBadRequest( "Could not find a valid phone number to send from." );
            }

            var person = ResolvePerson();
            if ( person == null )
            {
                return ActionBadRequest( "Could not find a valid number for this person." );
            }

            var phoneNumber = GetSmsCapableMobilePhoneNumber( person );
            if ( phoneNumber == null )
            {
                return ActionBadRequest( "Could not find a valid number for this person." );
            }

            /*
                6/16/26 - NA

                Check-in Manager needs volunteers to be able to reach a parent about a child's need with as
                little friction as possible, and those volunteers typically don't have access to the person
                profile to fix these fields themselves. Two v18-era safeguards silently prevent the send in
                cases we do want to override in this specific context:

                  - Rock/Communication/TransportComponent.cs (commit 369bebfb) marks the recipient Failed
                    when Person.RecordStatusValueId is Inactive.
                  - Rock/Utility/ExtensionMethods/ICollectionExtensions.cs (commit 126c8097) causes the
                    SMS transport to skip phones with IsMessagingOptedOut = true.

                Neither safeguard surfaces an error to the volunteer -- they just see "Message queued" while
                the SMS is quietly dropped. To make the send reliable, we auto-adjust the recipient's
                properties (enable SMS on the phone number, clear opt-out, reactivate the person record)
                before queueing the message. A custom person-history entry is written so admins can see the
                change was made by Check-in Manager and why.

                Reason: Check-in Manager SMS was silently dropped for inactive people or opted-out mobile numbers.
            */
            var prepResult = EnsureRecipientCanReceiveSms( RockContext, person, phoneNumber );

            List<BinaryFile> attachments = null;
            if ( bag.AttachmentGuid.HasValue )
            {
                var binaryFile = new BinaryFileService( RockContext ).Get( bag.AttachmentGuid.Value );
                if ( binaryFile != null )
                {
                    attachments = new List<BinaryFile> { binaryFile };
                }
            }

            Rock.Communication.Medium.Sms.CreateCommunicationMobile(
                RequestContext.CurrentPerson,
                person.PrimaryAliasId,
                message,
                smsFromNumber,
                null,
                attachments,
                RockContext );

            WriteHistoryLogForSmsAutoAdjustments( RockContext, person, phoneNumber, prepResult );

            return ActionOk( new PersonLeftSendSmsResponseBag
            {
                Message = "Message queued."
            } );
        }

        #endregion Block Actions
    }
}
