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

using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class Communication
    {
        /// <summary>
        /// Save hook implementation for <see cref="Communication"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Communication>
        {
            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                // ensure any attachments have the binaryFile.IsTemporary set to False
                var attachmentBinaryFilesIds = Entity.Attachments.Select( a => a.BinaryFileId ).ToList();
                if ( attachmentBinaryFilesIds.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var temporaryBinaryFiles = new BinaryFileService( rockContext ).GetByIds( attachmentBinaryFilesIds ).Where( a => a.IsTemporary == true ).ToList();
                        {
                            foreach ( var binaryFile in temporaryBinaryFiles )
                            {
                                binaryFile.IsTemporary = false;
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }

                base.PostSave();
            }
        }
    }
}
