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
    public partial class CommunicationTemplateAttachment
    {
        /// <summary>
        /// Save hook implementation for <see cref="CommunicationTemplateAttachment"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<CommunicationTemplateAttachment>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( this.Entity.BinaryFileId );
                if ( binaryFile != null )
                {

                    switch ( State )
                    {
                        case EntityContextState.Added:
                        case EntityContextState.Modified:
                            {

                                binaryFile.IsTemporary = false;

                                break;
                            }

                        case EntityContextState.Deleted:
                            {
                                var isAttachmentInUse = new CommunicationAttachmentService( rockContext )
                                                .Queryable()
                                                .Where( a => a.BinaryFileId == Entity.BinaryFileId )
                                                .Any();
                                if ( !isAttachmentInUse )
                                {
                                    binaryFile.IsTemporary = true;
                                }
                                break;
                            }
                    }
                }

                base.PreSave();
            }
        }
    }
}
