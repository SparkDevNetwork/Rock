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
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class UserLogin
    {
        /// <summary>
        /// Save hook implementation for <see cref="UserLogin"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<UserLogin>
        {
            private History.HistoryChangeList HistoryChanges { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                HistoryChanges = new History.HistoryChangeList();

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            // Get the authentication provider entity type
                            var entityType = EntityTypeCache.Get( Entity.EntityTypeId ?? 0 );
                            var change = HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Authentication Provider" ).SetNewValue( entityType?.FriendlyName );

                            // Don't log Pin Authentication user names.
                            var isUserNameSensitive = ( entityType?.Guid == Rock.SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid() ) ? true : false;

                            if ( isUserNameSensitive )
                            {
                                change.SetCaption( "User Account" );
                            }

                            History.EvaluateChange( HistoryChanges, "User Login", string.Empty, Entity.UserName, isUserNameSensitive );
                            History.EvaluateChange( HistoryChanges, "Is Confirmed", null, Entity.IsConfirmed );
                            History.EvaluateChange( HistoryChanges, "Is Password Change Required", null, Entity.IsPasswordChangeRequired );
                            History.EvaluateChange( HistoryChanges, "Is Locked Out", null, Entity.IsLockedOut );

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            var entityType = EntityTypeCache.Get( Entity.EntityTypeId ?? 0 );

                            // Don't log Pin Authentication user names.
                            var isUserNameSensitive = ( entityType?.Guid == Rock.SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid() ) ? true : false;

                            History.EvaluateChange( HistoryChanges, "User Login", OriginalValues["UserName"].ToStringSafe(), Entity.UserName, isUserNameSensitive );
                            History.EvaluateChange( HistoryChanges, "Is Confirmed", OriginalValues["IsConfirmed"].ToStringSafe().AsBooleanOrNull(), Entity.IsConfirmed );
                            History.EvaluateChange( HistoryChanges, "Is Password Change Required", OriginalValues["IsPasswordChangeRequired"].ToStringSafe().AsBooleanOrNull(), Entity.IsPasswordChangeRequired );
                            History.EvaluateChange( HistoryChanges, "Is Locked Out", OriginalValues["IsLockedOut"].ToStringSafe().AsBooleanOrNull(), Entity.IsLockedOut );
                            History.EvaluateChange( HistoryChanges, "Password", OriginalValues["Password"].ToStringSafe(), Entity.Password, true );

                            // Did the provider type change?
                            int? origEntityTypeId = OriginalValues["EntityTypeId"].ToStringSafe().AsIntegerOrNull();
                            int? entityTypeId = Entity.EntityType != null ? Entity.EntityType.Id : Entity.EntityTypeId;
                            if ( !entityTypeId.Equals( origEntityTypeId ) )
                            {
                                var origProviderType = EntityTypeCache.Get( origEntityTypeId ?? 0 )?.FriendlyName;
                                var providerType = EntityTypeCache.Get( Entity.EntityTypeId ?? 0 )?.FriendlyName;
                                History.EvaluateChange( HistoryChanges, "User Login", origProviderType, providerType );
                            }

                            // Change the caption if this is a sensitive user account
                            if ( HistoryChanges.Count > 0 && isUserNameSensitive )
                            {
                                var change = HistoryChanges.FirstOrDefault();
                                change.SetCaption( "User Account" );
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            // By this point EF has stripped out some of the data we need to save history
                            // Reload the data using a new context.
                            RockContext newRockContext = new RockContext();
                            var userLogin = new UserLoginService( newRockContext ).Get( Entity.Id );
                            if ( userLogin != null && userLogin.PersonId != null )
                            {
                                try
                                {
                                    var entityType = EntityTypeCache.Get( userLogin.EntityTypeId ?? 0 );
                                    var isUserNameSensitive = ( entityType?.Guid == Rock.SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid() ) ? true : false;

                                    if ( !isUserNameSensitive )
                                    {
                                        HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "User Login" ).SetOldValue( userLogin.UserName );
                                        HistoryService.SaveChanges( newRockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), userLogin.PersonId.Value, HistoryChanges, Entity.UserName, typeof( UserLogin ), Entity.Id, true, userLogin.ModifiedByPersonAliasId, null );
                                    }
                                    else
                                    {
                                        HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Authentication Provider" ).SetOldValue( entityType?.FriendlyName ).SetCaption( "User Account" );
                                        HistoryService.SaveChanges( newRockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), userLogin.PersonId.Value, HistoryChanges, entityType?.FriendlyName, typeof( UserLogin ), Entity.Id, true, userLogin.ModifiedByPersonAliasId, null );
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    // Just log the problem and move on...
                                    ExceptionLogService.LogException( ex );
                                }
                            }

                            HistoryChanges.Clear();
                            return;
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
                if ( HistoryChanges?.Any() == true && Entity.PersonId.HasValue )
                {
                    try
                    {
                        HistoryService.SaveChanges( RockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), Entity.PersonId.Value, HistoryChanges, Entity.UserName, typeof( UserLogin ), Entity.Id, true, Entity.ModifiedByPersonAliasId, null );
                    }
                    catch ( Exception ex )
                    {
                        // Just log the problem and move on...
                        ExceptionLogService.LogException( ex );
                    }
                }

                base.PostSave();
            }
        }
    }
}
