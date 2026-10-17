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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="PersonSkill"/>.
/// </summary>
[TestClass]
public partial class PersonSkillTests
{
    private const string OriginalEmail = "p9v2h-original@example.invalid";
    private const string NewEmail = "p9v2h-new@example.invalid";
    private const string OriginalNumber = "6235550201";
    private const string NewNumber = "6235550299";
    private const string NotAuthorizedMessage = "You are not authorized to edit this person.";
    private const string LookupsRequiredMessage = "Lookups Required";

    #region GetPersonAvailableAttributes

    [TestMethod]
    public void GetPersonAvailableAttributes_WithValidPerson_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = new Rock.Model.Person
        {
            Id = 80,
            Guid = new Guid( "9b000001-0000-4000-8000-000000000001" ),
            FirstName = "Test",
            LastName = "Person",
            PrimaryAliasId = 81
        };

        rockContext.Set<Rock.Model.Person>().Add( person );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPersonAvailableAttributes( IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetPersonAvailableAttributes_WithMissingPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPersonAvailableAttributes( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion

    #region Support

    private static Rock.AI.Agent.Skills.PersonSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.PersonSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds a person with an active email address and returns it.
    /// </summary>
    private static Rock.Model.Person CreatePersonWithEmail( RockContext rockContext, string firstName )
    {
        var person = MockData.CreatePerson( rockContext, firstName, "Person" );

        person.NickName = firstName;
        person.Email = OriginalEmail;
        person.IsEmailActive = true;

        return person;
    }

    private static void AssertNotAuthorized( AgentToolResult result )
    {
        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages(), NotAuthorizedMessage );
    }

    #endregion
}
