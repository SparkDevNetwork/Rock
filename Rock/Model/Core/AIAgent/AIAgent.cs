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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents an AI chat agent in Rock. The agent and the related skills
    /// define how the agent will interact with people and what features it
    /// will support.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AIAgent" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "ee3fe609-5c7c-492e-b0e9-5461045fc825" )]
    public partial class AIAgent : Model<AIAgent>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The friendly name of the agent that will be used to identify it in the UI.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The description of the agent, which provides additional context or
        /// information about its intended purpose and functionality.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The identifier of the binary file that contains the image to use
        /// as the avatar to represent the agent. This will be used in the
        /// administrative UI and the chat UI to represent the agent visually.
        /// </summary>
        [DataMember]
        public int? AvatarBinaryFileId { get; set; }

        /// <summary>
        /// The persona of the agent, which is a string that describes how the
        /// agent should behavor or respond. This can include tone, style, and
        /// special instructions it should follow when interacting with people.
        /// </summary>
        [DataMember]
        public string Persona { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The binary file that contains the image to use as the avatar to
        /// represent the agent. This will be used in the administrative UI and
        /// the chat UI to represent the agent visually.
        /// </summary>
        [DataMember]
        public virtual BinaryFile AvatarBinaryFile { get; set; }

        /// <summary>
        /// A collection containing the <see cref="AIAgentSkill" /> entities
        /// that represent the skills attached to this agent.
        /// </summary>
        [DataMember]
        public virtual ICollection<AIAgentSkill> AIAgentSkills { get; set; } = new Collection<AIAgentSkill>();

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
    public partial class AIAgentConfiguration : EntityTypeConfiguration<AIAgent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentConfiguration"/> class.
        /// </summary>
        public AIAgentConfiguration()
        {
            this.HasRequired( a => a.AvatarBinaryFile ).WithMany().HasForeignKey( a => a.AvatarBinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
