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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Groups
{
    /// <summary>
    /// Allows the user to authenticate.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Group Member List" )]
    [Category( "Obsidian > Groups" )]
    [Description( "Lists the members of a group." )]
    [IconCssClass( "fa fa-users" )]

    public class GroupMemberList : RockObsidianBlockType
    {
        #region Actions

        /// <summary>
        /// Handles the login action
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="filterOptions">The filter options.</param>
        /// <param name="sortOptions">The sort options.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetGroupMemberList( int groupId, FilterOptions filterOptions, SortProperty sortProperty )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var query = groupMemberService.Queryable()
                    .AsNoTracking()
                    .Where( gm => gm.GroupId == groupId );

                // Filter
                if ( !filterOptions.FirstName.IsNullOrWhiteSpace() )
                {
                    query = query.Where( gm => gm.Person.FirstName.StartsWith( filterOptions.FirstName ) );
                }

                if ( !filterOptions.LastName.IsNullOrWhiteSpace() )
                {
                    query = query.Where( gm => gm.Person.LastName.StartsWith( filterOptions.LastName ) );
                }

                // Sort
                if ( sortProperty?.Property.IsNullOrWhiteSpace() == false )
                {
                    query = query.Sort( sortProperty ).ThenBy( gm => gm.Id );
                }
                else
                {
                    query = query.OrderBy( gm => gm.Id );
                }

                // Paginate
                query = query.Skip( filterOptions.Skip ).Take( filterOptions.Take );

                // Select necessary data to enumerate
                var selectQuery = query.Select( gm => new
                {
                    Person = gm.Person,
                    GroupMemberId = gm.Id,
                    RoleName = gm.GroupRole.Name,
                    StatusName = gm.GroupMemberStatus.ToString(),
                } );

                // Enumerate and use non-linq-capable functions like FullName
                return new BlockActionResult( HttpStatusCode.Created, new GroupMemberListResponse
                {
                    GroupMembers = selectQuery.ToList().Select( gm => new GroupMemberViewModel
                    {
                        FullName = gm.Person.FullName,
                        GroupMemberId = gm.GroupMemberId,
                        PersonId = gm.Person.Id,
                        PhotoUrl = gm.Person.PhotoUrl,
                        RoleName = gm.RoleName,
                        StatusName = gm.StatusName
                    } ).ToList()
                } );
            }
        }

        #endregion Actions

        #region Responses

        /// <summary>
        /// A group member list result object
        /// </summary>
        public class GroupMemberListResponse
        {
            /// <summary>
            /// Gets or sets the group members.
            /// </summary>
            /// <value>
            /// The group members.
            /// </value>
            public List<GroupMemberViewModel> GroupMembers { get; set; }
        }

        #endregion Responses

        #region View Models

        /// <summary>
        /// Filter Options
        /// </summary>
        public class FilterOptions
        {
            /// <summary>
            /// Gets or sets the top.
            /// </summary>
            /// <value>
            /// The top.
            /// </value>
            public int Take { get; set; }

            /// <summary>
            /// Gets or sets the offset.
            /// </summary>
            /// <value>
            /// The offset.
            /// </value>
            public int Skip { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }
        }

        /// <summary>
        /// Group Member View Model
        /// </summary>
        public class GroupMemberViewModel
        {
            /// <summary>
            /// Gets or sets the group member identifier.
            /// </summary>
            /// <value>
            /// The group member identifier.
            /// </value>
            public int GroupMemberId { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the photo URL.
            /// </summary>
            /// <value>
            /// The photo URL.
            /// </value>
            public string PhotoUrl { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName { get; set; }

            /// <summary>
            /// Gets or sets the name of the role.
            /// </summary>
            /// <value>
            /// The name of the role.
            /// </value>
            public string RoleName { get; set; }

            /// <summary>
            /// Gets or sets the name of the status.
            /// </summary>
            /// <value>
            /// The name of the status.
            /// </value>
            public string StatusName { get; set; }
        }

        #endregion View Models
    }
}
