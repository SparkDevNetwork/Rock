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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;

namespace Rock.AI.Agent.Tests.Skills.StepSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="StepSkill"/>. Each tool's tests live
/// in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class StepSkillTests
{
    #region Support

    private static Rock.AI.Agent.Skills.StepSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.StepSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds a step program and returns it.
    /// </summary>
    private static StepProgram SeedStepProgram( RockContext rockContext, int id )
    {
        var stepProgram = new StepProgram
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Program {id}",
            IsActive = true
        };

        rockContext.Set<StepProgram>().Add( stepProgram );

        return stepProgram;
    }

    /// <summary>
    /// Seeds an active, manually editable step type in the program and returns
    /// it. The mocked context performs no navigation-property fixup, so the
    /// program is wired by hand because step security falls back through it.
    /// </summary>
    private static StepType SeedStepType( RockContext rockContext, int id, StepProgram stepProgram )
    {
        var stepType = new StepType
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Step Type {id}",
            StepProgramId = stepProgram.Id,
            StepProgram = stepProgram,
            IsActive = true,
            AllowManualEditing = true,
            AllowMultiple = true,
            StepTypePrerequisites = new List<StepTypePrerequisite>()
        };

        rockContext.Set<StepType>().Add( stepType );

        return stepType;
    }

    /// <summary>
    /// Seeds a step status in the program and returns it.
    /// </summary>
    private static StepStatus SeedStepStatus( RockContext rockContext, int id, StepProgram stepProgram )
    {
        var stepStatus = new StepStatus
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Status {id}",
            StepProgramId = stepProgram.Id,
            StepProgram = stepProgram,
            IsActive = true
        };

        rockContext.Set<StepStatus>().Add( stepStatus );

        return stepStatus;
    }

    /// <summary>
    /// Seeds an existing step of the specified type for the person and returns it.
    /// </summary>
    private static Step SeedStep( RockContext rockContext, int id, StepType stepType, StepStatus stepStatus, Rock.Model.Person person )
    {
        var step = new Step
        {
            Id = id,
            Guid = Guid.NewGuid(),
            StepTypeId = stepType.Id,
            StepType = stepType,
            StepStatusId = stepStatus.Id,
            StepStatus = stepStatus,
            PersonAliasId = person.PrimaryAliasId.Value,
            StartDateTime = RockDateTime.Now
        };

        rockContext.Set<Step>().Add( step );

        return step;
    }

    #endregion
}
