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
using System.Net;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Allows for editing the value(s) of a set of attributes for person.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Attribute Values" )]
    [Category( "Obsidian > CRM" )]
    [Description( "Allows for editing the value(s) of a set of attributes for person." )]
    [IconCssClass( "fa fa-user" )]

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

    [TextField( "Block Title",
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

    public class AttributeValues : RockObsidianBlockType
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string Category = "Category";
            public const string AttributeOrder = "AttributeOrder";
            public const string UseAbbreviatedName = "UseAbbreviatedName";
            public const string BlockTitle = "BlockTitle";
            public const string BlockIcon = "BlockIcon";
            public const string ShowCategoryNamesAsSeparators = "ShowCategoryNamesasSeparators";
        }

        #endregion Attribute Keys

        #region IRockObsidianBlockType Implementation

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            return new
            {
                BlockIconCssClass = GetBlockIconCssClass(),
                BlockTitle = GetBlockTitle(),
                ShowCategoryNamesAsSeparators = GetAttributeValue( AttributeKey.ShowCategoryNamesAsSeparators ).AsBoolean(),
                UseAbbreviatedNames = GetAttributeValue( AttributeKey.UseAbbreviatedName ).AsBoolean(),
                CategoryGuids = GetCategoryGuids(),
                Attributes = GetClientAttributeValues( RequestContext.GetContextEntity<Person>() )
            };
        }

        #endregion

        #region Actions

        /// <summary>
        /// Gets the attribute values for editing.
        /// </summary>
        /// <returns>A collection of attribute values that can be edited.</returns>
        [BlockAction]
        public BlockActionResult GetAttributeValuesForEdit()
        {
            var categoryGuids = GetCategoryGuids();
            var person = RequestContext.GetContextEntity<Person>();

            var attributeValues = person.GetClientEditableAttributeValues( RequestContext.CurrentPerson )
                .Where( av => categoryGuids.Count == 0 || categoryGuids.Any( g => av.Categories.FirstOrDefault( c => c.Guid == g ) != null ) )
                .ToList();

            return ActionOk( attributeValues );
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="attributeValues">The attribute values.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveAttributeValues( Dictionary<string, string> keyValueMap )
        {
            using ( var rockContext = new RockContext() )
            {
                var currentPerson = RequestContext.CurrentPerson;

                var personService = new PersonService( rockContext );
                var person = personService.Get( RequestContext.GetContextEntity<Person>()?.Id ?? 0 );

                if ( person == null || currentPerson == null )
                {
                    return ActionNotFound();
                }

                person.LoadAttributes();
                var validKeys = GetCategoryAttributeKeys( person );

                foreach ( var kvp in keyValueMap )
                {
                    if ( validKeys.Contains( kvp.Key ) )
                    {
                        person.SetClientAttributeValue( kvp.Key, kvp.Value, currentPerson );
                    }
                }

                person.SaveAttributeValues( rockContext );

                return ActionOk( GetClientAttributeValues( person ) );
            }
        }

        /// <summary>
        /// Gets the attributes keys that are valid given our category configuration.
        /// </summary>
        /// <returns></returns>
        private List<string> GetCategoryAttributeKeys( IHasAttributes entity )
        {
            var categories = GetCategoryGuids();

            return entity.Attributes.Values
                .Where( a => a.Categories.Any( c => categories.Contains( c.Guid ) ) )
                .Select( a => a.Key )
                .ToList();
        }

        #endregion Actions

        #region Data Access

        /// <summary>
        /// Gets the block icon CSS class.
        /// </summary>
        /// <returns></returns>
        private string GetBlockIconCssClass()
        {
            var icon = GetAttributeValue( AttributeKey.BlockIcon );

            if (!icon.IsNullOrWhiteSpace())
            {
                return icon;
            }

            var category = GetSoloCategory();

            if ( category != null )
            {
                return category.IconCssClass;
            }

            return "fa fa-user";
        }

        /// <summary>
        /// Gets the block title.
        /// </summary>
        /// <returns></returns>
        private string GetBlockTitle()
        {
            var blockTitle = GetAttributeValue( AttributeKey.BlockTitle );

            if ( !blockTitle.IsNullOrWhiteSpace() )
            {
                return blockTitle;
            }

            var category = GetSoloCategory();

            if ( category != null )
            {
                return category.Name;
            }

            return "Attribute Values";
        }

        /// <summary>
        /// Gets the solo category.
        /// </summary>
        /// <returns></returns>
        private CategoryCache GetSoloCategory()
        {
            var categoryGuids = GetCategoryGuids();
            return categoryGuids.Count == 1 ? CategoryCache.Get( categoryGuids[0] ) : null;
        }

        /// <summary>
        /// Gets the attribute category Guids.
        /// </summary>
        /// <returns></returns>
        private List<Guid> GetCategoryGuids()
        {
            return GetAttributeValueAsFieldType<List<Guid>>( AttributeKey.Category );
        }

        /// <summary>
        /// Gets the client attribute values.
        /// </summary>
        /// <returns></returns>
        private List<ClientAttributeValueViewModel> GetClientAttributeValues( IHasAttributes entity )
        {
            var categoryGuids = GetCategoryGuids();

            return entity.GetClientAttributeValues( RequestContext.CurrentPerson )
                .Where( av => categoryGuids.Count == 0 || categoryGuids.Any( g => av.Categories.FirstOrDefault( c => c.Guid == g ) != null ) )
                .ToList();
        }

        #endregion Data Access
    }
}
