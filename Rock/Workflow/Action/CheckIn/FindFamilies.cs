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

using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds families based on a given search criteria (i.e. phone, barcode, etc)
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Finds families based on a given search criteria (i.e. phone, barcode, etc)" )]
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
                checkInState.CheckIn.Families = new List<CheckInFamily>();

                if ( !string.IsNullOrWhiteSpace( checkInState.CheckIn.SearchValue ) )
                {
                    var personService = new PersonService( rockContext );
                    var memberService = new GroupMemberService( rockContext );
                    var groupService = new GroupService( rockContext );

                    int personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    int familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;
                    var dvInactive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                    IQueryable<int> familyIdQry = null;

                    if ( checkInState.CheckIn.SearchType.Guid.Equals( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() ) )
                    {
                        string numericPhone = checkInState.CheckIn.SearchValue.AsNumeric();

                        var phoneQry = new PhoneNumberService( rockContext ).Queryable().AsNoTracking();
                        if ( checkInState.CheckInType == null || checkInState.CheckInType.PhoneSearchType == PhoneSearchType.EndsWith )
                        {
                            char[] charArray = numericPhone.ToCharArray();
                            Array.Reverse( charArray );
                            phoneQry = phoneQry.Where( o =>
                                o.NumberReversed.StartsWith( new string( charArray ) ) );
                        }
                        else
                        {
                            phoneQry = phoneQry.Where( o =>
                                o.Number.Contains( numericPhone ) );
                        }

                        var tmpQry = phoneQry.Join( personService.Queryable().AsNoTracking(),
                                o => new { PersonId = o.PersonId, IsDeceased = false, RecordTypeValueId = personRecordTypeId },
                                p => new { PersonId = p.Id, IsDeceased = p.IsDeceased, RecordTypeValueId = p.RecordTypeValueId.Value },
                                ( pn, p ) => new { Person = p, PhoneNumber = pn } )
                                .Join( memberService.Queryable().AsNoTracking(),
                                pn => pn.Person.Id,
                                m => m.PersonId,
                                ( o, m ) => new { PersonNumber = o.PhoneNumber, GroupMember = m } );

                        familyIdQry = groupService.Queryable().Where( g => tmpQry.Any( o => o.GroupMember.GroupId == g.Id ) && g.GroupTypeId == familyGroupTypeId )
                            .Select( g => g.Id )
                            .Distinct();
                    }
                    else
                    {
                        var familyMemberQry = memberService
                            .AsNoFilter().AsNoTracking()
                            .Where( m =>
                                m.Group.GroupTypeId == familyGroupTypeId &&
                                m.Person.RecordTypeValueId == personRecordTypeId );

                        if ( checkInState.CheckIn.SearchType.Guid.Equals( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() ) )
                        {
                            var personIds = personService.GetByFullName( checkInState.CheckIn.SearchValue, false ).AsNoTracking().Select( p => p.Id );
                            familyMemberQry = familyMemberQry.Where( f => personIds.Contains( f.PersonId ) );
                        }
                        else if ( checkInState.CheckIn.SearchType.Guid.Equals( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID.AsGuid() ) )
                        {
                            var personIds = new List<int>();

                            var dv = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
                            if ( dv != null )
                            {
                                var searchValueService = new PersonSearchKeyService( rockContext );
                                var personAliases = searchValueService.Queryable().AsNoTracking()
                                    .Where( v =>
                                        v.SearchTypeValueId == dv.Id &&
                                        v.SearchValue == checkInState.CheckIn.SearchValue )
                                    .Select( v => v.PersonAlias );

                                if ( personAliases.Any() )
                                {
                                    checkInState.CheckIn.CheckedInByPersonAliasId = personAliases.First().Id;
                                    personIds = personAliases.Select( a => a.PersonId ).ToList();
                                }
                            }

                            if ( personIds.Any() )
                            {
                                familyMemberQry = familyMemberQry.Where( f => personIds.Contains( f.PersonId ) );
                            }
                            else
                            {
                                // if there were no matches, try to find a family check-in identifier. V8 has a "run once" job that moves the family identifiers
                                // to person search values, but in case the job has not yet completed, will still do the check for family ids.
                                var entityIds = new List<int>();

                                var attributeValueService = new AttributeValueService( rockContext );
                                var attr = AttributeCache.Get( "8F528431-A438-4488-8DC3-CA42E66C1B37".AsGuid() );
                                if ( attr != null )
                                {
                                    entityIds = new AttributeValueService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( v =>
                                            v.AttributeId == attr.Id &&
                                            v.EntityId.HasValue &&
                                            ( "|" + v.Value + "|" ).Contains( "|" + checkInState.CheckIn.SearchValue + "|" ) )
                                        .Select( v => v.EntityId.Value )
                                        .ToList();
                                }

                                familyMemberQry = familyMemberQry.Where( f => entityIds.Contains( f.GroupId ) );
                            }

                        }
                        else if ( checkInState.CheckIn.SearchType.Guid.Equals( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID.AsGuid() ) )
                        {
                            List<int> searchFamilyIds = checkInState.CheckIn.SearchValue.SplitDelimitedValues().AsIntegerList();
                            familyMemberQry = familyMemberQry.Where( f => searchFamilyIds.Contains( f.GroupId ) );
                        }
                        else
                        {
                            errorMessages.Add( "Invalid Search Type" );
                            return false;
                        }

                        familyIdQry = familyMemberQry
                            .Select( m => m.GroupId )
                            .Distinct();
                    }

                    int maxResults = checkInState.CheckInType != null ? checkInState.CheckInType.MaxSearchResults : 100;
                    if ( maxResults > 0 )
                    {
                        familyIdQry = familyIdQry.Take( maxResults );
                    }

                    // You might think we should do a ToList() on the familyIdQry and use it below,
                    // but through some extensive testing, we discovered that the next SQL query is better
                    // optimized if it has the familyIdQry without being to-listed.  It was over 270% slower
                    // when querying names and 120% slower when querying phone numbers.

                    // Load the family members
                    var familyMembers = memberService
                        .Queryable().AsNoTracking()
                        .Where( m => m.Group.GroupTypeId == familyGroupTypeId && familyIdQry.Contains( m.GroupId ) ).Select( a =>
                        new {
                            Group = a.Group,
                            GroupId = a.GroupId,
                            Order = a.GroupRole.Order,
                            BirthYear = a.Person.BirthYear,
                            BirthMonth = a.Person.BirthMonth,
                            BirthDay = a.Person.BirthDay,
                            Gender = a.Person.Gender,
                            NickName = a.Person.NickName,
                            RecordStatusValueId = a.Person.RecordStatusValueId
                        } )
                        .ToList();

                    // Add each family
                    foreach ( int familyId in familyMembers.Select( fm => fm.GroupId ).Distinct() )
                    {
                        // Get each of the members for this family
                        var familyMemberQry = familyMembers
                            .Where( m =>
                                m.GroupId == familyId &&
                                m.NickName != null );

                        if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeople && dvInactive != null )
                        {
                            familyMemberQry = familyMemberQry
                                .Where( m =>
                                    m.RecordStatusValueId != dvInactive.Id );
                        }

                        var thisFamilyMembers = familyMemberQry.ToList();

                        if ( thisFamilyMembers.Any() )
                        {
                            var group = thisFamilyMembers
                                .Select( m => m.Group )
                                .FirstOrDefault();

                            var firstNames = thisFamilyMembers
                                .OrderBy( m => m.Order )
                                .ThenBy( m => m.BirthYear )
                                .ThenBy( m => m.BirthMonth )
                                .ThenBy( m => m.BirthDay )
                                .ThenBy( m => m.Gender )
                                .Select( m => m.NickName )
                                .ToList();

                            var family = new CheckInFamily();
                            family.Group = group.Clone( false );
                            family.Caption = group.ToString();
                            family.FirstNames = firstNames;
                            family.SubCaption = firstNames.AsDelimited( ", " );
                            checkInState.CheckIn.Families.Add( family );
                        }
                    }
                }

                return true;
            }

            errorMessages.Add( "Invalid Check-in State" );
            return false;
        }

    }
}