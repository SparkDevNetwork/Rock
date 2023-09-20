using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    [ActionCategory("LPC Check-In")]
    [Description("Filters out family members who are eligible for check-out who have been checked in for less than the check-out holding period.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Filter Checkout Holding Period")]
    public class FilterCheckoutHoldingPeriod : CheckInActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                DateTime checkoutHoldExpiration = GetCheckOutHoldExpiration( checkInState.CheckinTypeId );
                var attendanceService = new AttendanceService( rockContext );
                foreach ( var family in checkInState.CheckIn.GetFamilies(true) )
                {
                    if ( family.CheckOutPeople.Any() )
                    {
                        List<int> peopleIdsToRemove = new List<int>();
                        foreach ( var person in family.CheckOutPeople )
                        {
                            int holdCount = attendanceService.Queryable().AsNoTracking()
                                .Where( a => person.AttendanceIds.Contains( a.Id ) )
                                .Where( a => a.StartDateTime > checkoutHoldExpiration ).Count();

                            if(holdCount > 0)
                            {
                                peopleIdsToRemove.Add( person.Person.Id );
                            }
                        }

                        family.CheckOutPeople.RemoveAll( c => peopleIdsToRemove.Contains( c.Person.Id ) );
                    }
                }

                return true;
            }

            return false;
        }

        private DateTime GetCheckOutHoldExpiration( int? checkInTypeId )
        {
            DateTime checkInBefore = DateTime.Now;

            if ( checkInTypeId.HasValue )
            {
                GroupTypeCache checkInType = GroupTypeCache.Get( checkInTypeId.Value );

                int? checkoutDelay = checkInType.GetAttributeValue( "CheckOutDelay" ).AsIntegerOrNull();

                if ( checkoutDelay.HasValue && checkoutDelay > 0 )
                {
                    checkInBefore = checkInBefore.AddMinutes( 0 - checkoutDelay.Value );
                }
            }

            return checkInBefore;
        }
    }
}
