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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.UniversalSearch
{
    [TestClass]
    public class GroupIndexTests : DatabaseTestsBase
    {
        [TestMethod]
        [Ignore]
        public void TestLoadByModel()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            //var group = groupService.Get( 285761 ); // 50k members
            var group = groupService.Get( 285760 ); // 500k members

            var groupIndex = Rock.UniversalSearch.IndexModels.GroupIndex.LoadByModel( group );
        }

        [TestMethod]
        [Ignore]
        public void TestGroupIndexTransaction()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            var largeGroup = groupService.Get( 285761 ); // 50k members
            var veryLargeGroup = groupService.Get( 285760 ); // 500k members

            List<GroupMember> groupMembers = new List<GroupMember>();

            groupMembers.AddRange( groupMemberService.GetByGroupId( largeGroup.Id ).Take( 50 ) );
            groupMembers.AddRange( groupMemberService.GetByGroupId( veryLargeGroup.Id ).Take( 10 ) );

            foreach( var gm in groupMembers )
            {
                gm.GroupMemberStatus = GroupMemberStatus.Inactive;
                rockContext.SaveChanges();
            }
        }
    }
}
