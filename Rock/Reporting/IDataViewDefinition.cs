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

using Rock.Attribute;

namespace Rock.Reporting
{
    /// <summary>
    /// A representation of a Data View that can be materialized to construct an Entity queryable.
    /// </summary>
    [RockInternal( "1.16.3", true )]
    public interface IDataViewDefinition
    {
        /// <summary>
        /// Gets the unique persisted identifier of the Data View.
        /// If the Data View is not persisted, this value is set to 0.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        int? CategoryId { get; }

        /// <summary>
        /// Gets the DataViewFilterId of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the DataViewFilterId of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. If there is 
        /// not a filter on this DataView, this value will be null.
        /// </value>
        int? DataViewFilterId { get; }

        /// <summary>
        /// Gets the DataViewFilter of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Reporting.IDataViewFilterDefinition"/> that represents the DataViewFilter of the root/base <see cref="Rock.Reporting.IDataViewFilterDefinition"/> that is used to generate this DataView. If there is 
        /// no filter on this DataView, this property will be null.
        /// </value>
        IDataViewFilterDefinition DataViewFilter { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }

        /// <summary>
        /// Gets a flag indicating if the use of a read-only Rock Context is disabled for this Data View.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable use of read-only]; otherwise, <c>false</c>.
        /// </value>
        bool DisableUseOfReadOnlyContext { get; }

        /// <summary>
        /// Gets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> (Rock.Data.IEntity) that this DataView reports on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        int? EntityTypeId { get; }

        /// <summary>
        /// Gets a value indicating whether deceased people should be included in the result set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include deceased]; otherwise, <c>false</c>.
        /// </value>
        bool IncludeDeceased { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a system-defined object.
        /// System objects cannot be modified or deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        bool IsSystem { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the persisted last refresh date time.
        /// </summary>
        /// <value>
        /// The persisted last refresh date time.
        /// </value>
        DateTime? PersistedLastRefreshDateTime { get; }

        /// <summary>
        /// Gets the Persisted Schedule Id.
        /// If this is null, then the DataView does not have a persisted schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/> for this DataView.
        /// If it does not have a persisted schedule, this value will be null.
        /// </value>
        int? PersistedScheduleId { get; }

        /// <summary>
        /// Returns true if this DataView is configured to be Persisted.
        /// </summary>
        /// <returns><c>true</c> if this instance is persisted; otherwise, <c>false</c>.</returns>
        int? PersistedScheduleIntervalMinutes { get; }

        /// <summary>
        /// Gets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> (MEF Component) that is used for an optional transformation on this DataView.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is used for an optional transformation on this DataView. If there
        /// is not a transformation on this DataView, this value will be null.
        /// </value>
        int? TransformEntityTypeId { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        string ToString();
    }
}
