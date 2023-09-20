using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    [ActionCategory( "LPC Check-In" )]
    [Description( "Verifies that the person performing the check-out action is allowed to checkout the selected person.." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Checkout Pre-Check" )]
    public class CheckoutPreCheck : CheckInActionComponent
    {
        private const string CHECKOUT_RESTRICTION_PERSON_ATTRIBUTE = "CheckOutRestrictions";
        private const string CHECKOUT_RESTRICTION_MESSAGE_KEY = "MessageToDisplay";

        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            var definedValueService = new DefinedValueService( rockContext );

            if ( checkInState != null )
            {
                bool cancelCheckout = false;
                var family = checkInState.CheckIn.CurrentFamily;

                foreach ( var person in family.CheckOutPeople.Where( p => p.Selected ) )
                {
                    person.Person.LoadAttributes();

                    var checkoutRestrictionGuid = person.Person.GetAttributeValue( CHECKOUT_RESTRICTION_PERSON_ATTRIBUTE ).AsGuidOrNull();


                    if ( !checkoutRestrictionGuid.HasValue )
                    {
                        continue;
                    }

                    var checkoutRestriction = DefinedValueCache.Get( checkoutRestrictionGuid.Value );
					
					if( checkoutRestriction == null)
					{
						continue;
					}
					
                    if ( checkoutRestriction.Guid == SystemGuid.DefinedValue.CHECKOUT_RESTRICTIONS_ASSISTED_CHECKOUT_ONLY.AsGuid() )
                    {
                        errorMessages.Add( string.Format( "Checkout Canceled - {0}", checkoutRestriction.GetAttributeValue( CHECKOUT_RESTRICTION_MESSAGE_KEY ) ) );
                        cancelCheckout = true;
                        break;
                    }
                    else if (checkoutRestriction.Guid == SystemGuid.DefinedValue.CHECKOUT_RESTRICTIONS_CHECKIN_PERSON_ONLY.AsGuid())
                    {
                        if ( checkInState.CheckIn.SearchType.Guid != Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID.AsGuid() )
                        {
                            errorMessages.Add( string.Format( "Check-out has been restricted.<br /> Please try again with key fob/card or proceed to an assisted Check-In Station." ) );
                            cancelCheckout = true;
                            break;
                        }
                        else
                        {
                            var personAliasService = new PersonAliasService( rockContext );
                            //var checkedInByPersonIds = new AttendanceService( rockContext ).Queryable().AsNoTracking()
                            //    .Where( a => person.AttendanceIds.Contains( a.Id ) )
                            //    .Join( personAliasService.Queryable( ).AsNoTracking(),
                            //     a => a.CheckedInByPersonAliasId, pa => pa.Id, ( a, checkedInBy ) => checkedInBy.PersonId )
                            //     .Distinct()
                            //     .ToList();
                            var personCheckedInByPersonAliasIds = new AttendanceService( rockContext ).Queryable().AsNoTracking()
                                .Where( a => person.AttendanceIds.Contains( a.Id ) )
                                .Where( a => a.CheckedInByPersonAliasId != null )
                                .Select( a => a.CheckedInByPersonAliasId )
                                .ToList();

                            List<int> checkedInByPersonIds = new List<int>();

                            if ( personCheckedInByPersonAliasIds.Count > 0 )
                            {
                                checkedInByPersonIds.AddRange( personAliasService.Queryable().AsNoTracking()
                                    .Where( pa => personCheckedInByPersonAliasIds.Contains( pa.Id ) )
                                    .Select( pa => pa.PersonId )
                                    .Distinct()
                                    .ToList() );
                            }

                            if ( checkInState.CheckIn.CheckedInByPersonAliasId.HasValue )
                            {
                                var currentPersonId = personAliasService.Get( checkInState.CheckIn.CheckedInByPersonAliasId.Value ).PersonId;

                                if ( !checkedInByPersonIds.Contains( currentPersonId ) )
                                {
                                    errorMessages.Add( string.Format( "Checkout Canceled - {0}", checkoutRestriction.GetAttributeValue( CHECKOUT_RESTRICTION_MESSAGE_KEY ) ) );
                                    cancelCheckout = true;
                                }
                                 
                            }
                        }
                    }
                }

                return !cancelCheckout;

            }
            return false;
        }


    }
}
