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
    /// <summary>
    /// FinancialTransactionImage SaveHook
    /// </summary>
    public partial class FinancialTransactionImage
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialTransactionImage>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;

                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( Entity.BinaryFileId );

                Entity.HistoryChangeList = new History.HistoryChangeList();

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            // If there is a binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=False
                            if ( binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }

                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Image" );
                            break;
                        }
                    case EntityContextState.Modified:
                        {
                            // If there is a binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=False
                            if ( binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }

                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Image" );
                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            ProcessImageDeletion( Entity, rockContext );
                            break;
                        }
                }

                base.PreSave();
            }

            /// <summary>
            /// This method ensures that <see cref="Rock.Model.BinaryFile"/> records associated with the <see cref="FinancialTransactionImage"/> are
            /// properly cleaned up and history records are created for the deletion.  This method exists because FinancialTransactionImages have a
            /// cascade delete relationship with <see cref="FinancialTransaction"/> parent records and deleting the parent does not automatically
            /// trigger the PreSave() event of the child record(s).
            /// WARNING:  It is unlikely that you need to use this method anywhere other than <see cref="FinancialTransaction.SaveHook.PreSave()"/> and
            /// <see cref="FinancialTransactionImage.SaveHook.PreSave()"/>, and only when deleting the <see cref="FinancialTransactionImage"/> or the
            /// parent <see cref="FinancialTransaction"/>.
            /// </summary>
            /// <param name="image">The <see cref="FinancialTransactionImage"/>.</param>
            /// <param name="rockContext">The <see cref="RockContext"/>.</param>
            internal static void ProcessImageDeletion( FinancialTransactionImage image, RockContext rockContext )
            {
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( image.BinaryFileId );

                // If deleting, and there is a binaryfile (image) associated with this, make sure that it is flagged as IsTemporary=true 
                // so that it'll get cleaned up
                if ( !binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = true;
                }

                image.HistoryChangeList = new History.HistoryChangeList();
                image.HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Image" );
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( Entity.HistoryChangeList?.Any() == true )
                {
                    var rockContext = ( RockContext ) DbContext;

                    // Save history for transaction.
                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialTransaction ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.TransactionId,
                        Entity.HistoryChangeList,
                        true,
                        Entity.ModifiedByPersonAliasId );

                    var txn = new FinancialTransactionService( rockContext ).Get( Entity.TransactionId );
                    if ( txn != null && txn.BatchId != null )
                    {
                        // Save history for batch.
                        var batchHistory = new History.HistoryChangeList();

                        batchHistory.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Transaction" );
                        HistoryService.SaveChanges( rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                            txn.BatchId.Value,
                            batchHistory,
                            string.Empty,
                            typeof( FinancialTransaction ),
                            Entity.TransactionId,
                            true,
                            Entity.ModifiedByPersonAliasId,
                            rockContext.SourceOfChange );
                    }
                }

                base.PostSave();
            }
        }
    }
}