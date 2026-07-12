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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.AttributeValues;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Displays and edits the values of a configured set of person attribute
    /// categories on the Person Profile page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Attribute Values" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows for editing the value(s) of a set of attributes for person." )]
    [IconCssClass( "ti ti-list-details" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [AttributeCategoryField(
        "Category",
        Key = AttributeKey.Category,
        AllowMultiple = true,
        Description = "The Attribute Categories to display attributes from",
        EntityTypeName = "Rock.Model.Person",
        IsRequired = true,
        Order = 0 )]

    [TextField(
        "Attribute Order",
        Key = AttributeKey.AttributeOrder,
        Description = "The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.",
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Use Abbreviated Name",
        Key = AttributeKey.UseAbbreviatedName,
        Description = "Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 2 )]

    [TextField(
        "Block Title",
        Key = AttributeKey.BlockTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "",
        Order = 3 )]

    [TextField(
        "Block Icon",
        Key = AttributeKey.BlockIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "",
        Order = 4 )]

    [BooleanField(
        "Show Category Names as Separators",
        Key = AttributeKey.ShowCategoryNamesAsSeparators,
        Description = "If enabled, attributes will be grouped by category and will include the category name as a heading separator.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 5 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "3AAC5269-7D19-4997-A896-FD8492B2F6D7" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "416F5159-C54F-41E7-914F-17BE9BC4C072" )]
    [Rock.SystemGuid.BlockTypeGuid( "D70A59DC-16BE-43BE-9880-59598FA7A94C" )]
    public class AttributeValues : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Category = "Category";
            public const string AttributeOrder = "AttributeOrder";
            public const string UseAbbreviatedName = "UseAbbreviatedName";
            public const string BlockTitle = "BlockTitle";
            public const string BlockIcon = "BlockIcon";
            public const string ShowCategoryNamesAsSeparators = "ShowCategoryNamesasSeparators";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<AttributeValuesViewBag, AttributeValuesOptionsBag>();
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                box.Options.IsVisible = false;

                return box;
            }

            var orderedAttributes = GetOrderedAttributes( person );
            var isGroupedByCategory = GetAttributeValue( AttributeKey.ShowCategoryNamesAsSeparators ).AsBoolean();

            box.Options.IsVisible = true;
            box.Options.IsGroupedByCategory = isGroupedByCategory;
            box.Options.IsEditAllowed = orderedAttributes.Any( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
            box.Options.IsOrderAllowed = orderedAttributes.Any()
                && !isGroupedByCategory
                && BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            box.Bag = BuildViewBag( person, orderedAttributes );

            SetPanelTitleAndIcon( box.Options );

            return box;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the data needed to render the edit view of the displayed
        /// attributes.
        /// </summary>
        /// <returns>An <see cref="AttributeValuesEditBag"/> describing the fields to edit.</returns>
        [BlockAction]
        public BlockActionResult GetEditAttributes()
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            var orderedAttributes = GetOrderedAttributes( person );

            return ActionOk( BuildEditBag( person, orderedAttributes ) );
        }

        /// <summary>
        /// Saves the edited attribute values for the displayed attributes the
        /// current person is authorized to edit.
        /// </summary>
        /// <param name="attributeValues">The edited attribute values, keyed by attribute key.</param>
        /// <returns>The refreshed view of the displayed attributes.</returns>
        [BlockAction]
        public BlockActionResult SaveAttributeValues( Dictionary<string, string> attributeValues )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            var orderedAttributes = GetOrderedAttributes( person );

            // Only the attributes the person is authorized to edit may be saved.
            var editableKeys = orderedAttributes
                .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                .Select( a => a.Key )
                .ToHashSet();

            RockContext.WrapTransaction( () =>
            {
                person.SetPublicAttributeValues( attributeValues ?? new Dictionary<string, string>(), RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => editableKeys.Contains( a.Key ) );
                person.SaveAttributeValues( RockContext );
            } );

            return ActionOk( BuildViewBag( person, orderedAttributes ) );
        }

        /// <summary>
        /// Saves a new display order for the displayed attributes into the
        /// block's own Attribute Order setting.
        /// </summary>
        /// <param name="orderedAttributeGuids">The attribute unique identifiers in their new order.</param>
        /// <returns>The refreshed view of the displayed attributes.</returns>
        [BlockAction]
        public BlockActionResult SaveOrder( List<string> orderedAttributeGuids )
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
            {
                return ActionForbidden( $"{currentPerson?.FullName} is not authorized to reorder these attributes." );
            }

            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            var orderedIds = ( orderedAttributeGuids ?? new List<string>() )
                .Select( guidString => guidString.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => AttributeCache.Get( guid.Value ) )
                .Where( a => a != null )
                .Select( a => a.Id )
                .ToList();

            var block = new BlockService( RockContext ).Get( BlockId );
            block.LoadAttributes( RockContext );
            block.SetAttributeValue( AttributeKey.AttributeOrder, string.Join( "|", orderedIds ) );
            block.SaveAttributeValues( RockContext );

            // Order using the ids just received rather than re-reading the
            // setting, so the refreshed view does not depend on cache timing.
            return ActionOk( BuildViewBag( person, GetOrderedAttributes( person, orderedIds ) ) );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the person whose attributes are being displayed, resolving from
        /// the page context first and falling back to the page parameter.
        /// </summary>
        /// <returns>The <see cref="Person"/> in context, or <c>null</c> if none could be determined.</returns>
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
        /// Gets the unique identifiers of the attribute categories configured
        /// on the block.
        /// </summary>
        /// <returns>The list of configured category unique identifiers.</returns>
        private List<Guid> GetCategoryGuids()
        {
            return GetAttributeValue( AttributeKey.Category ).SplitDelimitedValues( false ).AsGuidList();
        }

        /// <summary>
        /// Gets the configured attribute categories in their effective display
        /// order: by category order, then by category name so tied orders resolve
        /// alphabetically for a stable, predictable sequence.
        /// </summary>
        /// <returns>The ordered list of configured <see cref="CategoryCache"/> objects.</returns>
        private List<CategoryCache> GetOrderedCategories()
        {
            return GetCategoryGuids()
                .Distinct()
                .Select( guid => CategoryCache.Get( guid ) )
                .Where( c => c != null )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .ToList();
        }

        /// <summary>
        /// Determines whether an attribute has nothing to display. The legacy
        /// block hid an attribute (and its category separator) when its value
        /// formatted to whitespace HTML via FormatValueAsHtml. Some field types
        /// return a non-empty public value for an empty stored value, so
        /// emptiness is judged from the field type's HTML value - the
        /// System.Web-free analog of the legacy check - rather than the public
        /// value.
        /// </summary>
        private bool HasNoDisplayValue( Person person, AttributeCache attribute )
        {
            var rawValue = person.GetAttributeValue( attribute.Key );

            return attribute.FieldType.Field.GetHtmlValue( rawValue, attribute.ConfigurationValues ).IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets the person's attributes in the configured categories that the
        /// current person is authorized to view, in their effective display
        /// order: attributes listed in the order override first (in override
        /// order), then the remaining attributes by their own order and name.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed. The person's attributes are loaded by this method.</param>
        /// <param name="orderOverrideIds">The attribute identifier order to apply, or <c>null</c> to use the block's Attribute Order setting.</param>
        /// <returns>The ordered list of displayed <see cref="AttributeCache"/> objects.</returns>
        private List<AttributeCache> GetOrderedAttributes( Person person, List<int> orderOverrideIds = null )
        {
            var categoryIds = GetCategoryGuids()
                .Select( guid => CategoryCache.Get( guid ) )
                .Where( c => c != null )
                .Select( c => c.Id )
                .ToList();

            person.LoadAttributes( RockContext );

            if ( !categoryIds.Any() )
            {
                return new List<AttributeCache>();
            }

            var candidates = person.Attributes.Values
                .Where( a => a.IsActive
                    && a.CategoryIds.Any( id => categoryIds.Contains( id ) )
                    && a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            orderOverrideIds = orderOverrideIds ?? GetAttributeValue( AttributeKey.AttributeOrder )
                .SplitDelimitedValues()
                .Select( idString => idString.AsInteger() )
                .ToList();

            var ordered = orderOverrideIds
                .Distinct()
                .Select( id => candidates.FirstOrDefault( a => a.Id == id ) )
                .Where( a => a != null )
                .ToList();

            ordered.AddRange( candidates.Where( a => !orderOverrideIds.Contains( a.Id ) ) );

            return ordered;
        }

        /// <summary>
        /// Builds the read-only view of the displayed attributes.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <param name="orderedAttributes">The displayed attributes in their effective display order.</param>
        /// <returns>An <see cref="AttributeValuesViewBag"/> for rendering the view panel.</returns>
        private AttributeValuesViewBag BuildViewBag( Person person, List<AttributeCache> orderedAttributes )
        {
            var orderedKeys = orderedAttributes.Select( a => a.Key ).ToHashSet();
            var useAbbreviatedName = GetAttributeValue( AttributeKey.UseAbbreviatedName ).AsBoolean();
            var isGroupedByCategory = GetAttributeValue( AttributeKey.ShowCategoryNamesAsSeparators ).AsBoolean();

            var categoryRankByGuid = GetOrderedCategories()
                .Select( ( category, rank ) => new { category.Guid, rank } )
                .ToDictionary( x => x.Guid, x => x.rank );

            var viewBag = new AttributeValuesViewBag();

            viewBag.LoadAttributesAndValuesForPublicView( person, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => orderedKeys.Contains( a.Key ) );

            for ( var index = 0; index < orderedAttributes.Count; index++ )
            {
                if ( !viewBag.Attributes.TryGetValue( orderedAttributes[index].Key, out var attributeBag ) )
                {
                    continue;
                }

                // The effective display order is the index in the ordered list.
                attributeBag.Order = index;

                if ( useAbbreviatedName )
                {
                    attributeBag.Name = orderedAttributes[index].AbbreviatedName;
                }

                if ( HasNoDisplayValue( person, orderedAttributes[index] ) )
                {
                    viewBag.AttributeValues[orderedAttributes[index].Key] = string.Empty;
                }

                if ( isGroupedByCategory )
                {
                    attributeBag.Categories = attributeBag.Categories
                        .Where( c => categoryRankByGuid.ContainsKey( c.Guid ) )
                        .Select( c =>
                        {
                            c.Order = categoryRankByGuid[c.Guid];
                            return c;
                        } )
                        .ToList();
                }
                else
                {
                    attributeBag.Categories = new List<PublicAttributeCategoryBag>();
                }
            }

            return viewBag;
        }

        /// <summary>
        /// Builds the edit view of the displayed attributes. Attributes the
        /// person can edit are returned with edit metadata; attributes the
        /// person can only view are returned with view metadata and included
        /// only when they have a value.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <param name="orderedAttributes">The displayed attributes in their effective display order.</param>
        /// <returns>An <see cref="AttributeValuesEditBag"/> for rendering the edit panel.</returns>
        private AttributeValuesEditBag BuildEditBag( Person person, List<AttributeCache> orderedAttributes )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var useAbbreviatedName = GetAttributeValue( AttributeKey.UseAbbreviatedName ).AsBoolean();

            var editableKeys = orderedAttributes
                .Where( a => a.IsAuthorized( Authorization.EDIT, currentPerson ) )
                .Select( a => a.Key )
                .ToHashSet();
            var viewOnlyKeys = orderedAttributes
                .Where( a => !editableKeys.Contains( a.Key ) )
                .Select( a => a.Key )
                .ToHashSet();

            var attributes = person.GetPublicAttributesForEdit( currentPerson, enforceSecurity: true, attributeFilter: a => editableKeys.Contains( a.Key ) );
            var attributeValues = person.GetPublicAttributeValuesForEdit( currentPerson, enforceSecurity: true, attributeFilter: a => editableKeys.Contains( a.Key ) );

            var viewAttributes = person.GetPublicAttributesForView( currentPerson, enforceSecurity: true, attributeFilter: a => viewOnlyKeys.Contains( a.Key ) );
            var viewValues = person.GetPublicAttributeValuesForView( currentPerson, enforceSecurity: true, attributeFilter: a => viewOnlyKeys.Contains( a.Key ) );

            foreach ( var attribute in orderedAttributes )
            {
                if ( viewValues.ContainsKey( attribute.Key ) && HasNoDisplayValue( person, attribute ) )
                {
                    viewValues[attribute.Key] = string.Empty;
                }
            }

            // Merge the view-only metadata and values into the combined dictionaries.
            foreach ( var kvp in viewAttributes )
            {
                attributes[kvp.Key] = kvp.Value;
            }

            foreach ( var kvp in viewValues )
            {
                attributeValues[kvp.Key] = kvp.Value;
            }

            if ( useAbbreviatedName )
            {
                foreach ( var attribute in orderedAttributes )
                {
                    if ( attributes.TryGetValue( attribute.Key, out var attributeBag ) )
                    {
                        attributeBag.Name = attribute.AbbreviatedName;
                    }
                }
            }

            return new AttributeValuesEditBag
            {
                FieldGroups = BuildFieldGroups( orderedAttributes, editableKeys, attributes, viewValues ),
                Attributes = attributes,
                AttributeValues = attributeValues,
                SecurityGrantToken = GetSecurityGrantToken( person )
            };
        }

        /// <summary>
        /// Builds the ordered groups of fields to render in edit mode. A single
        /// unnamed group is used unless the block is grouping by category, in
        /// which case one group is built per configured category with the
        /// category name as its heading.
        /// </summary>
        /// <param name="orderedAttributes">The displayed attributes in their effective display order.</param>
        /// <param name="editableKeys">The keys of the attributes the current person may edit.</param>
        /// <param name="attributes">The merged public attribute metadata, keyed by attribute key.</param>
        /// <param name="viewValues">The formatted values of the view-only attributes, keyed by attribute key.</param>
        /// <returns>The ordered list of field groups.</returns>
        private List<AttributeValuesFieldGroupBag> BuildFieldGroups( List<AttributeCache> orderedAttributes, HashSet<string> editableKeys, Dictionary<string, PublicAttributeBag> attributes, Dictionary<string, string> viewValues )
        {
            // Editable fields always render; view-only fields render only when
            // they have a value to show.
            bool IsFieldIncluded( AttributeCache attribute )
            {
                if ( editableKeys.Contains( attribute.Key ) )
                {
                    return attributes.ContainsKey( attribute.Key );
                }

                return viewValues.TryGetValue( attribute.Key, out var viewValue ) && viewValue.IsNotNullOrWhiteSpace();
            }

            AttributeValuesFieldBag ToFieldBag( AttributeCache attribute )
            {
                return new AttributeValuesFieldBag
                {
                    Key = attribute.Key,
                    CanEdit = editableKeys.Contains( attribute.Key )
                };
            }

            if ( !GetAttributeValue( AttributeKey.ShowCategoryNamesAsSeparators ).AsBoolean() )
            {
                return new List<AttributeValuesFieldGroupBag>
                {
                    new AttributeValuesFieldGroupBag
                    {
                        Fields = orderedAttributes.Where( IsFieldIncluded ).Select( ToFieldBag ).ToList()
                    }
                };
            }

            var categories = GetOrderedCategories();

            // An attribute in multiple configured categories renders in its
            // first matching category only.
            var groups = new List<AttributeValuesFieldGroupBag>();
            var assignedAttributeIds = new HashSet<int>();

            foreach ( var category in categories )
            {
                var categoryAttributes = orderedAttributes
                    .Where( a => a.CategoryIds.Contains( category.Id )
                        && !assignedAttributeIds.Contains( a.Id )
                        && IsFieldIncluded( a ) )
                    .ToList();

                if ( !categoryAttributes.Any() )
                {
                    continue;
                }

                categoryAttributes.ForEach( a => assignedAttributeIds.Add( a.Id ) );

                groups.Add( new AttributeValuesFieldGroupBag
                {
                    CategoryName = category.Name,
                    Fields = categoryAttributes.Select( ToFieldBag ).ToList()
                } );
            }

            return groups;
        }

        /// <summary>
        /// Sets the panel title and icon from the block settings, falling back
        /// to the configured category's name and icon when a single category is
        /// configured.
        /// </summary>
        /// <param name="options">The block options to update.</param>
        private void SetPanelTitleAndIcon( AttributeValuesOptionsBag options )
        {
            var categoryGuids = GetCategoryGuids();
            var category = categoryGuids.Count == 1 ? CategoryCache.Get( categoryGuids[0] ) : null;

            var title = GetAttributeValue( AttributeKey.BlockTitle );
            options.Title = title.IsNotNullOrWhiteSpace() ? title : ( category?.Name ?? "Attribute Values" );

            var icon = GetAttributeValue( AttributeKey.BlockIcon );
            options.TitleIconCssClass = icon.IsNotNullOrWhiteSpace() ? icon : category?.IconCssClass;
        }

        /// <summary>
        /// Builds a security grant token that authorizes the current person for
        /// the person's attributes while editing.
        /// </summary>
        /// <param name="person">The person whose attributes are loaded.</param>
        /// <returns>The security grant token.</returns>
        private string GetSecurityGrantToken( Person person )
        {
            return new Rock.Security.SecurityGrant()
                .AddRulesForAttributes( person, RequestContext.CurrentPerson )
                .ToToken();
        }

        #endregion
    }
}
