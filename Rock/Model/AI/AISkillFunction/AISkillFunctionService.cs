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
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;

namespace Rock.Model
{
    public partial class AISkillFunctionService
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
        internal static void RegisterFunctions( int skillId, Type skillType, RockContext rockContext )
        {
            var existingTools = new AISkillFunctionService( rockContext )
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

            var semanticTools = instance.GetSemanticFunctions();

            foreach ( var semanticTool in semanticTools )
            {
                RegisterSemanticTool( skillId, semanticTool, existingTools, rockContext );
            }
        }

        /// <summary>
        /// Registers a single AI tool by adding it to the database or
        /// updating the existing functooltion if it already exists in
        /// <paramref name="existingTools"/>.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that this tool is associated with.</param>
        /// <param name="method">The C# method that represents the tool to register.</param>
        /// <param name="existingTools">The existing tools in the database for this skill.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        private static void RegisterTool( int skillId, MethodInfo method, List<AISkillFunction> existingTools, RockContext rockContext )
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
                tool = rockContext.Set<AISkillFunction>().Create();

                tool.Guid = toolGuid.Value;
                tool.AISkillId = skillId;
                tool.Name = name;
                tool.Description = description;
                tool.FunctionType = FunctionType.ExecuteCode;

                new AISkillFunctionService( rockContext ).Add( tool );

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

                if ( tool.FunctionType != FunctionType.ExecuteCode )
                {
                    tool.FunctionType = FunctionType.ExecuteCode;
                    needSave = true;
                }
            }

            var toolSettings = tool.GetAdditionalSettings<ToolInstructionSettings>();

            if ( toolSettings.Purposes == null || !toolSettings.Purposes.SequenceEqual( purposes ) )
            {
                toolSettings.Purposes = purposes;
                tool.SetAdditionalSettings( toolSettings );
                needSave = true;
            }

            if ( toolSettings.ReturnDescription != returnDescription )
            {
                toolSettings.ReturnDescription = returnDescription;
                tool.SetAdditionalSettings( toolSettings );
                needSave = true;
            }

            if ( toolSettings.Usages == null || !toolSettings.Usages.SequenceEqual( usages ) )
            {
                toolSettings.Usages = usages;
                tool.SetAdditionalSettings( toolSettings );
                needSave = true;
            }

            if ( toolSettings.Guardrails == null || !toolSettings.Guardrails.SequenceEqual( guardrails ) )
            {
                toolSettings.Guardrails = guardrails;
                tool.SetAdditionalSettings( toolSettings );
                needSave = true;
            }

            if ( toolSettings.Prerequisites == null || !toolSettings.Prerequisites.SequenceEqual( prerequisites ) )
            {
                toolSettings.Prerequisites = prerequisites;
                tool.SetAdditionalSettings( toolSettings );
                needSave = true;
            }

            if ( toolSettings.Examples == null || !toolSettings.Examples.SequenceEqual( examples ) )
            {
                toolSettings.Examples = examples;
                tool.SetAdditionalSettings( toolSettings );
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
        private static void RegisterSemanticTool( int skillId, AgentTool semanticTool, List<AISkillFunction> existingTools, RockContext rockContext )
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
                tool = rockContext.Set<AISkillFunction>().Create();

                tool.Guid = semanticTool.Guid;
                tool.AISkillId = skillId;
                tool.Name = name;
                tool.Description = description;
                tool.FunctionType = semanticTool.FunctionType;

                new AISkillFunctionService( rockContext ).Add( tool );

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

                if ( tool.FunctionType != semanticTool.FunctionType )
                {
                    tool.FunctionType = semanticTool.FunctionType;
                    needSave = true;
                }
            }

            if ( semanticTool.Instructions != null )
            {
                var toolSettings = tool.GetAdditionalSettings<ToolInstructionSettings>();

                if ( toolSettings.Purposes == null || !toolSettings.Purposes.SequenceEqual( semanticTool.Instructions.Purposes ) )
                {
                    toolSettings.Purposes = semanticTool.Instructions.Purposes;
                    tool.SetAdditionalSettings( toolSettings );
                    needSave = true;
                }

                if ( toolSettings.ReturnDescription != semanticTool.Instructions.ReturnDescription )
                {
                    toolSettings.ReturnDescription = semanticTool.Instructions.ReturnDescription;
                    tool.SetAdditionalSettings( toolSettings );
                    needSave = true;
                }

                if ( toolSettings.Usages == null || !toolSettings.Usages.SequenceEqual( semanticTool.Instructions.Usages ) )
                {
                    toolSettings.Usages = semanticTool.Instructions.Usages;
                    tool.SetAdditionalSettings( toolSettings );
                    needSave = true;
                }

                if ( toolSettings.Guardrails == null || !toolSettings.Guardrails.SequenceEqual( semanticTool.Instructions.Guardrails ) )
                {
                    toolSettings.Guardrails = semanticTool.Instructions.Guardrails;
                    tool.SetAdditionalSettings( toolSettings );
                    needSave = true;
                }

                if ( toolSettings.Prerequisites == null || !toolSettings.Prerequisites.SequenceEqual( semanticTool.Instructions.Prerequisites ) )
                {
                    toolSettings.Prerequisites = semanticTool.Instructions.Prerequisites;
                    tool.SetAdditionalSettings( toolSettings );
                    needSave = true;
                }

                if ( toolSettings.Examples == null || !toolSettings.Examples.SequenceEqual( semanticTool.Instructions.Examples ) )
                {
                    toolSettings.Examples = semanticTool.Instructions.Examples;
                    tool.SetAdditionalSettings( toolSettings );
                    needSave = true;
                }
            }
            else
            {
                var toolSettings = tool.GetAdditionalSettingsOrNull<ToolInstructionSettings>();

                if ( toolSettings != null )
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
