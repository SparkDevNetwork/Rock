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

using Humanizer;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.Utility.Enums;
using Rock.ViewModels.Blocks.Crm.PersonDetail.Bio;
using Rock.ViewModels.Crm;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

#if WEBFORMS
using System.Web.UI;

using Rock.Data;
using Rock.Web;
#endif

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block that displays the biographic and
    /// demographic information about a person along with their picture.
    /// </summary>

    [DisplayName( "Person Bio" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person biographic/demographic information and picture (Person detail page)." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( Enums.Cms.BlockReloadMode.Page )]

    [SecurityAction( Authorization.VIEW_PROTECTION_PROFILE, "The roles and/or users that can view the protection profile alert for the selected person." )]

    #region Block Attributes

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The label badges to display in this block.",
        IsRequired = false,
        Order = 0 )]

    [WorkflowTypeField(
        "Workflow Actions",
        Key = AttributeKey.WorkflowActions,
        Description = "The workflows to make available as actions.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1 )]

    [CodeEditorField(
        "Additional Custom Actions",
        Key = AttributeKey.AdditionalCustomActions,
        Description = BlockAttributeDescription.AdditionalCustomActions,
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 200,
        IsRequired = false,
        Order = 2 )]

    [BooleanField(
        "Enable Impersonation",
        Key = AttributeKey.EnableImpersonation,
        Description = "Should the Impersonate custom action be enabled? Note: If enabled, it is only visible to users that are authorized to administrate the person.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [LinkedPage(
        "Impersonation Start Page",
        Key = AttributeKey.ImpersonationStartPage,
        Description = "The page to navigate to after clicking the Impersonate action.",
        IsRequired = false,
        Order = 4 )]

    [LinkedPage(
        "Business Detail Page",
        Key = AttributeKey.BusinessDetailPage,
        Description = "The page to redirect user to if a business is requested.",
        IsRequired = false,
        Order = 5 )]

    [LinkedPage(
        "Nameless Person Detail Page",
        Key = AttributeKey.NamelessPersonDetailPage,
        Description = "The page to redirect user to if the person record is a Nameless Person record type.",
        IsRequired = false,
        Order = 6 )]

    [BooleanField(
        "Display Country Code",
        Key = AttributeKey.DisplayCountryCode,
        Description = "When enabled prepends the country code to all phone numbers.",
        DefaultBooleanValue = false,
        Order = 7 )]

    [BooleanField(
        "Display Middle Name",
        Key = AttributeKey.DisplayMiddleName,
        Description = "Display the middle name of the person.",
        DefaultBooleanValue = false,
        Order = 8 )]

    [CodeEditorField(
        "Custom Content",
        Key = AttributeKey.CustomContent,
        Description = "Custom Content will be rendered after the person's demographic information <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        Order = 9 )]

    [BooleanField(
        "Allow Following",
        Key = AttributeKey.AllowFollowing,
        Description = "Should people be able to follow a person by selecting the following badge?",
        DefaultBooleanValue = true,
        Order = 10 )]

    [BooleanField(
        "Display Graduation",
        Key = AttributeKey.DisplayGraduation,
        Description = "Should the Grade/Graduation be displayed?",
        DefaultBooleanValue = true,
        Order = 11 )]

    [BooleanField(
        "Display Anniversary Date",
        Key = AttributeKey.DisplayAnniversaryDate,
        Description = "Should the Anniversary Date be displayed?",
        DefaultBooleanValue = true,
        Order = 12 )]

    [AttributeCategoryField(
        "Social Media Category",
        Key = AttributeKey.SocialMediaCategory,
        Description = "The Attribute Category to display attributes from.",
        AllowMultiple = false,
        EntityType = typeof( Rock.Model.Person ),
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Category.PERSON_ATTRIBUTES_SOCIAL,
        Order = 13 )]

    [BooleanField(
        "Enable Call Origination",
        Key = AttributeKey.EnableCallOrigination,
        Description = "Should click-to-call links be added to phone numbers.",
        DefaultBooleanValue = true,
        Order = 14 )]

    [LinkedPage(
        "Communication Page",
        Key = AttributeKey.CommunicationPage,
        Description = "The communication page to use when the email button or person's email address is clicked. Leave this blank to use the default.",
        IsRequired = false,
        Order = 15 )]

    [LinkedPage(
        "SMS Page",
        Key = AttributeKey.SmsPage,
        Description = "The communication page to use when the text button is clicked. Leave this blank to use the default.",
        IsRequired = false,
        Order = 16 )]

    #endregion Block Attributes

    [InitialBlockHeight( 0 )]
    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "EA0CF743-D5C9-4B3B-9F21-5766F2F4E678" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "DAAD0340-5CAB-4518-8314-BC39433C8EA5" )]
    [Rock.SystemGuid.BlockTypeGuid( "030CCDDC-8D43-40F8-A298-78B416F9E828" )]
    public class Bio : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Badges = "Badges";
            public const string WorkflowActions = "WorkflowActions";
            public const string AdditionalCustomActions = "Actions";
            public const string EnableImpersonation = "EnableImpersonation";
            public const string ImpersonationStartPage = "ImpersonationStartPage";
            public const string BusinessDetailPage = "BusinessDetailPage";
            public const string NamelessPersonDetailPage = "NamelessPersonDetailPage";
            public const string DisplayCountryCode = "DisplayCountryCode";
            public const string DisplayMiddleName = "DisplayMiddleName";
            public const string CustomContent = "CustomContent";
            public const string AllowFollowing = "AllowFollowing";
            public const string DisplayGraduation = "DisplayGraduation";
            public const string DisplayAnniversaryDate = "DisplayAnniversaryDate";
            public const string SocialMediaCategory = "SocialMediaCategory";
            public const string EnableCallOrigination = "EnableCallOrigination";
            public const string CommunicationPage = "CommunicationPage";
            public const string SmsPage = "SmsPage";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string BusinessId = "BusinessId";
            public const string NamelessPersonId = "NamelessPersonId";
        }

        private static class BlockAttributeDescription
        {
            public const string AdditionalCustomActions = @"Additional custom actions (will be displayed before the list of workflow actions). Any instance of '{0}' will be replaced with the current person's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an &lt;li&gt; element for each available action. Example:<pre>&lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;</pre>";
        }

        #endregion Keys

        #region Fields

        private const string NameQualifierKey = "name";
        private const string IconCssClassQualifierKey = "iconcssclass";
        private const string ColorQualifierKey = "color";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var person = GetPerson();

            if ( person == null )
            {
                RedirectToCanonicalPersonIfMerged();

                return new BioInitializationBox { IsVisible = false };
            }

            // Record the view before any business/nameless redirect so the
            // person-viewed audit trail matches the WebForms block, which
            // recorded the view for every resolved person regardless of record
            // type.
            SendPersonViewedMessage( person );

            if ( person.IsBusiness() )
            {
                var businessUrl = this.GetLinkedPageUrl( AttributeKey.BusinessDetailPage, new Dictionary<string, string>
                {
                    [PageParameterKey.BusinessId] = person.IdKey
                } );

                if ( businessUrl.IsNotNullOrWhiteSpace() )
                {
                    RequestContext.Response.RedirectToUrl( businessUrl );

                    return new BioInitializationBox { IsVisible = false };
                }
            }

            if ( person.IsNameless() )
            {
                var namelessUrl = this.GetLinkedPageUrl( AttributeKey.NamelessPersonDetailPage, new Dictionary<string, string>
                {
                    [PageParameterKey.NamelessPersonId] = person.IdKey
                } );

                if ( namelessUrl.IsNotNullOrWhiteSpace() )
                {
                    RequestContext.Response.RedirectToUrl( namelessUrl );
                }

                return new BioInitializationBox { IsVisible = false };
            }

            RequestContext.Response.SetBrowserTitle( person.FullName );

            // Attributes are needed to build the social media links.
            person.LoadAttributes( RockContext );

            var currentPerson = RequestContext.CurrentPerson;

            var box = new BioInitializationBox
            {
                IsVisible = true,
                PersonIdKey = person.IdKey,
                FullName = person.FullName,
                IsDeceased = person.IsDeceased,
                PhotoUrl = GetPhotoUrl( person ),
                IsBusiness = person.IsBusiness(),
                Badges = GetBadges( person ),
                IsFollowingVisible = GetAttributeValue( AttributeKey.AllowFollowing ).AsBoolean() && currentPerson != null,
                SmsUrl = GetSmsUrl( person ),
                IsEditVisible = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ),
                EditPersonUrl = RequestContext.ResolveRockUrl( $"~/Person/{person.IdKey}/Edit" ),
                VCardUrl = RequestContext.ResolveRockUrl( $"~/api/People/VCard/{person.Guid}" ),
                CustomActionsHtml = GetCustomActionsHtml( person ),
                WorkflowActions = GetWorkflowActions( person ),
                EmailTagHtml = GetEmailTagHtml( person ),
                SocialLinks = GetSocialLinks( person ),
                CustomContentHtml = GetCustomContentHtml()
            };

            if ( person.AccountProtectionProfile > AccountProtectionProfile.Low && BlockCache.IsAuthorized( Authorization.VIEW_PROTECTION_PROFILE, currentPerson ) )
            {
                box.AccountProtectionProfileText = person.AccountProtectionProfile.ConvertToString( true );
            }

            SetNameDetails( box, person );
            SetDemographicDetails( box, person );
            SetEmailButtonDetails( box, person );
            SetImpersonationDetails( box, person, currentPerson );
            SetCallOriginationDetails( box, currentPerson );

            box.PhoneNumbers = GetPhoneNumberBags( person );

            if ( box.IsFollowingVisible )
            {
                box.Following = GetFollowingBag( person );
            }

            return box;
        }

        /// <summary>
        /// Gets the person to display, either from the page context or the
        /// PersonId page parameter.
        /// </summary>
        /// <returns>The resolved person or <c>null</c> when one could not be determined.</returns>
        private Person GetPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Checks if the requested person identifier belongs to a person that
        /// has been merged into another record and permanently redirects to
        /// the canonical person when a matching alias is found.
        /// </summary>
        private void RedirectToCanonicalPersonIfMerged()
        {
            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var aliasPersonId = personKey.AsIntegerOrNull() ?? IdHasher.Instance.GetId( personKey );

            if ( !aliasPersonId.HasValue )
            {
                return;
            }

            var personAlias = new PersonAliasService( RockContext ).GetByAliasId( aliasPersonId.Value );

            if ( personAlias == null )
            {
                return;
            }

            var canonicalPersonKey = IdHasher.Instance.GetHash( personAlias.PersonId );

            var url = this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.PersonId] = canonicalPersonKey
            } );

            RequestContext.Response.RedirectToUrl( url, permanent: true );
        }

        /// <summary>
        /// Records that the current person viewed this person's profile.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        private void SendPersonViewedMessage( Person person )
        {
            var currentPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            if ( !currentPersonAliasId.HasValue || !person.PrimaryAliasId.HasValue || person.PrimaryAliasId.Value == currentPersonAliasId.Value )
            {
                return;
            }

#if WEBFORMS
            /*
                7/6/26 - MSE

                Multiple blocks on the person profile page record the person
                view, each guarded by this same request item key so that only
                one view is recorded per page request.

                Reason: Prevent duplicate person-viewed records per request.
            */
            var requestItems = System.Web.HttpContext.Current?.Items;

            if ( requestItems != null )
            {
                if ( requestItems["PersonViewed"] != null )
                {
                    return;
                }

                requestItems["PersonViewed"] = "Handled";
            }
#endif

            new AddPersonViewed.Message
            {
                DateTimeViewed = RockDateTime.Now,
                TargetPersonAliasId = person.PrimaryAliasId.Value,
                ViewerPersonAliasId = currentPersonAliasId.Value,
                Source = PageCache.PageTitle,
                IPAddress = RequestContext.ClientInformation?.IpAddress
            }.Send();
        }

        /// <summary>
        /// Gets the URL of the person's profile photo.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The photo URL including the avatar styling parameters.</returns>
        private string GetPhotoUrl( Person person )
        {
            var photoUrl = RequestContext.ResolveRockUrl( Person.GetPersonPhotoUrl( person, 400 ) );

            return $"{photoUrl}&Style=icon&BackgroundColor=E4E4E7&ForegroundColor=A1A1AA";
        }

        /// <summary>
        /// Gets the rendered badge content configured for this block.
        /// </summary>
        /// <param name="person">The person to use as the entity when rendering the badges.</param>
        /// <returns>A list of <see cref="RenderedBadgeBag"/> objects that contain the rendered badges.</returns>
        private List<RenderedBadgeBag> GetBadges( Person person )
        {
            var badgeTypeGuids = GetAttributeValue( AttributeKey.Badges ).SplitDelimitedValues().AsGuidList();

            if ( !badgeTypeGuids.Any() )
            {
                return new List<RenderedBadgeBag>();
            }

            var badges = badgeTypeGuids
                .Select( g => BadgeCache.Get( g ) )
                .Where( b => b != null && b.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( b => b.Order )
                .ToList();

            return badges.Select( b => b.RenderBadge( person ) )
                .Where( b => b.Html.IsNotNullOrWhiteSpace() || b.JavaScript.IsNotNullOrWhiteSpace() )
                .ToList();
        }

        /// <summary>
        /// Sets the name related details on the initialization box.
        /// </summary>
        /// <param name="box">The initialization box being built.</param>
        /// <param name="person">The person being viewed.</param>
        private void SetNameDetails( BioInitializationBox box, Person person )
        {
            box.NickName = person.NickName;
            box.LastName = person.LastName;

            if ( box.IsBusiness )
            {
                return;
            }

            // Only prefix the name with a title when the title is a formal
            // one, such as "Dr." or "Rev.".
            if ( person.TitleValueId.HasValue )
            {
                var titleValue = DefinedValueCache.Get( person.TitleValueId.Value );

                if ( titleValue != null && titleValue.GetAttributeValue( "IsFormal" ).AsBoolean() )
                {
                    box.FormalTitle = titleValue.Value;
                }
            }

            if ( person.SuffixValueId.HasValue )
            {
                box.Suffix = DefinedValueCache.Get( person.SuffixValueId.Value )?.Value;
            }

            if ( GetAttributeValue( AttributeKey.DisplayMiddleName ).AsBoolean() && person.MiddleName.IsNotNullOrWhiteSpace() )
            {
                box.MiddleName = person.MiddleName;
            }

            // Show the first name as a secondary name when it differs from
            // the nick name.
            if ( person.NickName != person.FirstName && person.FirstName.IsNotNullOrWhiteSpace() )
            {
                box.FirstName = person.FirstName;
            }

            var previousNames = person.GetPreviousNames( RockContext )
                .Select( a => a.LastName )
                .ToList();

            if ( previousNames.Any() )
            {
                box.PreviousNames = previousNames.AsDelimited( ", " );
            }
        }

        /// <summary>
        /// Sets the demographic details on the initialization box.
        /// </summary>
        /// <param name="box">The initialization box being built.</param>
        /// <param name="person">The person being viewed.</param>
        private void SetDemographicDetails( BioInitializationBox box, Person person )
        {
            box.GenderText = person.Gender.ToString();

            var raceAndEthnicity = new List<string>();

            if ( person.RaceValueId.HasValue )
            {
                var raceValue = DefinedValueCache.Get( person.RaceValueId.Value )?.Value;

                if ( raceValue.IsNotNullOrWhiteSpace() )
                {
                    raceAndEthnicity.Add( raceValue );
                }
            }

            if ( person.EthnicityValueId.HasValue )
            {
                var ethnicityValue = DefinedValueCache.Get( person.EthnicityValueId.Value )?.Value;

                if ( ethnicityValue.IsNotNullOrWhiteSpace() )
                {
                    raceAndEthnicity.Add( ethnicityValue );
                }
            }

            if ( raceAndEthnicity.Any() )
            {
                box.RaceEthnicityText = raceAndEthnicity.AsDelimited( "/" );
                box.RaceEthnicityLabel = $"{Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL )}/{Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL )}";
            }

            if ( person.BirthDate.HasValue )
            {
                if ( person.BirthYear.HasValue && person.BirthYear != DateTime.MinValue.Year )
                {
                    var formattedAge = person.FormatAge();

                    if ( formattedAge.IsNotNullOrWhiteSpace() )
                    {
                        formattedAge += " old";
                    }

                    box.AgeText = formattedAge;
                    box.BirthDateText = person.BirthDate.Value.ToShortDateString();
                }
                else
                {
                    // Without a birth year the birth date itself becomes the
                    // primary term.
                    box.BirthDateText = person.BirthDate.Value.ToString( "MMM d" );
                }
            }

            if ( person.MaritalStatusValueId.HasValue )
            {
                box.MaritalStatusText = DefinedValueCache.Get( person.MaritalStatusValueId.Value )?.Value;
            }

            if ( !person.IsDeceased && person.AnniversaryDate.HasValue && GetAttributeValue( AttributeKey.DisplayAnniversaryDate ).AsBoolean() )
            {
                box.AnniversaryText = person.AnniversaryDate.Value.Humanize().Replace( "ago", string.Empty ).Trim();
                box.AnniversaryDateText = person.AnniversaryDate.Value.ToShortDateString();
            }

            if ( GetAttributeValue( AttributeKey.DisplayGraduation ).AsBoolean() )
            {
                if ( person.GradeFormatted.IsNotNullOrWhiteSpace() )
                {
                    box.GradeText = person.GradeFormatted;
                }

                if ( person.GraduationYear.HasValue && person.HasGraduated.HasValue )
                {
                    box.HasGraduated = person.HasGraduated;
                    box.GraduationText = person.HasGraduated.Value
                        ? $"Graduated {person.GraduationYear.Value}"
                        : $"Graduates {person.GraduationYear.Value}";
                }
            }
        }

        /// <summary>
        /// Gets the URL for the Text (SMS) action button.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The URL for the SMS button or <c>null</c> when the button should be hidden.</returns>
        private string GetSmsUrl( Person person )
        {
            if ( person.PhoneNumbers == null || !person.PhoneNumbers.Any( p => p.IsMessagingEnabled ) )
            {
                return null;
            }

            var mediums = GetCommunicationMediums();
            var linkedPageValue = GetAttributeValue( AttributeKey.SmsPage );

            if ( linkedPageValue.IsNotNullOrWhiteSpace() )
            {
                var pageReference = new Rock.Web.PageReference( linkedPageValue );
                var queryString = new System.Collections.Specialized.NameValueCollection( pageReference.QueryString ?? new System.Collections.Specialized.NameValueCollection() )
                {
                    ["person"] = person.IdKey
                };

                if ( mediums.ContainsKey( "SMS" ) )
                {
                    queryString.Add( "MediumId", mediums["SMS"].Value.ToString() );
                }

                return new Rock.Web.PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters, queryString ).BuildUrl();
            }

            var smsLink = $"{RequestContext.ResolveRockUrl( "/" )}communications/new/simple?person={person.IdKey}";

            if ( mediums.ContainsKey( "SMS" ) )
            {
                smsLink += $"&MediumId={mediums["SMS"].Value}";
            }

            return smsLink;
        }

        /// <summary>
        /// Sets the Email action button details on the initialization box.
        /// </summary>
        /// <param name="box">The initialization box being built.</param>
        /// <param name="person">The person being viewed.</param>
        private void SetEmailButtonDetails( BioInitializationBox box, Person person )
        {
            if ( person.Email.IsNullOrWhiteSpace() || !person.IsEmailActive || person.EmailPreference == EmailPreference.DoNotEmail )
            {
                return;
            }

            var emailLink = $"mailto:{person.Email}";
            var emailLinkPreference = GlobalAttributesCache.Get().GetValue( "PreferredEmailLinkType" );

            if ( emailLinkPreference.IsNullOrWhiteSpace() || emailLinkPreference == "1" )
            {
                var linkedPageValue = GetAttributeValue( AttributeKey.CommunicationPage );

                if ( linkedPageValue.IsNotNullOrWhiteSpace() )
                {
                    var pageReference = new Rock.Web.PageReference( linkedPageValue );
                    var queryString = new System.Collections.Specialized.NameValueCollection( pageReference.QueryString ?? new System.Collections.Specialized.NameValueCollection() )
                    {
                        ["person"] = person.IdKey
                    };

                    emailLink = new Rock.Web.PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters, queryString ).BuildUrl();
                }
                else
                {
                    emailLink = $"{RequestContext.ResolveRockUrl( "/" )}communications/new?person={person.IdKey}";
                }
            }

            box.EmailUrl = emailLink;
            box.EmailButtonTooltip = person.EmailPreference == EmailPreference.NoMassEmails
                ? @"Email Preference is set to ""No Mass Emails"""
                : "Send an email";
        }

        /// <summary>
        /// Sets the impersonation action details on the initialization box.
        /// The action is only shown to users that are authorized to
        /// administrate the person, and is disabled when person token usage is
        /// not allowed due to the person's account protection profile.
        /// </summary>
        /// <param name="box">The initialization box being built.</param>
        /// <param name="person">The person being viewed.</param>
        /// <param name="currentPerson">The currently logged in person.</param>
        private void SetImpersonationDetails( BioInitializationBox box, Person person, Person currentPerson )
        {
            if ( !GetAttributeValue( AttributeKey.EnableImpersonation ).AsBoolean() )
            {
                return;
            }

            if ( currentPerson == null || person.Id == currentPerson.Id || !person.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
            {
                return;
            }

            box.IsImpersonateVisible = true;
            box.IsImpersonateEnabled = person.IsPersonTokenUsageAllowed();
        }

        /// <summary>
        /// Sets the call origination details on the initialization box.
        /// </summary>
        /// <param name="box">The initialization box being built.</param>
        /// <param name="currentPerson">The currently logged in person.</param>
        private void SetCallOriginationDetails( BioInitializationBox box, Person currentPerson )
        {
            box.IsCallOriginationEnabled = GetAttributeValue( AttributeKey.EnableCallOrigination ).AsBoolean();

            if ( !box.IsCallOriginationEnabled )
            {
                return;
            }

            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( currentPerson );

            box.IsCallOriginationAvailable = pbxComponent != null;
            box.CurrentPersonGuid = currentPerson?.Guid;
            box.CurrentPersonFullName = currentPerson?.FullName;
        }

        /// <summary>
        /// Gets the additional custom action list items configured in the
        /// block settings with URL tokens and person id placeholders resolved.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The custom action HTML or <c>null</c> when none is configured.</returns>
        private string GetCustomActionsHtml( Person person )
        {
            var actions = GetAttributeValue( AttributeKey.AdditionalCustomActions );

            if ( actions.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var appRoot = RequestContext.ResolveRockUrl( "~/" );
            var themeRoot = RequestContext.ResolveRockUrl( "~~/" );
            actions = actions.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            return actions.Replace( "{0}", person.Id.ToString() );
        }

        /// <summary>
        /// Gets the workflow actions configured in the block settings that the
        /// current user is authorized to view.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>A list of workflow actions ordered by name.</returns>
        private List<BioWorkflowActionBag> GetWorkflowActions( Person person )
        {
            var workflowActions = GetAttributeValue( AttributeKey.WorkflowActions );

            if ( workflowActions.IsNullOrWhiteSpace() )
            {
                return new List<BioWorkflowActionBag>();
            }

            return workflowActions.SplitDelimitedValues()
                .AsGuidList()
                .Select( g => WorkflowTypeCache.Get( g ) )
                .Where( wt => wt != null
                    && ( wt.IsActive ?? true )
                    && wt.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( wt => wt.Name )
                .Select( wt => new BioWorkflowActionBag
                {
                    Name = wt.Name,
                    IconCssClass = wt.IconCssClass,
                    Url = RequestContext.ResolveRockUrl( $"~/WorkflowEntry/{wt.IdKey}?PersonId={person.IdKey}" )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the rendered HTML for the person's email address, including
        /// any email preference indicators.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The email tag HTML or an empty string when the person has no email.</returns>
        private string GetEmailTagHtml( Person person )
        {
            var linkedPageValue = GetAttributeValue( AttributeKey.CommunicationPage );
            var communicationPageReference = linkedPageValue.IsNotNullOrWhiteSpace()
                ? new Rock.Web.PageReference( linkedPageValue )
                : null;

            return person.GetEmailTag( RequestContext.ResolveRockUrl( "/" ), communicationPageReference, "d-inline-block mw-100 text-link text-truncate" );
        }

        /// <summary>
        /// Gets the person's social media links from the attributes in the
        /// configured social media category.
        /// </summary>
        /// <param name="person">The person being viewed. Attributes must already be loaded.</param>
        /// <returns>A list of social media links ordered by attribute order.</returns>
        private List<BioSocialLinkBag> GetSocialLinks( Person person )
        {
            var socialCategoryGuid = GetAttributeValue( AttributeKey.SocialMediaCategory ).AsGuidOrNull();

            if ( !socialCategoryGuid.HasValue )
            {
                return new List<BioSocialLinkBag>();
            }

            var attributes = person.Attributes.Where( p => p.Value.Categories.Select( c => c.Guid ).Contains( socialCategoryGuid.Value ) );
            var result = attributes.Join( person.AttributeValues, a => a.Key, v => v.Key, ( a, v ) => new { Attribute = a.Value, Value = v.Value, QualifierValues = a.Value.QualifierValues } );

            return result
                .Where( r =>
                    r.Value != null &&
                    r.Value.Value != string.Empty &&
                    r.QualifierValues != null &&
                    r.QualifierValues.ContainsKey( NameQualifierKey ) &&
                    r.QualifierValues.ContainsKey( IconCssClassQualifierKey ) &&
                    r.QualifierValues.ContainsKey( ColorQualifierKey ) )
                .OrderBy( r => r.Attribute.Order )
                .Select( r => new BioSocialLinkBag
                {
                    Url = r.Value.Value,
                    Name = r.QualifierValues[NameQualifierKey].Value,
                    IconCssClass = r.Attribute.QualifierValues[IconCssClassQualifierKey].Value.Contains( "ti-fw" )
                        ? r.Attribute.QualifierValues[IconCssClassQualifierKey].Value
                        : r.Attribute.QualifierValues[IconCssClassQualifierKey].Value + " ti-fw"
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the resolved custom content configured in the block settings.
        /// </summary>
        /// <returns>The resolved custom content HTML or <c>null</c> when none is configured.</returns>
        private string GetCustomContentHtml()
        {
            var customContent = GetAttributeValue( AttributeKey.CustomContent );

            if ( customContent.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            return customContent.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the person's phone numbers ordered by the phone type defined
        /// value order.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>A list of phone number bags.</returns>
        private List<BioPhoneNumberBag> GetPhoneNumberBags( Person person )
        {
            if ( person.PhoneNumbers == null )
            {
                return new List<BioPhoneNumberBag>();
            }

            var showCountryCode = GetAttributeValue( AttributeKey.DisplayCountryCode ).AsBoolean();

            var phoneNumbers = person.PhoneNumbers.AsEnumerable();
            var phoneNumberTypeIds = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() )
                ?.DefinedValues
                .Select( a => a.Id )
                .ToList();

            if ( phoneNumberTypeIds != null && phoneNumberTypeIds.Any() )
            {
                phoneNumbers = phoneNumbers.OrderBy( a => phoneNumberTypeIds.IndexOf( a.NumberTypeValueId ?? 0 ) );
            }

            return phoneNumbers
                .Select( phoneNumber => new BioPhoneNumberBag
                {
                    FormattedNumber = phoneNumber.IsUnlisted
                        ? "Unlisted"
                        : PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number, showCountryCode ),
                    PhoneTypeText = phoneNumber.NumberTypeValueId.HasValue
                        ? DefinedValueCache.Get( phoneNumber.NumberTypeValueId.Value )?.Value
                        : null,
                    IsUnlisted = phoneNumber.IsUnlisted,
                    IsMessagingEnabled = phoneNumber.IsMessagingEnabled,
                    IsMessagingOptedOut = phoneNumber.IsMessagingOptedOut,
                    MessagingOptedOutTooltip = GetMessagingOptedOutTooltip( person, phoneNumber ),
                    RawNumber = phoneNumber.IsUnlisted ? null : phoneNumber.Number,
                    SmsTelUri = phoneNumber.IsUnlisted ? null : phoneNumber.ToSmsNumber()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the tooltip describing when the person opted out of messaging
        /// on the phone number.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <param name="phoneNumber">The phone number to describe.</param>
        /// <returns>The tooltip text or <c>null</c> when the person has not opted out.</returns>
        private string GetMessagingOptedOutTooltip( Person person, PhoneNumber phoneNumber )
        {
            if ( !phoneNumber.IsMessagingOptedOut )
            {
                return null;
            }

            if ( phoneNumber.MessagingOptedOutDateTime.HasValue )
            {
                var formattedOptOutDate = phoneNumber.MessagingOptedOutDateTime.Value.ToString( "MMMM d, yyyy" );

                return $"{person.NickName} opted out from messaging on {formattedOptOutDate}";
            }

            return $"{person.NickName} opted out from messaging.";
        }

        /// <summary>
        /// Gets the follow state and follower count for the person being
        /// viewed.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The follow state for the current person.</returns>
        private BioFollowingBag GetFollowingBag( Person person )
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;
            var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS ).Value;

            var followingState = new FollowingService( RockContext )
                .Queryable()
                .Where( f => f.EntityTypeId == personAliasEntityTypeId && f.EntityId == person.PrimaryAliasId )
                .GroupBy( f => 1 )
                .Select( g => new
                {
                    Count = g.Count(),
                    IsFollowed = g.Any( f => f.PersonAlias.PersonId == currentPersonId )
                } )
                .FirstOrDefault();

            return new BioFollowingBag
            {
                FollowerCount = followingState?.Count ?? 0,
                IsFollowed = followingState?.IsFollowed ?? false
            };
        }

        /// <summary>
        /// Gets the dictionary of active communication mediums that the
        /// current user is authorized to view, keyed by friendly name.
        /// </summary>
        /// <returns>A dictionary of medium friendly names and entity type identifiers.</returns>
        private Dictionary<string, int?> GetCommunicationMediums()
        {
            var mediums = new Dictionary<string, int?>();

            foreach ( var item in Rock.Communication.MediumContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive && item.Value.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    mediums.TryAdd( item.Value.EntityType.FriendlyName, item.Value.EntityType.Id );
                }
            }

            return mediums;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Toggles whether the current person is following the person being
        /// viewed.
        /// </summary>
        /// <param name="personIdKey">The IdKey of the person being viewed.</param>
        /// <returns>The updated follow state and follower count.</returns>
        [BlockAction]
        public BlockActionResult ToggleFollowing( string personIdKey )
        {
            if ( !GetAttributeValue( AttributeKey.AllowFollowing ).AsBoolean() )
            {
                return ActionForbidden( "Following is not enabled." );
            }

            var currentPerson = RequestContext.CurrentPerson;

            if ( currentPerson?.PrimaryAliasId == null )
            {
                return ActionUnauthorized( "You must be logged in to follow someone." );
            }

            var person = new PersonService( RockContext ).Get( personIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( person?.PrimaryAliasId == null )
            {
                return ActionNotFound( "The person was not found." );
            }

            var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS ).Value;

            new FollowingService( RockContext ).ToggleFollowing( personAliasEntityTypeId, person.PrimaryAliasId.Value, currentPerson.PrimaryAliasId.Value );
            RockContext.SaveChanges();

            return ActionOk( GetFollowingBag( person ) );
        }

        /// <summary>
        /// Gets the URL that starts an impersonation session for the person
        /// being viewed.
        /// </summary>
        /// <param name="personIdKey">The IdKey of the person being viewed.</param>
        /// <returns>The URL to navigate to in order to impersonate the person.</returns>
        [BlockAction]
        public BlockActionResult GetImpersonationUrl( string personIdKey )
        {
            if ( !GetAttributeValue( AttributeKey.EnableImpersonation ).AsBoolean() )
            {
                return ActionForbidden( "Impersonation is not enabled." );
            }

            var currentPerson = RequestContext.CurrentPerson;
            var person = new PersonService( RockContext ).Get( personIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( person == null )
            {
                return ActionNotFound( "The person was not found." );
            }

            if ( currentPerson == null || person.Id == currentPerson.Id || !person.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
            {
                return ActionForbidden( "You are not authorized to impersonate this person." );
            }

            if ( !person.IsPersonTokenUsageAllowed() )
            {
                return ActionForbidden( "Impersonation is not allowed for this person." );
            }

            var impersonationToken = person.GetImpersonationToken( RockDateTime.Now.AddMinutes( 5 ), 1, null );
            var queryParams = new Dictionary<string, string>
            {
                ["rckipid"] = impersonationToken
            };

            var url = GetAttributeValue( AttributeKey.ImpersonationStartPage ).IsNotNullOrWhiteSpace()
                ? this.GetLinkedPageUrl( AttributeKey.ImpersonationStartPage, queryParams )
                : this.GetCurrentPageUrl( queryParams );

            return ActionOk( url );
        }

        #endregion Block Actions

#if WEBFORMS

        #region Custom Settings Providers

        /// <summary>
        /// Injects styling into the block settings dialog so the example
        /// markup in the Additional Custom Actions help text renders as a
        /// readable code block. Without this, the &lt;pre&gt; element
        /// inherits the tooltip's light text color onto its own light
        /// background and becomes unreadable.
        /// </summary>
        /*
            7/6/26 - MSE

            This provider only loads when the Bio block is being edited, so
            the injected CSS is naturally scoped to this block's settings
            dialog and doesn't affect tooltips elsewhere in Rock.

            Copied from McpServerList

            Reason: Style the <pre> example in the help tooltip.
        */
        [CustomSettingsBlockType( typeof( Bio ), Model.SiteType.Web )]
        public class BioCustomSettingsProvider : RockCustomSettingsProvider
        {
            private const string TooltipStyleScript = @"
<script>
    (function() {
        var style = document.createElement( 'style' );
        style.textContent = '.tooltip-inner { max-width: 400px; } .tooltip-inner pre { margin: 8px 0 0; padding: 8px; text-align: left; color: var(--color-interface-strong, #333); background-color: var(--color-interface-softest, #f5f5f5); white-space: pre-wrap; word-break: break-word; }';
        document.head.appendChild( style );
    })();
</script>";

            /// <inheritdoc />
            public override string CustomSettingsTitle => "Basic Settings";

            /// <inheritdoc />
            public override Control GetCustomSettingsControl( IHasAttributes attributeEntity, Control parent )
            {
                return new LiteralControl( TooltipStyleScript );
            }

            /// <inheritdoc />
            public override void ReadSettingsFromEntity( IHasAttributes attributeEntity, Control control )
            {
                // No persisted state; this provider only injects styling for tooltips.
            }

            /// <inheritdoc />
            public override void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control, RockContext rockContext )
            {
                // No persisted state; this provider only injects styling for tooltips.
            }
        }

        #endregion Custom Settings Providers

#endif
    }
}
