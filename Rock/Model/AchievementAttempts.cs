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
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Transactions;

namespace Rock.Model
{
    /// <summary>
    /// Represents an Achievement Attempts in Rock.
    /// </summary>
    [RockDomain( "Achievements" )]
    [Table( "AchievementAttempt" )]
    [DataContract]
    public partial class AchievementAttempt : Model<AchievementAttempt>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the achiever entity identifier.
        /// This was originally a Person Alias Id via the Streak.
        /// </summary>
        [DataMember( IsRequired = true )]
        [Required]
        public int AchieverEntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Model.AchievementType"/> to which this attempt belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AchievementTypeId { get; set; }

        /// <summary>
        /// Gets or sets the progress. This is a percentage so .25 is 25% and 1 is 100%.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        [DataMember]
        public decimal Progress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attempt is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is closed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attempt was a success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is closed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the achievement attempt start date time.
        /// </summary>
        /// <value>
        /// The achievement attempt start date time.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public DateTime AchievementAttemptStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the achievement attempt end date time.
        /// </summary>
        /// <value>
        /// The achievement attempt start date time.
        /// </value>
        [DataMember]
        public DateTime? AchievementAttemptEndDateTime { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Model.AchievementType"/>.
        /// </summary>
        [DataMember]
        public virtual AchievementType AchievementType { get; set; }

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// Achievement Attempt Configuration class.
        /// </summary>
        public partial class AchievementAttemptConfiguration : EntityTypeConfiguration<AchievementAttempt>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AchievementAttemptConfiguration"/> class.
            /// </summary>
            public AchievementAttemptConfiguration()
            {
                HasRequired( saa => saa.AchievementType ).WithMany( s => s.Attempts ).HasForeignKey( saa => saa.AchievementTypeId ).WillCascadeOnDelete( true );
            }
        }

        #endregion Entity Configuration

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( AchievementAttemptEndDateTime.HasValue && AchievementAttemptStartDateTime > AchievementAttemptEndDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The AchievementAttemptStartDateTime must occur before the AchievementAttemptEndDateTime" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        /// <summary>
        /// Perform tasks prior to saving changes to this entity.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, DbEntityEntry entry )
        {
            // Add a transaction to process workflows and add steps
            new AchievementAttemptChangeTransaction( entry ).Enqueue();
            base.PreSaveChanges( dbContext, entry );
        }

        #endregion Overrides
    }
}
