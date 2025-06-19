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

using Rock.AI.Agent;
using Rock.Data;
using Rock.Observability;
using Rock.SystemGuid;

namespace Rock.Model
{
    public partial class AISkillService
    {
        /// <summary>
        /// Registers all AI skills defined in the system by scanning for types
        /// that implement <see cref="AgentSkillComponent"/>. New skills are added to
        /// the database. Existing skills are updated if necessary. Skills that
        /// no longer exist are not currently deleted from the database.
        /// </summary>
        internal static void RegisterSkills()
        {
            using ( ObservabilityHelper.StartActivity( "Register AI Skills" ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Load all skills at once for performance.
                    var existingSkills = new AISkillService( rockContext ).Queryable().ToList();

                    var skillTypes = Reflection.FindTypes( typeof( AgentSkillComponent ) )
                        .Select( t => t.Value )
                        .Where( t => t.IsClass
                            && !t.IsAbstract
                            && t.GetCustomAttribute<AgentSkillGuidAttribute>() != null )
                        .ToList();

                    foreach ( var skillType in skillTypes )
                    {
                        try
                        {
                            RegisterSkillType( skillType, existingSkills, rockContext );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( new Exception( $"Failed to register AI skill type {skillType.FullName}'.", ex ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers a single AI skill type by adding it to the database or
        /// updating the existing skill if it already exists in
        /// <paramref name="existingSkills"/>.
        /// </summary>
        /// <param name="skillType">The C# type that represents the skill to register.</param>
        /// <param name="existingSkills">The existing skills in the database.</param>
        /// <param name="rockContext">The context to use when saving changes to the database.</param>
        private static void RegisterSkillType( Type skillType, List<AISkill> existingSkills, RockContext rockContext )
        {
            var entityTypeService = new EntityTypeService( rockContext );
            var skillGuid = skillType.GetCustomAttribute<AgentSkillGuidAttribute>().Guid;
            var skill = existingSkills.FirstOrDefault( s => s.Guid == skillGuid );
            var name = skillType.Name.SplitCase();
            var description = skillType.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var needSave = false;

            if ( skill == null )
            {
                skill = rockContext.Set<AISkill>().Create();

                skill.Guid = skillGuid;
                skill.Name = name;
                skill.Description = skillType.GetCustomAttribute<DescriptionAttribute>()?.Description;
                skill.UsageHint = skill.Description;

                // Skip the cache because there is almost zero chance it will
                // be in cache already and that would just cause an additional
                // query to the database.
                skill.CodeEntityTypeId = entityTypeService.Get( skillType, true, null ).Id;

                new AISkillService( rockContext ).Add( skill );

                needSave = true;
            }
            else
            {
                if ( skill.Name != name )
                {
                    skill.Name = name;
                    needSave = true;
                }

                if ( skill.Description != description )
                {
                    skill.Description = description;
                    needSave = true;
                }

                if ( skill.UsageHint != description )
                {
                    skill.UsageHint = description;
                    needSave = true;
                }
            }

            if ( needSave )
            {
                if ( skill.Id == 0 )
                {
                    skill.CreatedDateTime = RockDateTime.Now;
                }

                skill.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
            }

            AISkillFunctionService.RegisterFunctions( skill.Id, skillType, rockContext );
        }
    }
}
