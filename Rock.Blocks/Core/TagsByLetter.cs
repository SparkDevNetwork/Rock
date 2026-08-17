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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.TagsByLetter;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Lists tags grouped by the first letter of the name with counts for people to select.
    /// </summary>

    [DisplayName( "Tags By Letter" )]
    [Category( "Core" )]
    [Description( "Lists tags grouped by the first letter of the name with counts for people to select." )]
    [IconCssClass( "ti ti-tag" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page to navigate to when a tag is selected.",
        Key = AttributeKey.DetailPage )]

    [BooleanField( "User-Selectable Entity Type",
        Description = "Should user be able to select the entity type to show tags for?",
        DefaultBooleanValue = true,
        Order = 0,
        Key = AttributeKey.ShowEntityType )]

    [EntityTypeField( "Entity Type",
        IncludeGlobalAttributeOption = false,
        Description = "The entity type to display tags for. If entity type is user-selectable, this will be the default entity type.",
        IsRequired = false,
        DefaultValue = SystemGuid.EntityType.PERSON,
        Order = 1,
        Key = AttributeKey.EntityType )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "6C1B27A4-27A9-408E-8C74-E85B062BF466" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "62837E1A-19F5-45B8-9A15-5B61C00ED08B" )]
    [Rock.SystemGuid.BlockTypeGuid( "784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B" )]
    public class TagsByLetter : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowEntityType = "ShowEntityType";
            public const string EntityType = "EntityType";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PersonPreferenceKey
        {
            public const string ActiveTab = "active-tab";
            public const string SelectedEntityType = "selected-entity-type";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<TagsByLetterBag, TagsByLetterOptionsBag>();

            box.Options = new TagsByLetterOptionsBag
            {
                IsEntityTypePickerVisible = GetAttributeValue( AttributeKey.ShowEntityType ).AsBoolean(),
                DefaultEntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull()
            };

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Builds the tag data bag by querying tags with the specified filters,
        /// performing authorization checks, and grouping results by first letter.
        /// </summary>
        /// <param name="ownershipType">The ownership filter for the tag query.</param>
        /// <param name="entityTypeGuid">The optional entity type unique identifier to filter tags by.</param>
        /// <param name="includeInactive">If <c>true</c>, inactive tags are included in the results.</param>
        /// <returns>A <see cref="TagsByLetterBag"/> containing tags grouped by their first letter.</returns>
        private TagsByLetterBag BuildTagsByLetterBag( TagOwnershipType ownershipType, Guid? entityTypeGuid, bool includeInactive )
        {
            var rockContext = RockContext;
            var tagQry = new TagService( rockContext ).Queryable();

            // Filter by entity type if specified.
            if ( entityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                if ( entityType != null )
                {
                    var entityTypeId = entityType.Id;
                    tagQry = tagQry.Where( t => t.EntityTypeId.HasValue && t.EntityTypeId.Value == entityTypeId );
                }
            }

            // Filter by active status.
            if ( !includeInactive )
            {
                tagQry = tagQry.Where( t => t.IsActive );
            }

            var isPersonal = ownershipType == TagOwnershipType.Personal;
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( isPersonal )
            {
                // Personal tags: filter to tags owned by the current person.
                var personId = currentPersonId ?? 0;
                tagQry = tagQry.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.PersonId == personId );
            }
            else
            {
                // Organizational tags: filter to tags with no owner.
                tagQry = tagQry.Where( t => t.OwnerPersonAlias == null );
            }

            // Project to include the tagged item count and order by name.
            var tags = tagQry
                .Select( t => new
                {
                    Tag = t,
                    Count = t.TaggedItems.Count()
                } )
                .OrderBy( t => t.Tag.Name )
                .ToList();

            // Initialize the alphabet dictionary with all 28 keys (A-Z, #, *).
            var tagAlphabet = new Dictionary<string, List<TagItemBag>>();
            for ( var c = 'A'; c <= 'Z'; c++ )
            {
                tagAlphabet.Add( c.ToString(), new List<TagItemBag>() );
            }

            tagAlphabet.Add( "#", new List<TagItemBag>() );
            tagAlphabet.Add( "*", new List<TagItemBag>() );

            var currentPerson = RequestContext.CurrentPerson;

            foreach ( var tagInfo in tags )
            {
                /*
                    3/27/2026 - MSE

                    Organizational tags require an in-memory authorization check since
                    IsAuthorized cannot be translated to SQL. Personal tags skip this
                    check because the query already filters to the current person's tags.

                    Reason: Security model requires per-entity VIEW authorization for unowned tags.
                */
                if ( !isPersonal )
                {
                    if ( !tagInfo.Tag.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        continue;
                    }
                }

                var tagItem = new TagItemBag
                {
                    IdKey = tagInfo.Tag.IdKey,
                    Name = tagInfo.Tag.Name,
                    Count = tagInfo.Count
                };

                var firstChar = tagItem.Name.Substring( 0, 1 ).ToUpper()[0];
                string key;

                if ( char.IsLetter( firstChar ) )
                {
                    key = firstChar.ToString();
                }
                else if ( char.IsDigit( firstChar ) )
                {
                    key = "#";
                }
                else
                {
                    key = "*";
                }

                tagAlphabet[key].Add( tagItem );
            }

            return new TagsByLetterBag
            {
                TagsByLetter = tagAlphabet
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "TagId", "((Key))" )
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets tags filtered by ownership type, entity type, and active status,
        /// grouped by first letter.
        /// </summary>
        /// <param name="request">The request bag containing the filter parameters.</param>
        /// <returns>A <see cref="TagsByLetterBag"/> containing the filtered and grouped tags.</returns>
        [BlockAction]
        public BlockActionResult GetTags( GetTagsRequestBag request )
        {
            var bag = BuildTagsByLetterBag( request.OwnerType, request.EntityTypeGuid, request.IncludeInactive );
            return ActionOk( bag );
        }

        #endregion Block Actions
    }
}
