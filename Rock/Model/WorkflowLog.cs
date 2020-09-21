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
    /// Represents a WorkflowLog entry of a <see cref="Rock.Model.Workflow"/> instance event.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowLog" )]
    [DataContract]
    [NotAudited]
    public partial class WorkflowLog
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the value of the identifier.  This value is the primary field/key for the entity object.  This value is system and database
        /// dependent, and is not guaranteed to be unique. This id should only be used to identify an object internally to a single implementation
        /// of Rock since this value has a very high probability of not being consistent in an external implementation of Rock.
        /// </summary>
        /// <value>
        /// Primary and system dependent <see cref="System.Int32" /> based identity/key of an entity object in Rock.
        /// </value>
        [Key]
        [DataMember]
        [IncludeForReporting]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is being logged.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is being logged.
        /// </value>
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the WorkflowLog entry was created. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowLog entry was created.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime LogDateTime { get; set; }

        /// <summary>
        /// Gets or sets the body/text of the WorkflowLog entry. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the body/text of the WorkflowLog entry.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string LogText { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Workflow"/> instance that is being logged.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Workflow"/> that is being logged.
        /// </value>
        [LavaInclude]
        public virtual Workflow Workflow { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowLog.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowLog.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}: {1}", this.LogDateTime.ToStringSafe(), this.LogText );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// WorkflowLog Configuration class.
    /// </summary>
    public partial class WorkflowLogConfiguration : EntityTypeConfiguration<WorkflowLog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowLogConfiguration"/> class.
        /// </summary>
        public WorkflowLogConfiguration()
        {
            this.HasRequired( m => m.Workflow ).WithMany().HasForeignKey( m => m.WorkflowId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

