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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Report (based off of a <see cref="Rock.Model.DataView"/> in Rock).
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "Report" )]
    [DataContract]
    public partial class Report : Model<Report>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Report is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if the Report is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Report. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Report.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Report's Description.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Report's Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that the Report belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CateogryId of the <see cref="Rock.Model.Category"/> that the report belongs to. If the Report does not belong to
        /// a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or the DataViewId of the root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DataViewId of the root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the number of records to fetch in the report.  Null means all records.
        /// </summary>
        /// <value>
        /// The fetch top.
        /// </value>
        [DataMember]
        public int? FetchTop { get; set; }

        /// <summary>
        /// Gets or sets the query hint that is included in the SQL that is executed on the database server
        /// </summary>
        /// <value>
        /// The query hint.
        /// </value>
        [DataMember]
        public string QueryHint { get; set; }

        /// <summary>
        /// Gets or sets the last run date time.
        /// </summary>
        /// <value>
        /// The last run date time.
        /// </value>
        [DataMember]
        public DateTime? LastRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets the persisted last run duration in mulliseconds.
        /// </summary>
        /// <value>
        /// The persisted last run duration in mulliseconds.
        /// </value>
        [DataMember]
        public int? RunCount { get; set; }

        /// <summary>
        /// The amount of time in milliseconds that it took to run the <see cref="DataView"/>
        /// </summary>
        /// <value>
        /// The time to run in ms.
        /// </value>
        [DataMember]
        public int? TimeToRunDurationMilliseconds { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this Report belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this Report belongs to. If the Report does not belong to a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> that is being reported on. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the base/root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </value>
        [DataMember]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the report fields.
        /// </summary>
        /// <value>
        /// The report fields.
        /// </value>
        [DataMember]
        public virtual ICollection<ReportField> ReportFields
        {
            get
            {
                return _reportFields ?? ( _reportFields = new Collection<ReportField>() );
            }

            set
            {
                _reportFields = value;
            }
        }
        private ICollection<ReportField> _reportFields;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class ReportConfiguration : EntityTypeConfiguration<Report>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        public ReportConfiguration()
        {
            this.HasOptional( r => r.Category ).WithMany().HasForeignKey( r => r.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.DataView ).WithMany().HasForeignKey( r => r.DataViewId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
