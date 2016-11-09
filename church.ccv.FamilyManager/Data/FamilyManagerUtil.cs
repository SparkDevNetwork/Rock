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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Data.Entity;
using church.ccv.FamilyManager.Models;
using Rock.Rest.Controllers;
using System.Net;

namespace church.ccv.FamilyManager
{
    // Class containing functions related to launching the Mobile App
    public static class Launch
    {
        // the attribute Id for the Family Manager's version
        const int FamilyManagerVersionAttributeId = 0;//29469;

        // Returns the startup data for the Mobile App
        public static CoreData BuildCoreData()
        {
            RockContext rockContext = new RockContext();

            CoreData coreData = new CoreData();

            // setup the campuses
            coreData.Campuses = new List<Campus>( );
            foreach (CampusCache campus in CampusCache.All(false))
            {
                // only include campuses with a location
                if( campus.Location.Longitude.HasValue && campus.Location.Latitude.HasValue )
                {
                    Campus campusModel = new Campus();
                    campusModel.Guid = campus.Guid;
                    campusModel.Id = campus.Id;
                    campusModel.Name = campus.Name;
                
                    campusModel.Location = new Location( );
                    campusModel.Location.SetLocationPointFromLatLong( campus.Location.Latitude.Value, campus.Location.Longitude.Value );

                    coreData.Campuses.Add( campusModel );
                }
            }

            // get needed defined types
            DefinedTypeService definedTypeService = new DefinedTypeService( rockContext );

            coreData.MaritalStatus = definedTypeService.GetByGuid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid( ) ).DefinedValues.ToList( );
            coreData.SchoolGrades = definedTypeService.GetByGuid( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid( ) ).DefinedValues.ToList( );
            coreData.SourceOfVisit = definedTypeService.GetByGuid( Guids.SOURCE_OF_VISIT_DEFINED_TYPE.AsGuid( ) ).DefinedValues.ToList( );
            
            // get needed attributes
            AttributeService attributeService = new AttributeService( rockContext );

            // here, we can control what attributes Family Manager will display!
            coreData.PersonAttributes = new List<PersonAttribute>( );
                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 676 )
                });

                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 715 )
                });

                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 1055 )
                });

                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 1068)
                });
            

            // get needed group type roles
            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( rockContext );
            coreData.FamilyMember_Adult_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid( ) );
            coreData.FamilyMember_Child_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid( ) );

            coreData.CanCheckIn_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid( ) );
            coreData.AllowedCheckInBy_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_CHECK_IN_BY.AsGuid( ) );
            
            return coreData;
        }
    }

    public static class Core
    {
        public class FamilyReturnObject
        {
            public GroupsController.FamilySearchResult Family;
            public IQueryable<GroupsController.GuestFamily> GuestFamilies;
            public Group FamilyGroupObject;
        }

        /// <summary>
        /// Will return the family requested. The statusCode contains the result that should be returned
        /// if needed.
        /// </summary>
        public static FamilyReturnObject GetFamily( int familyId, out HttpStatusCode statusCode )
        {
            statusCode = HttpStatusCode.InternalServerError;

            GroupsController groupController = new GroupsController( );
            FamilyReturnObject familyReturnObj = null;

            do
            {
                // first, get the family's metadata and validate it (and return Not Found if we can't)
                GroupsController.FamilySearchResult familyResult = groupController.GetFamily( familyId );
                if( familyResult == null || familyResult.Id != familyId ) { statusCode = HttpStatusCode.NotFound; break; }


                // next, get the family group (and return Not Found if we can't
                Rock.Model.Group familyGroup = groupController.GetById( familyId );
                if ( familyGroup == null || familyGroup.Id != familyId ) { statusCode = HttpStatusCode.NotFound; break; }

                // sort the family members for the caller
                familyResult.FamilyMembers.Sort( Util.CompareFamilyGroupMembers );

                // now get the guests for this family
                IQueryable<GroupsController.GuestFamily> guestFamilies = groupController.GetGuestsForFamily( familyId );

                foreach( GroupsController.GuestFamily guestFamily in guestFamilies )
                {
                    // before adding, sort the family's members
                    guestFamily.FamilyMembers.Sort( Util.CompareGuestFamilyMembers );
                }

                // and finally package it all up into something FamilyManager can use.
                familyReturnObj = new FamilyReturnObject( )
                {
                    Family = familyResult,
                    GuestFamilies = guestFamilies,
                    FamilyGroupObject = familyGroup
                };

                statusCode = HttpStatusCode.OK;
            }
            while( false );

            return familyReturnObj;
        }


        public class PersonReturnObject
        {
            public Person Person;
            public List<GroupsController.FamilySearchResult> FamiliesForPerson;
        }

        public static PersonReturnObject GetPersonForEdit( int personId, out HttpStatusCode statusCode )
        {
            PersonReturnObject personReturnObj = null;

            statusCode = HttpStatusCode.InternalServerError;
            
            do
            {
                // start by getting the person
                RockContext rockContext = new RockContext( );
                PersonService personService = new PersonService( rockContext );

                Person person = personService.Get( personId );
                if ( person == null ) { statusCode = HttpStatusCode.NotFound; break; }
                
                person.LoadAttributes( );

                // now we want all the families that are allowed to check this person in.
                // get the ID for the ALLOW_CHECK_IN_BY group role.
                GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( rockContext );
                GroupTypeRole allowCheckinByRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_CHECK_IN_BY.AsGuid( ) );
                if( allowCheckinByRole == null ) break;
                

                // now get all the people who are allowed to checkin this person
                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                IQueryable<GroupMember> allowCheckinByMembers = groupMemberService.GetKnownRelationship( personId, allowCheckinByRole.Id );

                
                // now get the families of each groupMember with this permission
                GroupsController groupController = new GroupsController( );
                List<GroupsController.FamilySearchResult> familiesForPerson = new List<GroupsController.FamilySearchResult>( );

                foreach( GroupMember groupMember in allowCheckinByMembers )
                {
                    // first, get the group representing the family.
                    IQueryable<Rock.Model.Group> familyGroups = groupController.GetFamilies( groupMember.PersonId );

                    // now convert each of these into FamilySearchResult objects
                    foreach( Group group in familyGroups )
                    {
                        GroupsController.FamilySearchResult familyObj = groupController.GetFamily( group.Id );
                        if( familyObj != null )
                        {
                            // before adding, sort the family's members
                            familyObj.FamilyMembers.Sort( Util.CompareFamilyGroupMembers );

                            familiesForPerson.Add( familyObj );
                        }
                    }
                }
                                
                // finally, package it up into an anonymous class and return it
                personReturnObj = new PersonReturnObject( )
                {
                    Person = person,
                    FamiliesForPerson = familiesForPerson
                };
                
                statusCode = HttpStatusCode.OK;
            }
            while( false );

            return personReturnObj;
        }


        public static int UpdateOrAddPerson( UpdatePersonBody updatePersonBody, out HttpStatusCode statusCode )
        {
            statusCode = HttpStatusCode.InternalServerError;
            int personId = 0;

            do
            {
                RockContext rockContext = new RockContext( );
                PersonService personService = new PersonService( rockContext );
                GroupService groupService = new GroupService( rockContext );
                GroupsController groupController = new GroupsController( );

                // get all required values and make sure they exist
                DefinedValueCache cellPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                DefinedValueCache connectionStatusVisitor = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR);
                DefinedValueCache recordStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING);
                DefinedValueCache recordTypePerson = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON);
                
                if( cellPhoneType == null || connectionStatusVisitor == null || recordStatusPending == null || recordTypePerson == null || groupController == null ) break;
                
                // store history changes
                var changes = new List<string>();
                
                // if the person ID passed up is 0, we're creating a new person
                Person person = null;
                if( updatePersonBody.Person.Id == 0 )
                {
                    person = new Person( );

                    // record who made these changes
                    person.ModifiedAuditValuesAlreadyUpdated = true;
                    person.CreatedByPersonAliasId = updatePersonBody.CurrentPersonAliasId;
                    person.ModifiedByPersonAliasId = updatePersonBody.CurrentPersonAliasId;

                    // for new people, copy the stuff sent by Family Manager
                    person.FirstName = updatePersonBody.Person.NickName.Trim(); //(Per Rock design, always set FirstName to be NickName)
                    person.MiddleName = updatePersonBody.Person.MiddleName.Trim();
                    person.LastName = updatePersonBody.Person.LastName.Trim();
                    person.SetBirthDate( updatePersonBody.Person.BirthDate );
                    person.Gender = updatePersonBody.Person.Gender;
                    person.MaritalStatusValueId = updatePersonBody.Person.MaritalStatusValueId;

                    person.Email = updatePersonBody.Person.Email.Trim();
                    person.IsEmailActive = string.IsNullOrWhiteSpace( person.Email ) == false ? true : false;
                    person.EmailPreference = EmailPreference.EmailAllowed;

                    // since this is a new person, flag now as their first visit
                    AttributeService attributeService = new AttributeService( rockContext );
                    Rock.Model.Attribute firstVisitAttrib = attributeService.Get( Guids.FIRST_TIME_VISIT_DEFINED_TYPE.AsGuid( ) );
                    updatePersonBody.Attributes.Add( new KeyValuePair<string, string>( firstVisitAttrib.Key, RockDateTime.Now.ToString( ) ) );


                    // now set values so it's a Person Record Type, and pending visitor.
                    person.ConnectionStatusValueId = connectionStatusVisitor.Id;
                    person.RecordStatusValueId = recordStatusPending.Id;
                    person.RecordTypeValueId = recordTypePerson.Id;

                    // lastly, save the person so that all the extra stuff (known relationship groups) gets created.
                    Group newFamily = PersonService.SaveNewPerson( person, rockContext, updatePersonBody.CampusId );

                    // if there's an existing family to place them in, put them in it and remove them from the existing, newly created family.
                    if ( updatePersonBody.FamilyId > 0 )
                    {
                        // and now put this person into the family they were just added to, and then remove them from all other families (which would just be the newly created one above)
                        PersonService.AddPersonToFamily( person, false, updatePersonBody.FamilyId, updatePersonBody.GroupRoleId, rockContext );
                        PersonService.RemovePersonFromOtherFamilies( updatePersonBody.FamilyId, person.Id, rockContext );
                    }
                }
                // Otherwise, this is editing an existing person
                else
                {
                    // if the person can't be found, stop and let the caller know.
                    person = personService.Get( updatePersonBody.Person.Id );
                    if( person == null ) { statusCode = HttpStatusCode.NotFound; break; }


                    // store history
                    History.EvaluateChange( changes, "First Name", person.FirstName, updatePersonBody.Person.FirstName );
                    History.EvaluateChange( changes, "Nick Name", person.NickName, updatePersonBody.Person.NickName );
                    History.EvaluateChange( changes, "Middle Name", person.MiddleName, updatePersonBody.Person.MiddleName );
                    History.EvaluateChange( changes, "Last Name", person.LastName, updatePersonBody.Person.LastName );
                    History.EvaluateChange( changes, "Birth Month", person.BirthMonth, updatePersonBody.Person.BirthMonth );
                    History.EvaluateChange( changes, "Birth Day", person.BirthDay, updatePersonBody.Person.BirthDay );
                    History.EvaluateChange( changes, "Birth Year", person.BirthYear, updatePersonBody.Person.BirthYear );
                    History.EvaluateChange( changes, "Gender", person.Gender, updatePersonBody.Person.Gender );
                    History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( updatePersonBody.Person.MaritalStatusValueId ) );
                    History.EvaluateChange( changes, "Email", person.Email, updatePersonBody.Person.Email );
                    
                    personService.SetValues( updatePersonBody.Person, person );

                    // record the person making these changes
                    person.ModifiedAuditValuesAlreadyUpdated = true;
                    person.CreatedByPersonAliasId = updatePersonBody.CurrentPersonAliasId;
                    person.ModifiedByPersonAliasId = updatePersonBody.CurrentPersonAliasId;

                    // if there's no family, stop and we'll return the default internal error.
                    // It's not possible to edit an existing person in a NEW family.
                    if( updatePersonBody.FamilyId == 0 ) break;


                    // Now update their adult / child status in the family.
                    Group family = groupService.Get( updatePersonBody.FamilyId );
                    if( family == null ) break;

                    // get them as a family member and update their group role
                    GroupMember familyMember = family.Members.Where( gm => gm.PersonId == person.Id ).SingleOrDefault( );
                    if( familyMember == null ) break;

                    familyMember.GroupRoleId = updatePersonBody.GroupRoleId;
                }

                // attempt to set their graduation year (if this is an adult and null is passed, it won't cause an existing grade to be erased)
                person.GraduationYear = Person.GraduationYearFromGradeOffset( updatePersonBody.GradeOffset );
                History.EvaluateChange( changes, "Graduation Year", person.GraduationYear, updatePersonBody.Person.GraduationYear );


                // now set the attributes
                person.LoadAttributes( );
                foreach( KeyValuePair<string, string> attributeKV in updatePersonBody.Attributes )
                {
                    person.SetAttributeValue( attributeKV.Key, attributeKV.Value );
                }

                // set the phone number (we only support Cell Phone for Family Manager)
                person.UpdatePhoneNumber( cellPhoneType.Id, updatePersonBody.CellPhoneNumber.CountryCode, updatePersonBody.CellPhoneNumber.Number, null, null, rockContext ); 
                
                // save all changes
                person.SaveAttributeValues( rockContext );
                rockContext.SaveChanges( );
                
                HistoryService.SaveChanges(
                    rockContext,
                    typeof( Person ),
                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                    person.Id,
                    changes,
                    true,
                    updatePersonBody.CurrentPersonAliasId );
                
                // report noContent, implying we updated or created.
                statusCode = HttpStatusCode.NoContent;

                // set the person ID for the person updated
                personId = person.Id;
            }
            while( false );
            
            return personId;
        }
    }

    // Common reusable functions for supporting Family Manager
    public static class Util
    {
        public static void LaunchWorkflow( RockContext rockContext, WorkflowType workflowType, GroupMember groupMember )
        {
            try
            {
                List<string> workflowErrors;
                var workflow = Workflow.Activate( workflowType, workflowType.Name );
                new WorkflowService( rockContext ).Process( workflow, groupMember, out workflowErrors );
            }
            catch (Exception ex)
            {
                ExceptionLogService.LogException( ex, null );
            }
        }
        
        public static int CompareGuestFamilyMembers( GroupsController.GuestFamilyMember x, GroupsController.GuestFamilyMember y )
        {
            // determine whether they're adults by finding out if they ARE NOT children (that will let them be adults
            // if their GroupRole happens to be "Unknown" or something)
            bool xIsAdult = x.Role == "Child"? false : true;
            bool yIsAdult = y.Role == "Child" ? false : true;

            // if they're both adults or children
            if( xIsAdult == yIsAdult )
            {
                // then the non-female should win
                if( x.Gender != Gender.Female )
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            // if one is an adult and the other a child, the adult should win
            if( xIsAdult == true && yIsAdult == false )
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public static int CompareFamilyGroupMembers( Rock.Model.GroupMember x, Rock.Model.GroupMember y )
        {
            // grab the ID for the child group role
            GroupTypeCache groupTypeFamily = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            int childRoleId = groupTypeFamily != null ? groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id : 0;

            // determine whether they're adults by finding out if they ARE NOT children (that will let them be adults
            // if their GroupRole happens to be "Unknown" or something)
            bool xIsAdult = x.GroupRoleId == childRoleId ? false : true;
            bool yIsAdult = y.GroupRoleId == childRoleId ? false : true;

            // if they're both adults or children
            if( xIsAdult == yIsAdult )
            {
                // then the non-female should win
                if( x.Person.Gender != Gender.Female )
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            // if one is an adult and the other a child, the adult should win
            if( xIsAdult == true && yIsAdult == false )
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
