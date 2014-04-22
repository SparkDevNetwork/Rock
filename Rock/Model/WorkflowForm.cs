// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Workflow;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowForm"/> (action or task) that is performed as part of a <see cref="Rock.Model.WorkflowForm"/>.
    /// </summary>
    [Table( "WorkflowForm" )]
    [DataContract]
    public partial class WorkflowForm : Model<WorkflowForm>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Footer { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Workflow Form Configuration class.
    /// </summary>
    public partial class WorkflowFormConfiguration : EntityTypeConfiguration<WorkflowForm>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowFormConfiguration"/> class.
        /// </summary>
        public WorkflowFormConfiguration()
        {
        }
    }

    #endregion

}

