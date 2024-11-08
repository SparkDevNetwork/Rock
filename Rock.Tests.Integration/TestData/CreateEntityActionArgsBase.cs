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

namespace Rock.Tests.Integration.TestData
{
    public enum CreateExistingItemStrategySpecifier
    {
        Ignore = 0,
        Update = 1,
        Replace = 2,
        Fail = 3
    }

    /// <summary>
    /// A base class for actions that create Rock Entities.
    /// </summary>
    public abstract class CreateEntityActionArgsBase
    {
        /// <summary>
        /// The unique identifier of the new entity.
        /// If not specified, this value is automatically generated when the entity is saved.
        /// </summary>
        public Guid? Guid;

        /// <summary>
        /// A Foreign Key identifier for the entity.
        /// This is used as a secondary identifier for the entity, to associate it with a specific batch of test data.
        /// </summary>
        public string ForeignKey;

        /// <summary>
        /// The strategy for handling existing data that has an identical unique identifier.
        /// </summary>
        public CreateExistingItemStrategySpecifier ExistingItemStrategy = CreateExistingItemStrategySpecifier.Fail;
    }

    /// <summary>
    /// A base class for actions that create Rock Entities.
    /// </summary>
    // TODO: Properties in this class should be moved to an EntityPropertiesInfoBase class, which should form part of the Properties<T> constraint.
    public abstract class CreateEntityActionArgsBase<T>
        where T : class, new()
    {
        /// <summary>
        /// The unique identifier of the new entity.
        /// If not specified, this value is automatically generated when the entity is saved.
        /// </summary>
        public Guid? Guid;

        /// <summary>
        /// A Foreign Key identifier for the entity.
        /// This is used as a secondary identifier for the entity, to associate it with a specific batch of test data.
        /// </summary>
        public string ForeignKey;

        /// <summary>
        /// The strategy for handling existing data that has an identical unique identifier.
        /// </summary>
        public CreateExistingItemStrategySpecifier ExistingItemStrategy = CreateExistingItemStrategySpecifier.Fail;

        /// <summary>
        /// The entity properties.
        /// </summary>
        public T Properties { get; set; } = new T();
    }

    public abstract class UpdateEntityActionArgsBase<T>
        where T : class, new()
    {
        /// <summary>
        /// The unique identifier of the entity to be updated.
        /// </summary>
        public string UpdateTargetIdentifier { get; set; }

        /// <summary>
        /// The entity properties.
        /// </summary>
        public T Properties { get; set; } = new T();
    }
}
