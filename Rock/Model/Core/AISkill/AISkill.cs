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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents an AI skill in Rock. The skill defines a set of related
    /// functions that the AI agent can perform, such as answering questions,
    /// or performing data manipulation tasks.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AISkill" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "d953ab34-4ab6-47c6-857b-53044a99ed75" )]
    public partial class AISkill : Model<AISkill>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The friendly name of the skill that will be used to identify it in
        /// the UI.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The description of the skill, which provides additional context or
        /// information about its intended purpose and functionality.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// A concise, but descriptive, hint to the language model that provides
        /// context about when this skill's functions should be used in response
        /// to an individual's input.
        /// </summary>
        [DataMember]
        public string UsageHint { get; set; }

        /// <summary>
        /// The entity type identifier that represents the C# class that
        /// implements the functions for this skill. If this is not null then
        /// the skill and related functions should not allow editing beyond
        /// enabling or disabling them.
        /// </summary>
        [DataMember]
        public int? CodeEntityTypeId { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The entity type that represents the C# class that implements the
        /// functions for this skill. If this is not null then the skill and
        /// related functions should not allow editing beyond enabling or
        /// disabling them.
        /// </summary>
        [DataMember]
        public virtual EntityType CodeEntityType { get; set; }

        /// <summary>
        /// A collection containing the <see cref="AIAgentSkill" /> entities
        /// that represent the agents this skill is attached to.
        /// </summary>
        [DataMember]
        public virtual ICollection<AIAgentSkill> AIAgentSkills { get; set; }

        /// <summary>
        /// A collection containing the <see cref="AISkillFunction" /> entities
        /// that represent the functions this skill has.
        /// </summary>
        [DataMember]
        public virtual ICollection<AISkillFunction> AISkillFunctions{ get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AISkillConfiguration : EntityTypeConfiguration<AISkill>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AISkillConfiguration"/> class.
        /// </summary>
        public AISkillConfiguration()
        {
            this.HasRequired( a => a.CodeEntityType ).WithMany().HasForeignKey( a => a.CodeEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
