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

using Rock.Configuration;
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
        [Obsolete( "Use the constructor that takes only a groupTypeGuid and name." )]
        [RockObsolete( "20.0" )]
        public GroupRoleFieldAttribute( string groupTypeGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.GROUP_ROLE.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
            if ( !string.IsNullOrWhiteSpace( groupTypeGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( groupTypeGuid, out guid ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var groupType = GroupTypeCache.Get( guid );
                    if ( groupType != null )
                    {
                        var configValue = new Field.ConfigurationValue( groupType.Id.ToString() );
                        FieldConfigurationValues.AddOrReplace( "grouptype", configValue );

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="groupTypeGuid">The group type GUID.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// This is essentially a temporary constructor. Once the constructor
        /// takes multiple parameters is removed, this constructor can be marked
        /// as obsolete and a new constructor that takes only a name parameter
        /// can be added to match the pattern of all other field attributes.
        /// We can't go directly to a single name parameter because it would
        /// conflict with the original constructor that takes the group type guid
        /// as the first parameter.
        /// </remarks>
        public GroupRoleFieldAttribute( string groupTypeGuid, string name )
            : base( SystemGuid.FieldType.GROUP_ROLE.AsGuid(), name )
        {
            GroupTypeGuid = groupTypeGuid;
        }

        /// <summary>
        /// The unique identifier of the group type that will be used when
        /// presenting the list of roles to choose from.
        /// </summary>
        public string GroupTypeGuid
        {
            get
            {
                var configValue = FieldConfigurationValues.GetValueOrNull( "grouptype" );

                if ( int.TryParse( configValue, out var id ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var groupType = GroupTypeCache.Get( id );

                    if ( groupType != null )
                    {
                        return groupType.Guid.ToString();
                    }
                }

                return null;
            }
            set
            {
                if ( Guid.TryParse( value, out var guid ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var groupType = GroupTypeCache.Get( guid );

                    if ( groupType != null )
                    {
                        var configValue = new Field.ConfigurationValue( groupType.Id.ToString() );
                        FieldConfigurationValues.AddOrReplace( "grouptype", configValue );
                    }
                }
            }
        }
    }
}
