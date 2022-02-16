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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "MetricCategory" )]
    [DataContract]
    public partial class MetricCategory : Entity<MetricCategory>, IOrdered, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        [DataMember]
        [Index( "IX_MetricCategory", 0 )]
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        [Index( "IX_MetricCategory", 1 )]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        [DataMember]
        public virtual Metric Metric { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        public virtual Security.ISecured ParentAuthorityPre
        {
            get { return null; }
        }

        /// <summary>
        /// Provides a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public virtual System.Collections.Generic.Dictionary<string, string> SupportedActions
        {
            get
            {
                return this.Metric.SupportedActions;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public virtual string IconCssClass
        {
            get
            {
                return this.Metric.IconCssClass;
            }
        }

        #region ICategorized

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name
        {
            get
            {
                return Metric.Title;
            }
        }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        int? ICategorized.CategoryId
        {
            get
            {
                return this.CategoryId;
            }
        }

        #endregion ICategorized

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class MetricCategoryConfiguration : EntityTypeConfiguration<MetricCategory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricCategoryConfiguration"/> class.
        /// </summary>
        public MetricCategoryConfiguration()
        {
            this.HasRequired( a => a.Metric ).WithMany( a => a.MetricCategories );
            this.HasRequired( a => a.Category ).WithMany();
        }
    }

    #endregion
}