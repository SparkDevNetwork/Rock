using System;
using System.ComponentModel;
using System.Linq;


using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;


namespace org.lakepointe.MinistryPoint.Jobs
{
    [RegistrationInstanceField( "Ministry Point Registrtaion Instance",
        Description = "Registration Instance for Ministry Point",
        IsRequired = true,
        Order = 0,
        Key = "MinistryPointInstance" )]
    [IntegerField( "Days Back",
        Description = "Number of days back to check for renewals",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Key = "DaysBack" )]

    [IntegerField( "Subscription Type Attribute Id",
        Description = "Subscription Type Attribute Id",
        IsRequired = false,
        DefaultIntegerValue = 80971,
        Key = "SubscriptionTypeAttribute" )]

    [IntegerField( "Renewal Date Attribute Id",
        Description = "Renewal Date Attribute ID",
        IsRequired = false,
        DefaultIntegerValue = 80974,
        Key = "RenewalDateAttribute" )]

    [IntegerField( "Subscription Status Attribute Id",
        Description = "The Id of the attribute that contains the subscription status",
        IsRequired = false,
        DefaultIntegerValue = 80971,
        Key = "SubscriptionStatusAttribute" )]

    [DisplayName( "Process Ministry Point Renewals" )]
    [Description( "Processes Ministry Point Renewals on/around the renewal date." )]
    [DisallowConcurrentExecution]
    public class ProcessSubscriptionRenewal : IJob
    {
        private string SubscriptionTypeDefinedTypeGuid = "b69b23c1-d831-4390-be83-ab69b89219ca";

        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;

            var registrationInstanceGuid = dataMap.GetString( "MinistryPointInstance" ).AsGuid();
            var daysBack = dataMap.GetString( "DaysBack" ).AsInteger();
            var subscriptionTypeAttributeID = dataMap.GetString( "SubscriptionTypeAttribute" ).AsInteger();
            var renewalDateAttributeId = dataMap.GetString( "RenewalDateAttribute" ).AsInteger();
            var subscriptionStatusAttributeId = dataMap.GetString( "SubscriptionStatusAttribute" ).AsInteger();
            var renewalStartDate = RockDateTime.Today.AddDays( -daysBack );
            var renewalEndDate = RockDateTime.Today;

            using ( var rockContext = new RockContext() )
            {
                var sql = @"Select r.Id, Try_Cast(subType.Value as UniqueIdentifier) as SubscriptionTypeGuid, renewal.ValueAsDateTime as RenewalDate, ISNULL(subStatus.ValueAsBoolean, 1) as SubscriptionStatus 
                            FROM Registration r
                            INNER JOIN RegistrationInstance i on r.RegistrationInstanceId = i.Id
                            LEFT OUTER JOIN AttributeValue subType on r.Id = subType.EntityId and subType.AttributeId = @SubscriptionTypeAttributeId
                            LEFT OUTER JOIN AttributeValue renewal on r.Id = renewal.EntityId and renewal.AttributeId = @RenewalDateAttributeId
                            LEFT OUTER JOIN AttributeValue subStatus on r.Id = subStatus.EntityId and subStatus.AttributeID = @SubscriptionStatusAttributeId
                            WHERE i.Guid = @RegistrationInstanceGuid AND renewal.ValueAsDateTime between @StartDate and @EndDate AND ISNULL(subStatus.ValueAsBoolean, 1)  = 1 ";


                var subscriptionsToProcess = rockContext.Database.SqlQuery<SubscriptionItemForRenewal>( sql, new object[]
                    { new System.Data.SqlClient.SqlParameter("@RegistrationInstanceGuid", registrationInstanceGuid),
                    new System.Data.SqlClient.SqlParameter("@StartDate", renewalStartDate ),
                    new System.Data.SqlClient.SqlParameter("@EndDate", renewalEndDate),
                    new System.Data.SqlClient.SqlParameter("@SubscriptionTypeAttributeId", subscriptionTypeAttributeID),
                    new System.Data.SqlClient.SqlParameter("@RenewalDateAttributeId", renewalDateAttributeId),
                    new System.Data.SqlClient.SqlParameter("@SubscriptionStatusAttributeId", subscriptionStatusAttributeId)} )
                    .ToList();

                var subscriptionTypes = DefinedTypeCache.Get( SubscriptionTypeDefinedTypeGuid.AsGuid() ).DefinedValues;


                foreach ( var subscription in subscriptionsToProcess )
                {
                    using ( var subscriptionContext = new RockContext() )
                    {
                        var registrationService = new RegistrationService( subscriptionContext );
                        var registration = registrationService.Get( subscription.Id );
                        registration.LoadAttributes( subscriptionContext );
                        var renewalDate = registration.GetAttributeValue( "RenewalDate" ).AsDateTime();
                        var subscriptionType = subscriptionTypes.Where( t => t.Guid == registration.GetAttributeValue( "SubscriptionType" ).AsGuid() ).SingleOrDefault();

                        if ( registration.Registrants.Where( r => r.CreatedDateTime >= renewalDate ).Count() == 0 )
                        {
                            var registrant = new RegistrationRegistrant();
                            registrant.PersonAliasId = registration.PersonAliasId;
                            registrant.DiscountApplies = true;
                            registrant.Cost = subscriptionType.GetAttributeValue( "Price" ).AsDecimal();
                            registration.Registrants.Add( registrant );

                            subscriptionContext.SaveChanges();

                            var newRenewalDate = renewalDate.Value.AddMonths( subscriptionType.GetAttributeValue( "SubscriptionMonths" ).AsInteger() );
                            registration.SetAttributeValue( "RenewalDate", newRenewalDate );
                            registration.SaveAttributeValue( "RenewalDate", subscriptionContext );


                        }
                    }
                }

            }
        }
    }

    class SubscriptionItemForRenewal
    {
        public int Id { get; set; }
        public Guid? SubscriptionTypeGuid { get; set; }
        public DateTime? RenewalDate { get; set; }
        public bool SubscriptionStatus { get; set; }
    }
}
