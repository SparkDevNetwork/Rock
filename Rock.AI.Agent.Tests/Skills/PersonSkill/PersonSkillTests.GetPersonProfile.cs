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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Entity;
using Rock.Model;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

public partial class PersonSkillTests
{
    #region GetPersonProfile

    [TestMethod]
    public void GetKnownRelationshipResults_WithDeceasedRelatedPerson_ExcludesThem()
    {
        var owner = CreateTestPerson( 1, "Owner", AgeClassification.Adult );
        var living = CreateTestPerson( 2, "Living", AgeClassification.Adult );
        var deceased = CreateTestPerson( 3, "Deceased", AgeClassification.Adult, isDeceased: true );
        var group = CreateTestGroup(
            CreateTestMember( owner, 10, "Owner" ),
            CreateTestMember( living, 11, "Sibling" ),
            CreateTestMember( deceased, 12, "Parent" ) );

        var results = Rock.AI.Agent.Skills.PersonSkill.GetKnownRelationshipResults( group, owner.Id );

        Assert.HasCount( 1, results );
        Assert.AreEqual( living.Id, results[0].Person.Id );
    }

    [TestMethod]
    public void GetKnownRelationshipResults_WithLivingRelatedPerson_ReturnsTheirIdStatusAndRole()
    {
        var owner = CreateTestPerson( 1, "Owner", AgeClassification.Adult );
        var related = CreateTestPerson( 2, "Related", AgeClassification.Adult );
        var group = CreateTestGroup(
            CreateTestMember( owner, 10, "Owner" ),
            CreateTestMember( related, 11, "Sibling" ) );

        var result = Rock.AI.Agent.Skills.PersonSkill.GetKnownRelationshipResults( group, owner.Id ).Single();

        Assert.AreEqual( related.Id, result.Person.Id );
        Assert.AreEqual( "Related", result.Person.NickName );
        Assert.AreEqual( GroupMemberStatus.Active, result.Status );
        Assert.AreEqual( 11, result.Role.Id );
        Assert.AreEqual( "Sibling", result.Role.Name );
    }

    [TestMethod]
    public void PopulateAdults_WithDeceasedAdult_ExcludesThem()
    {
        var living = CreateTestPerson( 1, "Living", AgeClassification.Adult );
        var deceased = CreateTestPerson( 2, "Deceased", AgeClassification.Adult, isDeceased: true );
        var family = CreateTestGroup( CreateTestMember( living, 3, "Adult" ), CreateTestMember( deceased, 3, "Adult" ) );
        var profile = new PersonDetailResult();

        Rock.AI.Agent.Skills.PersonSkill.PopulateAdults( profile, family );

        CollectionAssert.AreEqual( new[] { living.Id }, profile.AdultsInFamily.Select( p => p.Id ).ToArray() );
    }

    [TestMethod]
    public void PopulateChildren_WithDeceasedChild_ExcludesThem()
    {
        var living = CreateTestPerson( 1, "Living", AgeClassification.Child );
        var deceased = CreateTestPerson( 2, "Deceased", AgeClassification.Child, isDeceased: true );
        var family = CreateTestGroup( CreateTestMember( living, 4, "Child" ), CreateTestMember( deceased, 4, "Child" ) );
        var profile = new PersonDetailResult();

        Rock.AI.Agent.Skills.PersonSkill.PopulateChildren( profile, family );

        CollectionAssert.AreEqual( new[] { living.Id }, profile.ChildrenInFamily.Select( p => p.Id ).ToArray() );
    }

    #endregion

    #region GetPersonProfile Support

    private static Rock.Model.Person CreateTestPerson( int id, string nickName, AgeClassification ageClassification, bool isDeceased = false )
    {
        return new Rock.Model.Person
        {
            Id = id,
            Guid = Guid.NewGuid(),
            FirstName = nickName,
            NickName = nickName,
            LastName = "Person",
            AgeClassification = ageClassification,
            IsDeceased = isDeceased
        };
    }

    private static GroupMember CreateTestMember( Rock.Model.Person person, int roleId, string roleName )
    {
        return new GroupMember
        {
            PersonId = person.Id,
            Person = person,
            GroupRoleId = roleId,
            GroupRole = new GroupTypeRole { Id = roleId, Guid = Guid.NewGuid(), Name = roleName },
            GroupMemberStatus = GroupMemberStatus.Active
        };
    }

    private static Group CreateTestGroup( params GroupMember[] members )
    {
        var group = new Group { Id = 100 };

        foreach ( var member in members )
        {
            member.GroupId = group.Id;
            member.Group = group;
            group.Members.Add( member );
        }

        return group;
    }

    #endregion
}
