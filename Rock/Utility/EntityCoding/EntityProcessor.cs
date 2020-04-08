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

using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Entity processors must inherit from this class to be able to provide
    /// custom processing capabilities.
    /// </summary>
    /// <typeparam name="T">The IEntity class type that this processor is for.</typeparam>
    public abstract class EntityProcessor<T> : IEntityProcessor where T : IEntity
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        abstract public Guid Identifier { get; }

        /// <summary>
        /// Evaluate the list of referenced entities. This is a list of key value pairs that identify
        /// the property that the reference came from as well as the referenced entity itself. Implementations
        /// of this method may add or remove from this list. For example, an AttributeValue has
        /// the entity it is referencing in a EntityId column, but there is no general use information for
        /// what kind of entity it is. The processor can provide that information.
        /// </summary>
        /// <param name="entity">The parent entity of the references.</param>
        /// <param name="references">The referenced entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        public void EvaluateReferencedEntities( IEntity entity, List<KeyValuePair<string, IEntity>> references, EntityCoder helper )
        {
            EvaluateReferencedEntities( ( T ) entity, references, helper );
        }

        /// <summary>
        /// Evaluate the list of referenced entities. This is a list of key value pairs that identify
        /// the property that the reference came from as well as the referenced entity itself. Implementations
        /// of this method may add or remove from this list. For example, an AttributeValue has
        /// the entity it is referencing in a EntityId column, but there is no general use information for
        /// what kind of entity it is. The processor can provide that information.
        /// </summary>
        /// <param name="entity">The parent entity of the references.</param>
        /// <param name="references">The referenced entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        protected virtual void EvaluateReferencedEntities( T entity, List<KeyValuePair<string, IEntity>> references, EntityCoder helper )
        {
        }

        /// <summary>
        /// Evaluate the list of child entities. This is a list of key value pairs that identify
        /// the property that the child came from as well as the child entity itself. Implementations
        /// of this method may add or remove from this list. For example, a WorkflowActionForm has
        /// it's actions encoded in a single string. This must processed to include any other
        /// objects that should exist (such as a DefinedValue for the button type).
        /// </summary>
        /// <param name="entity">The parent entity of the children.</param>
        /// <param name="children">The child entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        public void EvaluateChildEntities( IEntity entity, List<KeyValuePair<string, IEntity>> children, EntityCoder helper )
        {
            EvaluateChildEntities( ( T ) entity, children, helper );
        }

        /// <summary>
        /// Evaluate the list of child entities. This is a list of key value pairs that identify
        /// the property that the child came from as well as the child entity itself. Implementations
        /// of this method may add or remove from this list. For example, a WorkflowActionForm has
        /// it's actions encoded in a single string. This must processed to include any other
        /// objects that should exist (such as a DefinedValue for the button type).
        /// </summary>
        /// <param name="entity">The parent entity of the children.</param>
        /// <param name="children">The child entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        protected virtual void EvaluateChildEntities( T entity, List<KeyValuePair<string, IEntity>> children, EntityCoder helper )
        {
        }

        /// <summary>
        /// An entity has been exported and can now have any post-processing done to it
        /// that is needed. For example a processor might remove some properties that shouldn't
        /// actually have been exported.
        /// </summary>
        /// <param name="entity">The source entity that was exported.</param>
        /// <param name="encodedEntity">The exported data from the entity.</param>
        /// <param name="helper">The helper that is doing the exporting.</param>
        /// <returns>An object that will be encoded with the entity and passed to the ProcessImportEntity method later, or null.</returns>
        public object ProcessExportedEntity( IEntity entity, EncodedEntity encodedEntity, EntityCoder helper )
        {
            return ProcessExportedEntity( ( T ) entity, encodedEntity, helper );
        }

        /// <summary>
        /// An entity has been exported and can now have any post-processing done to it
        /// that is needed. For example a processor might remove some properties that shouldn't
        /// actually have been exported.
        /// </summary>
        /// <param name="entity">The source entity that was exported.</param>
        /// <param name="encodedEntity">The exported data from the entity.</param>
        /// <param name="helper">The helper that is doing the exporting.</param>
        /// <returns>An object that will be encoded with the entity and passed to the ProcessImportEntity method later, or null.</returns>
        protected virtual object ProcessExportedEntity( T entity, EncodedEntity encodedEntity, EntityCoder helper )
        {
            return null;
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
        public void ProcessImportedEntity( IEntity entity, EncodedEntity encodedEntity, object data, EntityDecoder helper )
        {
            ProcessImportedEntity( ( T ) entity, encodedEntity, data, helper );
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
        protected virtual void ProcessImportedEntity( T entity, EncodedEntity encodedEntity, object data, EntityDecoder helper )
        {
        }
    }
}
