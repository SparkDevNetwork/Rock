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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
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
        [LavaInclude]
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> of the image.
        /// </summary>
        /// <value>
        /// The image's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [LavaInclude]
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        public List<string> HistoryChanges { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( BinaryFileId );

            HistoryChanges = new List<string>();

            switch ( entry.State )
            {
                case System.Data.Entity.EntityState.Added:
                    {
                        // if there is an binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=False
                        if ( binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }

                        HistoryChanges.Add( "Added Image" );
                        break;
                    }

                case System.Data.Entity.EntityState.Modified:
                    {
                        // if there is an binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=False
                        if ( binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }

                        HistoryChanges.Add( "Updated Image" );
                        break;
                    }
                case System.Data.Entity.EntityState.Deleted:
                    {
                        // if deleting, and there is an binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=true 
                        // so that it'll get cleaned up
                        if ( !binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = true;
                        }

                        HistoryChanges.Add( "Removed Image" );
                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( DbContext dbContext )
        {
            if ( HistoryChanges.Any() )
            {
                HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialTransaction ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), this.TransactionId, HistoryChanges, true, this.ModifiedByPersonAliasId );

                var txn = new FinancialTransactionService( (RockContext)dbContext ).Get( this.TransactionId );
                if ( txn != null && txn.BatchId != null )
                {
                    var batchHistory = new List<string> { string.Format( "Updated <span class='field-name'>Transaction</span> ID: <span class='field-value'>{0}</span>.", txn.Id ) };
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialBatch ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), txn.BatchId.Value, batchHistory, string.Empty, typeof( FinancialTransaction ), this.TransactionId, true, this.ModifiedByPersonAliasId );
                }
            }

            base.PostSaveChanges( dbContext );
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