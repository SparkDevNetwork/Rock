// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Caching;

using Rock.CheckIn;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds people with a relationship to members of family
    /// </summary>
    [Description( "Finds people with a relationship to members of family" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Find Relationships" )]
    public class FindRelationships : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            string cacheKey = "Rock.FindRelationships.Roles";

            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            List<int> roles = cache[cacheKey] as List<int>;

            if ( roles == null )
            {
                roles = new List<int>();

                foreach ( var role in new GroupTypeRoleService( rockContext ).Queryable()
                    .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) ) )
                {
                    role.LoadAttributes( rockContext );
                    if ( role.Attributes.ContainsKey( "CanCheckin" ) )
                    {
                        bool canCheckIn = false;
                        if ( bool.TryParse( role.GetAttributeValue( "CanCheckin" ), out canCheckIn ) && canCheckIn )
                        {
                            roles.Add( role.Id );
                        }
                    }
                }

                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( 300 );
                cache.Set( cacheKey, roles, cacheItemPolicy );
            }

            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                if ( !roles.Any() )
                {
                    return true;
                }

                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var service = new GroupMemberService( rockContext );

                    var familyMemberIds = family.People.Select( p => p.Person.Id ).ToList();

                    // Get the Known Relationship group id's for each person in the family
                    var relationshipGroups = service.Queryable()
                        .Where( g =>
                            g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                            familyMemberIds.Contains( g.PersonId ) )
                        .Select( g => g.GroupId )
                        .ToList();

                    // Get anyone in any of those groups that has a role with the canCheckIn attribute set
                    foreach ( var person in service.Queryable()
                        .Where( g =>
                            relationshipGroups.Contains( g.GroupId ) &&
                            roles.Contains( g.GroupRoleId ) )
                        .Select( g => g.Person )
                        .Distinct().ToList() )
                    {
                        if ( !family.People.Any( p => p.Person.Id == person.Id ) )
                        {
                            var relatedPerson = new CheckInPerson();
                            relatedPerson.Person = person.Clone( false );
                            relatedPerson.FamilyMember = false;
                            family.People.Add( relatedPerson );
                        }
                    }

                    return true;
                }
                else
                {
                    errorMessages.Add( "There is not a family that is selected" );
                }

                return false;
            }

            return false;
        }
    }
}