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
using Rock.Data;
using Rock.Web.Cache;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Personalization Segment Entity
    /// </summary>
    /// <seealso cref="Data.Model{TEntity}" />
    /// <seealso cref="ICacheable" />
    [RockDomain( "CMS" )]
    [Table( "PersonalizationSegment" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "368A3581-C8C4-4960-901A-9587864226F3" )]
    public partial class PersonalizationSegment : Model<PersonalizationSegment>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the segment key.
        /// </summary>
        /// <value>
        /// The segment key.
        /// </value>
        [DataMember]
        public string SegmentKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the filter data view identifier.
        /// </summary>
        /// <value>
        /// The filter data view identifier.
        /// </value>
        [DataMember]
        public int? FilterDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the additional filter json.
        /// </summary>
        /// <value>
        /// The additional filter json.
        /// </value>
        [DataMember]
        public string AdditionalFilterJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Personalization Segment values
        /// is considered dirty. If it is dirty then it should be assumed that a calculation
        /// is being run on it is yet to be completed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dirty; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsDirty { get; set; }

        /// <summary>
        /// Gets or sets the description of the segment.
        /// </summary>
        /// <value>
        /// The description of the segment.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId for the segment.
        /// </summary>
        /// <value>
        /// The CategoryId of the segment.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the duration in milliseconds it takes to update the segment.
        /// </summary>
        /// <value>
        /// The time to update duration in milliseconds.
        /// </value>
        [DataMember]
        public double? TimeToUpdateDurationMilliseconds { get; set; }


        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the filter data view.
        /// </summary>
        /// <value>The filter data view.</value>
        [DataMember]
        public virtual DataView FilterDataView { get; set; }

        /// <summary>
        /// Gets or sets the category associated with the segment.
        /// </summary>
        /// <value>
        /// The category associated with the segment.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Segment Configuration Class
    /// </summary>
    public class SegmentConfiguration : EntityTypeConfiguration<PersonalizationSegment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAliasConfiguration"/> class.
        /// </summary>
        public SegmentConfiguration()
        {
            HasOptional( a => a.FilterDataView ).WithMany().HasForeignKey( a => a.FilterDataViewId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
