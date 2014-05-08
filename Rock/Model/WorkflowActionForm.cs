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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Workflow;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowActionForm"/> (action or task) that is performed as part of a <see cref="Rock.Model.WorkflowActionForm"/>.
    /// </summary>
    [Table( "WorkflowActionForm" )]
    [DataContract]
    public partial class WorkflowActionForm : Model<WorkflowActionForm>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [DataMember]
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [DataMember]
        public string Footer { get; set; }

        [DataMember]
        public string InactiveMessage { get; set; }

        /// <summary>
        /// Gets or sets the delimited list of action buttons and actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [MaxLength( 300 )]
        [DataMember]
        public string Actions { get; set; }


        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual ICollection<WorkflowActionFormAttribute> FormAttributes
        {
            get { return _formAttributes ?? ( _formAttributes = new Collection<WorkflowActionFormAttribute>() ); }
            set { _formAttributes = value; }
        }
        private ICollection<WorkflowActionFormAttribute> _formAttributes;
        
        #endregion

        #region Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Workflow Form Configuration class.
    /// </summary>
    public partial class WorkflowActionFormConfiguration : EntityTypeConfiguration<WorkflowActionForm>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormConfiguration"/> class.
        /// </summary>
        public WorkflowActionFormConfiguration()
        {
        }
    }

    #endregion

}

