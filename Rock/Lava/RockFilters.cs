// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// 
    /// </summary>
    public static class RockFilters
    {

        #region Attribute Filters

        /// <summary>
        /// DotLiquid Attribute Filter
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static string Attribute( DotLiquid.Context context, object input, string attributeKey, string qualifier = "" )
        {
            if ( input == null || attributeKey == null )
            {
                return string.Empty;
            }

            // Try to get RockContext from the dotLiquid context
            var rockContext = GetRockContext(context);

            AttributeCache attribute = null;
            string rawValue = string.Empty;

            // If Input is "Global" then look for a global attribute with key
            if (input.ToString().Equals( "Global", StringComparison.OrdinalIgnoreCase ) )
            {
                var globalAttributeCache = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext );
                attribute = globalAttributeCache.Attributes
                    .FirstOrDefault( a => a.Key.Equals(attributeKey, StringComparison.OrdinalIgnoreCase));
                if (attribute != null )
                {
                    rawValue = globalAttributeCache.GetValue( attributeKey );
                }
            }

            // If input is an object that has attributes, find it's attribute value
            else if ( input is IHasAttributes)
            {
                var item = (IHasAttributes)input;
                if ( item.Attributes == null)
                {
                    item.LoadAttributes( rockContext );
                }

                if ( item.Attributes.ContainsKey(attributeKey))
                {
                    attribute = item.Attributes[attributeKey];
                    rawValue = item.AttributeValues[attributeKey].Value;
                }
            }

            // If valid attribute and value were found
            if ( attribute != null && 
                !string.IsNullOrWhiteSpace(rawValue) &&
                attribute.IsAuthorized( Authorization.VIEW, null ) )
            {
                // Check qualifier for 'Raw' if present, just return the raw unformatted value
                if ( qualifier.Equals("RawValue", StringComparison.OrdinalIgnoreCase) )
                {
                    return rawValue;
                }

                // Check qualifier for 'Url' and if present and attribute's field type is a ILinkableFieldType, then return the formatted url value
                var field = attribute.FieldType.Field;
                if ( qualifier.Equals("Url", StringComparison.OrdinalIgnoreCase) && field is Rock.Field.ILinkableFieldType )
                {
                    return ( (Rock.Field.ILinkableFieldType)field ).UrlLink( rawValue, attribute.QualifierValues );
                }

                // If qualifier was specified, and the attribute field type is an IEntityFieldType, try to find a property on the entity
                if ( !string.IsNullOrWhiteSpace(qualifier) && field is Rock.Field.IEntityFieldType )
                {
                    IEntity entity = ( (Rock.Field.IEntityFieldType)field ).GetEntity( rawValue );
                    if (entity != null)
                    {
                        return entity.GetPropertyValue(qualifier).ToStringSafe();
                    }
                }

                // Otherwise return the formatted value
                return field.FormatValue( null, rawValue, attribute.QualifierValues, false );
            }

            return string.Empty;
        }

        #endregion

        #region Person Filters

        /// <summary>
        /// Addresses the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <returns></returns>
        public static string Address( DotLiquid.Context context, object input, string addressType )
        {
            if ( input != null && input is Person )
            {
                var person = (Person)input;
                

                Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                var location = new GroupMemberService( GetRockContext(context) )
                    .Queryable( "GroupLocations.Location" )
                    .Where( m => 
                        m.PersonId == person.Id && 
                        m.Group.GroupType.Guid == familyGuid )
                    .SelectMany( m => m.Group.GroupLocations )
                    .Where( gl => 
                        gl.GroupLocationTypeValue.Value == addressType )
                    .Select( gl => gl.Location )
                    .FirstOrDefault();

                if (location != null)
                {
                    return location.GetFullStreetAddress();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static RockContext GetRockContext( DotLiquid.Context context)
        {
            if ( context.Registers.ContainsKey("rock_context"))
            {
                return context.Registers["rock_context"] as RockContext;
            }
            else
            {
                var rockContext = new RockContext();
                context.Registers.Add( "rock_context", rockContext );
                return rockContext;
            }
        }

        #endregion
    }
}
