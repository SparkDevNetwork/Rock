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

using System;
using System.Linq;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPledge Save Hook
    /// </summary>
    public partial class FinancialPledge
    {
        internal class SaveHook : EntitySaveHook<FinancialPledge>
        {
            private History.HistoryChangeList HistoryChangeList { get; set; }

            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;
                HistoryChangeList = new History.HistoryChangeList();

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Pledge" );

                            History.EvaluateChange( HistoryChangeList, "Person", string.Empty, History.GetValue( Entity.PersonAlias, Entity.PersonAliasId, rockContext ) );
                            History.EvaluateChange( HistoryChangeList, "Group", string.Empty, History.GetValue( Entity.Group, Entity.GroupId, rockContext ) );
                            History.EvaluateChange( HistoryChangeList, "Account", string.Empty, History.GetValue( Entity.Account, Entity.AccountId, rockContext ) );
                            History.EvaluateChange( HistoryChangeList, "Amount", string.Empty, Entity.TotalAmount.FormatAsCurrency() );
                            History.EvaluateChange( HistoryChangeList, "Frequency", ( null as int? ), Entity.PledgeFrequencyValue, Entity.PledgeFrequencyValueId );
                            History.EvaluateChange( HistoryChangeList, "Start Date", ( null as DateTime? ), Entity.StartDate );
                            History.EvaluateChange( HistoryChangeList, "End Date", ( null as DateTime? ), Entity.EndDate );

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            var originalPerson = Entry.OriginalValues[nameof( Entity.PersonAliasId )].ToStringSafe().AsIntegerOrNull();
                            if ( !Entity.PersonAliasId.Equals( originalPerson ) )
                            {
                                History.EvaluateChange( HistoryChangeList, "Person", History.GetValue<PersonAlias>( null, originalPerson, rockContext ), History.GetValue<PersonAlias>( Entity.PersonAlias, Entity.PersonAliasId, rockContext ) );
                            }

                            var originalGroup = Entry.OriginalValues[nameof( Entity.GroupId )].ToStringSafe().AsIntegerOrNull();
                            if ( !Entity.GroupId.Equals( originalGroup ) )
                            {
                                History.EvaluateChange( HistoryChangeList, "Group", History.GetValue<Group>( null, originalGroup, rockContext ), History.GetValue<Group>( Entity.Group, Entity.GroupId, rockContext ) );
                            }

                            var originalAccount = Entry.OriginalValues[nameof( Entity.AccountId )].ToStringSafe().AsIntegerOrNull();
                            if ( !Entity.AccountId.Equals( originalAccount ) )
                            {
                                History.EvaluateChange( HistoryChangeList, "Account", History.GetValue<FinancialAccount>( null, originalAccount, rockContext ), History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext ) );
                            }

                            History.EvaluateChange( HistoryChangeList, "Amount", Entry.OriginalValues[nameof( Entity.TotalAmount )].ToStringSafe().AsDecimal().FormatAsCurrency(), Entity.TotalAmount.FormatAsCurrency() );
                            History.EvaluateChange( HistoryChangeList, "Frequency", Entry.OriginalValues[nameof( Entity.PledgeFrequencyValueId )].ToStringSafe().AsIntegerOrNull(), Entity.PledgeFrequencyValue, Entity.PledgeFrequencyValueId );
                            History.EvaluateChange( HistoryChangeList, "Start Date", Entry.OriginalValues[nameof( Entity.StartDate )].ToStringSafe().AsDateTime(), Entity.StartDate );
                            History.EvaluateChange( HistoryChangeList, "End Date", Entry.OriginalValues[nameof( Entity.EndDate )].ToStringSafe().AsDateTime(), Entity.EndDate );

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Pledge" );

                            break;
                        }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( HistoryChangeList?.Any() == true )
                {
                    var rockContext = ( RockContext ) DbContext;

                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialPledge ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.Id,
                        HistoryChangeList,
                        true,
                        Entity.ModifiedByPersonAliasId );
                }

                base.PostSave();
            }
        }
    }
}
