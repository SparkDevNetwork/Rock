using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.Utility
{
    [TextField( "Message", "The message to be sent via SMS.", true, "See what {{ Kids }} learned today in service: http://ccv.church/parent-kid-resources" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "From Number", "The phone number to send message from.", true, false, "", "", 0 )]
    [CustomRadioListField( "Mode", "The mode this job operates in.", "Production, Staff Only", true, "Staff Only" )]

    [DisallowConcurrentExecution]
    class ParentResourceNotifications : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ParentResourceNotifications()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? fromGuid = dataMap.GetString( "FromNumber" ).AsGuidOrNull();
            string message = dataMap.GetString( "Message" ).ToString();
            string mode = dataMap.GetString( "Mode" ).ToString();

            int messagesSent = 0;

            // get from number id
            int? fromId = null;
            if ( fromGuid.HasValue )
            {
                var fromValue = DefinedValueCache.Read( fromGuid.Value, rockContext );
                if ( fromValue != null )
                {
                    fromId = fromValue.Id;
                }
            }

            if ( fromId != null && message != null )
            {
                // make sure the job only triggers on Saturday or Sunday to prevent accidents
                if ( RockDateTime.Now.DayOfWeek == DayOfWeek.Saturday || RockDateTime.Now.DayOfWeek == DayOfWeek.Sunday )
                {
                    // get family group type id
                    Guid familyGroupTypeGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                    var familyGroupTypeId = new GroupTypeService( rockContext ).Get( familyGroupTypeGuid ).Id;

                    // get adult family role id
                    Guid adultFamilyRole = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
                    var adultFamilyRoleId = new GroupTypeRoleService( rockContext ).Get( adultFamilyRole ).Id;

                    Guid cellNumberTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                    var cellNumberTypeId = new DefinedValueService( rockContext ).Get( cellNumberTypeGuid ).Id;

                    var attendService = new AttendanceService( rockContext );
                    var personService = new PersonService( rockContext );
                    var gmService = new GroupMemberService( rockContext );

                    var familyMembers = gmService.Queryable().Where( g => g.Group.GroupTypeId == familyGroupTypeId );

                    // get kids who checked into Pre - 6th Grade today
                    var attendance = attendService.Queryable()
                                            .Where( a => ( a.Group.GroupTypeId == 20 || a.Group.GroupTypeId == 46 ||
                                                           a.Group.GroupTypeId == 35 || a.Group.GroupTypeId == 36 ) &&
                                                    a.DidAttend == true &&
                                                    a.StartDateTime.Year == RockDateTime.Now.Year &&
                                                    a.StartDateTime.Month == RockDateTime.Now.Month &&
                                                    a.StartDateTime.Day == RockDateTime.Now.Day )
                                            .GroupBy( a => a.PersonAliasId )
                                            .Select( grp => new
                                            {
                                                kidName = grp.FirstOrDefault().PersonAlias.Person.NickName,
                                                FamilyId = familyMembers.Where( f => f.PersonId == grp.FirstOrDefault().PersonAlias.PersonId )
                                                                         .Select( f => f.GroupId ).FirstOrDefault()
                                            } );

                    // get the parents with their cell number 
                    var parents = gmService.Queryable()
                                    .Where( g => attendance.Select( k => k.FamilyId ).Contains( g.Group.Id ) &&
                                            g.GroupRole.Id == adultFamilyRoleId )
                                    .Where( g => g.Person.PhoneNumbers
                                                    .Where( p => p.NumberTypeValueId == cellNumberTypeId && 
                                                                 p.IsMessagingEnabled == true )
                                                    .Any() )
                                    .Select( g => new
                                    {
                                        NickName = g.Person.NickName,
                                        LastName = g.Person.LastName,
                                        PersonId = g.PersonId,
                                        Number = g.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == cellNumberTypeId && 
                                                                              p.IsMessagingEnabled == true)
                                                                      .Select( p => p.Number )
                                                                      .FirstOrDefault(),
                                        Kids = attendance.Where( k => k.FamilyId == g.GroupId ).Select( k => k.kidName ).ToList()
                                    } ).ToList();

                    // Filter the list by staff when in staff mode.
                    if ( mode == "Staff Only" )
                    {
                        var staff = gmService.Queryable()
                                         .Where( s => s.Group.GroupTypeId == 52 && s.GroupMemberStatus == GroupMemberStatus.Active )
                                         .Select( s => s.PersonId );

                        parents = parents.Where( p => staff.Contains( p.PersonId ) ).ToList();
                    }

                    // send each parent a personalized text message
                    foreach ( var parent in parents )
                    {
                        var mergeFields = new Dictionary<string, object>();

                        // format the kids names so that they are humanized i.e.. "Amanda and Bob" or "Amanda, Jake and Bob"
                        string kids;
                        if ( parent.Kids.Count() > 1 )
                        {
                            kids = String.Join( ", ", parent.Kids.ToArray(), 0, parent.Kids.Count - 1 ) + " and " + parent.Kids.LastOrDefault();
                        }
                        else
                        {
                            kids = parent.Kids.FirstOrDefault();
                        }

                        mergeFields.Add( "Kids", kids );

                        // send SMS messages to each parent
                        var recipient = new RecipientData();
                        var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid(), rockContext );

                        if ( mediumEntity != null )
                        {
                            var medium = MediumContainer.GetComponent( mediumEntity.Name );
                            if ( medium != null && medium.IsActive )
                            {
                                var transport = medium.Transport;
                                if ( transport != null && transport.IsActive )
                                {
                                    var appRoot = GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );
                                    var mediumData = new Dictionary<string, string>();
                                    mediumData.Add( "FromValue", fromId.Value.ToString() );
                                    mediumData.Add( "Message", message.ResolveMergeFields( mergeFields ) );

                                    var numbers = new List<string> { parent.Number };

                                    transport.Send( mediumData, numbers, appRoot, string.Empty );
                                }
                            }
                        }

                        messagesSent++;
                    }
                }                
            }

            context.Result = string.Format( "{0} parents notified.", messagesSent );
        }
    }
}
