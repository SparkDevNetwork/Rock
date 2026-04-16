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
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Cms.PersonalLinks;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Renders a bookmark icon in the page chrome that opens a popover showing
    /// the current person's personal links and recently-viewed "quick return" items.
    /// </summary>
    [DisplayName( "Personal Links" )]
    [Category( "CMS" )]
    [Description( "This block is used to show both personal and shared bookmarks as well as 'Quick Return' links." )]

    #region Block Attributes

    [LinkedPage(
        "Manage Links Page",
        Description = "The page where a person can manage their sections and personal links.",
        Order = 0,
        Key = AttributeKey.ManageLinksPage )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "83E197B9-7A42-463A-8975-DC15A5D31D8E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "C553DFC6-3A57-43AC-A187-E558817E5C78" )]
    [Rock.SystemGuid.BlockTypeGuid( "4D42DF90-97A3-470B-A7D4-A6FD00673761" )]
    public class PersonalLinks : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ManageLinksPage = "ManageLinksPage";
        }

        private static class NavigationUrlKey
        {
            public const string ManageLinksPage = "ManageLinksPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// This is a private variable used by <see cref="GetInitializationBox"/>
        /// to return a cached version of the box during startup so both
        /// <see cref="GetObsidianBlockInitialization"/> and
        /// <see cref="GetInitialHtmlContent"/> share the same instance.
        /// </summary>
        private PersonalLinksInitializationBox _initBox;

        /// <summary>
        /// Schemes allowed on personal-link URLs. Blocks javascript:, data:,
        /// and other code-executing schemes that would run in the viewer's
        /// session when clicked (critical for shared sections).
        /// </summary>
        private static readonly HashSet<string> _allowedUrlSchemes = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            Uri.UriSchemeHttp,
            Uri.UriSchemeHttps,
            Uri.UriSchemeMailto,
            "tel"
        };

        /// <summary>
        /// Template for the bookmark icon plus the inline addQuickReturn script.
        /// {storageKey} is substituted server-side with the JS-encoded per-person
        /// localStorage key. NOTE: keep this as a plain verbatim string (not
        /// interpolated with $@) — the JS body contains literal { and } that
        /// would otherwise require escaping as {{ and }}.
        /// </summary>
        private const string InitialHtmlTemplate = @"<a class=""rock-bookmark js-rock-bookmark"" href=""#""><i class=""ti ti-bookmark""></i></a><script>(function () {
    window.Rock = window.Rock || {};
    if (window.Rock.personalLinks && window.Rock.personalLinks.addQuickReturn) { return; }
    var storageKey = '{storageKey}';
    window.Rock.personalLinks = window.Rock.personalLinks || {};
    window.Rock.personalLinks.addQuickReturn = function (type, typeOrder, itemName) {
        try {
            var items = JSON.parse(localStorage.getItem(storageKey)) || [];
            var url = window.location.href;
            items = items.filter(function (el) {
                return !(el.url.toLowerCase() === url.toLowerCase() && el.type.toLowerCase() === type.toLowerCase());
            });
            items = items.filter(function (el) {
                return !(el.type === type && el.typeOrder === typeOrder && el.itemName === itemName);
            });
            items.push({ type: type, typeOrder: typeOrder, createdDateTime: new Date(), itemName: itemName, url: url });
            if (items.length > 20) { items.splice(0, items.length - 20); }
            localStorage.setItem(storageKey, JSON.stringify(items));
            if (window.Rock.personalLinks.onQuickReturnUpdated) {
                window.Rock.personalLinks.onQuickReturnUpdated();
            }
        } catch (e) { /* swallow - quick returns are best-effort */ }
    };
}());</script>";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <summary>
        /// Gets the initialization box data, caching the result so it can
        /// be shared between <see cref="GetObsidianBlockInitialization"/>
        /// and <see cref="GetInitialHtmlContent"/>.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private PersonalLinksInitializationBox GetInitializationBox()
        {
            if ( _initBox != null )
            {
                return _initBox;
            }

            var box = new PersonalLinksInitializationBox();
            var currentPerson = GetCurrentPerson();

            if ( currentPerson?.PrimaryAliasId.HasValue == true )
            {
                box.IsBlockVisible = true;
                box.QuickLinksLocalStorageKey = PersonalLinkService.GetQuickLinksLocalStorageKey( currentPerson );
                box.PersonalLinksModificationHash = PersonalLinkService.GetPersonalLinksModificationHash( currentPerson );
                box.CurrentPageTitle = !string.IsNullOrWhiteSpace( PageCache?.BrowserTitle )
                    ? PageCache.BrowserTitle
                    : PageCache?.InternalName;
                box.CurrentPageUrl = RequestContext?.RequestUri?.AbsoluteUri;
                box.NavigationUrls = new Dictionary<string, string>
                {
                    [NavigationUrlKey.ManageLinksPage] = this.GetLinkedPageUrl( AttributeKey.ManageLinksPage )
                };
            }

            _initBox = box;

            return _initBox;
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            /*
                4/16/2026 - MSE

                Emits the bookmark icon plus a small inline <script> directly into
                the server response. There are two reasons we do this here rather
                than letting the Obsidian Vue component render everything on mount:

                1. No Vue-mount flicker on page chrome.
                   This block lives in the layout, so it renders on every page
                   navigation. If the bookmark were only produced by the Vue
                   component, every page load would briefly show an empty slot
                   before Vue mounted and faded the icon in - a visible flicker
                   in the header on every click. Server-rendering the <a> tag
                   here means the bookmark is present in the initial HTML and
                   stays put while Vue hydrates on top of it.

                2. addQuickReturn must exist before $(document).ready fires.
                   The AddQuickReturn Lava filter (see Rock.Lava.Filters) emits
                   $(document).ready callbacks on many pages (Person Bio,
                   GroupDetail, WorkflowDetail, etc.) that call
                   Rock.personalLinks.addQuickReturn(...). If we only defined
                   that function inside Vue's onMounted, the async Vue bootstrap
                   could finish after the ready queue has already fired - any
                   filter calls that beat Vue would silently no-op and the
                   quick return for that page would be lost. Defining the
                   function in an inline <script> during HTML parse guarantees
                   it's registered before any $(document).ready handler runs.
            */

            var box = GetInitializationBox();

            if ( !box.IsBlockVisible )
            {
                return string.Empty;
            }

            var storageKey = HttpUtility.JavaScriptStringEncode( box.QuickLinksLocalStorageKey ?? string.Empty );

            return InitialHtmlTemplate.Replace( "{storageKey}", storageKey );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the current person's personal links data, including both
        /// shared and private sections they are authorized to view.
        /// </summary>
        /// <returns>A <see cref="PersonalLinksDataBag"/> matching the shape the client caches in localStorage.</returns>
        [BlockAction]
        public BlockActionResult GetPersonalLinksData()
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var data = PersonalLinkService.GetPersonalLinksData( currentPerson );

            var bag = new PersonalLinksDataBag
            {
                ModificationHash = data.ModificationHash,
                Sections = data.PersonLinksSectionList?.Select( s => new PersonalLinksSectionBag
                {
                    IdKey = IdHasher.Instance.GetHash( s.Id ),
                    Name = s.Name,
                    IsShared = s.IsShared,
                    Order = s.Order,
                    PersonalLinks = s.PersonalLinks?.Select( l => new PersonalLinkBag
                    {
                        IdKey = IdHasher.Instance.GetHash( l.Id ),
                        Name = l.Name,
                        Url = l.Url,
                        Order = l.Order
                    } ).ToList()
                } ).ToList() ?? new List<PersonalLinksSectionBag>()
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Returns the current person's non-shared sections as a list suitable
        /// for populating the Add Link form's Section dropdown.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> where Value is the section IdKey.</returns>
        [BlockAction]
        public BlockActionResult GetSections()
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var sections = new PersonalLinkService( RockContext )
                .GetOrderedPersonalLinkSectionsQuery( currentPerson )
                .Where( a => a.PersonAliasId.HasValue && a.PersonAlias.PersonId == currentPerson.Id )
                .Select( a => new { a.Id, a.Name } )
                .ToList()
                .Select( a => new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( a.Id ),
                    Text = a.Name
                } )
                .ToList();

            return ActionOk( sections );
        }

        /// <summary>
        /// Creates a new private personal link section for the current person.
        /// </summary>
        /// <param name="name">The name of the new section.</param>
        /// <returns>A <see cref="ListItemBag"/> describing the new section so the client can add it to the dropdown.</returns>
        [BlockAction]
        public BlockActionResult SaveSection( string name )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson?.PrimaryAliasId == null )
            {
                return ActionUnauthorized();
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Section name is required." );
            }

            var section = CreatePrivateSection( name, currentPerson );

            return ActionOk( new ListItemBag
            {
                Value = IdHasher.Instance.GetHash( section.Id ),
                Text = section.Name
            } );
        }

        /// <summary>
        /// Creates a new personal link for the current person. If no section
        /// is specified, a "Links" section is auto-created first.
        /// </summary>
        /// <param name="bag">The link details.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult SaveLink( SaveLinkRequestBag bag )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson?.PrimaryAliasId == null )
            {
                return ActionUnauthorized();
            }

            if ( bag == null || bag.Name.IsNullOrWhiteSpace() || bag.Url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Link name and URL are required." );
            }

            if ( !IsUrlSchemeAllowed( bag.Url ) )
            {
                return ActionBadRequest( "Link URL must be an http, https, mailto, or tel address." );
            }

            int? sectionId = null;

            if ( !bag.SectionIdKey.IsNullOrWhiteSpace() )
            {
                sectionId = IdHasher.Instance.GetId( bag.SectionIdKey );
            }

            // Verify the current person can edit the chosen section — prevents
            // a forged SectionIdKey from implanting a link in another user's
            // (or a shared) section.
            if ( sectionId.HasValue )
            {
                var targetSection = new PersonalLinkSectionService( RockContext ).Get( sectionId.Value );
                if ( targetSection == null || !targetSection.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return ActionBadRequest( "Invalid section." );
                }
            }

            if ( !sectionId.HasValue )
            {
                // Auto-create a "Links" section to match the WebForms behavior
                // when the user saves a link without picking a section.
                var section = CreatePrivateSection( "Links", currentPerson );
                sectionId = section.Id;
            }

            var personalLinkService = new PersonalLinkService( RockContext );
            var personalLink = new PersonalLink
            {
                SectionId = sectionId.Value,
                PersonAliasId = currentPerson.PrimaryAliasId,
                Name = bag.Name,
                Url = bag.Url
            };

            personalLinkService.Add( personalLink );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions

        #region Private Helpers

        /// <summary>
        /// Creates a non-shared PersonalLinkSection owned by the specified
        /// person and restricts view/edit/administrate authorization to them.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="currentPerson">The person who will own the section.</param>
        /// <returns>The newly created section.</returns>
        private PersonalLinkSection CreatePrivateSection( string name, Person currentPerson )
        {
            var sectionService = new PersonalLinkSectionService( RockContext );
            var section = new PersonalLinkSection
            {
                IsShared = false,
                PersonAliasId = currentPerson.PrimaryAliasId,
                Name = name
            };

            sectionService.Add( section );
            RockContext.SaveChanges();

            section.MakePrivate( Authorization.VIEW, currentPerson, RockContext );
            section.MakePrivate( Authorization.EDIT, currentPerson, RockContext );
            section.MakePrivate( Authorization.ADMINISTRATE, currentPerson, RockContext );

            return section;
        }

        /// <summary>
        /// Returns true for relative URLs or absolute URLs whose scheme is in
        /// <see cref="_allowedUrlSchemes"/>.
        /// </summary>
        private static bool IsUrlSchemeAllowed( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Relative URLs resolve against the current origin and can't carry a foreign scheme.
            if ( !Uri.TryCreate( url, UriKind.Absolute, out var uri ) )
            {
                return true;
            }

            return _allowedUrlSchemes.Contains( uri.Scheme );
        }

        #endregion Private Helpers
    }
}
