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
using System.Runtime.Serialization;
using Rock.Data;
namespace Rock.Model
{
    /// <summary>
    /// Represents a benevolence workflow that will be associated with a benevolence type.
    /// </summary>
    [RockDomain("Finance")]
    [Table("BenevolenceWorkflow")]
    [DataContract]
    public partial class BenevolenceWorkflow : Model<BenevolenceWorkflow>
    {

        #region entity properties
        /// <summary>
        /// Gets or sets the benevolence type identifier.
        /// </summary>
        /// <value>
        /// The benevolence type identifier.
        /// </value>
        [Required]
        [DataMember(IsRequired = true)]
        public int BenevolenceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the workflow type identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int WorkflowTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [DataMember]
        public ConnectionWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the workflow qualifier.
        /// </summary>
        /// <value>
        /// The workflow qualifier.
        /// </value>
        [DataMember]
        public String QualifierValue { get; set; }
        #endregion
    }
}
