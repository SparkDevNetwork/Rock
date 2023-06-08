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
using Rock.Data;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;
using System;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Crm
        {
            public static Site GetInternalSite( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var siteService = new SiteService( rockContext );

                var internalSite = siteService.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                return internalSite;
            }

            public static List<Page> GetInternalSitePages( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var pageService = new PageService( rockContext );

                var internalSite = GetInternalSite( rockContext );
                var pages = pageService.Queryable()
                    .Where( p => p.Layout != null && p.Layout.SiteId == internalSite.Id )
                    .ToList();

                return pages;
            }

            public class AddGroupArgs
            {
                //public RockContext DataContext { get; set; }
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupTypeIdentifier { get; set; }
                public string ParentGroupIdentifier { get; set; }
                public string GroupName { get; set; }
                public string GroupGuid { get; set; }
                public List<GroupMember> GroupMembers { get; set; }
                public string CampusIdentifier { get; set; }

            }

            /// <summary>
            /// Add a new Group.
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public static Group AddGroup( RockContext rockContext, AddGroupArgs args )
            {
                Group group = null;

                rockContext.WrapTransaction( () =>
                {
                    var groupTypeService = new GroupTypeService( rockContext );
                    var groupType = groupTypeService.Get( args.GroupTypeIdentifier );
                    var campus = CampusCache.Get( args.CampusIdentifier, allowIntegerIdentifier: true );
                    var groupService = new GroupService( rockContext );
                    var parentGroup = groupService.Get( args.ParentGroupIdentifier );

                    var groupGuid = args.GroupGuid.AsGuidOrNull();

                    if ( groupGuid != null )
                    {
                        var existingGroup = groupService.Queryable().FirstOrDefault( g => g.Guid == groupGuid );
                        if ( existingGroup != null )
                        {
                            if ( !args.ReplaceIfExists )
                            {
                                return;
                            }
                            DeleteGroup( rockContext, args.GroupGuid );
                            rockContext.SaveChanges();
                        }
                    }

                    group = GroupService.SaveNewGroup( rockContext,
                        groupType.Id,
                        parentGroup?.Guid,
                        args.GroupName,
                        args.GroupMembers ?? new List<GroupMember>(),
                        campus?.Id,
                        savePersonAttributes: true );

                    group.Guid = args.GroupGuid.AsGuidOrNull() ?? Guid.NewGuid();
                    group.ForeignKey = args.ForeignKey;

                    rockContext.SaveChanges();
                } );

                return group;
            }

            public class AddGroupMemberArgs
            {
                //public RockContext DataContext { get; set; }
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupIdentifier { get; set; }
                public string PersonIdentifier { get; set; }
                public string GroupRoleIdentifier { get; set; }
            }

            public static GroupMember AddGroupMember( RockContext rockContext, AddGroupMemberArgs args )
            {
                GroupMember groupMember = null;

                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var groupRoleService = new GroupService( rockContext );

                    var group = groupService.Get( args.GroupIdentifier );
                    AssertRockEntityIsNotNull( group, args.GroupIdentifier );

                    var groupId = args.GroupIdentifier.AsInteger();
                    var groupGuid = args.GroupIdentifier.AsGuid();

                    var roleId = args.GroupRoleIdentifier.AsIntegerOrNull() ?? 0;
                    var roleGuid = args.GroupRoleIdentifier.AsGuidOrNull();

                    var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                    var role = groupTypeRoleService.Queryable()
                        .FirstOrDefault( r => ( r.GroupTypeId == group.GroupTypeId )
                            && ( r.Id == roleId || r.Guid == roleGuid || r.Name == args.GroupRoleIdentifier ) );
                    AssertRockEntityIsNotNull( role, args.GroupRoleIdentifier );

                    var personService = new PersonService( rockContext );
                    var person = personService.Get( args.PersonIdentifier );
                    AssertRockEntityIsNotNull( person, args.PersonIdentifier );

                    groupMember = new GroupMember
                    {
                        ForeignKey = args.ForeignKey,
                        GroupId = group.Id,
                        PersonId = person.Id,
                        GroupRoleId = role.Id
                    };

                    var groupMemberService = new GroupMemberService( rockContext );
                    groupMemberService.Add( groupMember );

                    rockContext.SaveChanges();
                } );

                return groupMember;
            }

            public static bool DeleteGroup( RockContext rockContext, string groupIdentifier )
            {
                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var group = groupService.Get( groupIdentifier );

                    groupService.Delete( group, removeFromAuthTables: true );

                    rockContext.SaveChanges();
                } );

                return true;
            }
        }
    }
}
