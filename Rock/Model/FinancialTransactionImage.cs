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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an image that is associated with a <see cref="Rock.Model.FinancialTransaction"/>. Examples could be 
    /// the front or back side of a check or an offering envelope.
    /// </summary>
    [Table( "FinancialTransactionImage" )]
    [DataContract]
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
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> of the image.
        /// </summary>
        /// <value>
        /// The image's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        public virtual BinaryFile BinaryFile { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.EntityState state )
        {
            BinaryFileService binaryFileService = new BinaryFileService( (RockContext)dbContext );
            var binaryFile = binaryFileService.Get( BinaryFileId );
            if ( binaryFile != null )
            {
                if ( state != System.Data.Entity.EntityState.Deleted )
                {
                    // if there is an binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=False
                    if ( binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = false;
                    }
                }
                else
                {
                    // if deleting, and there is an binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=true 
                    // so that it'll get cleaned up
                    if ( !binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = true;
                    }
                }
            }
        }
        
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