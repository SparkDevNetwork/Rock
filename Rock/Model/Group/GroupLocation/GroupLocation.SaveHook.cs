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
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class GroupLocation
    {
        /// <summary>
        /// Save hook implementation for <see cref="GroupLocation"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<GroupLocation>
        {
            private History.HistoryChangeList GroupHistoryChanges { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                GroupHistoryChanges = new History.HistoryChangeList();

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            string locationType = History.GetDefinedValueValue( null, Entity.GroupLocationTypeValueId );
                            locationType = locationType.IsNotNullOrWhiteSpace() ? locationType : "Unknown";
                            History.EvaluateChange( GroupHistoryChanges, $"{locationType} Location", ( int? ) null, Entity.Location, Entity.LocationId, rockContext );
                            History.EvaluateChange( GroupHistoryChanges, $"{locationType} Is Mailing", false, Entity.IsMailingLocation );
                            History.EvaluateChange( GroupHistoryChanges, $"{locationType} Is Map Location", false, Entity.IsMappedLocation );

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            string locationTypeName = DefinedValueCache.GetName( Entity.GroupLocationTypeValueId ) ?? "Unknown";
                            int? oldLocationTypeId = OriginalValues["GroupLocationTypeValueId"].ToStringSafe().AsIntegerOrNull();
                            if ( ( oldLocationTypeId ?? 0 ) == ( Entity.GroupLocationTypeValueId ?? 0 ) )
                            {
                                History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Location", OriginalValues["LocationId"].ToStringSafe().AsIntegerOrNull(), Entity.Location, Entity.LocationId, rockContext );
                            }
                            else
                            {
                                Location newLocation = null;
                                History.EvaluateChange( GroupHistoryChanges, $"{DefinedValueCache.GetName( oldLocationTypeId ) ?? "Unknown"} Location", OriginalValues["LocationId"].ToStringSafe().AsIntegerOrNull(), newLocation, ( int? ) null, rockContext );
                                History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Location", ( int? ) null, Entity.Location, Entity.LocationId, rockContext );
                            }

                            History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Is Mailing", OriginalValues["IsMailingLocation"].ToStringSafe().AsBoolean(), Entity.IsMailingLocation );
                            History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Is Map Location", OriginalValues["IsMappedLocation"].ToStringSafe().AsBoolean(), Entity.IsMappedLocation );

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            string locationType = History.GetDefinedValueValue( null, OriginalValues["GroupLocationTypeValueId"].ToStringSafe().AsIntegerOrNull() );
                            locationType = locationType.IsNotNullOrWhiteSpace() ? locationType : "Unknown";
                            Location loc = null;
                            History.EvaluateChange( GroupHistoryChanges, $"{locationType} Location", OriginalValues["LocationId"].ToStringSafe().AsIntegerOrNull(), loc, ( int? ) null, rockContext );
                            break;
                        }
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                if ( GroupHistoryChanges?.Any() == true )
                {
                    HistoryService.SaveChanges( rockContext, typeof( Group ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(), Entity.GroupId, GroupHistoryChanges, true, Entity.ModifiedByPersonAliasId );
                }

                // If this is a Group of type Family, update the ModifiedDateTime on the Persons that are members of this family (since one of their Addresses changed)
                int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
                var groupService = new GroupService( rockContext );

                int groupTypeId = groupService.GetSelect( Entity.GroupId, s => s.GroupTypeId );
                if ( groupTypeId == groupTypeIdFamily )
                {
                    var currentDateTime = RockDateTime.Now;
                    var qryPersonsToUpdate = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == Entity.GroupId ).Select( a => a.Person );
                    rockContext.BulkUpdate( qryPersonsToUpdate, p => new Person { ModifiedDateTime = currentDateTime, ModifiedByPersonAliasId = Entity.ModifiedByPersonAliasId } );
                }

                base.PostSave();
            }
        }
    }
}
