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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Rock.AI.Agent;
using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;

namespace Rock.Model
{
    public partial class AISkillToolService
    {
        /// <summary>
        /// Registers all AI skill tools defined on <paramref name="skillType"/>.
        /// New tools are added to the database. Existing tools are updated if
        /// necessary. Tools that no longer exist are not currently deleted from
        /// the database.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that these tools are associated with.</param>
        /// <param name="skillType">The C# type that represents the skill containing the tools to register.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        internal static void RegisterTools( int skillId, Type skillType, RockContext rockContext )
        {
            var existingTools = new AISkillToolService( rockContext )
                .Queryable()
                .ToList();

            var methods = skillType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );

            foreach ( var method in methods )
            {
                RegisterTool( skillId, method, existingTools, rockContext );
            }

            var serviceProvider = RockApp.Current.CreateScope().ServiceProvider;
            AgentSkillComponent instance;

            try
            {
                instance = ( AgentSkillComponent ) ActivatorUtilities.CreateInstance( serviceProvider, skillType );
            }
            catch
            {
                // Intentionally ignore any exceptions that occur when
                // trying to create an instance of the skill for registration.
                return;
            }

            var promptTools = instance.GetDymanicTools().Where( t => t.ToolType == ToolType.AIPrompt );

            foreach ( var semanticTool in promptTools )
            {
                RegisterDynamicTool( skillId, semanticTool, existingTools, rockContext );
            }
        }

        /// <summary>
        /// Registers a single AI tool by adding it to the database or
        /// updating the existing tool if it already exists in
        /// <paramref name="existingTools"/>.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that this tool is associated with.</param>
        /// <param name="method">The C# method that represents the tool to register.</param>
        /// <param name="existingTools">The existing tools in the database for this skill.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        private static void RegisterTool( int skillId, MethodInfo method, List<AISkillTool> existingTools, RockContext rockContext )
        {
            var toolGuid = method.GetCustomAttribute<AgentToolGuidAttribute>()?.Guid;

            if ( !toolGuid.HasValue )
            {
                return;
            }

            var tool = existingTools.FirstOrDefault( f => f.Guid == toolGuid.Value );
            var toolName = method.GetCustomAttribute<AgentToolNameAttribute>()?.Name;
            var name = toolName.IsNotNullOrWhiteSpace() ? toolName.SplitCase() : method.Name.SplitCase();
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var preamble = method.GetCustomAttribute<AgentToolPreambleAttribute>()?.Preamble ?? string.Empty;
            var purposes = method.GetCustomAttributes<AgentPurposeAttribute>()
                .Select( a => a.Purpose ?? string.Empty )
                .ToList();
            var returnDescription = method.GetCustomAttribute<AgentToolReturnDescriptionAttribute>()?.Description;
            var usages = method.GetCustomAttributes<AgentUsageAttribute>()
                .Select( a => a.Usage ?? string.Empty )
                .ToList();
            var guardrails = method.GetCustomAttributes<AgentGuardrailAttribute>()
                .Select( a => a.Guardrail ?? string.Empty )
                .ToList();
            var prerequisites = method.GetCustomAttributes<AgentToolPrerequisiteAttribute>()
                .Select( a => a.Prerequisite ?? string.Empty )
                .ToList();
            var examples = method.GetCustomAttributes<AgentToolExampleAttribute>()
                .Select( a => a.Example ?? string.Empty )
                .ToList();
            var needSave = false;

            if ( tool == null )
            {
                tool = rockContext.Set<AISkillTool>().Create();

                tool.Guid = toolGuid.Value;
                tool.AISkillId = skillId;
                tool.Name = name;
                tool.Description = description;
                tool.ToolType = ToolType.ExecuteCode;

                new AISkillToolService( rockContext ).Add( tool );

                needSave = true;
            }
            else
            {
                if ( tool.AISkillId != skillId )
                {
                    tool.AISkillId = skillId;
                    needSave = true;
                }

                if ( tool.Name != name )
                {
                    tool.Name = name;
                    needSave = true;
                }

                if ( tool.Description != description )
                {
                    tool.Description = description;
                    needSave = true;
                }

                if ( tool.ToolType != ToolType.ExecuteCode )
                {
                    tool.ToolType = ToolType.ExecuteCode;
                    needSave = true;
                }
            }

            var additionalSettings = tool.GetAdditionalSettings<ToolAdditionalSettings>();
            var instructionSettings = tool.GetAdditionalSettings<ToolInstructionSettings>();

            if ( additionalSettings.Preamble != preamble )
            {
                additionalSettings.Preamble = preamble;
                tool.SetAdditionalSettings( additionalSettings );
                needSave = true;
            }

            if ( instructionSettings.Purposes == null || !instructionSettings.Purposes.SequenceEqual( purposes ) )
            {
                instructionSettings.Purposes = purposes;
                tool.SetAdditionalSettings( instructionSettings );
                needSave = true;
            }

            if ( instructionSettings.ReturnDescription != returnDescription )
            {
                instructionSettings.ReturnDescription = returnDescription;
                tool.SetAdditionalSettings( instructionSettings );
                needSave = true;
            }

            if ( instructionSettings.Usages == null || !instructionSettings.Usages.SequenceEqual( usages ) )
            {
                instructionSettings.Usages = usages;
                tool.SetAdditionalSettings( instructionSettings );
                needSave = true;
            }

            if ( instructionSettings.Guardrails == null || !instructionSettings.Guardrails.SequenceEqual( guardrails ) )
            {
                instructionSettings.Guardrails = guardrails;
                tool.SetAdditionalSettings( instructionSettings );
                needSave = true;
            }

            if ( instructionSettings.Prerequisites == null || !instructionSettings.Prerequisites.SequenceEqual( prerequisites ) )
            {
                instructionSettings.Prerequisites = prerequisites;
                tool.SetAdditionalSettings( instructionSettings );
                needSave = true;
            }

            if ( instructionSettings.Examples == null || !instructionSettings.Examples.SequenceEqual( examples ) )
            {
                instructionSettings.Examples = examples;
                tool.SetAdditionalSettings( instructionSettings );
                needSave = true;
            }

            if ( needSave )
            {
                if ( tool.Id == 0 )
                {
                    tool.CreatedDateTime = RockDateTime.Now;
                }

                tool.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
            }
        }

        /// <summary>
        /// Registers a single AI tool by adding it to the database or
        /// updating the existing tool if it already exists in
        /// <paramref name="existingTools"/>.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that this tool is associated with.</param>
        /// <param name="semanticTool">The semantic tool to register.</param>
        /// <param name="existingTools">The existing tools in the database for this skill.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        private static void RegisterDynamicTool( int skillId, AgentTool semanticTool, List<AISkillTool> existingTools, RockContext rockContext )
        {
            if ( semanticTool.Guid == Guid.Empty )
            {
                return;
            }

            var tool = existingTools.FirstOrDefault( f => f.Guid == semanticTool.Guid );
            var name = semanticTool.Name.SplitCase();
            var description = semanticTool.Description;
            var instructions = semanticTool.Instructions;
            var needSave = false;

            if ( tool == null )
            {
                tool = rockContext.Set<AISkillTool>().Create();

                tool.Guid = semanticTool.Guid;
                tool.AISkillId = skillId;
                tool.Name = name;
                tool.Description = description;
                tool.ToolType = semanticTool.ToolType;

                new AISkillToolService( rockContext ).Add( tool );

                needSave = true;
            }
            else
            {
                if ( tool.AISkillId != skillId )
                {
                    tool.AISkillId = skillId;
                    needSave = true;
                }

                if ( tool.Name != name )
                {
                    tool.Name = name;
                    needSave = true;
                }

                if ( tool.Description != description )
                {
                    tool.Description = description;
                    needSave = true;
                }

                if ( tool.ToolType != semanticTool.ToolType )
                {
                    tool.ToolType = semanticTool.ToolType;
                    needSave = true;
                }
            }

            var additionalSettings = tool.GetAdditionalSettings<ToolAdditionalSettings>();

            if ( additionalSettings.Preamble != semanticTool.Preamble )
            {
                additionalSettings.Preamble = semanticTool.Preamble;
                tool.SetAdditionalSettings( additionalSettings );
                needSave = true;
            }

            if ( semanticTool.Instructions != null )
            {
                var instructionSettings = tool.GetAdditionalSettings<ToolInstructionSettings>();

                if ( instructionSettings.Purposes == null || !instructionSettings.Purposes.SequenceEqual( semanticTool.Instructions.Purposes ) )
                {
                    instructionSettings.Purposes = semanticTool.Instructions.Purposes;
                    tool.SetAdditionalSettings( instructionSettings );
                    needSave = true;
                }

                if ( instructionSettings.ReturnDescription != semanticTool.Instructions.ReturnDescription )
                {
                    instructionSettings.ReturnDescription = semanticTool.Instructions.ReturnDescription;
                    tool.SetAdditionalSettings( instructionSettings );
                    needSave = true;
                }

                if ( instructionSettings.Usages == null || !instructionSettings.Usages.SequenceEqual( semanticTool.Instructions.Usages ) )
                {
                    instructionSettings.Usages = semanticTool.Instructions.Usages;
                    tool.SetAdditionalSettings( instructionSettings );
                    needSave = true;
                }

                if ( instructionSettings.Guardrails == null || !instructionSettings.Guardrails.SequenceEqual( semanticTool.Instructions.Guardrails ) )
                {
                    instructionSettings.Guardrails = semanticTool.Instructions.Guardrails;
                    tool.SetAdditionalSettings( instructionSettings );
                    needSave = true;
                }

                if ( instructionSettings.Prerequisites == null || !instructionSettings.Prerequisites.SequenceEqual( semanticTool.Instructions.Prerequisites ) )
                {
                    instructionSettings.Prerequisites = semanticTool.Instructions.Prerequisites;
                    tool.SetAdditionalSettings( instructionSettings );
                    needSave = true;
                }

                if ( instructionSettings.Examples == null || !instructionSettings.Examples.SequenceEqual( semanticTool.Instructions.Examples ) )
                {
                    instructionSettings.Examples = semanticTool.Instructions.Examples;
                    tool.SetAdditionalSettings( instructionSettings );
                    needSave = true;
                }
            }
            else
            {
                var instructionSettings = tool.GetAdditionalSettingsOrNull<ToolInstructionSettings>();

                if ( instructionSettings != null )
                {
                    tool.RemoveAdditionalSettings<ToolInstructionSettings>();
                    needSave = true;
                }
            }

            if ( needSave )
            {
                if ( tool.Id == 0 )
                {
                    tool.CreatedDateTime = RockDateTime.Now;
                }

                tool.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
            }
        }
    }
}
