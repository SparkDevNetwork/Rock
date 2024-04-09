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

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Group Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class GroupIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [RockIndexField]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type.
        /// </summary>
        /// <value>
        /// The name of the group type.
        /// </value>
        [RockIndexField]
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the parent group identifier.
        /// </summary>
        /// <value>
        /// The parent group identifier.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public override string IconCssClass {
            get
            {
                if ( string.IsNullOrWhiteSpace( _iconCssClass ) )
                {
                    return "fa fa-users";
                }

                return _iconCssClass;
            }

            set
            {
                _iconCssClass = value;
            }
        }

        private string _iconCssClass = string.Empty;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [RockIndexField( Boost = 3 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [RockIndexField]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the member list.
        /// </summary>
        /// <value>
        /// The member list.
        /// </value>
        [RockIndexField]
        public string MemberList { get; set; }

        /// <summary>
        /// Gets or sets the leader list.
        /// </summary>
        /// <value>
        /// The leader list.
        /// </value>
        [RockIndexField]
        public string LeaderList { get; set; }

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static GroupIndex LoadByModel( Group group )
        {
            var groupIndex = new GroupIndex
            {
                SourceIndexModel = "Rock.Model.Group",
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                GroupTypeId = group.GroupTypeId,
                DocumentName = group.Name,
                ModelOrder = 5
            };

            if ( group.GroupType != null )
            {
                groupIndex.IconCssClass = group.GroupType.IconCssClass;
                groupIndex.GroupTypeName = group.GroupType.Name;
            }

            var rockContext = new Rock.Data.RockContext();
            rockContext.Configuration.AutoDetectChangesEnabled = false;
            
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMembers = groupMemberService
                .Queryable()
                .AsNoTracking()
                .Where( gm => gm.GroupId == group.Id
                    && gm.IsArchived == false
                    && gm.GroupMemberStatus == GroupMemberStatus.Active );

            var groupMemberCount = rockContext.Set<GroupMember>().Count( gm => gm.GroupId == group.Id && gm.IsArchived == false && gm.GroupMemberStatus == GroupMemberStatus.Active );
            if ( groupMemberCount > 10000 )
            {
                Rock.Logging.RockLogger.Log.Debug( Logging.RockLogDomains.Bus, $"Skipping universal search index of Group {group.Name} becauses the number of active users {groupMemberCount} exceeds the limit of 10K." );
            }
            else if ( groupMemberCount > 0 )
            {
                var memberListStringBuilder = new StringBuilder();
                var leaderListStringBuilder = new StringBuilder();

                foreach ( var groupMember in groupMembers )
                {
                    if ( groupMember.GroupRole.IsLeader != true )
                    {
                        memberListStringBuilder.Append( $"{groupMember.Person.FullName}," );
                    }
                    else
                    {
                        leaderListStringBuilder.Append( $"{groupMember.Person.FullName}," );
                    }
                }

                // Trim the trailing commas from the string builders
                if ( memberListStringBuilder.Length > 0 )
                {
                    memberListStringBuilder.Length--;
                }

                if ( leaderListStringBuilder.Length > 0 )
                {
                    leaderListStringBuilder.Length--;
                }

                groupIndex.MemberList = memberListStringBuilder.ToString();
                groupIndex.LeaderList = leaderListStringBuilder.ToString();
            }
            
            AddIndexableAttributes( groupIndex, group );

            return groupIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="displayOptions">The display options.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null, Dictionary<string, object> mergeFields = null )
        {
            var result = base.FormatSearchResult( person, displayOptions );
            bool isSecurityDisabled = false;
            result.IsViewAllowed = false;

            if ( displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "Group-IsSecurityDisabled" ) )
                {
                    isSecurityDisabled = displayOptions["Group-IsSecurityDisabled"].ToString().AsBoolean();
                }
            }

            // Check security on the group if security is enabled
            var group = new GroupService(new Data.RockContext()).Get( (int)this.Id );
            if ( group != null )
            {
                result.IsViewAllowed = group.IsAuthorized( Rock.Security.Authorization.VIEW, person ) || isSecurityDisabled;
            }

            return result;
        }
    }
}
