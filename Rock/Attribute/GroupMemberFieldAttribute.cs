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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
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
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public GroupMemberFieldAttribute( string groupGuid, string name, string description, bool required, bool allowMultiple, bool enhanced, string defaultValue, string category, int order, string key = null )
            : base( SystemGuid.FieldType.GROUP_MEMBER.AsGuid(), name, description, required, defaultValue, category, order, key )
        {

            Group group = null;
            using ( var rockContext = new RockContext() )
            {
                group = new GroupService( rockContext ).Get( new Guid( groupGuid ) );
            }

            if ( group != null )
            {
                var groupConfigValue = new Field.ConfigurationValue( group.Id.ToString() );
                FieldConfigurationValues.AddOrReplace( GROUP_KEY, groupConfigValue );

                var allowMultipleConfigValue = new Field.ConfigurationValue( allowMultiple.ToString() );
                FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, allowMultipleConfigValue );

                var enhancedConfigValue = new Field.ConfigurationValue( enhanced.ToString() );
                FieldConfigurationValues.AddOrReplace( ENHANCED_SELECTION_KEY, enhancedConfigValue );

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

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberFieldAttribute"/> class.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// This is essentially a temporary constructor. Once the constructor
        /// takes multiple parameters is removed, this constructor can be marked
        /// as obsolete and a new constructor that takes only a name parameter
        /// can be added to match the pattern of all other field attributes.
        /// We can't go directly to a single name parameter because it would
        /// conflict with the original constructor that takes the group guid
        /// as the first parameter.
        /// </remarks>
        public GroupMemberFieldAttribute( string groupGuid, string name )
            : base( SystemGuid.FieldType.GROUP_MEMBER.AsGuid(), name )
        {
            GroupGuid = groupGuid;
            AllowMultiple = false;
            EnhancedSelection = false;
        }

        /// <summary>
        /// The unique identifier of the group that should be used when presenting
        /// the members to pick from.
        /// </summary>
        public string GroupGuid
        {
            get
            {
                var configValue = FieldConfigurationValues.GetValueOrNull( GROUP_KEY );

                if ( int.TryParse( configValue, out var id ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var group = GroupCache.Get( id );

                    if ( group != null )
                    {
                        return group.Guid.ToString();
                    }
                }

                return null;
            }
            set
            {
                if ( Guid.TryParse( value, out var guid ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var group = GroupCache.Get( guid );

                    if ( group != null )
                    {
                        var configValue = new Field.ConfigurationValue( group.Id.ToString() );
                        FieldConfigurationValues.AddOrReplace( GROUP_KEY, configValue );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether multiple group members can be selected.
        /// </summary>
        public bool AllowMultiple
        {
            get => FieldConfigurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean();
            set => FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new Field.ConfigurationValue( value.ToString() ) );
        }

        /// <summary>
        /// Determines whether the enhanced selection mode should be used
        /// when selecting group members.
        /// </summary>
        public bool EnhancedSelection
        {
            get => FieldConfigurationValues.GetValueOrNull( ENHANCED_SELECTION_KEY ).AsBoolean();
            set => FieldConfigurationValues.AddOrReplace( ENHANCED_SELECTION_KEY, new Field.ConfigurationValue( value.ToString() ) );
        }
    }
}
