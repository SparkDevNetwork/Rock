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
using System.Reflection;

using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more Attributes for the given EntityType Guid.  Stored as Attribute.Guid.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class AttributeFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_KEY = "entitytype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string QUALIFIER_COLUMN_KEY = "qualifierColumn";
        private const string QUALIFIER_VALUE_KEY = "qualifierValue";

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AttributeFieldAttribute( string name )
            : this( "", name )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFieldAttribute" /> class.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public AttributeFieldAttribute( string entityTypeGuid, string name = "", string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.AttributeFieldType ).FullName )
        {
            var entityTypeConfigValue = new Field.ConfigurationValue( entityTypeGuid );
            FieldConfigurationValues.Add( ENTITY_TYPE_KEY, entityTypeConfigValue );

            var allowMultipleConfigValue = new Field.ConfigurationValue( allowMultiple.ToString() );
            FieldConfigurationValues.Add( ALLOW_MULTIPLE_KEY, allowMultipleConfigValue );

            if ( string.IsNullOrWhiteSpace( Name ) && RockInstanceConfig.DatabaseIsAvailable )
            {
                var entityType = EntityTypeCache.Get( new Guid( entityTypeGuid ) );
                name = ( entityType != null ? entityType.Name : "Entity" ) + " Attribute";
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name.Replace( " ", string.Empty );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFieldAttribute"/> class.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public AttributeFieldAttribute( string entityTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.AttributeFieldType ).FullName )
        {
            var entityTypeConfigValue = new Field.ConfigurationValue( entityTypeGuid );
            FieldConfigurationValues.Add( ENTITY_TYPE_KEY, entityTypeConfigValue );

            var allowMultipleConfigValue = new Field.ConfigurationValue( allowMultiple.ToString() );
            FieldConfigurationValues.Add( ALLOW_MULTIPLE_KEY, allowMultipleConfigValue );

            var entityTypeQualifierColumnConfigValue = new Field.ConfigurationValue( entityTypeQualifierColumn );
            FieldConfigurationValues.Add( QUALIFIER_COLUMN_KEY, entityTypeQualifierColumnConfigValue );

            if ( entityTypeQualifierColumn.EndsWith( "Id" ) && entityTypeQualifierValue.AsGuid() != Guid.Empty && RockInstanceConfig.DatabaseIsAvailable )
            {
                EntityTypeCache itemEntityType = EntityTypeCache.Get( "Rock.Model." + entityTypeQualifierColumn.Left( entityTypeQualifierColumn.Length - 2 ) );
                if ( itemEntityType.AssemblyName != null )
                {
                    // get the actual type of what is being followed 
                    Type entityType = itemEntityType.GetEntityType();
                    if ( entityType != null )
                    {
                        var dbContext = Reflection.GetDbContextForEntityType( entityType );
                        if ( dbContext != null )
                        {
                            var serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
                            if ( serviceInstance != null )
                            {
                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );
                                var entity = getMethod.Invoke( serviceInstance, new object[] { entityTypeQualifierValue.AsGuid() } ) as Rock.Data.IEntity;
                                if ( entity != null )
                                {
                                    FieldConfigurationValues.Add( QUALIFIER_VALUE_KEY, new Field.ConfigurationValue( entity.Id.ToString() ) );
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var entityTypeQualifierValueConfigValue = new Field.ConfigurationValue( entityTypeQualifierValue );
                FieldConfigurationValues.Add( QUALIFIER_VALUE_KEY, entityTypeQualifierValueConfigValue );
            }

            if ( string.IsNullOrWhiteSpace( Name ) && RockInstanceConfig.DatabaseIsAvailable )
            {
                var entityType = EntityTypeCache.Get( new Guid( entityTypeGuid ) );
                name = ( entityType != null ? entityType.Name : "Entity" ) + " Attribute";
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name.Replace( " ", string.Empty );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }


        /// <summary>
        /// Gets or sets the EntityType.Guid that the Attribute List should be limited to.
        /// </summary>
        /// <value>
        /// The entity type unique identifier.
        /// </value>
        public string EntityTypeGuid
        {
            get => FieldConfigurationValues.GetValueOrNull( ENTITY_TYPE_KEY );
            set => FieldConfigurationValues.AddOrReplace( ENTITY_TYPE_KEY, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        public string EntityTypeQualifierColumn
        {
            get => FieldConfigurationValues.GetValueOrNull( QUALIFIER_COLUMN_KEY );
            set => EnsureEntityQualifierSettings( value, this.EntityTypeQualifierValue );
        }

        /// <summary>
        /// Gets or sets the entity type qualifier value.  Please note that if you are using a Rock entity (from Rock.Models), you may set this
        /// value to the Guid of the entity type you are utilizing and (if the EntityTypeQualifierColumn is an Id field) it will translate the
        /// Guid to the Id.  WARNING:  This ONLY works for entities in Rock.Models.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue
        {
            get => FieldConfigurationValues.GetValueOrNull( QUALIFIER_VALUE_KEY );
            set => EnsureEntityQualifierSettings( this.EntityTypeQualifierColumn, value );
        }

        /// <summary>
        /// This method is utilized by the setters for EntityTypeQualifierColumn and EntityTypeQualifierValue to translate between Guids and Ids.
        /// It must be called by both setters to ensure that it is executed after both values have been set.
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column (either "value" or this.EntityTypeQualifierColumn).</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value (either "value" or this.EntityTypeQualifierValue).</param>
        private void EnsureEntityQualifierSettings( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            // Set EntityTypeQualifierColumn value.
            FieldConfigurationValues.AddOrReplace( QUALIFIER_COLUMN_KEY, new Field.ConfigurationValue( entityTypeQualifierColumn ) );

            if ( !string.IsNullOrEmpty( entityTypeQualifierColumn ) && entityTypeQualifierColumn.EndsWith( "Id" ) && entityTypeQualifierValue.AsGuid() != Guid.Empty && RockInstanceConfig.DatabaseIsAvailable )
            {
                // If this is a Guid, but the Column value is an Id, fetch the Id from the Guid and then set the Id as the EntityTypeQualifierValue value.
                EntityTypeCache itemEntityType = EntityTypeCache.Get( "Rock.Model." + entityTypeQualifierColumn.Left( entityTypeQualifierColumn.Length - 2 ) );
                if ( itemEntityType.AssemblyName != null )
                {
                    // get the actual type of what is being followed
                    Type entityType = itemEntityType.GetEntityType();
                    if ( entityType != null )
                    {
                        var dbContext = Reflection.GetDbContextForEntityType( entityType );
                        if ( dbContext != null )
                        {
                            var serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
                            if ( serviceInstance != null )
                            {
                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );
                                var entity = getMethod.Invoke( serviceInstance, new object[] { entityTypeQualifierValue.AsGuid() } ) as Rock.Data.IEntity;
                                if ( entity != null )
                                {
                                    FieldConfigurationValues.AddOrReplace( QUALIFIER_VALUE_KEY, new Field.ConfigurationValue( entity.Id.ToString() ) );
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Set EntityTypeQualifierValue value.
                var entityTypeQualifierValueConfigValue = new Field.ConfigurationValue( entityTypeQualifierValue );
                FieldConfigurationValues.AddOrReplace( QUALIFIER_VALUE_KEY, entityTypeQualifierValueConfigValue );
            } 
        }

    }
}