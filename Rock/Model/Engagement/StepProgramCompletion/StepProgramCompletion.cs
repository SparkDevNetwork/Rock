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
using Rock.Lava;

namespace Rock.Model
{
    /*
       5/20/2021 SK, NA, JE 
      
       We originally planned to try to track Step Program "Engagement" but this became difficult to accurately
       determine which steps should be considered part of an active, on-going program.  That is because a 
       person could have multiple attempts at one step over several years and they really shouldn't necessarily
       be considered part of the current "engagement".  Therefore, we changed this to track 'completed' 
       engagements using a simple rule described below.
     */

    /// <summary>
    /// This represents a the completion of a Step Program for a particular person. Since any particular Step Program
    /// could have a variety of Steps with various settings, this entity represents the simple attempt to record when
    /// someone has completed each step in the program.  It does this with a simple rule: As soon as there is a
    /// full set of "completed" steps (for each step type of a program), it uses the latest/newest completed step
    /// for each type and assigns it to a new Step Program Completion record.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "StepProgramCompletion" )]
    [DataContract]
    public partial class StepProgramCompletion : Model<StepProgramCompletion>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.StepProgram"/> to which this step program completion belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepProgramId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.PersonAlias"/> that identifies the Person associated with the step. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> identifier. This will be the campus
        /// from whichever step was completed last (most recently).
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the start of the step program.
        /// </summary>
        [DataMember]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the end of the step program.
        /// </summary>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets the start date key.
        /// </summary>
        /// <value>
        /// The start date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int StartDateKey
        {
            get => StartDateTime.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        /// <summary>
        /// Gets the end date key.
        /// </summary>
        /// <value>
        /// The end date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? EndDateKey
        {
            get => ( EndDateTime == null || EndDateTime.Value == default ) ?
                        ( int? ) null :
                        EndDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.StepProgram"/>.
        /// </summary>
        [DataMember]
        public virtual StepProgram StepProgram { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/>. This will be the campus
        /// from whichever step was completed last (most recently).
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Step">Steps</see> that are related to step program completion.
        /// </summary>
        [DataMember]
        public virtual ICollection<Step> Steps
        {
            get => _steps ?? ( _steps = new Collection<Step>() );
            set => _steps = value;
        }

        private ICollection<Step> _steps;

        #endregion Navigation Properties

        #region Entity Configuration

        /// <summary>
        /// Step Program Completion Configuration class.
        /// </summary>
        public partial class StepProgramCompletionConfiguration : EntityTypeConfiguration<StepProgramCompletion>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepProgramCompletionConfiguration"/> class.
            /// </summary>
            public StepProgramCompletionConfiguration()
            {
                HasRequired( s => s.StepProgram ).WithMany().HasForeignKey( s => s.StepProgramId ).WillCascadeOnDelete( false );
                HasRequired( s => s.PersonAlias ).WithMany().HasForeignKey( s => s.PersonAliasId ).WillCascadeOnDelete( false );
                HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration
    }
}
