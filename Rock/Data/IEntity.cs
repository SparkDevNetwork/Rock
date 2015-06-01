// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.DataAnnotations;

namespace Rock.Data
{
    /// <summary>
    /// Interface for all code-first entities
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        string ForeignId { get; set; }

        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        int TypeId { get; }

        /// <summary>
        /// Gets the unique type name of the entity.  Typically this is the qualified name of the class
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        string TypeName { get; }

        /// <summary>
        /// Gets the encrypted key.
        /// </summary>
        /// <value>
        /// The encrypted key.
        /// </value>
        string EncryptedKey { get; }

        /// <summary>
        /// Gets the context key.
        /// </summary>
        /// <value>
        /// The context key.
        /// </value>
        string ContextKey { get; }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        /// <value>
        /// The validation results.
        /// </value>
        List<ValidationResult> ValidationResults { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        bool IsValid { get; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        IEntity Clone();

        /// <summary>
        /// Creates a dictionary containing the majority of the entity object's properties
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> ToDictionary();

        /// <summary>
        /// Gets or sets the additional lava fields.
        /// </summary>
        /// <value>
        /// The additional lava fields.
        /// </value>
        Dictionary<string, object> AdditionalLavaFields { get; set; }
    }
}
