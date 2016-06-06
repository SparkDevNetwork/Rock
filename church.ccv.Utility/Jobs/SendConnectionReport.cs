
using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace church.ccv.Utility
{
    /// <summary>
    /// Creates workflows for first visit calls
    /// </summary>
    [SystemEmailField( "Email Template", "The system email to use.", true, "", Order = 1 )]
    [LinkedPage( "Connection Report Page", "The page of the connection report.", true, Order = 2 )]
    [TextField( "Development Email List", "Email addresses of people that receive the development connection report.", false, "", "Email", Order = 3 )]
    [TextField( "Anthem Email List", "Email addresses of people that receive the Anthem connection report.", false, "", "Email", Order = 4 )]
    [TextField( "Avondale Email List", "Email addresses of people that receive the Avondale connection report.", false, "", "Email", Order = 5 )]
    [TextField( "East Valley Email List", "Email addresses of people that receive the East Valley connection report.", false, "", "Email", Order = 6 )]
    [TextField( "Midtown Email List", "Email addresses of people that receive the Midtown connection report.", false, "", "Email", Order = 7 )]
    [TextField( "Peoria Email List", "Email addresses of people that receive the Peoria connection report.", false, "", "Email", Order = 8 )]
    [TextField( "Scottsdale Email List", "Email addresses of people that receive the Scottsdale connection report.", false, "", "Email", Order = 9 )]
    [TextField( "Surprise Email List", "Email addresses of people that receive the Surprise connection report.", false, "", "Email", Order = 10 )]

    [DisallowConcurrentExecution]
    public class SendConnectionReport : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendConnectionReport()
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

            Guid? systemEmailGuid = dataMap.GetString( "EmailTemplate" ).AsGuidOrNull();
            Guid? pageGuid = dataMap.GetString( "ConnectionReportPage" ).AsGuidOrNull();
            List<string> devEmailList = dataMap.GetString( "DevelopmentEmailList" ).SplitDelimitedValues().ToList();
            List<string> anthemEmailList = dataMap.GetString( "AnthemEmailList" ).SplitDelimitedValues().ToList();
            List<string> avondaleEmailList = dataMap.GetString( "AvondaleEmailList" ).SplitDelimitedValues().ToList();
            List<string> eastvalleyEmailList = dataMap.GetString( "EastValleyEmailList" ).SplitDelimitedValues().ToList();
            List<string> midtownEmailList = dataMap.GetString( "MidtownEmailList" ).SplitDelimitedValues().ToList();
            List<string> peoriaEmailList = dataMap.GetString( "PeoriaEmailList" ).SplitDelimitedValues().ToList();
            List<string> scottsdaleEmailList = dataMap.GetString( "ScottsdaleEmailList" ).SplitDelimitedValues().ToList();
            List<string> surpriseEmailList = dataMap.GetString( "SurpriseEmailList" ).SplitDelimitedValues().ToList();

            // timeframes
            var sevenDaysAgo = RockDateTime.Now.AddDays( -7 );
            var thirtyDaysAgo = RockDateTime.Now.AddDays( -30 );

            // get system email
            SystemEmail systemEmail = null;
            if ( systemEmailGuid.HasValue )
            {
                SystemEmailService emailService = new SystemEmailService( rockContext );
                systemEmail = emailService.Get( systemEmailGuid.Value );
            }

            // get connection report page
            var connectionPageId = new PageService( rockContext )
                .Queryable().Where( p => p.Guid == pageGuid )
                .Select( p => p.Id ).FirstOrDefault();

            // send Development email
            if ( devEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "All Connections" );
                mergeFields.Add( "CampusId", 100 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r => 
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, devEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send Anthem email
            if ( anthemEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "Anthem Campus" );
                mergeFields.Add( "CampusId", 8 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 8 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 8 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, anthemEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send Avondale email
            if ( avondaleEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "Avondale Campus" );
                mergeFields.Add( "CampusId", 9 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 9 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 9 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, avondaleEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send East valley email
            if ( eastvalleyEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "East Valley Campus" );
                mergeFields.Add( "CampusId", 7 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 7 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 7 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, eastvalleyEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send Midtown email
            if ( midtownEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "Midtown Campus" );
                mergeFields.Add( "CampusId", 10 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 10 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 10 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, midtownEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send Peoria email
            if ( peoriaEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "Peoria Campus" );
                mergeFields.Add( "CampusId", 1 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 1 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total= s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 1 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, peoriaEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send Scottsdale email
            if ( scottsdaleEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "Scottsdale Campus" );
                mergeFields.Add( "CampusId", 6 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 6 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 6 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, scottsdaleEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            // send Surprise email
            if ( surpriseEmailList.Any() )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Title", "Surprise Campus" );
                mergeFields.Add( "CampusId", 5 );

                var connectionStatuses = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 5 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .GroupBy( r => new
                    {
                        r.ConnectionStatus
                    } )
                    .Select( s => new
                    {
                        ConnectionStatus = s.Key.ConnectionStatus,
                        Total = s.Count()
                    } ).ToList();

                mergeFields.Add( "NoContact", connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 1 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressPlacement", connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).Any()
                    ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 2 ).FirstOrDefault().Total : 0 );
                mergeFields.Add( "InProgressOther", connectionStatuses
                    .Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Any()
                        ? connectionStatuses.Where( c => c.ConnectionStatus.Id == 9 || c.ConnectionStatus.Id == 10 || c.ConnectionStatus.Id == 13 ).Sum( c => c.Total ) : 0 );

                var requests = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.CampusId == 5 )
                    .Where( r => r.ConnectionOpportunity.ConnectionTypeId == 1 )
                    .Where( r => r.ConnectionState == ConnectionState.Active )
                    .Where( r =>
                            ( r.ConnectionStatus.Id == 1 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( sevenDaysAgo ) <= 0 ) ||
                            ( r.ConnectionStatus.Id == 2 && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 ) ||
                            ( ( r.ConnectionStatus.Id == 9 || r.ConnectionStatus.Id == 10 || r.ConnectionStatus.Id == 13 )
                                && r.ConnectionRequestActivities.Max( a => a.CreatedDateTime ).Value.CompareTo( thirtyDaysAgo ) <= 0 )
                        )
                    .Select( s => new
                    {
                        Person = s.PersonAlias.Person,
                        CurrentStatus = s.ConnectionStatus.Name,
                        LastUpdated = s.ModifiedDateTime.ToString(),
                        Connector = s.ConnectorPersonAlias.Person
                    } ).OrderBy( r => r.Person.LastName ).ToList();

                mergeFields.Add( "rows", requests );
                mergeFields.Add( "PageId", connectionPageId );

                if ( requests.Any() )
                {
                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, surpriseEmailList, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );
                }
            }

            context.Result = "Emails sent.";
        }
    }
}
