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
    public partial class Auth
    {
        /// <summary>
        /// Save hook implementation for <see cref="Auth"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Auth>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var auth = this.Entity as Auth;

                var rockContext = this.RockContext;
                if ( rockContext != null &&
                    ( State == EntityContextState.Added || State == EntityContextState.Modified || State == EntityContextState.Deleted ) )
                {
                    var authAuditLogService = new AuthAuditLogService( rockContext );
                    var authAuditLog = new AuthAuditLog();
                    authAuditLog.EntityTypeId = auth.EntityTypeId;
                    authAuditLog.EntityId = auth.EntityId;
                    authAuditLog.Action = auth.Action;
                    authAuditLog.ChangeDateTime = RockDateTime.Now;
                    authAuditLog.SpecialRole = auth.SpecialRole;
                    authAuditLog.ChangeByPersonAliasId = rockContext.GetCurrentPersonAliasId();

                    authAuditLogService.Add( authAuditLog );

                    if ( State == EntityContextState.Added )
                    {
                        authAuditLog.GroupId = auth.GroupId;
                        authAuditLog.PersonAliasId = auth.PersonAliasId;
                        authAuditLog.ChangeType = ChangeType.Add;
                    }
                    else if ( State == EntityContextState.Modified )
                    {
                        authAuditLog.GroupId = auth.GroupId;
                        authAuditLog.PersonAliasId = auth.PersonAliasId;
                        authAuditLog.ChangeType = ChangeType.Modify;
                    }
                    else
                    {
                        authAuditLog.GroupId = this.Entry.OriginalValues[nameof( Auth.GroupId )] as int?;
                        authAuditLog.PersonAliasId = this.Entry.OriginalValues[nameof( Auth.PersonAliasId )] as int?;
                        authAuditLog.ChangeType = ChangeType.Delete;
                    }

                    if ( State == EntityContextState.Added || State == EntityContextState.Modified )
                    {
                        authAuditLog.PostAllowOrDeny = auth.AllowOrDeny;
                        authAuditLog.PostOrder = auth.Order;
                    }

                    if ( State == EntityContextState.Modified || State == EntityContextState.Deleted )
                    {
                        authAuditLog.PreAllowOrDeny = this.Entry.OriginalValues[nameof( Auth.AllowOrDeny )] as string;
                        authAuditLog.PreOrder = this.Entry.OriginalValues[nameof( Auth.Order )] as int?;
                    }
                }

                base.PreSave();
            }
        }
    }
}
