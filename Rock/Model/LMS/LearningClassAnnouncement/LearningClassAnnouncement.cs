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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    /// <summary>
    /// Represents an announcement within a <see cref="Rock.Model.LearningClass"/>.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningClassAnnouncement" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_CLASS_ANNOUNCEMENT )]
    public partial class LearningClassAnnouncement : Model<LearningClassAnnouncement>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the title of the announcement.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the title of the announcement
        /// </value>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the summary text of the announcement.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Summary text of the announcement.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the description for the announcement.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the announcement.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the url where more details can be found (if any).
        /// </summary>
        /// <value>
        /// The image binary file id.
        /// </value>
        [DataMember]
        public string DetailsUrl { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningClass"/> the announcement belongs to.
        /// </summary>
        /// <value>
        /// The identifier of the LearningClass.
        /// </value>
        [DataMember]
        public int LearningClassId { get; set; }

        /// <summary>
        /// The communication mode used for the announcement.
        /// </summary>
        [DataMember]
        public CommunicationMode CommunicationMode { get; set; }

        /// <summary>
        /// Gets or sets the date the announcement should be published/visible.
        /// </summary>
        [DataMember]
        public DateTime PublishDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the communication has been sent.
        /// <remarks>
        /// This will alsways be false when the 'None' CommunicationMode is specified.
        /// </remarks>
        /// </summary>
        [DataMember]
        public bool CommunicationSent { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningClass"/>.
        /// </summary>
        [DataMember]
        public virtual LearningClass LearningClass { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The title of the announcement.
        /// </summary>
        /// <returns>The title of the announcement.</returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningCourse Configuration class.
    /// </summary>
    public partial class LearningClassAnnouncementConfiguration : EntityTypeConfiguration<LearningClassAnnouncement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningCourseConfiguration" /> class.
        /// </summary>
        public LearningClassAnnouncementConfiguration()
        {
            this.HasRequired( a => a.LearningClass ).WithMany( a => a.Announcements ).HasForeignKey( a => a.LearningClassId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
