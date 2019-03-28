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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or 1 GroupType
    /// GroupType value stored as GroupType.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class GroupTypeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultGroupTypeGuid">The default group type GUID.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="groupTypePurposeValueGuid">The group type purpose value unique identifier.</param>
        public GroupTypeFieldAttribute( string name, string description = "", bool required = true, string defaultGroupTypeGuid = "", string category = "", int order = 0, string key = null, string groupTypePurposeValueGuid = "" )
            : base( name, description, required, defaultGroupTypeGuid, category, order, key, typeof( Rock.Field.Types.GroupTypeFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( groupTypePurposeValueGuid ) )
            {
                Guid? guid = groupTypePurposeValueGuid.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    FieldConfigurationValues.Add( "groupTypePurposeValueGuid", new Field.ConfigurationValue( groupTypePurposeValueGuid ) );
                }
            }
        }
    }
}