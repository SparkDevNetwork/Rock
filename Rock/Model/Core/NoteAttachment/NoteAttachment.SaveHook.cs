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
using Rock.Data;

namespace Rock.Model
{
    public partial class NoteAttachment
    {
        /// <summary>
        /// Save hook implementation for <see cref="NoteAttachment"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<NoteAttachment>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                BinaryFileService binaryFileService = new BinaryFileService( RockContext );
                var binaryFile = binaryFileService.Get( this.Entity.BinaryFileId );

                if ( this.Entry.State == EntityContextState.Added || this.Entry.State == EntityContextState.Modified )
                {
                    /*
                        11/10/2026 - MSE

                        The attachment file id comes from the caller (REST), so only allow the file the
                        attachment already has or a temporary file the current person uploaded. Otherwise
                        a caller could point an attachment at someone else's file, which would flag it as
                        permanent when saved and as temporary (cleanup) when the attachment is deleted.
                        Saves with no current person (jobs, imports) are not checked.

                        Reason: Prevent an attachment from taking over a file the person did not upload.
                    */
                    var originalBinaryFileId = this.Entry.State == EntityContextState.Modified ? OriginalValues[nameof( NoteAttachment.BinaryFileId )] as int? : null;

                    if ( this.Entity.BinaryFileId != originalBinaryFileId )
                    {
                        var currentPersonAlias = DbContext.GetCurrentPersonAlias();
                        var currentPerson = currentPersonAlias != null ? new PersonAliasService( RockContext ).GetPerson( currentPersonAlias.Id ) : null;

                        if ( currentPerson != null && !binaryFileService.IsUploadedBinaryFileAllowedForPerson( this.Entity.BinaryFileId, originalBinaryFileId, currentPerson ) )
                        {
                            throw new System.ComponentModel.DataAnnotations.ValidationException( "The attachment file must be a file you uploaded." );
                        }
                    }
                }

                switch ( this.Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            // if there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=False
                            if ( binaryFile != null && binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            // if there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=False
                            if ( binaryFile != null && binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            // if deleting, and there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=true
                            // so that it'll get cleaned up
                            if ( binaryFile != null && !binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = true;
                            }

                            break;
                        }
                }

                base.PreSave();
            }

        }
    }
}