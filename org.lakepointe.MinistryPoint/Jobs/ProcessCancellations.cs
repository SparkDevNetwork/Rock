using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;


namespace org.lakepointe.MinistryPoint.Jobs
{
    [RegistrationInstanceField( "Ministry Point Registration Instance",
        Description = "Registration Instance for Ministry Point",
        IsRequired = true,
        Order = 0,
        Key = "MinistryPointInstance" )]


    [DisplayName( "Process Ministry Point Cancellations" )]
    [Description( "Processes Minsitry Point Cancellations" )]
    [DisallowConcurrentExecution]
    public class ProcessCancellations : IJob
    {
        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;

            var registrationInstanceGuid = dataMap.GetString( "MinistryPointInstance" ).AsGuid();

            using ( var rockContext = new RockContext() )
            {
                var registrationInstance = new RegistrationInstanceService( rockContext ).GetInclude( registrationInstanceGuid, r => r.RegistrationTemplate );
                var registrationEntityType = EntityTypeCache.Get( typeof( Registration ) );
                var registrationInstanceIdStr = registrationInstance.RegistrationTemplateId.ToString();

                var attributeQry = new AttributeValueService( rockContext ).Queryable()
                    .Where( a => a.Attribute.EntityTypeId == registrationEntityType.Id )
                    .Where( a => a.Attribute.EntityTypeQualifierColumn == "RegistrationTemplateId" )
                    .Where( a => a.Attribute.EntityTypeQualifierValue == registrationInstanceIdStr );

                var subscriptionSummary = new RegistrationService( rockContext ).Queryable().Include( r => r.Registrants )
                    .Where( r => r.RegistrationInstanceId == registrationInstance.Id )
                    .GroupJoin( attributeQry, r => r.Id, a => a.EntityId, ( r, a ) => new { Registration = r, AttributeValues = a } )
                    .SelectMany( r => r.AttributeValues.DefaultIfEmpty(), ( r, a ) => new { Registration = r.Registration, AttributeValues = a } )
                    .GroupBy( r => r.Registration )
                    .Select( r => new SubscriptionSummaryItem
                    {
                        Registration = r.Key,
                        OrganizationName = r.Where( r1 => r1.AttributeValues.Attribute.Key == "OrganizationName" ).Select( r1 => r1.AttributeValues.Value ).FirstOrDefault(),
                        RenewalDate = r.Where( r1 => r1.AttributeValues.Attribute.Key == "RenewalDate" ).Select( r1 => r1.AttributeValues.ValueAsDateTime ).FirstOrDefault(),
                        SubscriptionStatus = r.Where( r1 => r1.AttributeValues.Attribute.Key == "SubscriptionStatus" ).Select( r1 => r1.AttributeValues.ValueAsBoolean ).FirstOrDefault() ?? true,
                        GroupGuidString = r.Where( r1 => r1.AttributeValues.Attribute.Key == "OrganizationUserGroup" ).Select( r1 => r1.AttributeValues.Value ).FirstOrDefault(),
                        SubscriptionTypeGuidString = r.Where( r1 => r1.AttributeValues.Attribute.Key == "SubscriptionType" ).Select( r1 => r1.AttributeValues.Value ).FirstOrDefault(),
                        CancellationDate = r.Where( r1 => r1.AttributeValues.Attribute.Key == "CancellationDate" ).Select( r1 => r1.AttributeValues.ValueAsDateTime ).FirstOrDefault()
                    } )
                    .Where( r => r.SubscriptionStatus == true )
                    .ToList();


                //process pending cancellations
                var today = RockDateTime.Now;
                var pendingCancellations = subscriptionSummary.Where( r => r.CancellationDate < today ).ToList();

                ProcessPendingCancellations( pendingCancellations );

                context.UpdateLastStatusMessage( $"{pendingCancellations.Count()} {"cancellation".PluralizeIf( pendingCancellations.Count() != 1 )} processed." );

            }
        }

        private bool CancelScheduledTransaction( FinancialScheduledTransactionService scheduledTxService,  FinancialScheduledTransaction scheduledTx, RockContext rockContext )
        {
            if ( scheduledTx != null && scheduledTx.FinancialGateway != null )
            {
                scheduledTx.FinancialGateway.LoadAttributes( rockContext );

                string errorMessage = string.Empty;
                if ( scheduledTxService.Cancel( scheduledTx, out errorMessage ))
                {
                    scheduledTxService.GetStatus( scheduledTx, out errorMessage );
                    rockContext.SaveChanges();

                    return errorMessage.IsNotNullOrWhiteSpace();
                }
            }

            return false;


        }

        private void ProcessPendingCancellations( List<SubscriptionSummaryItem> pendingCancellations )
        {
            foreach ( var summaryItem in pendingCancellations )
            {
                using ( var cancellationContext = new RockContext()  )
                {
                    var registrationEntity = EntityTypeCache.Get( typeof( Registration ) );

                    var registrationService = new RegistrationService( cancellationContext );
                    var registration = registrationService.Get( summaryItem.RegistrationId );
                    registration.LoadAttributes();

                    registration.SetAttributeValue( "SubscriptionStatus", bool.FalseString );
                    registration.SaveAttributeValue( "SubscriptionStatus", cancellationContext );

                    var scheduledTxService = new FinancialScheduledTransactionService( cancellationContext );
                    var scheduledTxDetailService = new FinancialScheduledTransactionDetailService( cancellationContext );
                    var scheduledTx = scheduledTxDetailService.Queryable().Where( d => d.EntityTypeId == registrationEntity.Id )
                        .Where( d => d.EntityId == registration.Id )
                        .Where( d => d.ScheduledTransaction.IsActive )
                        .Select( d => d.ScheduledTransaction )
                        .FirstOrDefault();

                    if ( scheduledTx != null && scheduledTx.Id > 0 )
                    {
                        CancelScheduledTransaction(scheduledTxService,  scheduledTx, cancellationContext );
                    }

                    if ( summaryItem.GroupGuid.HasValue )
                    {
                        var groupService = new GroupService( cancellationContext );
                        var group = groupService.Get( summaryItem.GroupGuid.Value );

                        group.IsActive = false;
                    }

                    

                    cancellationContext.SaveChanges();

                }
            }
        }



        class SubscriptionSummaryItem
        {

            public DateTime? CancellationDate { get; set; }

            public Guid? GroupGuid
            {
                get
                {
                    if ( GroupGuidString.IsNotNullOrWhiteSpace() )
                    {
                        return GroupGuidString.AsGuidOrNull();
                    }

                    return null;
                }
            }

            public string GroupGuidString { private get; set; }

            public string OrganizationName { get; set; }

            public Registration Registration { get; set; }

            public int RegistrationId
            {
                get
                {
                    if ( Registration != null )
                    {
                        return Registration.Id;
                    }
                    return 0;
                }
            }


            public DateTime? RenewalDate { get; set; }
            public bool SubscriptionStatus { get; set; }

            public Guid? SubscriptionTypeGuid
            {
                get
                {
                    if ( SubscriptionTypeGuidString.IsNotNullOrWhiteSpace() )
                    {
                        return SubscriptionTypeGuidString.AsGuidOrNull();
                    }

                    return null;
                }
            }

            public string SubscriptionTypeGuidString { private get; set; }

        }

    }


}
