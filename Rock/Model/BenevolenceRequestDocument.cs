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
    /// Represents a benevolence request document.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "BenevolenceRequestDocument" )]
    [DataContract]
    public partial class BenevolenceRequestDocument : Model<BenevolenceRequestDocument>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the benevolence request identifier.
        /// </summary>
        /// <value>
        /// The benevolence request identifier.
        /// </value>
        [Required]
        [DataMember]
        public int BenevolenceRequestId { get; set; }

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        [Required]
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int? Order { get; set; }
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the package.
        /// </summary>
        /// <value>
        /// The package.
        /// </value>
        [LavaInclude]
        public virtual BenevolenceRequest BenevolenceRequest { get; set; }

        /// <summary>
        /// Gets or sets the binary file.
        /// </summary>
        /// <value>
        /// The binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile BinaryFile { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// BenevolenceRequest Configuration class.
    /// </summary>
    public partial class BenevolenceRequestDocumentConfiguration : EntityTypeConfiguration<BenevolenceRequestDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenevolenceRequestDocumentConfiguration" /> class.
        /// </summary>
        public BenevolenceRequestDocumentConfiguration()
        {
            this.HasRequired( p => p.BenevolenceRequest ).WithMany( p => p.Documents ).HasForeignKey( p => p.BenevolenceRequestId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.BinaryFile ).WithMany().HasForeignKey( p => p.BinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}