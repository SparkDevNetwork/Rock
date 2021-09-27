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

                switch ( this.Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            // if there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=False
                            if ( binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            // if there is an binaryfile (attachment) associated with this, make sure that it is flagged as IsTemporary=False
                            if ( binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
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

                base.PreSave();
            }

        }
    }
}