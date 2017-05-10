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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "PersonDuplicate" )]
    [DataContract]
    public partial class PersonDuplicate : Model<PersonDuplicate>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_PersonAliasId_DuplicatePersonAliasId", 0, IsUnique = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the duplicate person alias identifier.
        /// </summary>
        /// <value>
        /// The duplicate person alias identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_PersonAliasId_DuplicatePersonAliasId", 1, IsUnique = true )]
        public int DuplicatePersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is confirmed as not duplicate.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is confirmed as not duplicate; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsConfirmedAsNotDuplicate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [ignore until score changes].
        /// Setting this to true will hide the personduplicate record until the score changes
        /// </summary>
        /// <value>
        /// <c>true</c> if [ignore until score changes]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IgnoreUntilScoreChanges { get; set; }

        /// <summary>
        /// Gets or sets the score.
        /// Calculated in the [spCrm_PersonDuplicateFinder] stored procedure
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        [DataMember]
        public int? Score { get; set; }

        /// <summary>
        /// Gets or sets the score detail.
        /// </summary>
        /// <value>
        /// The score detail.
        /// </value>
        [DataMember]
        public string ScoreDetail { get; set; }

        /// <summary>
        /// Gets or sets the capacity.
        /// The max possible score based on what items they have values for.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        [DataMember]
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets the total capacity.
        /// The max possible score if they had values for all matchable items
        /// </summary>
        /// <value>
        /// The total capacity.
        /// </value>
        [DataMember]
        public int? TotalCapacity { get; set; }

        /// <summary>
        /// Gets the confidence score, which is the Geometric Mean of the "weighted score of things that are matchable"% and "weighted score of things that match"%
        /// </summary>
        /// <value>
        /// The match score.
        /// </value>
        [DataMember]
        [Index]
        public double? ConfidenceScore { get; private set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the duplicate person alias.
        /// </summary>
        /// <value>
        /// The duplicate person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias DuplicatePersonAlias { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Person Configuration class.
    /// </summary>
    public partial class PersonDuplicateConfiguration : EntityTypeConfiguration<PersonDuplicate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDuplicateConfiguration"/> class.
        /// </summary>
        public PersonDuplicateConfiguration()
        {
            this.HasRequired( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.DuplicatePersonAlias ).WithMany().HasForeignKey( p => p.DuplicatePersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
