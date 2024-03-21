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

using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or 1 GroupTypeRole
    /// Stored as GroupTypeRole.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class GroupRoleFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="groupTypeGuid">The group type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public GroupRoleFieldAttribute( string groupTypeGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.GroupRoleFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( groupTypeGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( groupTypeGuid, out guid ) && RockInstanceConfig.DatabaseIsAvailable )
                {
                    var groupType = GroupTypeCache.Get( guid );
                    if ( groupType != null )
                    {
                        var configValue = new Field.ConfigurationValue( groupType.Id.ToString() );
                        FieldConfigurationValues.Add( "grouptype", configValue );

                        if ( string.IsNullOrWhiteSpace( Name ) )
                        {
                            Name = groupType.Name + " Role";
                            if ( string.IsNullOrWhiteSpace( Key ) )
                            {
                                Key = Name.Replace( " ", string.Empty );
                            }
                        }
                    }
                }
            }
        }
    }
}