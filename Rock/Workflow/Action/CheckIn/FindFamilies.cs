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
    [Description("Finds families based on a given search critieria (i.e. phone, barcode, etc)")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Find Families" )]
    [IntegerField( "Max Results", "The maximum number of families to return ( Default is 100, a value of 0 will not limit results ).", false, 100, "", 0 )]
    [CustomRadioListField("Phone Search Type", "The type of search to use when finding families with a matching phone number.", 
        "0^Include families with a phone number that CONTAINS the value entered,1^Include families with a phone number that END WITH the value entered", true, "0", "",  1)]
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
            if ( checkInState != null )
            {
                var personService = new PersonService( rockContext );
                var memberService = new GroupMemberService( rockContext );

                Guid familyGroupTypeGuid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

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

                    int? phoneSearchType = GetAttributeValue( action, "PhoneSearchType" ).AsIntegerOrNull();
                    if ( phoneSearchType.HasValue && phoneSearchType.Value == 1 )
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

                    int maxResults = GetAttributeValue( action, "MaxResults" ).AsInteger();
                    if ( maxResults > 0 )
                    {
                        familyIdQry = familyIdQry.Take( maxResults );
                    }

                    var familyIds = familyIdQry.ToList();

                    // Load the family members
                    var familyMembers = memberService
                        .Queryable( "Group,GroupRole,Person" ).AsNoTracking()
                        .Where( m => familyIds.Contains(m.GroupId) )
                        .ToList();

                    // Add each family
                    foreach ( int familyId in familyIds )
                    {
                        // Get each of the members for this family
                        var thisFamilyMembers = familyMembers
                            .Where( m => 
                                m.GroupId == familyId && 
                                m.Person.NickName != null )
                            .ToList();

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
                    foreach ( var person in personService.GetByFullName( checkInState.CheckIn.SearchValue, false ).AsNoTracking() )
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
                                family.SubCaption = memberService.GetFirstNames( group.Id ).ToList().AsDelimited( ", " );
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