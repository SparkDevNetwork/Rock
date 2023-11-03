﻿// <copyright>
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
using Rock.Data;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using System.Data.Entity;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Core
        {
            #region Attributes

            /// <summary>
            /// Arguments for the AddEntityAttribute action.
            /// </summary>
            public class AddEntityAttributeArgs
            {
                public string ForeignKey { get; set; }

                public Guid? Guid { get; set; }

                public string EntityTypeIdentifier { get; set; }

                public string Key { get; set; }
                public string Name { get; set; }

                public string FieldTypeIdentifier { get; set; }

                public string CategoryIdentifier { get; set; }

                public string EntityTypeQualifierColumn { get; set; }
                public string EntityTypeQualifierValue { get; set; }
            }

            /// <summary>
            /// Add an Attribute for the specified Rock Entity.
            /// </summary>
            /// <param name="args"></param>
            /// <param name="rockContext"></param>
            /// <returns></returns>
            public static Rock.Model.Attribute AddEntityAttribute( AddEntityAttributeArgs args, RockContext rockContext = null )
            {
                var attributes = AddEntityAttributes( new List<AddEntityAttributeArgs> { args }, rockContext );
                return attributes.FirstOrDefault();
            }

            /// <summary>
            /// Add Attributes for specified Rock Entities.
            /// </summary>
            /// <param name="args"></param>
            /// <param name="rockContext"></param>
            /// <returns></returns>
            public static List<Rock.Model.Attribute> AddEntityAttributes( List<AddEntityAttributeArgs> args, RockContext rockContext = null )
            {
                rockContext = rockContext ?? new RockContext();

                var attributeService = new AttributeService( rockContext );
                var entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;
                var attributeCategoryList = new CategoryService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();

                var attributes = new List<Rock.Model.Attribute>();

                foreach ( var attributeArgs in args )
                {
                    var fieldType = FieldTypeCache.Get( attributeArgs.FieldTypeIdentifier, allowIntegerIdentifier: true );
                    Assert.That.IsNotNull( fieldType, "Field Type is invalid." );

                    var entityType = EntityTypeCache.Get( attributeArgs.EntityTypeIdentifier, allowIntegerIdentifier: true );
                    Assert.That.IsNotNull( entityType, "Entity Type is invalid." );

                    var name = attributeArgs.Name;
                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = attributeArgs.Key;
                    }

                    var newAttribute = new Rock.Model.Attribute()
                    {
                        Key = attributeArgs.Key,
                        Name = name,
                        Guid = attributeArgs.Guid ?? Guid.NewGuid(),
                        ForeignKey = attributeArgs.ForeignKey,
                        EntityTypeId = entityType.Id,
                        FieldTypeId = fieldType.Id,
                        EntityTypeQualifierColumn = attributeArgs.EntityTypeQualifierColumn,
                        EntityTypeQualifierValue = attributeArgs.EntityTypeQualifierValue
                    };

                    if ( !string.IsNullOrWhiteSpace( attributeArgs.CategoryIdentifier ) )
                    {
                        var attributeCategory = attributeCategoryList.GetByIdentifier( attributeArgs.CategoryIdentifier );
                        if ( attributeCategory == null )
                        {
                            attributeCategory = attributeCategoryList.FirstOrDefault( a => a.Name.Equals( attributeArgs.CategoryIdentifier, StringComparison.OrdinalIgnoreCase ) );
                        }
                        if ( attributeCategory == null )
                        {
                            throw new Exception( $"Invalid Category. [CategoryReference={ attributeArgs.CategoryIdentifier }]" );
                        }

                        newAttribute.Categories = new List<Category>();
                        newAttribute.Categories.Add( attributeCategory );
                    }

                    attributes.Add( newAttribute );
                    attributeService.Add( newAttribute );
                }

                return attributes;
            }

            #endregion

            #region Cache

            /// <summary>
            /// Arguments for the AddCacheTag action.
            /// </summary>
            public class AddCacheTagArgs
            {
                public string ForeignKey { get; set; }

                public Guid? Guid { get; set; }
                public string Name { get; set; }

                public string Description { get; set; }
            }

            /// <summary>
            /// Add a defined Cache Tag.
            /// </summary>
            /// <param name="args"></param>
            /// <param name="rockContext"></param>
            /// <returns></returns>
            public static Rock.Model.DefinedValue AddCacheTag( AddCacheTagArgs args, RockContext rockContext = null )
            {
                var newItems = AddCacheTags( new List<AddCacheTagArgs> { args }, rockContext );
                return newItems.FirstOrDefault();
            }

            /// <summary>
            /// Add defined Cache Tags.
            /// Cache Tags are tracked using a Defined Type.
            /// </summary>
            /// <param name="args"></param>
            /// <param name="rockContext"></param>
            /// <returns></returns>
            public static List<Rock.Model.DefinedValue> AddCacheTags( List<AddCacheTagArgs> args, RockContext rockContext = null )
            {
                rockContext = rockContext ?? new RockContext();

                var cachedTagDefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ) ?? 0;

                var definedValueService = new DefinedValueService( rockContext );
                var valuesQuery = definedValueService.Queryable()
                    .AsNoTracking()
                    .Where( v => v.DefinedTypeId == cachedTagDefinedTypeId );

                var newItems = new List<Rock.Model.DefinedValue>();

                var order = 0;
                if ( valuesQuery.Any() )
                {
                    order = valuesQuery.Max( v => v.Order ) + 1;
                }

                foreach ( var attributeArgs in args )
                {
                    var definedValue = new DefinedValue
                    {
                        DefinedTypeId = cachedTagDefinedTypeId,
                        Value = attributeArgs.Name?.Trim().ToLower(),
                        Description = attributeArgs.Description,
                        Order = order
                    };

                    definedValueService.Add( definedValue );
                    order++;

                    newItems.Add( definedValue );
                }

                rockContext.SaveChanges();

                return newItems;
            }

            #endregion
        }
    }
}
