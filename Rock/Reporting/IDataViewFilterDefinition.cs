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

using Rock.Attribute;
using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    /// Represents a filter that forms part of a Data View.
    /// </summary>
    [RockInternal( "1.16.3", true )]
    public interface IDataViewFilterDefinition
    {
        /// <summary>
        /// Gets the unique persisted identifier of the Data View.
        /// If the Data View is not persisted, this value is set to 0.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets an internal identifier for the Data View Filter that can be used to create internal references.
        /// </summary>
        Guid? Guid { get; }

        /// <summary>
        /// Gets the id of the Data View that owns this record.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        int? DataViewId { get; }

        /// <summary>
        /// Gets the EntityTypeId of the <see cref="Rock.Reporting.DataFilterComponent"/> that this filter is using.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being used in the filter.
        /// </value>
        int? EntityTypeId { get; }

        /// <summary>
        /// Gets the expression type of this DataViewFilter.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.FilterExpressionType" /> that represents the expression type for the filter.  When <c>FilterExpressionType.Filter</c> it represents a filter expression, when <c>FilterExpressionType.GroupAll</c> it means that 
        /// all conditions found in child expressions must be met, when <c>FilterExpressionType.GroupOr</c> it means that at least one condition found in the child filter expressions must be met.
        /// </value>
        FilterExpressionType ExpressionType { get; }

        /// <summary>
        /// Gets the DataViewFilterId of the parent DataViewFilter.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DataViewFilterId of the parent DataViewFilter. If this DataViewFilter does not have a parent, this value will be null.
        /// </value>
        int? ParentId { get; }

        /// <summary>
        /// Gets the id of the data view that this record uses for filtering.
        /// </summary>
        /// <value>
        /// The related data view identifier.
        /// </value>
        int? RelatedDataViewId { get; }

        /// <summary>
        /// Gets the settings and values that determine the operation of this filter.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the value to be used as a filter.
        /// </value>
        string Selection { get; }

        /// <summary>
        /// Gets the date/time on which this item was last modified.
        /// </summary>
        /// <value>
        /// The date/time on which this item was last modified.
        /// </value>
        DateTime? ModifiedDateTime { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        string ToString();

        /// <summary>
        /// Gets the optional DataView with which this filter is associated, if applicable.
        /// Filters that are not associated with a specific DataView exist as configuration parameters for other Rock components.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Reporting.IDataViewDefinition" /> object, or null if the filter is not associated with a DataView.
        /// </value>
        IDataViewDefinition DataView { get; }

        /// <summary>
        /// Gets the child DataViewFilters.
        /// </summary>
        /// <value>
        /// The child DataViewFilters.
        /// </value>
        ICollection<IDataViewFilterDefinition> ChildFilters { get; }
    }
}
