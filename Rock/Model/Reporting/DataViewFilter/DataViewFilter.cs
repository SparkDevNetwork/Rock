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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Reporting;

namespace Rock.Model
{
    /*
	    06/12/2020 - MSB 
        This class is not only used by dataviews, but also by content channel filters to filter which items from the content channels should be shown, 
        and Registration Instance Group Placement for filtering purposes.
        The above two places will add records to the DataViewFilter table, but no corresponding records will be added to the DataView table so
        the DataViewFilter records will incorrectly appear to be orphans.
    */

    /// <summary>
    /// Represents a filter on a <see cref="Rock.Model.DataView"/> in Rock.
    /// </summary>
    [RockDomain( "Reporting" )]
    [NotAudited]
    [Table( "DataViewFilter" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "507E646B-9943-4DD6-8FB7-8BA9F95E6BD0")]
    public partial class DataViewFilter : Model<DataViewFilter>, IDataViewFilterDefinition
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the expression type of this DataViewFilter.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.FilterExpressionType" /> that represents the expression type for the filter.  When <c>FilterExpressionType.Filter</c> it represents a filter expression, when <c>FilterExpressionType.GroupAll</c> it means that 
        /// all conditions found in child expressions must be met, when <c>FilterExpressionType.GroupOr</c> it means that at least one condition found in the child filter expressions must be met.
        /// </value>
        [DataMember]
        public FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the DataViewFilterId of the parent DataViewFilter.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DataViewFilterId of the parent DataViewFilter. If this DataViewFilter does not have a parent, this value will be null.
        /// </value>
        [DataMember]
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Reporting.DataFilterComponent"/> that this filter is using.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being used in the filter.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the value that the DataViewFilter is filtering by.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the value to be used as a filter.
        /// </value>
        [DataMember]
        public string Selection { get; set; }

        /// <summary>
        /// Gets or sets the id of the Data View that owns this record.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the id of the data view that this record uses for filtering.
        /// </summary>
        /// <value>
        /// The related data view identifier.
        /// </value>
        [DataMember]
        public int? RelatedDataViewId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets sets the parent DataViewFilter.
        /// </summary>
        /// <value>
        /// The parent DataViewFilter.
        /// </value>
        [LavaVisible]
        public virtual DataViewFilter Parent { get; set; }

        /// <summary>
        /// Gets or sets the data view that owns this record.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the data view that this record uses to filter.
        /// </summary>
        /// <value>
        /// The related data view.
        /// </value>
        public virtual DataView RelatedDataView { get; set; }

        /// <summary>
        /// Gets or sets the EntityType of the <see cref="Rock.Reporting.DataFilterComponent" /> that this filter is using.
        /// </summary>
        /// <value>
        /// The DataFilterComponent EntityType
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the child DataViewFilters.
        /// </summary>
        /// <value>
        /// The child DataViewFilters.
        /// </value>
        [DataMember]
        public virtual ICollection<DataViewFilter> ChildFilters
        {
            get { return _filters ?? ( _filters = new Collection<DataViewFilter>() ); }
            set { _filters = value; }
        }
        private ICollection<DataViewFilter> _filters;

        #endregion

        #region IDataViewFilterDefinition Implementation

        /// <inheritdoc/>
        ICollection<IDataViewFilterDefinition> IDataViewFilterDefinition.ChildFilters => ChildFilters.Cast<IDataViewFilterDefinition>().ToList();

        /// <inheritdoc/>
        System.Guid? IDataViewFilterDefinition.Guid => this.Guid;

        /// <inheritdoc/>
        IDataViewDefinition IDataViewFilterDefinition.DataView => this.DataView;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class DataViewFilterConfiguration : EntityTypeConfiguration<DataViewFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFilterConfiguration"/> class.
        /// </summary>
        public DataViewFilterConfiguration()
        {
            this.HasOptional( r => r.Parent ).WithMany( r => r.ChildFilters ).HasForeignKey( r => r.ParentId ).WillCascadeOnDelete( false );
            this.HasOptional( e => e.EntityType ).WithMany().HasForeignKey( e => e.EntityTypeId ).WillCascadeOnDelete( false );

            this.HasOptional( r => r.DataView ).WithMany().HasForeignKey( r => r.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( e => e.RelatedDataView ).WithMany().HasForeignKey( e => e.RelatedDataViewId ).WillCascadeOnDelete( false );
        }
    }

#endregion
}
