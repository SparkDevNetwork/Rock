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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Rock.Attribute Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region IHasAttributes extensions

        /// <summary>
        /// Loads the attribute.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static void LoadAttributes( this Rock.Attribute.IHasAttributes entity )
        {
            Rock.Attribute.Helper.LoadAttributes( entity );
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( this Rock.Attribute.IHasAttributes entity, RockContext rockContext )
        {
            Rock.Attribute.Helper.LoadAttributes( entity, rockContext );
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValues( this Rock.Attribute.IHasAttributes entity, RockContext rockContext = null )
        {
            Rock.Attribute.Helper.SaveAttributeValues( entity, rockContext );
        }

        /// <summary>
        /// Saves the specified attribute values to the database.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="keys">The attribute keys.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValues( this Rock.Attribute.IHasAttributes entity, IEnumerable<string> keys, RockContext rockContext = null )
        {
            foreach ( var key in keys )
            {
                if ( entity.AttributeValues.ContainsKey( key ) )
                {
                    Rock.Attribute.Helper.SaveAttributeValue( entity, entity.Attributes[key], entity.AttributeValues[key].Value, rockContext );
                }
            }
        }

        /// <summary>
        /// Saves an attribute value to the database.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValue( this Rock.Attribute.IHasAttributes entity, string key, RockContext rockContext = null)
        {
            if ( entity.AttributeValues.ContainsKey( key ) )
            {
                Rock.Attribute.Helper.SaveAttributeValue( entity, entity.Attributes[key], entity.AttributeValues[key].Value, rockContext );
            }
        }

        /// <summary>
        /// Copies the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="source">The source.</param>
        public static void CopyAttributesFrom( this Rock.Attribute.IHasAttributes entity, Rock.Attribute.IHasAttributes source )
        {
            Rock.Attribute.Helper.CopyAttributes( source, entity );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this Rock.Attribute.IHasAttributes entity, string key, int? value )
        {
            entity.SetAttributeValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this Rock.Attribute.IHasAttributes entity, string key, decimal? value )
        {
            entity.SetAttributeValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this Rock.Attribute.IHasAttributes entity, string key, Guid? value )
        {
            entity.SetAttributeValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this Rock.Attribute.IHasAttributes entity, string key, DateTime? value )
        {
            if ( value.HasValue )
            {
                entity.SetAttributeValue( key, value.Value.ToString( "o" ) );
            }
            else
            {
                entity.SetAttributeValue( key, string.Empty );
            }
        }

        /// <summary>
        /// Gets the authorized attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static Dictionary<string, AttributeCache> GetAuthorizedAttributes ( this Rock.Attribute.IHasAttributes entity, string action, Person person)
        {
            var authorizedAttributes = new Dictionary<string, AttributeCache>();

            if ( entity != null )
            {
                foreach( var item in entity.Attributes )
                {
                    if ( item.Value.IsAuthorized( action, person ) )
                    {
                        authorizedAttributes.Add( item.Key, item.Value );
                    }
                }
            }

            return authorizedAttributes;
        }

        #endregion IHasAttributes extensions

    }
}
