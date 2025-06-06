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
    /// Represents a link between an agent and a skill.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AIAgentSkill" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "36deda2f-75bd-4bee-ac36-46d20dcd1331" )]
    public partial class AIAgentSkill : Entity<AIAgentSkill>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the agent that the skill is associated with.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AIAgentId { get; set; }

        /// <summary>
        /// The identifier of the skill that the agent is associated with.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AISkillId { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The agent that the skill is associated with.
        /// </summary>
        public virtual AIAgent AIAgent { get; set; }

        /// <summary>
        /// The skill that the agent is associated with.
        /// </summary>
        public virtual AISkill AISkill { get; set; }

        #endregion

        #region Public Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AIAgentSkillConfiguration : EntityTypeConfiguration<AIAgentSkill>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentSkillConfiguration"/> class.
        /// </summary>
        public AIAgentSkillConfiguration()
        {
            this.HasRequired( a => a.AIAgent ).WithMany( a => a.AIAgentSkills ).HasForeignKey( a => a.AIAgentId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.AISkill ).WithMany( s => s.AIAgentSkills ).HasForeignKey( a => a.AISkillId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
