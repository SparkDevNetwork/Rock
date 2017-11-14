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
using church.ccv.MobileApp.Models;
using System.Net;
using church.ccv.Actions;
using System.Data.Entity.Spatial;

namespace church.ccv.MobileApp
{
    // Class containing functions related to finding and joining a group
    public static class GroupFinder
    {
        // the workflow type id for the alert note re-route
        const int AlertNoteReReouteWorkflowId = 166;

        // The Coach Role Type ID for the various groups you can join thru the Mobile App
        static int[] Coach_GroupRole_Id =
            {
                50,  //Neighborhood
                133, //Next Gen
                139, //Young Adults
                158  //Short Term Group
            };
        
        const string GroupInfo_Key = "GroupDescription";
        const string ChildcareInfo_Key = "Childcare";
        const string FamilyPicture_Key = "FamilyPicture";
        const string GroupFilters_Key = "GroupFilters";

        public static List<GroupResult> GetGroupsByLocation( int groupTypeId, int locationId, int skip, int top, bool publicOnly )
        {
            var rockContext = new RockContext( );
            GroupService groupService = new GroupService( rockContext );

            List<GroupResult> resultGroups = new List<GroupResult>();
            
            do
            {
                // we MUST have a valid location
                var specifiedLocation = new LocationService( rockContext ).Get( locationId );
                if( specifiedLocation == null ) break;

                // we MUST have a geoPoint (from which latitude / long derive)
                if( specifiedLocation.GeoPoint == null ) break;

                // get all groups of this group type that are either public, if publicOnly is true, or either if publicOnly is false
                IEnumerable<Group> groupList = groupService.Queryable( "Schedule,GroupLocations.Location" ).AsNoTracking( )
                                                           .Where( a => a.GroupTypeId == groupTypeId && (a.IsPublic == publicOnly || publicOnly == false) )
                                                           .Include( a => a.GroupLocations ).ToList( );
                

                // calculate the distance of each of the groups locations from the specified geoFence
                foreach ( var group in groupList )
                {
                    foreach ( var gl in group.GroupLocations )
                    {
                        // Calculate distance
                        if ( gl.Location.GeoPoint != null )
                        {
                            double meters = gl.Location.GeoPoint.Distance( specifiedLocation.GeoPoint ) ?? 0.0D;
                            gl.Location.SetDistance( meters * Location.MilesPerMeter );
                        }
                    }
                }

                // remove groups that don't have a GeoPoint
                groupList = groupList.Where( a => a.GroupLocations.Any( x => x.Location.GeoPoint != null ) );
                
                // sort by distance
                groupList = groupList.OrderBy( a => a.GroupLocations.FirstOrDefault() != null ? a.GroupLocations.FirstOrDefault().Location.Distance : int.MaxValue );
                
                // grab the nth set
                groupList = groupList.Skip( skip );
                groupList = groupList.Take( top ).ToList();

                // now take only what we need from each group (drops our return package to about 2kb, from 40kb)
                foreach( Group group in groupList )
                {
                    Location locationObj = group.GroupLocations.First( ).Location;

                    GroupResult groupResult = new GroupResult( )
                    {
                        Name = group.Name,
                        Id = group.Id,
                        Longitude = locationObj.Longitude.Value,
                        Latitude = locationObj.Latitude.Value,
                        DistanceFromSource = locationObj.Distance,
                        MeetingTime = group.Schedule != null ? group.Schedule.FriendlyScheduleText : ""
                    };
                    
                    // load the attributes so we can take the filters value
                    group.LoadAttributes( );
                    if ( group.AttributeValues.ContainsKey( GroupFilters_Key ) )
                    {
                        groupResult.Filters = group.AttributeValues[ GroupFilters_Key ].Value;
                    }

                    resultGroups.Add( groupResult );
                }
            }
            while( false );
            
            return resultGroups;
        }
        
        // Returns the info for a joinable Group in the Mobile App
        public static bool GetGroupInfo(int groupId, out GroupInfo groupInfo)
        {
            bool success = false;
            RockContext rockContext = new RockContext();

            groupInfo = new GroupInfo();

            // first, find the group they want
            Group group = new GroupService(rockContext).Queryable().Where(g => g.Id == groupId).SingleOrDefault();
            if (group != null)
            {
                // now get the group leader. If there isn't one, we'll fail.
                GroupMember leader = group.Members.Where(gm => Coach_GroupRole_Id.Contains(gm.GroupRole.Id)).SingleOrDefault();
                if (leader != null)
                {
                    // load the group attributes so we can populate 
                    group.LoadAttributes();

                    // test for the known attributes, and set them if they exist.
                    if (group.AttributeValues.ContainsKey(GroupInfo_Key))
                    {
                        groupInfo.Description = group.AttributeValues[GroupInfo_Key].Value;
                    }

                    if (group.AttributeValues.ContainsKey( ChildcareInfo_Key ))
                    {
                        groupInfo.ChildcareDesc = group.AttributeValues[ ChildcareInfo_Key ].Value;
                    }

                    if( group.AttributeValues.ContainsKey( FamilyPicture_Key ) )
                    {
                        groupInfo.FamilyPhotoGuid = group.AttributeValues[ FamilyPicture_Key ].Value.AsGuid( );
                    }

                    if ( group.AttributeValues.ContainsKey( GroupFilters_Key ) )
                    {
                        groupInfo.Filters = group.AttributeValues[ GroupFilters_Key ].Value;
                    }

                    success = true;
                }
            }

            return success;
        }

        public static bool RegisterPersonInGroup(GroupRegModel regModel)
        {
            bool success = false;

            // setup all variables we'll need
            var rockContext = new RockContext();
            var personService = new PersonService(rockContext);
            var groupService = new GroupService(rockContext);

            DefinedValueCache connectionStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT);
            DefinedValueCache recordStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING);
            DefinedValueCache homeAddressType = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME);

            Person person = null;
            Person spouse = null;
            Group family = null;
            GroupLocation homeLocation = null;

            // setup history tracking
            var changes = new List<string>();
            var familyChanges = new List<string>();

            // first, get the group the person wants to join
            Group requestedGroup = groupService.Get(regModel.RequestedGroupId);
            if (requestedGroup != null)
            {
                // Try to find person by name/email 
                var matches = personService.GetByMatch(regModel.FirstName.Trim(), regModel.LastName.Trim(), regModel.Email.Trim());
                if (matches.Count() == 1)
                {
                    person = matches.First();
                }

                // Check to see if this is a new person
                if (person == null)
                {
                    // If so, create the person and family record for the new person
                    person = new Person();
                    person.FirstName = regModel.FirstName.Trim();
                    person.LastName = regModel.LastName.Trim();
                    person.Email = regModel.Email.Trim();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                    person.ConnectionStatusValueId = connectionStatusPending.Id;
                    person.RecordStatusValueId = recordStatusPending.Id;
                    person.Gender = Gender.Unknown;

                    family = PersonService.SaveNewPerson(person, rockContext, requestedGroup.CampusId, false);
                }
                else
                {
                    // updating existing person
                    History.EvaluateChange(changes, "Email", person.Email, regModel.Email);
                    person.Email = regModel.Email;

                    // Get the current person's families
                    var families = person.GetFamilies(rockContext);

                    // look for first family with a home location
                    foreach (var aFamily in families)
                    {
                        homeLocation = aFamily.GroupLocations
                            .Where(l =>
                               l.GroupLocationTypeValueId == homeAddressType.Id &&
                               l.IsMappedLocation)
                            .FirstOrDefault();
                        if (homeLocation != null)
                        {
                            family = aFamily;
                            break;
                        }
                    }

                    // If a family wasn't found with a home location, use the person's first family
                    if (family == null)
                    {
                        family = families.FirstOrDefault();
                    }
                }

                // if provided, store their phone number
                if (string.IsNullOrWhiteSpace(regModel.Phone) == false)
                {
                    DefinedValueCache mobilePhoneType = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE);
                    person.UpdatePhoneNumber(mobilePhoneType.Id, PhoneNumber.DefaultCountryCode(), regModel.Phone, null, null, rockContext);
                }


                // Check for a spouse. 
                if (string.IsNullOrWhiteSpace(regModel.SpouseName) == false)
                {
                    // this is super simple. get the person's spouse
                    Person tempSpouse = person.GetSpouse();

                    // if they have one...
                    if (tempSpouse != null)
                    {
                        // split out the first and last name provided
                        string[] spouseName = regModel.SpouseName.Split(' ');
                        string spouseFirstName = spouseName[0];
                        string spouseLastName = spouseName.Count() > 1 ? spouseName[1] : "";

                        // we'll take them as a spouse if they match the provided name
                        if (tempSpouse.FirstName.Equals(spouseFirstName, StringComparison.OrdinalIgnoreCase))
                        {
                            // if there was no last name, or it matches, we're good.
                            if (string.IsNullOrWhiteSpace(spouseLastName) == true ||
                                tempSpouse.LastName.Equals(spouseLastName, StringComparison.OrdinalIgnoreCase))
                            {
                                spouse = tempSpouse;
                            }
                        }
                    }
                }

                // Save all changes
                rockContext.SaveChanges();

                HistoryService.SaveChanges(rockContext, typeof(Person),
                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes);

                HistoryService.SaveChanges(rockContext, typeof(Person),
                    Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), person.Id, familyChanges);


                // now, it's time to either add them to the group, or kick off the Alert Re-Route workflow
                // (Or nothing if there's no problem but they're already in the group)
                GroupMember primaryGroupMember = PersonToGroupMember(rockContext, person, requestedGroup);

                GroupMember spouseGroupMember = null;
                if (spouse != null)
                {
                    spouseGroupMember = PersonToGroupMember(rockContext, spouse, requestedGroup);
                }

                // prep the workflow service
                var workflowTypeService = new WorkflowTypeService(rockContext);

                bool addToGroup = true;

                // First, check to see if an alert re-route workflow should be launched
                WorkflowType alertRerouteWorkflowType = workflowTypeService.Get(AlertNoteReReouteWorkflowId);

                // do either of the people registering have alert notes?
                int alertNoteCount = new NoteService(rockContext).Queryable().Where(n => n.EntityId == person.Id && n.IsAlert == true).Count();

                if (spouse != null)
                {
                    alertNoteCount += new NoteService(rockContext).Queryable().Where(n => n.EntityId == spouse.Id && n.IsAlert == true).Count();
                }

                if (alertNoteCount > 0)
                {
                    // yes they do. so first, flag that we should NOT put them in the group
                    addToGroup = false;

                    // and kick off the re-route workflow so security can review.
                    Util.LaunchWorkflow(rockContext, alertRerouteWorkflowType, primaryGroupMember);

                    if (spouseGroupMember != null)
                    {
                        Util.LaunchWorkflow(rockContext, alertRerouteWorkflowType, spouseGroupMember);
                    }
                }

                // if above, we didn't flag that they should not join the group, let's add them
                if (addToGroup == true)
                {
                    // try to add them to the group (would only fail if the're already in it)
                    TryAddGroupMemberToGroup(rockContext, primaryGroupMember, requestedGroup);

                    if (spouseGroupMember != null)
                    {
                        TryAddGroupMemberToGroup(rockContext, spouseGroupMember, requestedGroup);
                    }
                }

                // if we mae it here, all is good!
                success = true;
            }

            return success;
        }

        private static GroupMember PersonToGroupMember(RockContext rockContext, Person person, Group group)
        {
            // puts a person into a group member object, so that we can pass it to a workflow
            GroupMember newGroupMember = new GroupMember();
            newGroupMember.PersonId = person.Id;
            newGroupMember.GroupRoleId = group.GroupType.DefaultGroupRole.Id;
            newGroupMember.GroupMemberStatus = GroupMemberStatus.Pending;
            newGroupMember.GroupId = group.Id;

            return newGroupMember;
        }

        /// <summary>
        /// Adds the group member to the group if they aren't already in it
        /// </summary>
        private static void TryAddGroupMemberToGroup(RockContext rockContext, GroupMember newGroupMember, Group group)
        {
            if (!group.Members.Any(m =>
                                    m.PersonId == newGroupMember.PersonId &&
                                    m.GroupRoleId == group.GroupType.DefaultGroupRole.Id))
            {
                var groupMemberService = new GroupMemberService(rockContext);
                groupMemberService.Add(newGroupMember);

                rockContext.SaveChanges();
            }
        }
    }

    // Class containing functions related to launching the Mobile App
    public static class Launch
    {
        // the attribute Id for the Mobile App's version
        const int MobileAppVersionAttributeId = 29469;

        // Returns the startup data for the Mobile App
        public static LaunchData GetLaunchData()
        {
            RockContext rockContext = new RockContext();

            LaunchData launchData = new LaunchData();

            // setup the campuses
            launchData.Campuses = new List<Campus>();
            foreach (CampusCache campus in CampusCache.All(false))
            {
                Campus campusModel = new Campus();
                campusModel.Guid = campus.Guid;
                campusModel.Id = campus.Id;
                campusModel.Name = campus.Name;
                launchData.Campuses.Add(campusModel);
            }

            // setup the prayer categories
            launchData.PrayerCategories = new List<KeyValuePair<string, int>>();
            CategoryCache prayerCategories = CategoryCache.Read(1);
            foreach (CategoryCache category in prayerCategories.Categories)
            {
                launchData.PrayerCategories.Add(new KeyValuePair<string, int>(category.Name, category.Id));
            }

            // get the latest mobile app version
            var mobileAppAttribute = new AttributeValueService(rockContext).Queryable().Where(av => av.AttributeId == MobileAppVersionAttributeId).SingleOrDefault();
            if (mobileAppAttribute != null)
            {
                int.TryParse(mobileAppAttribute.Value, out launchData.MobileAppVersion);
            }

            return launchData;
        }
    }

    // Common reusable functions for supporting the Mobile App
    public static class Util
    {
        public static PersonData GetPersonData( string userID )
        {
            PersonData personData = new PersonData( );

            do
            {
                RockContext rockContext = new RockContext( );
                PersonService personService = new PersonService( rockContext );
                GroupService groupService = new GroupService( rockContext );
                UserLoginService userLoginService = new UserLoginService( rockContext );

                int? personId = userLoginService.Queryable()
                    .Where( u => u.UserName.Equals( userID ) )
                    .Select( a => a.PersonId )
                    .FirstOrDefault();

                if ( personId.HasValue == false ) break;

                // start by getting the person. if we can't do that, we should fail
                var person = personService.Queryable().Include( a => a.PhoneNumbers ).Include(a => a.Aliases )
                    .FirstOrDefault( p => p.Id == personId );

                // do a shallow copy of the person
                personData.Person = new Person( );
                personData.Person.CopyPropertiesFrom( person );
                if( personData.Person == null ) break;

                // take one deep object, the aliases, so we can provide the client a PrimaryAliasId
                personData.Person.Aliases = person.Aliases;
                
                var mobilePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( mobilePhoneType != null )
                {
                    personData.CellPhoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneType.Id );
                }
                
                // the rest is optional, as they may not have filled it out yet
                Group family = person.GetFamily( );
                
                // take a shallow copy of the family
                personData.Family = new Group( );
                personData.Family.CopyPropertiesFrom( family );

                // now try to get ther home address for this family
                Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                if ( homeAddressGuid.HasValue )
                {
                    var homeAddressDv = DefinedValueCache.Read( homeAddressGuid.Value );
                    if ( homeAddressDv != null )
                    {
                        // take the group location flagged as a home address and mapped
                        GroupLocation familyAddress = family.GroupLocations
                            .Where( l =>
                                l.GroupLocationTypeValueId == homeAddressDv.Id &&
                                l.IsMappedLocation )
                            .FirstOrDefault();

                        if ( familyAddress != null )
                        {
                            // take a shallow copy of the address, and then copy the location.
                            personData.FamilyAddress = new GroupLocation( );
                            personData.FamilyAddress.CopyPropertiesFrom( familyAddress );

                            personData.FamilyAddress.Location = familyAddress.Location;
                        }
                    }
                }
                
                // their age determines whether we use adult vs student actions. If they have no age, or are >= 18, they're an adult
                if ( person.Age.HasValue == false || person.Age >= 18 )
                {
                    DateTime? baptismDate;
                    personData.IsBaptised = Actions_Adult.Baptised.IsBaptised( person.Id, out baptismDate );
                    personData.IsERA = Actions_Adult.ERA.IsERA( person.Id );
                    personData.IsGiving = Actions_Adult.Give.IsGiving( person.Id );

                    DateTime? membershipDate;
                    personData.IsMember = Actions_Adult.Member.IsMember( person.Id, out membershipDate );

                    Actions_Adult.Mentored.Result mentoredResult;
                    Actions_Adult.Mentored.IsMentored( person.Id, out mentoredResult );
                    personData.IsMentored = mentoredResult.IsMentored( );

                    Actions_Adult.PeerLearning.Result peerLearningResult;
                    Actions_Adult.PeerLearning.IsPeerLearning( person.Id, out peerLearningResult );
                    personData.IsPeerLearning = peerLearningResult.IsPeerLearning( );

                    Actions_Adult.Serving.Result servingResult;
                    Actions_Adult.Serving.IsServing( person.Id, out servingResult );
                    personData.IsServing = servingResult.IsServing;

                    Actions_Adult.Teaching.Result teachingResult;
                    Actions_Adult.Teaching.IsTeaching( person.Id, out teachingResult );
                    personData.IsTeaching = teachingResult.IsTeaching( );

                    DateTime? startingPointDate;
                    personData.TakenStartingPoint = Actions_Adult.StartingPoint.TakenStartingPoint( person.Id, out startingPointDate );

                    List<int> storyIds;
                    personData.SharedStory = Actions_Adult.ShareStory.SharedStory( person.Id, out storyIds );
                }
                // get the students version
                else
                {
                    DateTime? baptismDate;
                    personData.IsBaptised = Actions_Student.Baptised.IsBaptised( person.Id, out baptismDate );
                    personData.IsERA = Actions_Student.ERA.IsERA( person.Id );
                    personData.IsGiving = Actions_Student.Give.IsGiving( person.Id );

                    DateTime? membershipDate;
                    personData.IsMember = Actions_Student.Member.IsMember( person.Id, out membershipDate );

                    Actions_Student.Mentored.Result mentoredResult;
                    Actions_Student.Mentored.IsMentored( person.Id, out mentoredResult );
                    personData.IsMentored = mentoredResult.IsMentored( );

                    Actions_Student.PeerLearning.Result peerLearningResult;
                    Actions_Student.PeerLearning.IsPeerLearning( person.Id, out peerLearningResult );
                    personData.IsPeerLearning = peerLearningResult.IsPeerLearning( );

                    Actions_Student.Serving.Result servingResult;
                    Actions_Student.Serving.IsServing( person.Id, out servingResult );
                    personData.IsServing = servingResult.IsServing;

                    Actions_Student.Teaching.Result teachingResult;
                    Actions_Student.Teaching.IsTeaching( person.Id, out teachingResult );
                    personData.IsTeaching = teachingResult.IsTeaching( );

                    DateTime? startingPointDate;
                    personData.TakenStartingPoint = Actions_Student.StartingPoint.TakenStartingPoint( person.Id, out startingPointDate );

                    List<int> storyIds;
                    personData.SharedStory = Actions_Student.ShareStory.SharedStory( person.Id, out storyIds );
                }

                return personData;
            }
            while( false );

            return null;
        }

        public static HttpStatusCode RegisterNewPerson( RegAccountData regAccountData )
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            do
            {
                RockContext rockContext = new RockContext( );
                PersonService personService = new PersonService( rockContext );
                GroupService groupService = new GroupService( rockContext );

                // get all required values and make sure they exist
                DefinedValueCache cellPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                DefinedValueCache connectionStatusWebProspect = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT);
                DefinedValueCache recordStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING);
                DefinedValueCache recordTypePerson = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON);
                
                if( cellPhoneType == null || connectionStatusWebProspect == null || recordStatusPending == null || recordTypePerson == null ) break;
                                
                // create a new person, which will give us a new Id
                Person person = new Person( );
                
                // for new people, copy the stuff sent by the Mobile App
                person.FirstName = regAccountData.FirstName.Trim();
                person.LastName = regAccountData.LastName.Trim();

                person.Email = regAccountData.Email.Trim();
                person.IsEmailActive = string.IsNullOrWhiteSpace( person.Email ) == false ? true : false;
                person.EmailPreference = EmailPreference.EmailAllowed;
                
                // now set values so it's a Person Record Type, and pending web prospect.
                person.ConnectionStatusValueId = connectionStatusWebProspect.Id;
                person.RecordStatusValueId = recordStatusPending.Id;
                person.RecordTypeValueId = recordTypePerson.Id;

                // now, save the person so that all the extra stuff (known relationship groups) gets created.
                Group newFamily = PersonService.SaveNewPerson( person, rockContext );
                

                //TODO: Only add this if Robin wants it, which i don't think she does.
                // since this is a new person, flag now as their first visit
                //AttributeService attributeService = new AttributeService( rockContext );
                //Rock.Model.Attribute firstVisitAttrib = attributeService.Get( Guids.FIRST_TIME_VISIT_DEFINED_TYPE.AsGuid( ) );
                //person.SetAttributeValue( firstVisitAttrib.Key, RockDateTime.Now.ToString( ) );

                // set the phone number (we only support Cell Phone for the Mobile App)
                if( string.IsNullOrWhiteSpace( regAccountData.CellPhoneNumber ) == false )
                {
                    person.UpdatePhoneNumber( cellPhoneType.Id, PhoneNumber.DefaultCountryCode(), regAccountData.CellPhoneNumber, null, null, rockContext );
                }

                // save all changes
                person.SaveAttributeValues( rockContext );
                rockContext.SaveChanges( );
                
                
                // and now create the login for this person
                try
                {
                    UserLogin login = UserLoginService.Create(
                                    rockContext,
                                    person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    regAccountData.Username,
                                    regAccountData.Password,
                                    true,
                                    false );
                }
                catch
                {
                    // fail on exception
                    break;
                }
                
                // report that we created the person / login
                statusCode = HttpStatusCode.Created;
            }
            while( false );
            
            return statusCode;
        }

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
    }
}
