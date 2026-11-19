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

using Rock.AI.Agent;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.PrayerSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="PrayerSkill"/>. Each tool's tests live
/// in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class PrayerSkillTests
{
    private const string OriginalText = "P3C43-original";

    #region Support

    private static Rock.AI.Agent.Skills.PrayerSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.PrayerSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds an approved prayer request in a prayer category and returns it.
    /// </summary>
    private static PrayerRequest SeedPrayerRequest( RockContext rockContext, int id )
    {
        var category = new Category
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = "General",
            EntityTypeId = EntityTypeCache.Get<PrayerRequest>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );

        var prayerRequest = new PrayerRequest
        {
            Id = id,
            Guid = Guid.NewGuid(),
            FirstName = "P3C43",
            LastName = "One",
            Text = OriginalText,
            CategoryId = category.Id,
            Category = category,
            IsActive = true,
            IsApproved = true,
            IsPublic = false,
            IsUrgent = false
        };

        rockContext.Set<PrayerRequest>().Add( prayerRequest );

        return prayerRequest;
    }

    /// <summary>
    /// Sets the person who requested the prayer request.
    /// </summary>
    private static void SetRequester( PrayerRequest prayerRequest, Rock.Model.Person person )
    {
        prayerRequest.RequestedByPersonAlias = person.Aliases.First();
        prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
    }

    /// <summary>
    /// Sets the person who created the prayer request.
    /// </summary>
    private static void SetCreator( PrayerRequest prayerRequest, Rock.Model.Person person )
    {
        prayerRequest.CreatedByPersonAlias = person.Aliases.First();
        prayerRequest.CreatedByPersonAliasId = person.PrimaryAliasId;
    }

    #endregion
}
