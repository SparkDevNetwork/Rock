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
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData.Core
{
    /// <summary>
    /// Provides actions to manage Entity Attributes.
    /// </summary>
    public class EntityAttributeDataManager
    {
        private static Lazy<EntityAttributeDataManager> _dataManager = new Lazy<EntityAttributeDataManager>();
        public static EntityAttributeDataManager Instance => _dataManager.Value;

        #region Attributes

        /// <summary>
        /// Add an Attribute for the specified Rock Entity.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public Rock.Model.Attribute AddEntityAttribute( AddEntityAttributeArgs args, RockContext rockContext = null )
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
        public List<Rock.Model.Attribute> AddEntityAttributes( List<AddEntityAttributeArgs> args, RockContext rockContext = null )
        {
            var saveChanges = ( rockContext == null );
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
                        throw new Exception( $"Invalid Category. [CategoryReference={attributeArgs.CategoryIdentifier}]" );
                    }

                    newAttribute.Categories = new List<Category>();
                    newAttribute.Categories.Add( attributeCategory );
                }

                attributes.Add( newAttribute );
                attributeService.Add( newAttribute );
            }

            if ( saveChanges )
            {
                rockContext.SaveChanges();
            }

            return attributes;
        }

        #endregion

    }

    #region Support Classes

    public static class Extensions
    {
        public static AddEntityAttributeArgs WithEntityTypeQualifier( this AddEntityAttributeArgs args, string databaseColumnName, string value )
        {
            args.EntityTypeQualifierColumn = databaseColumnName;
            args.EntityTypeQualifierValue = value;

            return args;
        }
        public static AddEntityAttributeArgs WithIdentifier( this AddEntityAttributeArgs args, object guid )
        {
            args.Guid = guid.ToStringSafe().AsGuidOrNull();
            if ( guid != null
                 && args.Guid == null )
            {
                throw new Exception( $@"Invalid identifier [Guid={guid}]" );
            }

            return args;
        }
    }

    /// <summary>
    /// Arguments for the AddEntityAttribute action.
    /// </summary>
    public class AddEntityAttributeArgs
    {
        public static AddEntityAttributeArgs New( string key, string fieldTypeIdentifier, string entityTypeIdentifier )
        {
            var args = new AddEntityAttributeArgs
            {
                Key = key,
                FieldTypeIdentifier = fieldTypeIdentifier,
                EntityTypeIdentifier = entityTypeIdentifier
            };
            return args;
        }

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

    #endregion
}