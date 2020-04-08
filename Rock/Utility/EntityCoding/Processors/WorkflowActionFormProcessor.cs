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

namespace Rock.Utility.EntityCoding.Processors
{
    /// <summary>
    /// Handles processing of WorkflowActionForm entities.
    /// </summary>
    public class WorkflowActionFormProcessor : EntityProcessor<WorkflowActionForm>
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        public override Guid Identifier { get { return new Guid( "5df81bd2-6d4e-438e-98a8-6e756b738264" ); } }

        /// <summary>
        /// Evaluate the list of referenced entities. This is a list of key value pairs that identify
        /// the property that the reference came from as well as the referenced entity itself. Implementations
        /// of this method may add or remove from this list. For example, an AttributeValue has
        /// the entity it is referencing in a EntityId column, but there is no general use information for
        /// what kind of entity it is. The processor can provide that information.
        /// </summary>
        /// <param name="entity">The parent entity of the references.</param>
        /// <param name="references"></param>
        /// <param name="helper">The helper class for this export.</param>
        protected override void EvaluateReferencedEntities( WorkflowActionForm entity, List<KeyValuePair<string, IEntity>> references, EntityCoder helper )
        {
            //
            // Workflow Action Forms have a string field of "Actions" that contains references
            // to Defined Values for the button types.
            //
            List<string> actions = entity.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            for ( int i = 0; i < actions.Count; i++ )
            {
                var details = actions[i].Split( new char[] { '^' } );

                if ( details.Length > 2 )
                {
                    Guid definedValueGuid = details[1].AsGuid();
                    IEntity definedValue = helper.GetExistingEntity( "Rock.Model.DefinedValue", definedValueGuid );

                    if ( definedValue != null )
                    {
                        references.Add( new KeyValuePair<string, IEntity>( "Actions", definedValue ) );
                    }
                }
            }
        }

        /// <summary>
        /// An entity has been exported and can now have any post-processing done to it
        /// that is needed. For example a processor might remove some properties that shouldn't
        /// actually have been exported.
        /// </summary>
        /// <param name="entity">The source entity that was exported.</param>
        /// <param name="encodedEntity">The exported data from the entity.</param>
        /// <param name="helper">The helper that is doing the exporting.</param>
        /// <returns>
        /// An object that will be encoded with the entity and passed to the ProcessImportEntity method later, or null.
        /// </returns>
        protected override object ProcessExportedEntity( WorkflowActionForm entity, EncodedEntity encodedEntity, EntityCoder helper )
        {
            //
            // Return the Actions data that we will pre-process on import to fixup any references.
            //
            return entity.Actions;
        }

        /// <summary>
        /// This method is called before the entity is saved and allows any final changes to the
        /// entity before it is stored in the database. Any Guid references that are not standard
        /// properties must also be updated, such as the Actions string of a WorkflowActionForm.
        /// </summary>
        /// <param name="entity">The in-memory entity that is about to be saved.</param>
        /// <param name="encodedEntity">The encoded information that was used to reconstruct the entity.</param>
        /// <param name="data">Custom data that was previously returned by ProcessExportedEntity.</param>
        /// <param name="helper">The helper in charge of the import process.</param>
        protected override void ProcessImportedEntity( WorkflowActionForm entity, EncodedEntity encodedEntity, object data, EntityDecoder helper )
        {
            if ( data != null && data is string )
            {
                //
                // Update the Guids in all the action buttons.
                //
                List<string> actions = ( ( string ) data ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                for ( int i = 0; i < actions.Count; i++ )
                {
                    var details = actions[i].Split( new char[] { '^' } );
                    if ( details.Length > 2 )
                    {
                        Guid definedValueGuid = details[1].AsGuid();
                        Guid? activityTypeGuid = details[2].AsGuidOrNull();

                        details[1] = helper.FindMappedGuid( definedValueGuid ).ToString();
                        if ( activityTypeGuid.HasValue )
                        {
                            details[2] = helper.FindMappedGuid( activityTypeGuid.Value ).ToString();
                        }

                        actions[i] = string.Join( "^", details );
                    }
                }

                entity.Actions = string.Join( "|", actions );
            }
        }
    }
}
