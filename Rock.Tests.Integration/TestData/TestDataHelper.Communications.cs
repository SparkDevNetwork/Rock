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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData
{
    public static partial class TestDataHelper
    {
        #region Communications

        public static class Communications
        {
            public class CreateCommunicationListArgs : CreateEntityActionArgsBase
            {
                public string Name;
            }

            public static Group CreateCommunicationList( CreateCommunicationListArgs args )
            {
                Group listGroup = null;

                var rockContext = new RockContext();
                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );

                    if ( args.Guid != null )
                    {
                        listGroup = groupService.Get( args.Guid.Value );
                        if ( listGroup != null )
                        {
                            if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                            {
                                throw new Exception( "Item exists." );
                            }
                            else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Ignore )
                            {
                                return;
                            }
                            else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                            {
                                var isDeleted = DeleteCommunicationList( args.Guid.ToString() );
                                if ( !isDeleted )
                                {
                                    throw new Exception( "Could not replace existing item." );
                                }
                                listGroup = null;
                            }
                        }
                    }

                    if ( listGroup == null )
                    {
                        listGroup = new Group();
                    }

                    var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST );

                    listGroup.GroupTypeId = groupType.Id;
                    listGroup.Name = args.Name;
                    listGroup.Guid = args.Guid ?? Guid.NewGuid();
                    listGroup.IsActive = true;
                    listGroup.ForeignKey = args.ForeignKey;

                    groupService.Add( listGroup );

                    rockContext.SaveChanges();
                } );

                return listGroup;
            }

            public static bool DeleteCommunicationList( string communicationListIdentifier )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );

                var listGroup = groupService.Get( communicationListIdentifier );

                if ( listGroup == null )
                {
                    return false;
                }

                groupService.Delete( listGroup );

                rockContext.SaveChanges();

                return true;
            }

            public class CommunicationListAddPeopleArgs
            {
                public string CommunicationListGroupIdentifier;
                public List<string> PersonIdentifiers;
                public string ForeignKey;
            }

            public static int AddPeopleToCommunicationList( CommunicationListAddPeopleArgs args )
            {
                var rockContext = new RockContext();

                var groupService = new GroupService( rockContext );
                var listGroup = groupService.Get( args.CommunicationListGroupIdentifier );

                var groupMembers = new List<GroupMember>();

                var personIdList = args.PersonIdentifiers.Select( i => i.ToIntSafe( 0 ) ).ToList();

                if ( personIdList.Any( i => i == 0 ) )
                {
                    throw new Exception( "One or more Person identifiers could not be resolved." );
                }

                var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST );
                var memberRoleId = groupType.Roles
                    .Where( r => r.Name == "Recipient" )
                    .Select( r => r.Id )
                    .FirstOrDefault();

                if ( memberRoleId == 0 )
                {
                    throw new Exception( "Role \"Member\" not defined for Communication List." );
                }

                foreach ( var personId in personIdList )
                {
                    var groupMember = new GroupMember()
                    {
                        GroupId = listGroup.Id,
                        GroupTypeId = groupType.Id,
                        GroupMemberStatus = GroupMemberStatus.Active,
                        PersonId = personId,
                        GroupRoleId = memberRoleId
                    };
                    groupMembers.Add( groupMember );
                }

                rockContext.BulkInsert( groupMembers );

                return personIdList.Count;
            }
        }

        #endregion
    }
}
