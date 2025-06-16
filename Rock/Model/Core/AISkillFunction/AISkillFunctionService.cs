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

using Microsoft.SemanticKernel;

using Rock.AI.Agent;
using Rock.Data;
using Rock.Enums.Core.AI.Agent;
using Rock.SystemGuid;

namespace Rock.Model
{
    public partial class AISkillFunctionService
    {
        /// <summary>
        /// Registers all AI skill functions defined on <paramref name="skillType"/>.
        /// New functions are added to the database. Existing fuinctions are
        /// updated if necessary. Funcations that no longer exist are not
        /// currently deleted from the database.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that these functions are associated with.</param>
        /// <param name="skillType">The C# type that represents the skill containing the functions to register.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        internal static void RegisterFunctions( int skillId, Type skillType, RockContext rockContext )
        {
            var existingFunctions = new AISkillFunctionService( rockContext )
                .Queryable()
                .Where( f => f.AISkillId == skillId )
                .ToList();

            var methods = skillType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );

            foreach ( var method in methods )
            {
                RegisterFunction( skillId, method, existingFunctions, rockContext );
            }

            if ( skillType.GetConstructor( Type.EmptyTypes ) != null )
            {
                AgentSkill instance;

                try
                {
                    instance = ( AgentSkill ) Activator.CreateInstance( skillType );
                }
                catch
                {
                    // Intentionally ignore any exceptions that occur when
                    // trying to create an instance of the skill for registration.
                    return;
                }

                var semanticFunctions = instance.GetSemanticFunctions();

                foreach ( var semanticFunction in semanticFunctions )
                {
                    RegisterSemanticFunction( skillId, semanticFunction, existingFunctions, rockContext );
                }
            }
        }

        /// <summary>
        /// Registers a single AI function by adding it to the database or
        /// updating the existing function if it already exists in
        /// <paramref name="existingFunctions"/>.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that this function is associated with.</param>
        /// <param name="method">The C# method that represents the function to register.</param>
        /// <param name="existingFunctions">The existing functions in the database for this skill.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        private static void RegisterFunction( int skillId, MethodInfo method, List<AISkillFunction> existingFunctions, RockContext rockContext )
        {
            var kernelFunction = method.GetCustomAttribute<KernelFunctionAttribute>();
            var functionGuid = method.GetCustomAttribute<AgentFunctionGuidAttribute>()?.Guid;

            if ( !functionGuid.HasValue )
            {
                return;
            }

            if ( kernelFunction == null )
            {
                return;
            }

            var function = existingFunctions.FirstOrDefault( f => f.Guid == functionGuid.Value );
            var name = kernelFunction.Name ?? method.Name.SplitCase();
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var needSave = false;

            if ( function == null )
            {
                function = rockContext.Set<AISkillFunction>().Create();

                function.Guid = functionGuid.Value;
                function.AISkillId = skillId;
                function.Name = name;
                function.Description = description;
                function.UsageHint = description;
                function.FunctionType = FunctionType.ExecuteCode;

                new AISkillFunctionService( rockContext ).Add( function );

                needSave = true;
            }
            else
            {
                if ( function.Name != name )
                {
                    function.Name = name;
                    needSave = true;
                }

                if ( function.Description != description )
                {
                    function.Description = description;
                    needSave = true;
                }

                if ( function.UsageHint != description )
                {
                    function.UsageHint = description;
                    needSave = true;
                }

                if ( function.FunctionType != FunctionType.ExecuteCode )
                {
                    function.FunctionType = FunctionType.ExecuteCode;
                    needSave = true;
                }
            }

            if ( needSave )
            {
                if ( function.Id == 0 )
                {
                    function.CreatedDateTime = RockDateTime.Now;
                }

                function.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
            }
        }

        /// <summary>
        /// Registers a single AI function by adding it to the database or
        /// updating the existing function if it already exists in
        /// <paramref name="existingFunctions"/>.
        /// </summary>
        /// <param name="skillId">The identifier of the AI skill that this function is associated with.</param>
        /// <param name="semanticFunction">The semantic function to register.</param>
        /// <param name="existingFunctions">The existing functions in the database for this skill.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        private static void RegisterSemanticFunction( int skillId, AgentFunction semanticFunction, List<AISkillFunction> existingFunctions, RockContext rockContext )
        {
            if ( semanticFunction.Guid == Guid.Empty )
            {
                return;
            }

            var function = existingFunctions.FirstOrDefault( f => f.Guid == semanticFunction.Guid );
            var name = semanticFunction.Name.SplitCase();
            var description = semanticFunction.UsageHint;
            var needSave = false;

            if ( function == null )
            {
                function = rockContext.Set<AISkillFunction>().Create();

                function.Guid = semanticFunction.Guid;
                function.AISkillId = skillId;
                function.Name = name;
                function.Description = description;
                function.UsageHint = description;
                function.FunctionType = semanticFunction.FunctionType;

                new AISkillFunctionService( rockContext ).Add( function );

                needSave = true;
            }
            else
            {
                if ( function.Name != name )
                {
                    function.Name = name;
                    needSave = true;
                }

                if ( function.Description != description )
                {
                    function.Description = description;
                    needSave = true;
                }

                if ( function.UsageHint != description )
                {
                    function.UsageHint = description;
                    needSave = true;
                }

                if ( function.FunctionType != semanticFunction.FunctionType )
                {
                    function.FunctionType = semanticFunction.FunctionType;
                    needSave = true;
                }
            }

            if ( needSave )
            {
                if ( function.Id == 0 )
                {
                    function.CreatedDateTime = RockDateTime.Now;
                }

                function.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
            }
        }
    }
}
