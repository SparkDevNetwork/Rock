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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds families based on a given search critieria (i.e. phone, barcode, etc)
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Finds families based on a given search critieria (i.e. phone, barcode, etc)" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Find Families" )]
    public class FindFamilies : CheckInActionComponent
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
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null && checkInState.CheckIn.SearchType != null )
            {
                var personService = new PersonService( rockContext );
                var memberService = new GroupMemberService( rockContext );

                Guid familyGroupTypeGuid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                var dvInactive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER ) ) )
                {
                    string numericPhone = checkInState.CheckIn.SearchValue.AsNumeric();

                    var personRecordTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                    // Find the families with any member who has a phone number that contains selected value
                    var familyQry = memberService
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupType.Guid.Equals( familyGroupTypeGuid ) &&
                            m.Person.RecordTypeValueId == personRecordTypeId );

                    if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeopele && dvInactive != null )
                    {
                        familyQry = familyQry.Where( m =>
                            m.Person.RecordStatusValueId != dvInactive.Id );
                    }

                    if ( checkInState.CheckInType == null || checkInState.CheckInType.PhoneSearchType == PhoneSearchType.EndsWith )
                    {
                        familyQry = familyQry.Where( m =>
                            m.Person.PhoneNumbers.Any( n => n.Number.EndsWith( numericPhone ) ) );
                    }
                    else
                    {
                        familyQry = familyQry.Where( m =>
                            m.Person.PhoneNumbers.Any( n => n.Number.Contains( numericPhone ) ) );
                    }

                    var familyIdQry = familyQry
                        .Select( m => m.GroupId )
                        .Distinct();

                    int maxResults = checkInState.CheckInType != null ? checkInState.CheckInType.MaxSearchResults : 100;
                    if ( maxResults > 0 )
                    {
                        familyIdQry = familyIdQry.Take( maxResults );
                    }

                    var familyIds = familyIdQry.ToList();

                    // Load the family members
                    var familyMembers = memberService
                        .Queryable( "Group,GroupRole,Person" ).AsNoTracking()
                        .Where( m => familyIds.Contains( m.GroupId ) )
                        .ToList();

                    // Add each family
                    foreach ( int familyId in familyIds )
                    {
                        // Get each of the members for this family
                        var familyMemberQry = familyMembers
                            .Where( m =>
                                m.GroupId == familyId &&
                                m.Person.NickName != null );

                        if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeopele && dvInactive != null )
                        {
                            familyMemberQry = familyMemberQry
                                .Where( m =>
                                    m.Person.RecordStatusValueId != dvInactive.Id );
                        }

                        var thisFamilyMembers = familyMemberQry.ToList();

                        if ( thisFamilyMembers.Any() )
                        {
                            var group = thisFamilyMembers
                                .Select( m => m.Group )
                                .FirstOrDefault();

                            var firstNames = thisFamilyMembers
                                .OrderBy( m => m.GroupRole.Order )
                                .ThenBy( m => m.Person.BirthYear )
                                .ThenBy( m => m.Person.BirthMonth )
                                .ThenBy( m => m.Person.BirthDay )
                                .ThenBy( m => m.Person.Gender )
                                .Select( m => m.Person.NickName )
                                .ToList();

                            var family = new CheckInFamily();
                            family.Group = group.Clone( false );
                            family.Caption = group.ToString();
                            family.SubCaption = firstNames.AsDelimited( ", " );
                            checkInState.CheckIn.Families.Add( family );
                        }
                    }
                }
                else if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME ) ) )
                {
                    var people = personService.GetByFullName( checkInState.CheckIn.SearchValue, false ).AsNoTracking();
                    if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeopele && dvInactive != null )
                    {
                        people = people.Where( p => p.RecordStatusValueId != dvInactive.Id );
                    }

                    foreach ( var person in people )
                    {
                        foreach ( var group in person.Members.Where( m => m.Group.GroupType.Guid.Equals( familyGroupTypeGuid ) ).Select( m => m.Group ).ToList() )
                        {
                            var family = checkInState.CheckIn.Families.Where( f => f.Group.Id == group.Id ).FirstOrDefault();
                            if ( family == null )
                            {
                                family = new CheckInFamily();
                                family.Group = group.Clone( false );
                                family.Group.LoadAttributes( rockContext );
                                family.Caption = group.ToString();

                                if ( checkInState.CheckInType == null || !checkInState.CheckInType.PreventInactivePeopele )
                                {
                                    family.SubCaption = memberService.GetFirstNames( group.Id ).ToList().AsDelimited( ", " );
                                }
                                else
                                {
                                    family.SubCaption = memberService.GetFirstNames( group.Id, false, false ).ToList().AsDelimited( ", " );
                                }

                                checkInState.CheckIn.Families.Add( family );
                            }
                        }
                    }
                }
                else
                {
                    errorMessages.Add( "Invalid Search Type" );
                    return false;
                }

                return true;
            }

            errorMessages.Add( "Invalid Check-in State" );
            return false;
        }
    }
}