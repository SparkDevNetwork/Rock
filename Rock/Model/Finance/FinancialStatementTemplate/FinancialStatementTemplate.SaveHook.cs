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
    /// <summary>
    /// FinancialStatementTemplate SaveHook
    /// </summary>
    public partial class FinancialStatementTemplate
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialStatementTemplate>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                if ( Entity.LogoBinaryFileId.HasValue )
                {
                    var rockContext = ( RockContext ) DbContext;
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( Entity.LogoBinaryFileId.Value );
                    if ( binaryFile != null )
                    {
                        switch ( Entry.State )
                        {
                            case EntityContextState.Added:
                            case EntityContextState.Modified:
                                {
                                    binaryFile.IsTemporary = false;
                                    break;
                                }
                            case EntityContextState.Deleted:
                                {
                                    binaryFile.IsTemporary = true;
                                    break;
                                }
                        }
                    }
                }

                base.PreSave();
            }
        }
    }
}