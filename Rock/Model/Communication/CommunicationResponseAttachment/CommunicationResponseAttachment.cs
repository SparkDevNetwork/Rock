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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// A link table between CommunicationResponse and BinaryFile
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationResponseAttachment" )]
    [DataContract]
    public partial class CommunicationResponseAttachment : Model<CommunicationResponseAttachment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the binary file identifier.
        /// </summary>
        /// <value>
        /// The binary file identifier.
        /// </value>
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the communication response identifier.
        /// </summary>
        /// <value>
        /// The communication response identifier.
        /// </value>
        [DataMember]
        public int CommunicationResponseId { get; set; }

        /// <summary>
        /// Gets or sets the type of the communication.
        /// </summary>
        /// <value>
        /// The type of the communication.
        /// </value>
        [DataMember]
        public CommunicationType CommunicationType { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the binary file.
        /// </summary>
        /// <value>
        /// The binary file.
        /// </value>
        [LavaVisible]
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the communication response.
        /// </summary>
        /// <value>
        /// The communication response.
        /// </value>
        [LavaVisible]
        public virtual CommunicationResponse CommunicationResponse { get; set; }

        #endregion Virtual Properties
    }

    #region Entity Configuration

    /// <summary>
    /// CommunicationResponseAttachment configration class
    /// </summary>
    public partial class CommunicationResponseAttachmentConfiguration : EntityTypeConfiguration<CommunicationResponseAttachment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationAttachmentConfiguration"/> class.
        /// </summary>
        public CommunicationResponseAttachmentConfiguration()
        {
            this.HasRequired( r => r.BinaryFile ).WithMany().HasForeignKey( r => r.BinaryFileId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.CommunicationResponse ).WithMany( c => c.Attachments ).HasForeignKey( r => r.CommunicationResponseId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration


}
