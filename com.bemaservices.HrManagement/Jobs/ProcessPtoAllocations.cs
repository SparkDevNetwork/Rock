using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using com.bemaservices.HrManagement.Model;
using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.HrManagement.Jobs
{

    [DisallowConcurrentExecution]

    [AttributeField( Rock.SystemGuid.EntityType.PERSON
        , "Person Hired Date Attribute"
        , "The Person Attribute that contains the Person's Hired Date.  This will be used to determine if the person is currently staff or not."
        , true
        , false
        , ""
        , ""
        , 0
        , "HireDate" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON
        , "Person Fired Date Attribute"
        , "The Person Attribute that contains the Person's Fired Date.  This will be used to determine if the person is currently staff or not."
        , true
        , false
        , ""
        , ""
        , 0
        , "FireDate" )]

    [EnumField( "New Allocations Status"
        , "What should newly created allocations have as a status"
        , typeof( PtoAllocationStatus )
        , Order = 2
        , DefaultValue = "2"
        , IsRequired = true )]
    [EnumField( "New Allocation Accrual Schedule"
        , "What should newly created allocations accrual schdule be"
        , typeof( PtoAccrualSchedule )
        , Order = 3
        , DefaultValue = "1"
        , IsRequired = true )]
    [CustomCheckboxListField( "Pto Types"
        , "The Pto Types to Create Allocations For"
        , "Select [Guid] as [Value], [Name] as [Text] From _com_bemaservices_HrManagement_PtoType t Where t.IsActive = 1"
        , true
        , ""
        , ""
        , 4
        , "PtoTypes" )]
    [IntegerField( "Days Back", "The Number of Days prior to the Fiscal Start Date to create new Allocations", true, 30, "", 5, "DaysBack" )]
    [BooleanField( "Inactivate Past Allocations", "Whether the job should set a status of 'Inactive' on allocations whose End Date has past", true, "", 6, "InactivatePastAllocations" )]
    public class ProcessPtoAllocations : IJob
    {
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );
            PtoTypeService ptoTypeService = new PtoTypeService( rockContext );
            PtoAllocationService ptoAllocationService = new PtoAllocationService( rockContext );
            PtoTierService ptoTierService = new PtoTierService( rockContext );

            var defaultStatusValue = dataMap.GetString( "NewAllocationsStatus" ).AsIntegerOrNull();
            var defaultScheduleValue = dataMap.GetString( "NewAllocationAccrualSchedule" ).AsIntegerOrNull();
            var arePastAllocationsInactivated = dataMap.GetString( "InactivatePastAllocations" ).AsBoolean();

            if ( arePastAllocationsInactivated )
            {
                var pastAllocations = ptoAllocationService.Queryable().Where( a => a.EndDate.HasValue && a.EndDate.Value <= RockDateTime.Now && a.PtoAllocationStatus != PtoAllocationStatus.Inactive );
                foreach ( var pastAllocation in pastAllocations )
                {
                    pastAllocation.PtoAllocationStatus = PtoAllocationStatus.Inactive;
                }
                rockContext.SaveChanges();
            }

            PtoAllocationStatus defaultStatus = PtoAllocationStatus.Pending;
            PtoAccrualSchedule defaultSchedule = PtoAccrualSchedule.None;

            if ( defaultStatusValue.HasValue && Enum.IsDefined( typeof( PtoAllocationStatus ), defaultStatusValue.Value ) )
            {
                defaultStatus = ( PtoAllocationStatus ) defaultStatusValue.Value;
            }

            if ( defaultScheduleValue.HasValue && Enum.IsDefined( typeof( PtoAccrualSchedule ), defaultScheduleValue.Value ) )
            {
                defaultSchedule = ( PtoAccrualSchedule ) defaultScheduleValue.Value;
            }

            var daysOffset = dataMap.GetString( "DaysBack" ).AsIntegerOrNull();

            DateTime todayOffset = RockDateTime.Now;
            if ( daysOffset.HasValue )
            {
                todayOffset = todayOffset.AddDays( daysOffset.Value );
            }

            var hireDateAttributeGuid = dataMap.GetString( "HireDate" ).AsGuidOrNull();
            var fireDateAttributeGuid = dataMap.GetString( "FireDate" ).AsGuidOrNull();

            if ( !hireDateAttributeGuid.HasValue || !fireDateAttributeGuid.HasValue )
            {
                context.UpdateLastStatusMessage( "Hire date or Fire Date was not provided" );
            }

            var ptoTypeGuids = dataMap.GetString( "PtoTypes" ).Split( ',' ).AsGuidList();

            if ( ptoTypeGuids.Count < 1 )
            {
                context.UpdateLastStatusMessage( "No Pto Types were selected." );
            }

            //Get Active Employees
            var staff = personService.Queryable()
                                .AsNoTracking()
                                .WhereAttributeValue( rockContext, x => x.Attribute.Guid == hireDateAttributeGuid.Value && x.ValueAsDateTime <= todayOffset )
                                .ToList();

            foreach ( var person in staff )
            {

                // Get the person's Pto Tier
                person.LoadAttributes();

                var ptoTierGuid = person.GetAttributeValue( AttributeCache.Get( SystemGuid.Attribute.PTO_TIER_PERSON_ATTRIBUTE ).Key ).AsGuidOrNull();

                var hiredDate = person.GetAttributeValue( AttributeCache.Get( hireDateAttributeGuid.Value.ToString() ).Key ).AsDateTime();
                var firedDate = person.GetAttributeValue( AttributeCache.Get( fireDateAttributeGuid.Value.ToString() ).Key ).AsDateTime();
                int yearsWorked = 0;

                if ( hiredDate.HasValue && !firedDate.HasValue )
                {
                    yearsWorked = todayOffset.Year - hiredDate.Value.Year;

                    if ( ptoTierGuid.HasValue )
                    {
                        var ptoTier = ptoTierService.Get( ptoTierGuid.Value );
                        var brackets = ptoTier.PtoBrackets.Where( b => b.MinimumYear <= yearsWorked && ( !b.MaximumYear.HasValue || b.MaximumYear.Value > yearsWorked ) ).ToList();

                        foreach ( var bracket in brackets )
                        {
                            foreach ( var bracketType in bracket.PtoBracketTypes )
                            {
                                if ( ptoTypeGuids.Contains( bracketType.PtoType.Guid ) )
                                {
                                    //Get the currect fiscal start date to set the allocations dates.
                                    var fiscalStartDateValue = GlobalAttributesCache.Value( AttributeCache.Get( SystemGuid.Attribute.FISCAL_YEAR_START_DATE_ATTRIBUTE ).Key );
                                    var fiscalStartDate = DateTime.Parse( fiscalStartDateValue );

                                    var fiscalStartMonth = fiscalStartDate.Month;
                                    var fiscalStartDay = fiscalStartDate.Day;
                                    var fiscalStartYear = todayOffset.AddMonths( ( fiscalStartMonth - 1 ) * -1 ).AddDays( ( fiscalStartDay - 1 ) * -1 ).Year;
                                    var calculatedFiscalStartDate = new DateTime( fiscalStartYear, fiscalStartMonth, fiscalStartDay );
                                    var calculatedFiscalEndDate = calculatedFiscalStartDate.AddYears( 1 ).AddDays( -1 );

                                    var personAliasIds = person.Aliases.Select( a => a.Id ).ToList();

                                    //Check for an exsisting Pto Allocation.  If one doesn't exsist, create a new one.
                                    var ptoAllocation = ptoAllocationService.Queryable()
                                                                            .Where( a => personAliasIds.Contains( a.PersonAliasId )
                                                                                        && a.PtoTypeId == bracketType.PtoTypeId
                                                                                        && a.StartDate == calculatedFiscalStartDate
                                                                                        && a.EndDate == calculatedFiscalEndDate )
                                                                            .FirstOrDefault();
                                    if ( ptoAllocation.IsNull() )
                                    {
                                        ptoAllocation = new PtoAllocation();
                                        ptoAllocation.PtoAllocationSourceType = PtoAllocationSourceType.Automatic;
                                        ptoAllocation.PersonAliasId = person.PrimaryAliasId.Value;
                                        ptoAllocation.StartDate = calculatedFiscalStartDate;
                                        ptoAllocation.EndDate = calculatedFiscalStartDate.AddYears( 1 ).AddDays( -1 );
                                        ptoAllocation.PtoTypeId = bracketType.PtoTypeId;
                                        ptoAllocation.PtoAllocationStatus = defaultStatus;
                                        ptoAllocation.PtoAccrualSchedule = defaultSchedule;
                                        ptoAllocation.CreatedDateTime = RockDateTime.Now;
                                        ptoAllocation.ModifiedDateTime = RockDateTime.Now;
                                        ptoAllocation.Hours = bracketType.DefaultHours;

                                        ptoAllocationService.Add( ptoAllocation );

                                    }

                                }
                            }
                        }
                    }
                }
            }
            var recordsAdded = rockContext.SaveChanges();
            context.Result = string.Format( "{0} Allocations have been added", recordsAdded );
        }
    }
}