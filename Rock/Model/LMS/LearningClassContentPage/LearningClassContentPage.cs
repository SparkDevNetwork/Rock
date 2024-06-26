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

namespace Rock.Model
{
    /// <summary>
    /// Represents a learning class (an instance of a course for a given semester).
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningClassContentPage" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_CLASS_CONTENT_PAGE )]
    public partial class LearningClassContentPage : Model<LearningClassContentPage>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the id of the related <see cref="Rock.Model.LearningClass"/> for the content tab.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningClass"/>.
        /// </value>
        [DataMember]
        public int LearningClassId { get; set; }

        /// <summary>
        /// Gets or sets the title of the content tab.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the title of the content tab
        /// </value>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content of the tab.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the structured content of the tab.
        /// </value>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the date the content tab becomes visible to students.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the semester begins.
        /// </value>
        [DataMember]
        public DateTime? StartDateTime { get; set; }

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
        /// The title of the content page.
        /// </summary>
        /// <returns>The title of the content page.</returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningClassContentPage Configuration class.
    /// </summary>
    public partial class LearningClassContentPageConfiguration : EntityTypeConfiguration<LearningClassContentPage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningClassConfiguration" /> class.
        /// </summary>
        public LearningClassContentPageConfiguration()
        {
            this.HasRequired( a => a.LearningClass ).WithMany( a => a.ContentPages ).HasForeignKey( a => a.LearningClassId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
