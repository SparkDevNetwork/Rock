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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/Service class for entities of the <see cref="Rock.Model.NoteType"/>
    /// </summary>
    public partial class NoteTypeService : Service<NoteType>
    {
        /// <summary>
        /// Gets the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="create">if set to <c>true</c> [create].</param>
        /// <returns></returns>
        public NoteType Get( int entityTypeId, string name, bool create = true )
        {
            var noteTypes = Get( entityTypeId, string.Empty, string.Empty ).ToList();
            var noteType = noteTypes.Where( t => t.Name == name ).FirstOrDefault();

            if ( noteType == null && create )
            {
                noteType = new NoteType();
                noteType.IsSystem = false;
                noteType.EntityTypeId = entityTypeId;
                noteType.EntityTypeQualifierColumn = string.Empty;
                noteType.EntityTypeQualifierValue = string.Empty;
                noteType.Name = name;
                noteType.UserSelectable = true;
                noteType.IconCssClass = string.Empty;
                noteType.Order = noteTypes.Any() ? noteTypes.Max( t => t.Order ) + 1 : 0;

                // Create a new context/service so that save does not affect calling method's context
                using ( var rockContext = new RockContext() )
                {
                    var noteTypeService = new NoteTypeService( rockContext );
                    noteTypeService.Add( noteType );
                    rockContext.SaveChanges();
                }

                // requery using calling context
                noteType = Get( noteType.Id );
            }

            return noteType;
        }

        /// <summary>
        /// Gets the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        public IQueryable<NoteType> Get( int entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            return Queryable()
                .Where( n =>
                    n.EntityTypeId == entityTypeId &&
                    ( n.EntityTypeQualifierColumn ?? "" ) == ( entityTypeQualifierColumn ?? "" ) &&
                    ( n.EntityTypeQualifierValue ?? "" ) == ( entityTypeQualifierValue ?? "" ) );
        }

        /// <summary>
        /// Gets the approvers.
        /// </summary>
        /// <param name="noteTypeId">The note type identifier.</param>
        /// <returns></returns>
        public List<Person> GetApprovers( int noteTypeId )
        {
            var rockContext = this.Context as RockContext;
            var groupMemberService = new GroupMemberService( rockContext );

            var noteType = NoteTypeCache.Get( noteTypeId );
            int? noteTypeEntityTypeId = EntityTypeCache.GetId<NoteType>();
            if ( !noteTypeEntityTypeId.HasValue || noteType == null )
            {
                // shouldn't happen
                return new List<Person>();
            }

            var authService = new AuthService( rockContext );

            var approvalAuths = authService.GetAuths( noteTypeEntityTypeId.Value, noteTypeId, Rock.Security.Authorization.APPROVE );

            // Get a list of all PersonIds that are allowed that are included in the Auths
            // Then, when we get a list of all the allowed people that are in the auth as a specific Person or part of a Role (Group), we'll run all those people thru NoteType.IsAuthorized
            // That way, we don't have to figure out all the logic of Allow/Deny based on Order, etc
            List<int> authPersonIdListAll = new List<int>();
            var approvalAuthsAllowed = approvalAuths.Where( a => a.AllowOrDeny == "A" ).ToList();

            foreach ( var approvalAuth in approvalAuthsAllowed )
            {
                var personId = approvalAuth.PersonAlias?.PersonId;
                if ( personId.HasValue )
                {
                    authPersonIdListAll.Add( personId.Value );
                }
                else if ( approvalAuth.GroupId.HasValue )
                {
                    var groupPersonIdsQuery = groupMemberService.Queryable().Where( a => a.GroupId == approvalAuth.GroupId.Value && a.GroupMemberStatus == GroupMemberStatus.Active && a.Person.IsDeceased == false ).Select( a => a.PersonId );
                    authPersonIdListAll.AddRange( groupPersonIdsQuery.ToList() );
                }
                else if ( approvalAuth.SpecialRole != SpecialRole.None )
                {
                    // Not Supported: Get people that belong to Special Roles like AllUsers, AllAuthenticatedUser, and AllUnAuthenicatedUsers doesn't really make sense, so ignore it
                }
            }

            authPersonIdListAll = authPersonIdListAll.Distinct().ToList();

            var authPersonsAll = new PersonService( rockContext ).Queryable().Where( a => authPersonIdListAll.Contains( a.Id ) ).ToList();
            var authorizedApprovers = new List<Person>();

            // now that we have a list of all people that have at least one Allow auth, run them thru noteType.IsAuthorized, to get rid of people that would have been excluded due to a Deny auth
            foreach ( var authPerson in authPersonsAll )
            {
                if ( noteType.IsAuthorized( Rock.Security.Authorization.APPROVE, authPerson ) )
                {
                    authorizedApprovers.Add( authPerson );
                }
            }

            return authorizedApprovers;
        }
    }
}
