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
using Rock.ViewModels.Blocks.Crm.PersonDetail.KeyAttributes;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Displays and manages a person's bookmarked (key) attributes on the
    /// Person Profile page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Person Key Attributes" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person key attributes (Person Detail Page)." )]
    [IconCssClass( "ti ti-bookmark" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "45A9EF77-4048-4097-B7AD-2F610EC96755" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "633FC7C7-8DE4-42DA-AF06-6884C292281F" )]
    [Rock.SystemGuid.BlockTypeGuid( "23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178" )]
    public class KeyAttributes : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        private static class PersonPreferenceKey
        {
            /// <summary>
            /// The block-person preference that stores the bookmarked attributes
            /// as a comma-delimited list of attribute identifiers, in display
            /// order.
            /// </summary>
            public const string SelectedAttributes = "selected-attributes";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return new KeyAttributesInitializationBox
                {
                    IsVisible = false
                };
            }

            return new KeyAttributesInitializationBox
            {
                IsVisible = true,
                View = BuildViewBag( person )
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the data needed to render the edit view of the bookmarked
        /// attributes.
        /// </summary>
        /// <returns>A <see cref="KeyAttributesEditBag"/> describing the fields to edit.</returns>
        [BlockAction]
        public BlockActionResult GetEditAttributes()
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            return ActionOk( BuildEditBag( person ) );
        }

        /// <summary>
        /// Saves the edited attribute values for the bookmarked attributes the
        /// current person is authorized to edit.
        /// </summary>
        /// <param name="attributeValues">The edited attribute values, keyed by attribute key.</param>
        /// <returns>The refreshed view of the bookmarked attributes.</returns>
        [BlockAction]
        public BlockActionResult SaveAttributeValues( Dictionary<string, string> attributeValues )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            // Only the attributes the person is authorized to edit may be saved.
            var editableKeys = GetBookmarkedAttributes( person )
                .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                .Select( a => a.Key )
                .ToHashSet();

            person.LoadAttributes( RockContext );

            RockContext.WrapTransaction( () =>
            {
                person.SetPublicAttributeValues( attributeValues ?? new Dictionary<string, string>(), RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => editableKeys.Contains( a.Key ) );
                person.SaveAttributeValues( RockContext );
            } );

            return ActionOk( BuildViewBag( person ) );
        }

        /// <summary>
        /// Saves a new display order for the bookmarked attributes.
        /// </summary>
        /// <param name="orderedAttributeGuids">The bookmarked attribute unique identifiers in their new order.</param>
        /// <returns>The refreshed view of the bookmarked attributes.</returns>
        [BlockAction]
        public BlockActionResult SaveOrder( List<string> orderedAttributeGuids )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            SavePreference( orderedAttributeGuids );

            return ActionOk( BuildViewBag( person ) );
        }

        /// <summary>
        /// Gets the data needed to drive the configure dialog.
        /// </summary>
        /// <returns>A <see cref="KeyAttributesConfigurationBag"/> describing the selectable attributes.</returns>
        [BlockAction]
        public BlockActionResult GetConfiguration()
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            return ActionOk( BuildConfigurationBag( person ) );
        }

        /// <summary>
        /// Saves the set of bookmarked attributes selected in the configure dialog.
        /// </summary>
        /// <param name="selectedAttributeGuids">The selected attribute unique identifiers, in display order.</param>
        /// <returns>The refreshed view of the bookmarked attributes.</returns>
        [BlockAction]
        public BlockActionResult SaveConfiguration( List<string> selectedAttributeGuids )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "No person is available." );
            }

            SavePreference( selectedAttributeGuids );

            return ActionOk( BuildViewBag( person ) );
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
        /// Gets the current person's bookmarked attributes, in their saved
        /// order, filtered to those that still exist and that the current
        /// person is authorized to view.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <returns>The ordered list of bookmarked <see cref="AttributeCache"/> objects.</returns>
        private List<AttributeCache> GetBookmarkedAttributes( Person person )
        {
            var selectedValue = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.SelectedAttributes );

            return selectedValue.SplitDelimitedValues()
                .Select( idString => idString.AsIntegerOrNull() )
                .Where( id => id.HasValue )
                .Select( id => AttributeCache.Get( id.Value ) )
                .Where( a => a != null && a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Builds the read-only view of the bookmarked attributes.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <returns>A <see cref="KeyAttributesViewBag"/> for rendering the view panel.</returns>
        private KeyAttributesViewBag BuildViewBag( Person person )
        {
            var bookmarked = GetBookmarkedAttributes( person );
            var bookmarkedKeys = bookmarked.Select( a => a.Key ).ToHashSet();

            person.LoadAttributes( RockContext );

            var viewBag = new KeyAttributesViewBag
            {
                HasBookmarkedAttributes = bookmarked.Count > 0
            };

            viewBag.LoadAttributesAndValuesForPublicView( person, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => bookmarkedKeys.Contains( a.Key ) );

            // Force the flat, custom bookmarked order by setting each attribute's
            // order to its bookmarked index and clearing its categories so the
            // control renders a single ungrouped list.
            for ( var index = 0; index < bookmarked.Count; index++ )
            {
                if ( viewBag.Attributes.TryGetValue( bookmarked[index].Key, out var attributeBag ) )
                {
                    attributeBag.Order = index;
                    attributeBag.Categories = new List<PublicAttributeCategoryBag>();
                }
            }

            return viewBag;
        }

        /// <summary>
        /// Builds the edit view of the bookmarked attributes. Attributes the
        /// person can edit are returned with edit metadata; attributes the
        /// person can only view are returned with view metadata and included
        /// only when they have a value.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <returns>A <see cref="KeyAttributesEditBag"/> for rendering the edit panel.</returns>
        private KeyAttributesEditBag BuildEditBag( Person person )
        {
            var bookmarked = GetBookmarkedAttributes( person );

            person.LoadAttributes( RockContext );

            var editableKeys = bookmarked
                .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                .Select( a => a.Key )
                .ToHashSet();
            var viewOnlyKeys = bookmarked
                .Where( a => !editableKeys.Contains( a.Key ) )
                .Select( a => a.Key )
                .ToHashSet();

            var attributes = person.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => editableKeys.Contains( a.Key ) );
            var attributeValues = person.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => editableKeys.Contains( a.Key ) );

            var viewAttributes = person.GetPublicAttributesForView( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => viewOnlyKeys.Contains( a.Key ) );
            var viewValues = person.GetPublicAttributeValuesForView( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => viewOnlyKeys.Contains( a.Key ) );

            // Merge the view-only metadata and values into the combined dictionaries.
            foreach ( var kvp in viewAttributes )
            {
                attributes[kvp.Key] = kvp.Value;
            }

            foreach ( var kvp in viewValues )
            {
                attributeValues[kvp.Key] = kvp.Value;
            }

            // Build the ordered list of fields. Editable fields always appear;
            // view-only fields appear only when they have a value.
            var fields = new List<KeyAttributeFieldBag>();

            foreach ( var attribute in bookmarked )
            {
                var canEdit = editableKeys.Contains( attribute.Key );

                if ( canEdit )
                {
                    if ( !attributes.ContainsKey( attribute.Key ) )
                    {
                        continue;
                    }
                }
                else
                {
                    var hasValue = viewValues.TryGetValue( attribute.Key, out var viewValue ) && viewValue.IsNotNullOrWhiteSpace();

                    if ( !hasValue )
                    {
                        continue;
                    }
                }

                fields.Add( new KeyAttributeFieldBag
                {
                    Key = attribute.Key,
                    CanEdit = canEdit
                } );
            }

            return new KeyAttributesEditBag
            {
                Fields = fields,
                Attributes = attributes,
                AttributeValues = attributeValues,
                SecurityGrantToken = GetSecurityGrantToken( person )
            };
        }

        /// <summary>
        /// Builds the data that drives the configure dialog: the view-authorized
        /// person attributes grouped by their view-authorized categories, with
        /// an "Uncategorized" bucket for attributes that have no category.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <returns>A <see cref="KeyAttributesConfigurationBag"/> for the configure dialog.</returns>
        private KeyAttributesConfigurationBag BuildConfigurationBag( Person person )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var uncategorizedKey = Guid.Empty.ToString();

            var personAttributes = AttributeCache.GetPersonAttributes()
                .Where( a => a != null && a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            var attributesByCategory = new Dictionary<string, List<ListItemBag>>();
            var categoryNames = new Dictionary<string, string>();
            var categoryOrder = new Dictionary<string, int>();

            void EnsureCategory( string key, string name, int order )
            {
                if ( attributesByCategory.ContainsKey( key ) )
                {
                    return;
                }

                attributesByCategory[key] = new List<ListItemBag>();
                categoryNames[key] = name;
                categoryOrder[key] = order;
            }

            foreach ( var attribute in personAttributes )
            {
                var attributeItem = new ListItemBag { Value = attribute.Guid.ToString(), Text = attribute.Name };

                var authorizedCategories = attribute.Categories
                    .Where( c => c.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    .ToList();

                if ( authorizedCategories.Count == 0 )
                {
                    // int.MaxValue keeps the Uncategorized bucket sorted last.
                    EnsureCategory( uncategorizedKey, "Uncategorized", int.MaxValue );
                    attributesByCategory[uncategorizedKey].Add( attributeItem );
                    continue;
                }

                foreach ( var category in authorizedCategories )
                {
                    var categoryKey = category.Guid.ToString();

                    EnsureCategory( categoryKey, category.Name, category.Order );
                    attributesByCategory[categoryKey].Add( attributeItem );
                }
            }

            var categories = attributesByCategory.Keys
                .OrderBy( key => categoryOrder[key] )
                .ThenBy( key => categoryNames[key] )
                .Select( key => new ListItemBag { Value = key, Text = categoryNames[key] } )
                .ToList();

            return new KeyAttributesConfigurationBag
            {
                Categories = categories,
                AttributesByCategory = attributesByCategory,
                SelectedAttributeGuids = GetBookmarkedAttributes( person ).Select( a => a.Guid.ToString() ).ToList()
            };
        }

        /// <summary>
        /// Persists the bookmarked attribute selection/order to the block-person
        /// preference. The client works in attribute unique identifiers; these
        /// are stored as the legacy comma-delimited list of attribute identifiers
        /// so existing bookmarks remain compatible.
        /// </summary>
        /// <param name="orderedAttributeGuids">The attribute unique identifiers to store, in order.</param>
        private void SavePreference( List<string> orderedAttributeGuids )
        {
            var ids = ( orderedAttributeGuids ?? new List<string>() )
                .Select( guidString => guidString.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => AttributeCache.Get( guid.Value ) )
                .Where( a => a != null )
                .Select( a => a.Id.ToString() );

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( PersonPreferenceKey.SelectedAttributes, string.Join( ",", ids ) );
            preferences.Save();
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
