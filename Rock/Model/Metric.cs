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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Metric POCO Entity.
    /// </summary>
    [Table( "Metric" )]
    [DataContract]
    public partial class Metric : Model<Metric>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Metric is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the Metric is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Title of this Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined title of this Metric. This property is required.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Subtitle of the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Subtitle of the Metric.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the Metric.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is cumulative].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is cumulative]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCumulative { get; set; }

        /// <summary>
        /// Gets or sets the source value type identifier.
        /// </summary>
        /// <value>
        /// The source value type identifier.
        /// </value>
        [DataMember]
        public int? SourceValueTypeId { get; set; }

        /// <summary>
        /// Gets or sets the SQL query that returns the data for the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> that represents the SQL Query that returns the data for the Metric.
        /// </value>
        [DataMember]
        public string SourceSql { get; set; }

        /// <summary>
        /// Gets or sets the data view identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the x axis label.
        /// Note that in Rock, graphs typically actually use the MetricValue.MetricValueDateTime as the graph's X Axis.
        /// Therefore, in most cases, Metric.XAxisLabel and MetricValue.XAxis are NOT used
        /// </summary>
        /// <value>
        /// The x axis label.
        /// </value>
        [DataMember]
        public string XAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the y axis label.
        /// </summary>
        /// <value>
        /// The y axis label.
        /// </value>
        [DataMember]
        public string YAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the metric champion person alias identifier.
        /// </summary>
        /// <value>
        /// The metric champion person alias identifier.
        /// </value>
        [DataMember]
        public int? MetricChampionPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the admin person alias identifier.
        /// </summary>
        /// <value>
        /// The admin person alias identifier.
        /// </value>
        [DataMember]
        public int? AdminPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// For SQL or DataView based Metrics, this is the DateTime that the MetricValues where scheduled to be updated according to Schedule
        /// </summary>
        /// <value>
        /// The last run date time.
        /// </value>
        [DataMember]
        public DateTime? LastRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection that contains all the <see cref="MetricValue">Metric Values</see> (values) for this Metric.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.MetricValue">MetricValues</see> that are associated with this Metric.
        /// </value>
        public virtual ICollection<MetricValue> MetricValues { get; set; }

        /// <summary>
        /// Gets or sets the type of the source value.
        /// </summary>
        /// <value>
        /// The type of the source value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceValueType { get; set; }

        /// <summary>
        /// Gets or sets the data view.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the metric champion person alias.
        /// </summary>
        /// <value>
        /// The metric champion person alias.
        /// </value>
        public virtual PersonAlias MetricChampionPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the admin person alias.
        /// </summary>
        /// <value>
        /// The admin person alias.
        /// </value>
        public virtual PersonAlias AdminPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the metric categories.
        /// </summary>
        /// <value>
        /// The metric categories.
        /// </value>
        [DataMember]
        public virtual ICollection<MetricCategory> MetricCategories
        {
            get { return _metricCategories ?? ( _metricCategories = new Collection<MetricCategory>() ); }
            set { _metricCategories = value; }
        }
        private ICollection<MetricCategory> _metricCategories;

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the merge objects that can be used in the SourceSql
        /// </summary>
        /// <param name="runDateTime">The run date time. Note, this is the scheduled run date time, not the current datetime</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMergeObjects( DateTime runDateTime )
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "RunDateTime", runDateTime );
            mergeObjects.Add( "Metric", this );

            return mergeObjects;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Metric
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this Metric
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Metric Configuration class.
    /// </summary>
    public partial class MetricConfiguration : EntityTypeConfiguration<Metric>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricConfiguration"/> class.
        /// </summary>
        public MetricConfiguration()
        {
            this.HasOptional( p => p.SourceValueType ).WithMany().HasForeignKey( p => p.SourceValueTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.DataView ).WithMany().HasForeignKey( p => p.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CreatedByPersonAlias ).WithMany().HasForeignKey( p => p.CreatedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.MetricChampionPersonAlias ).WithMany().HasForeignKey( p => p.MetricChampionPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.AdminPersonAlias ).WithMany().HasForeignKey( p => p.AdminPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
