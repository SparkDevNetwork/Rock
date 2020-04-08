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

using Rock.Data;
using Rock.Model;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more GroupMember for the given Group Guid.
    /// Stored as either a single GroupMember.Guid or a comma-delimited list of GroupMember.Guids (if AllowMultiple)
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class GroupMemberFieldAttribute : FieldAttribute
    {
        private const string GROUP_KEY = "group";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberFieldAttribute" /> class.
        /// </summary>
        /// <param name="groupGuid">The group GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public GroupMemberFieldAttribute( string groupGuid, string name = "", string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
             : this( groupGuid, name, description, required, allowMultiple, false, defaultValue, category, order, key )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberFieldAttribute"/> class.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="enhanced">if set to <c>true</c> [enhanced].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public GroupMemberFieldAttribute( string groupGuid, string name, string description, bool required, bool allowMultiple, bool enhanced, string defaultValue, string category, int order, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.GroupMemberFieldType ).FullName )
        {

            Group group = null;
            using ( var rockContext = new RockContext() )
            {
                group = new GroupService( rockContext ).Get ( new Guid( groupGuid ) );
            }

            if ( group != null )
            {
                var groupConfigValue = new Field.ConfigurationValue( group.Id.ToString() );
                FieldConfigurationValues.Add( GROUP_KEY, groupConfigValue );

                var allowMultipleConfigValue = new Field.ConfigurationValue( allowMultiple.ToString() );
                FieldConfigurationValues.Add( ALLOW_MULTIPLE_KEY, allowMultipleConfigValue );

                var enhancedConfigValue = new Field.ConfigurationValue( enhanced.ToString() );
                FieldConfigurationValues.Add( ENHANCED_SELECTION_KEY, enhancedConfigValue );

                if ( string.IsNullOrWhiteSpace( Name ) )
                {
                    Name = group.Name;
                }

                if ( string.IsNullOrWhiteSpace( Key ) )
                {
                    Key = Name.Replace( " ", string.Empty );
                }
            }
        }
    }
}