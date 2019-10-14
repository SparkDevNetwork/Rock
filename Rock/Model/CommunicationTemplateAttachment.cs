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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Communication Attachment POCO Entity.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationTemplateAttachment" )]
    [DataContract]
    public partial class CommunicationTemplateAttachment : Model<CommunicationTemplateAttachment>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is being sent the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is being sent the <see cref="Rock.Model.Communication"/>.
        /// </value>
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the the CommunicationTemplateId of the <see cref="Rock.Model.CommunicationTemplate"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CommunicationId of the <see cref="Rock.Model.CommunicationTemplate"/>.
        /// </value>
        [DataMember]
        public int CommunicationTemplateId { get; set; }

        /// <summary>
        /// Indicates if the attachment is for SMS recipients or Email recipients
        /// </summary>
        /// <value>
        /// The communication type value identifier.
        /// </value>
        [DataMember]
        public CommunicationType CommunicationType { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who is receiving the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who is receiving the <see cref="Rock.Model.Communication"/>.
        /// </value>
        [LavaInclude]
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Communication"/>
        /// </value>
        [LavaInclude]
        public virtual CommunicationTemplate CommunicationTemplate { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;
            BinaryFileService binaryFileService = new BinaryFileService( ( RockContext ) dbContext );
            var binaryFile = binaryFileService.Get( BinaryFileId );
            if ( binaryFile != null )
            {

                switch ( entry.State )
                {
                    case EntityState.Added:
                    case EntityState.Modified:
                        {

                            binaryFile.IsTemporary = false;

                            break;
                        }

                    case EntityState.Deleted:
                        {
                            var isAttachmentInUse = new CommunicationAttachmentService( ( RockContext ) dbContext )
                                            .Queryable()
                                            .Where( a => a.BinaryFileId == this.BinaryFileId )
                                            .Any();
                            if ( !isAttachmentInUse )
                            {
                                binaryFile.IsTemporary = true;
                            }
                            break;
                        }
                }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Recipient Configuration class.
    /// </summary>
    public partial class CommunicationTemplateAttachmentConfiguration : EntityTypeConfiguration<CommunicationTemplateAttachment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationTemplateAttachmentConfiguration"/> class.
        /// </summary>
        public CommunicationTemplateAttachmentConfiguration()
        {
            this.HasRequired( r => r.BinaryFile ).WithMany().HasForeignKey( r => r.BinaryFileId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.CommunicationTemplate ).WithMany( c => c.Attachments ).HasForeignKey( r => r.CommunicationTemplateId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

