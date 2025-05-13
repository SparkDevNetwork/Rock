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
    /// Represents an image that is associated with a <see cref="Rock.Model.FinancialTransaction"/>. Examples could be 
    /// the front or back side of a check or an offering envelope.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionImage" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "78DCA7EE-C5FE-49AE-9995-0E254CC8E2A2")]
    public partial class FinancialTransactionImage : Model<FinancialTransactionImage>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the TransactionId of the <see cref="Rock.Model.FinancialTransaction"/> that this image belongs to
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.FinancialTransaction"/>that this image belongs to.
        /// </value>
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFileId of the image's <see cref="Rock.Model.BinaryFile"/> 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing BinaryFileId of the image's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransaction"/> that this image belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransaction"/> that this image belongs to.
        /// </value>
        [LavaVisible]
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> of the image.
        /// </summary>
        /// <value>
        /// The image's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [LavaVisible]
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// TransactionImage Configuration class
    /// </summary>
    public partial class FinancialTransactionImageConfiguration : EntityTypeConfiguration<FinancialTransactionImage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionImageConfiguration"/> class.
        /// </summary>
        public FinancialTransactionImageConfiguration()
        {
            this.HasRequired( i => i.Transaction ).WithMany( t => t.Images ).HasForeignKey( i => i.TransactionId ).WillCascadeOnDelete( true );
            this.HasRequired( i => i.BinaryFile ).WithMany().HasForeignKey( i => i.BinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}