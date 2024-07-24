using System;

using Quartz;
using Rock.Data;
using Rock.Attribute;
using System.Linq;
using Rock;

namespace org.lakepointe.RockJobCustomizations
{
    [EnumField(
        name: "Pricing Tier",
        description: "The pricing tier that SQL should be at.",
        enumSourceType: typeof( PricingTier ),
        key: "PricingTier",
        required: true,
        order: 0 )]

    [IntegerField(
        name: "Database ID",
        description: "The ID of the database to check. Only required if there are more than one. Use \"SELECT * FROM sys.database_service_objectives\" to find the ID.",
        key: "DatabaseId",
        required:false,
        order: 1 )]

    [DisallowConcurrentExecution]
    public class CheckAzureSqlScale : IJob
    {
        PricingTier _pricingTierParam;
        int _databaseIdParam;

        public void Execute( IJobExecutionContext context )
        {
            // Get parameters
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _databaseIdParam = dataMap.GetString( "DatabaseId" ).AsInteger();
            if ( Enum.TryParse( dataMap.GetString( "PricingTier" ), out _pricingTierParam ) == false )
            {
                throw new ArgumentException( "The provided pricing tier was not found." );
            }

            using ( var rockContext = new RockContext() )
            {
                // Get the current pricing tier from SQL
                var pricingTierResults = rockContext.Database.SqlQuery<PricingTierQueryResult>( "SELECT [database_id] as [DatabaseId], [edition] as [Edition], [service_objective] as [ServiceObjective] FROM sys.database_service_objectives" ).ToList();

                if ( pricingTierResults == null || pricingTierResults.Count <= 0 )
                {
                    throw new InvalidOperationException( "No rows were returned from sys.database_service_objectives." );
                }

                // If there are more than one, get the one that has a matching Database ID
                var pricingTierResult = pricingTierResults.FirstOrDefault();
                if ( pricingTierResults.Count > 1 )
                {
                    var matchingPricingTier = pricingTierResults.Where( p => p.DatabaseId == _databaseIdParam ).FirstOrDefault();
                    if ( matchingPricingTier != null )
                    {
                        pricingTierResult = matchingPricingTier;
                    }
                    else
                    {
                        throw new ArgumentException( "The provided Database ID does not match any of the existing databases." );
                    }
                }

                if ( pricingTierResult == null )
                {
                    throw new InvalidOperationException( "Could not find any databases to check." );
                }

                // Make sure the pricing tier from SQL meets the one we are expecting
                if ( Enum.TryParse( pricingTierResult.ToString(), out PricingTier pricingTier ) )
                {
                    if ( pricingTier != _pricingTierParam )
                    {
                        throw new ArgumentException( $"SQL is at a different pricing tier than the provided pricing tier. Current: \"{pricingTier}\". Provided: \"{_pricingTierParam}\"." );
                    }
                }
                else
                {
                    throw new InvalidOperationException( "The current pricing tier does not match any of the supported pricing tiers." );
                }

                // No issues found
                context.Result = $"SQL is at the provided pricing tier. Current pricing tier: \"{ pricingTier }\".";
            }
        }

        class PricingTierQueryResult
        {
            public int DatabaseId { get; set; }
            public string Edition { get; set; }
            public string ServiceObjective { get; set; }

            public override string ToString()
            {
                return $"{Edition}{ServiceObjective}";
            }
        }
    }

    enum PricingTier
    {
        Basic,
        StandardS0,
        StandardS1,
        StandardS2,
        StandardS3,
        StandardS4,
        StandardS6,
        StandardS7,
        StandardS9,
        StandardS12,
        PremiumP1,
        PremiumP2,
        PremiumP4,
        PremiumP6,
        PremiumP11,
        PremiumP15
    }
}
