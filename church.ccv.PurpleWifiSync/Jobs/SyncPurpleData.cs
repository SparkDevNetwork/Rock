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
using Quartz;
using Rock.Data;
using System.Net;
using System.Collections.Generic;
using Rock.Attribute;
using Rock;
using System;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;
using church.ccv.CCVPurpleWifiSync.Model;
using System.Text.RegularExpressions;

namespace church.ccv.CCVPurpleWifiSync
{
    [TextField("Public Key", "Your company's Purple Wifi Public Key.", true, "", order: 0)]
    [TextField("Private Key", "Your company's Purple Wifi Private Key.", true, "", order: 1)]
    [TextField("Host", "The host for your Purple Portal.", true, "", order: 2)]
    [TextField("Attendance Group Id", "The group Id where attendance records should be added.", true, "", order: 3)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status that should be set by default", false, false, 
                        Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR, "", 4 )]
    [DisallowConcurrentExecution]
    public class SyncPurpleData : IJob
    {
        int AttendanceGroupId { get; set; }

        // Defines the name of the key used to store the last time this job ran, so that
        // we only request people that have visited since the last run.
        const string LastUpdatePropertyKey = "church_ccv_CCVPurpleWifi_LastUpdate";

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SyncPurpleData()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            try
            {
                // get values needed to run
                JobDataMap dataMap = context.JobDetail.JobDataMap;

                // get the last day this job was processed.
                // Note - we only track with DAY precision (no time) so that we filter out people that may connect / disconnect / connect multiple times a day.
                DateTime? lastProcessedDate = Rock.Web.SystemSettings.GetValue( LastUpdatePropertyKey ).AsDateTime();

                string publicKey = dataMap.GetString( "PublicKey" );
                string privateKey = dataMap.GetString( "PrivateKey" );
                string host = dataMap.GetString( "Host" );
                Guid connectionStatusGuid = dataMap.GetString( "DefaultConnectionStatus" ).AsGuid( );

                AttendanceGroupId = dataMap.GetInt( "AttendanceGroupId" );

                PurpleWifi.API.Init( publicKey, privateKey, host );

                // first get a list of our venues
                List<PurpleWifi.Models.Venue> venueList = null;
                PurpleWifi.API.GetVenues( 
                    delegate( HttpStatusCode statusCode, string desc, List<PurpleWifi.Models.Venue> venues )
                    {
                        if( StatusInSuccessRange( statusCode ) )
                        {
                            venueList = venues;
                        }
                    });

                // now organize visitors per-venue
                Dictionary<int, List<PurpleWifi.Models.Visitor>> venueWithVisitorsDict = new Dictionary<int, List<PurpleWifi.Models.Visitor>>( );
                foreach( PurpleWifi.Models.Venue venue in venueList )
                {
                    // try to get the campus (it's possible they went to a venue we don't consider a campus)
                    int? campusId = VenueToCampus( venue );

                    // get all visitors between the start of day and now (when storing attendance, we'll ignore the time portion)
                    PurpleWifi.API.GetVisitors( int.Parse( venue.Id ), lastProcessedDate, RockDateTime.Now, 
                        delegate( HttpStatusCode statusCode, string desc, List<PurpleWifi.Models.Visitor> venueVisitors)
                        {
                            if( StatusInSuccessRange( statusCode ) )
                            {
                                // we got a response, so add the campus Id (or -1 if we couldn't find it) and visitor list to our dictionary
                                int campusIdVal = campusId.HasValue ? campusId.Value : -1;
                                if( venueWithVisitorsDict.ContainsKey( campusIdVal ) == false )
                                {
                                    venueWithVisitorsDict.Add( campusIdVal, venueVisitors );
                                }
                                else
                                {
                                    // if the campus is already in our dictionary (would happen if we get multiple -1 IDs), append the new visitors to it.
                                    var visitorsForVenu = venueWithVisitorsDict[campusIdVal];
                                    visitorsForVenu.AddRange( venueVisitors );
                                }
                            }
                        });
                }
                
                // get all required values
                DefinedValueCache connectionStatus = DefinedValueCache.Read(connectionStatusGuid);
                DefinedValueCache recordStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING);
                DefinedValueCache recordTypePerson = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON);
                    
                int numNewPeople = 0;
                int numAttendanceRecords = 0;
                int numPeopleSkipped = 0;

                // go thru each venue, and all visitors for it
                foreach ( KeyValuePair<int, List<PurpleWifi.Models.Visitor>> venueVisitors in venueWithVisitorsDict )
                {
                    foreach( PurpleWifi.Models.Visitor visitor in venueVisitors.Value )
                    {
                        using ( RockContext rockContext = new RockContext() )
                        {
                            // Now, process each person
                            PersonService personService = new PersonService( rockContext );
                            AttendanceService attendanceService = new AttendanceService( rockContext );
                            PersonAliasService personAliasService = new PersonAliasService( rockContext );
                            Service<PurpleUser> purpleUserService = new Service<PurpleUser>( rockContext );

                            // make sure the person has valid first/last/email, or we can't process them
                            if ( string.IsNullOrWhiteSpace( visitor.First_Name ) == false &&
                                 string.IsNullOrWhiteSpace( visitor.Last_Name ) == false &&
                                 string.IsNullOrWhiteSpace( visitor.Email ) == false )
                            {
                                int personId = 0;

                                // conver the Key, which will be -1 for unknown campus, or a campusId, into null or campusId.
                                int? campusId = venueVisitors.Key;
                                campusId = campusId == -1 ? null : campusId;

                                // first make sure the person exists, adding them if they don't.
                                if ( TryAddPerson( visitor, campusId, connectionStatus.Id, recordStatusPending.Id, recordTypePerson.Id, personService, personAliasService, purpleUserService, rockContext, out personId ) == true )
                                {
                                    numNewPeople++;
                                }

                                SyncFormData(visitor, personId);

                                // now update their attendance (note that we truncate to DATE with no time)
                                if ( CreateAttendanceRecord( personId, campusId, DateTime.Parse( visitor.Last_Seen ), attendanceService, personAliasService, rockContext ) == true )
                                {
                                    numAttendanceRecords++;
                                }
                            }
                            else
                            {
                                numPeopleSkipped++;
                            }

                            rockContext.SaveChanges( );
                        }
                    }
                }

                // now store the date (not time) we last ran, which will be the starting date next time.
                // Next time this runs, there will be some overlap, since we're technically getting people for this day twice,
                // but our attendance system will only store one record per date.
                lastProcessedDate = RockDateTime.Now.Date;

               Rock.Web.SystemSettings.SetValue( LastUpdatePropertyKey, lastProcessedDate.Value.ToString( ) );
               context.Result = string.Format( "Sync'd Purple Data. {0} people added. {1} attendance records added. {2} people skipped due to blank name fields.", numNewPeople, numAttendanceRecords, numPeopleSkipped);
            }
            catch( Exception e )
            {
                context.Result = e.Message;
            }
        }
        
        bool TryAddPerson( PurpleWifi.Models.Visitor visitor, 
                            int? campusId,
                            int connectionStatusId, 
                            int recordStatusPendingId, 
                            int recordTypePersonId, 
                            PersonService personService, 
                            PersonAliasService personAliasService,
                            Service<PurpleUser> purpleUserService,
                            RockContext rockContext,
                            out int personId )
        {

            bool createdNewPerson = false;

            // Before creating someone new, see if they already exist in the database.

            // Our first attempt will be finding them by a Purple Wifi Id in our lookup table. (Purple User ID -> Person Alias ID)
            Person person = GetPersonByPurpleWifiId( visitor.Id, personAliasService, purpleUserService );
            if( person == null )
            {
                // we couldn't, so see if they exist by a matching first, last, and email
                IEnumerable<Person> personList = personService.GetByMatch( visitor.First_Name, visitor.Last_Name, visitor.Email );
            
                // they don't, so add them
                if ( personList.Count( ) == 0 )
                {
                    // wrap the transaction since SaveNewPerson invokes multiple context saves
                    rockContext.WrapTransaction( ( ) =>
                    {
                        person = new Person( );

                        person.FirstName = visitor.First_Name.Trim( );
                        person.LastName = visitor.Last_Name.Trim( );

                        person.Email = visitor.Email.Trim( );
                        person.IsEmailActive = string.IsNullOrWhiteSpace( visitor.Email ) == false ? true : false;
                        person.EmailPreference = EmailPreference.EmailAllowed;

                        // now set values so it's a Person Record Type, and pending whatever status is set.
                        person.ConnectionStatusValueId = connectionStatusId;
                        person.RecordStatusValueId = recordStatusPendingId;
                        person.RecordTypeValueId = recordTypePersonId;

                        person.SystemNote = "Added by PurpleWifi";

                        // now, save the person so that all the extra stuff (known relationship groups) gets created.
                        Rock.Model.Group newFamily = PersonService.SaveNewPerson( person, rockContext, campusId );

                        createdNewPerson = true;
                    } );
                }
                else
                {
                    person = personList.First( );
                }

                // whether they exist in Rock or not, this was the first time we've encountered them,
                // so put them in our lookup table.
                PurpleUser purpleUser = new PurpleUser( );
                purpleUser.PurpleId = int.Parse( visitor.Id );
                purpleUser.PersonAliasId = person.PrimaryAliasId.Value;
                purpleUserService.Add( purpleUser );
            }
            
            // return the ID of the person we created / found, and whether we did create a new person
            personId = person.Id;
            return createdNewPerson;
        }
        void SyncFormData (PurpleWifi.Models.Visitor visitor, int personId)
        {
            try
            {
                // Loop through Form Data Questions that visitor has answered
                for (int i = 0; i < visitor.Form_Data.Count; i++ )
                {
                    // If visitor has answered form questions in PurpleWifi...
                    if (string.IsNullOrWhiteSpace(visitor.Form_Data[i].Response) == false)
                    {
                        // Create new rockContext that will be saved after every question that is copied from PurpleWifi into Rock
                        RockContext rockContext = new RockContext();

                        // Sanitize Key (Label/Question)
                        string fixedKey = Regex.Replace(visitor.Form_Data[i].Label, @"\s", "_").ToLower();
                        fixedKey = Regex.Replace(fixedKey, @"[^0-9a-zA-Z_]+", "");

                        // Load Attribute
                        AttributeService attribService = new AttributeService(rockContext);
                        Rock.Model.Attribute attribItem = attribService.Queryable().Where(ai => ai.Key == fixedKey).SingleOrDefault();

                        // Now load the Attribute Value, if it exists
                        AttributeValueService avService = new AttributeValueService(rockContext);
                        AttributeValue avItem = avService.Queryable().Where(av => av.EntityId == personId && av.AttributeId == attribItem.Id).SingleOrDefault();

                        // if Attribute Value doesn't exist, we'll create a new attribute value (which is tied to the Person ID)
                        if (avItem == null)
                        {
                            // Now create a new attribute value tied to the Person
                            avItem = new AttributeValue();
                            avItem.EntityId = personId;
                            avItem.AttributeId = attribItem.Id;
                            avService.Add(avItem);
                        }

                        // If Visitor has answered form questions, but the answers are still not in rock...
                        if (string.IsNullOrWhiteSpace(avItem.Value) == true)
                        {
                            // Copy answer from Purple Wifi into Rock 
                            avItem.Value = visitor.Form_Data[i].Response;
                        }
                        // Save changes to Rock
                        rockContext.SaveChanges();
                    }
                }
            }
            catch { } 
        }

        bool CreateAttendanceRecord( int personId, int? campusId, DateTime startDateTime, AttendanceService attendanceService, PersonAliasService personAliasService, RockContext rockContext )
        {
            // if we already have an attendance record for this start time, don't count it again.
            Attendance attendance = attendanceService.Get( startDateTime.Date, null, null, AttendanceGroupId, personId );
            if( attendance == null )
            {
                PersonAlias primaryAlias = personAliasService.GetPrimaryAlias( personId );
                if ( primaryAlias != null )
                {
                    attendance = rockContext.Attendances.Create( );
                    attendance.CampusId = campusId;
                    attendance.GroupId = AttendanceGroupId;
                    attendance.PersonAlias = primaryAlias;
                    attendance.PersonAliasId = primaryAlias.Id;
                    attendance.StartDateTime = startDateTime;
                    attendance.DidAttend = true;
                    attendanceService.Add( attendance );

                    return true;
                }
            }

            return false;
        }

        public Person GetPersonByPurpleWifiId( string purpleWifiId, PersonAliasService personAliasService, Service<PurpleUser> purpleUserService )
        {
            // find this purple user ID in the lookup table. If it exists, we've processed this person,
            // and the corresponding Person Alias Id will point to their record, whether a merged one or the original.
            int purpleWifiUserId = int.Parse( purpleWifiId );
            PurpleUser purpleUser = purpleUserService.Queryable( ).Where( pu => pu.PurpleId == purpleWifiUserId ).SingleOrDefault( );
            if( purpleUser != null )
            {
                // grab the person and return them
                PersonAlias personAlias = personAliasService.Get( purpleUser.PersonAliasId );
                if( personAlias != null )
                {
                    return personAlias.Person;
                }
            }

            return null;
        }

        int? VenueToCampus( PurpleWifi.Models.Venue venue )
        {
            List<CampusCache> campuses = CampusCache.All( false );
            CampusCache campus = campuses.Where( c => c.Location.Street1.ToLower( ) == venue.Address1.ToLower( ) ).SingleOrDefault( );

            if( campus != null )
            {
                return campus.Id;
            }
            else
            {
                return null;
            }
        }
        
        bool StatusInSuccessRange( HttpStatusCode statusCode )
        {
            if( (int)statusCode >= 200 && (int)statusCode < 300 )
            {
                return true;
            }
            return false;
        }
    }
}
