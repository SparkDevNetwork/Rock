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
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Obsidian.Blocks.Crm
{
    /// <summary>
    /// Allows for editing the value(s) of a set of attributes for person.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

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

    public class AttributeValues : ObsidianBlockType
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

        #region IObsidianBlockType Implementation

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetConfigurationValues()
        {
            return new
            {
                BlockIconCssClass = GetBlockIconCssClass(),
                BlockTitle = GetBlockTitle(),
                ShowCategoryNamesAsSeparators = GetAttributeValue( AttributeKey.ShowCategoryNamesAsSeparators ).AsBoolean()
            };
        }

        #endregion

        #region Actions

        /// <summary>
        /// Get data based on the configured category setting.
        /// </summary>
        [BlockAction( "GetAttributeDataList" )]
        public BlockActionResult GetAttributeDataList()
        {
            var currentPerson = GetCurrentPerson();
            var categories = GetCategoryGuids();
            var attributeDataList = new List<AttributeData>();

            if ( !categories.Any() || currentPerson == null )
            {
                return new BlockActionResult( HttpStatusCode.OK, attributeDataList );
            }

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var orderOverride = GetAttributeValue( AttributeKey.AttributeOrder ).SplitDelimitedValues().AsIntegerList();

            var orderedAttributeList = attributeService.Queryable()
                .Where( a => a.IsActive && a.Categories.Any( c => categories.Contains( c.Guid ) ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList();

            foreach ( var attributeId in orderOverride )
            {
                var attribute = orderedAttributeList.FirstOrDefault( a => a.Id == attributeId );

                if ( attribute != null && attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    attributeDataList.Add( GetAttributeData( attribute ) );
                }
            }

            foreach ( var attribute in orderedAttributeList.Where( a => !orderOverride.Contains( a.Id ) ) )
            {
                if ( attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    attributeDataList.Add( GetAttributeData( attribute ) );
                }
            }

            attributeDataList = attributeDataList.Where( a => a != null ).ToList();
            return new BlockActionResult( HttpStatusCode.OK, attributeDataList );
        }

        /// <summary>
        /// Gets the attribute data.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        private AttributeData GetAttributeData( AttributeCache attribute )
        {
            var useAbbreviatedName = GetAttributeValue( AttributeKey.UseAbbreviatedName ).AsBoolean();
            var contextPerson = GetContextPerson();
            var attributeValue = contextPerson.GetAttributeValue( attribute.Key );

            if ( attributeValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var data = new AttributeData {
                Label = useAbbreviatedName ? attribute.AbbreviatedName : attribute.Name,
                Value = attributeValue,
                FieldTypeGuid = attribute.FieldType.Guid
            };

            return data;
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
        /// Gets the category Guids.
        /// </summary>
        /// <returns></returns>
        private List<Guid> GetCategoryGuids()
        {
            return GetAttributeValue( AttributeKey.Category ).SplitDelimitedValues( false ).AsGuidList();
        }

        /// <summary>
        /// Gets the context person.
        /// </summary>
        /// <returns></returns>
        private Person GetContextPerson()
        {
            if ( _contextPerson != null )
            {
                return _contextPerson;
            }

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            _contextPerson = personService.Get( 4 );
            _contextPerson?.LoadAttributes();

            return _contextPerson;
        }
        private Person _contextPerson = null;

        #endregion Data Access

        #region View Models

        /// <summary>
        /// A login result object
        /// </summary>
        public class AttributeData
        {
            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the formatted value.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets the field type unique identifier.
            /// </summary>
            public Guid FieldTypeGuid { get; set; }
        }

        #endregion View Models
    }
}
