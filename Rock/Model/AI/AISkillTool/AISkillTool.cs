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
using Rock.Enums.AI.Agent;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a single tool that can be executed by the language model
    /// for a skill. The tool will provide additional data back to the
    /// model or execute a specific action based on the individual's request.
    /// </summary>
    [RockDomain( "AI" )]
    [Table( "AISkillTool" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "357d2625-fdca-41f8-ab8d-1cf2ce0abeed" )]
    public partial class AISkillTool : Model<AISkillTool>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the AI skill that this tool is associated with.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AISkillId { get; set; }

        /// <summary>
        /// The friendly name of the tool that will be used to identify it
        /// in the UI.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The description of the tool, which provides additional context
        /// or information about its intended purpose and functionality.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The type of tool represented by this entity. This indicates
        /// how the tool will be configured and executed in the language
        /// model.
        /// </summary>
        [DataMember]
        public ToolType ToolType { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The <see cref="AISkill"/> that this tool belongs to.
        /// </summary>
        [DataMember]
        public virtual AISkill AISkill { get; set; }

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
    public partial class AISkillToolConfiguration : EntityTypeConfiguration<AISkillTool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AISkillToolConfiguration"/> class.
        /// </summary>
        public AISkillToolConfiguration()
        {
            this.HasRequired( a => a.AISkill ).WithMany( s => s.AISkillTools ).HasForeignKey( a => a.AISkillId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
