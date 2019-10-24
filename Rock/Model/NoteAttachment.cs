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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an attachment that is associated with a <see cref="Rock.Model.Note"/>.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "NoteAttachment" )]
    [DataContract]
    public partial class NoteAttachment : Model<NoteAttachment>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the NoteId of the <see cref="Rock.Model.Note"/> that this attachment belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.Note"/> that this attachment belongs to.
        /// </value>
        [DataMember]
        public int NoteId { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFileId of the image's <see cref="Rock.Model.BinaryFile"/> 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing BinaryFileId of the attachment's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [DataMember]
        public int BinaryFileId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Note"/> that this attachment belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Note"/> that this attachment belongs to.
        /// </value>
        [LavaInclude]
        public virtual Note Note { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> of the attachment.
        /// </summary>
        /// <value>
        /// The attachment's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [LavaInclude]
        public virtual BinaryFile BinaryFile { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( BinaryFileId );

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        // if there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=False
                        if ( binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }

                        break;
                    }

                case EntityState.Modified:
                    {
                        // if there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=False
                        if ( binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }

                        break;
                    }
                case EntityState.Deleted:
                    {
                        // if deleting, and there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=true 
                        // so that it'll get cleaned up
                        if ( !binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = true;
                        }

                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// TransactionImage Configuration class
    /// </summary>
    public partial class NoteAttachmentConfiguration : EntityTypeConfiguration<NoteAttachment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionImageConfiguration"/> class.
        /// </summary>
        public NoteAttachmentConfiguration()
        {
            this.HasRequired( a => a.Note ).WithMany( n => n.Attachments ).HasForeignKey( a => a.NoteId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.BinaryFile ).WithMany().HasForeignKey( a => a.BinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}