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

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds families based on a given search critieria (i.e. phone, barcode, etc)
    /// </summary>
    [Description("Finds families based on a given search critieria (i.e. phone, barcode, etc)")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Find Families" )]    
    public class FindFamilies : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if (checkInState != null)
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var personService = new PersonService();
                    var memberService = new GroupMemberService();
                    IQueryable<Person> people = null;

                    if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER ) ) )
                    {
                        people = personService.GetByPhonePartial( checkInState.CheckIn.SearchValue );
                    }
                    else if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME ) ) )
                    {
                        people = personService.GetByFullName( checkInState.CheckIn.SearchValue, false );
                    }
                    else
                    {
                        errorMessages.Add( "Invalid Search Type" );
                        return false;
                    }

                    foreach ( var person in people.ToList() )
                    {
                        foreach ( var group in person.Members.Where( m => m.Group.GroupType.Guid == new Guid( SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Select( m => m.Group ).ToList() )
                        {
                            var family = checkInState.CheckIn.Families.Where( f => f.Group.Id == group.Id ).FirstOrDefault();
                            if ( family == null )
                            {
                                family = new CheckInFamily();
                                family.Group = group.Clone( false );
                                family.Group.LoadAttributes();
                                family.Caption = group.ToString();
                                family.SubCaption = memberService.GetFirstNames( group.Id ).ToList().AsDelimited( ", " );
                                checkInState.CheckIn.Families.Add( family );
                            }
                        }
                    }

                    return true;
                }
            }

            errorMessages.Add( "Invalid Check-in State" );
            return false;
        }
    }
}