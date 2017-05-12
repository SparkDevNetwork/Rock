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
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "BackgroundCheck" )]
    [DataContract]
    public partial class BackgroundCheck : Model<BackgroundCheck>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [DataMember]
        public int? WorkflowId { get; set; }
    
        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the response date.
        /// </summary>
        /// <value>
        /// The response date.
        /// </value>
        [DataMember]
        public DateTime? ResponseDate { get; set; }

        /// <summary>
        /// Gets or sets the record found.
        /// </summary>
        /// <value>
        /// The record found.
        /// </value>
        [DataMember]
        public bool? RecordFound { get; set; }

        /// <summary>
        /// Gets or sets the response XML.
        /// </summary>
        /// <value>
        /// The response XML.
        /// </value>
        [DataMember]
        public string ResponseXml { get; set; }

        /// <summary>
        /// Gets or sets the response document identifier.
        /// </summary>
        /// <value>
        /// The response document identifier.
        /// </value>
        [DataMember]
        public int? ResponseDocumentId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        [LavaInclude]
        public virtual Model.Workflow Workflow { get; set; }

        /// <summary>
        /// Gets or sets the response document.
        /// </summary>
        /// <value>
        /// The response document.
        /// </value>
        [LavaInclude]
        public virtual Model.BinaryFile ResponseDocument { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// BackgroundCheck Configuration class.
    /// </summary>
    public partial class BackgroundCheckConfiguration : EntityTypeConfiguration<BackgroundCheck>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCheckConfiguration"/> class.
        /// </summary>
        public BackgroundCheckConfiguration()
        {
            this.HasRequired( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete(true);
            this.HasOptional( p => p.Workflow ).WithMany().HasForeignKey( p => p.WorkflowId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.ResponseDocument ).WithMany().HasForeignKey( p => p.ResponseDocumentId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
