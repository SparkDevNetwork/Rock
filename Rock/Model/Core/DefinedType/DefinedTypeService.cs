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
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.DefinedType"/> entity objects.
    /// </summary>
    public partial class DefinedTypeService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DefinedType">DefinedTypes</see> by FieldTypeId
        /// </summary>
        /// <param name="fieldTypeId">A <see cref="System.Int32"/> representing the FieldTypeId of the <see cref="Rock.Model.FieldType"/> that is used for the 
        /// <see cref="Rock.Model.DefinedValue"/>
        /// </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DefinedType">DefinedTypes</see> that use the specified <see cref="Rock.Model.FieldType"/>.</returns>
        public IOrderedQueryable<Rock.Model.DefinedType> GetByFieldTypeId( int? fieldTypeId )
        {
            return Queryable()
                .Where( t => 
                    ( t.FieldTypeId == fieldTypeId || 
                        ( fieldTypeId == null && t.FieldTypeId == null ) ) )
                .OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.DefinedType"/> by GUID identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.DefinedType"/> with a matching Guid identifier. If a match is not found, null is returned.</returns>
        public Rock.Model.DefinedType GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }


        /// <summary>
        /// Gets the Guid for the DefinedType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = DefinedTypeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }

        #region Operations

        /// <summary>
        /// Add or update a value for a Defined Type.
        /// </summary>
        /// <param name="definedTypeGuidString">A string representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to which a new value will be added.</param>
        /// <param name="value">A string containing the Value of the new <see cref="Rock.Model.DefinedValue"/>.</param>
        /// <param name="description">A string containing the Description of the new <see cref="Rock.Model.DefinedValue"/>.</param>
        /// <param name="attributeValues">A dictionary of key-value pairs holding the value of the Attributes associated with the new <see cref="Rock.Model.DefinedValue"/>.</param>
        public void AddOrUpdateValue( string definedTypeGuidString, string value, string description, Dictionary<string, object> attributeValues )
        {
            var dataContext = ( RockContext ) this.Context;

            // Execute this operation in a transaction to allow rollback if there are any problems updating the value attributes.
            dataContext.WrapTransaction( () =>
            {
                // Resolve the Defined Type.
                var definedType = DefinedTypeCache.Get( new Guid( definedTypeGuidString ) );

                if ( definedType == null )
                {
                    throw new Exception( $"Defined Type is invalid. Could not map identifier \"{ definedTypeGuidString }\" to an existing Defined Type." );
                }

                // Get the existing Defined Value or create a new entry.
                var definedValueService = new DefinedValueService( dataContext );

                var definedValue = definedValueService.Queryable().FirstOrDefault( x => x.DefinedTypeId == definedType.Id && x.Value == value );

                if ( definedValue == null )
                {
                    // Create a new Defined Value.
                    definedValue = new DefinedValue();

                    definedValueService.Add( definedValue );

                    definedValue.DefinedTypeId = definedType.Id;
                    definedValue.IsSystem = false;
                    definedValue.IsActive = true;
                }

                definedValue.Value = value;
                definedValue.Description = description;

                dataContext.SaveChanges();

                // Set the attributes of the defined value.
                definedValue.LoadAttributes();

                foreach ( var keyValuePair in attributeValues )
                {
                    if ( !definedValue.Attributes.ContainsKey( keyValuePair.Key ) )
                    {
                        throw new Exception( $"Defined Type Attribute is invalid. Could not map key \"{ keyValuePair.Key }\" to an existing Attribute." );
                    }

                    definedValue.SetAttributeValue( keyValuePair.Key, keyValuePair.Value.ToStringSafe() );
                }

                definedValue.SaveAttributeValues( dataContext );
            } );
        }

        /// <summary>
        /// Delete a value from a Defined Type.
        /// </summary>
        /// <param name="definedTypeGuidString">A string representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to which a new value will be added.</param>
        /// <param name="value">A string containing the Value of the <see cref="Rock.Model.DefinedValue"/> to be deleted.</param>
        public void DeleteValue( string definedTypeGuidString, string value )
        {
            DeleteValues( definedTypeGuidString, new List<string> { value } );
        }

        /// <summary>
        /// Delete a set of values from a Defined Type.
        /// </summary>
        /// <param name="definedTypeGuidString">A string representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to which a new value will be added.</param>
        /// <param name="values">A collection of strings containing the Values of the <see cref="Rock.Model.DefinedValue"/> entries to be deleted.</param>
        public void DeleteValues( string definedTypeGuidString, IEnumerable<string> values )
        {
            if ( values == null )
            {
                return;
            }

            var valueList = values.ToList();

            if ( !valueList.Any() )
            {
                return;
            }

            var dataContext = ( RockContext ) this.Context;

            // Resolve the Defined Type.
            var definedType = DefinedTypeCache.Get( new Guid( definedTypeGuidString ) );

            if ( definedType == null )
            {
                throw new Exception( $"Defined Type is invalid. Could not map identifier \"{ definedTypeGuidString }\" to an existing Defined Type." );
            }

            // Delete the existing Defined Value if it exists.
            var definedValueService = new DefinedValueService( dataContext );

            var definedValues = definedValueService.Queryable().Where( x => x.DefinedTypeId == definedType.Id && valueList.Contains( x.Value ) );

            if ( definedValues.Any() )
            {
                definedValueService.DeleteRange( definedValues );

                dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
