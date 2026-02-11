using System;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;

using Microsoft.Extensions.Logging.Abstractions;

using Rock.AI.Agent;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Performance.Benchmarks
{
    /// <summary>
    /// Performs some basic performance tests on the AgentToolHelper class.
    /// This is primarily to prove that the helper methods don't have a drastic
    /// performance impact compared to standard LINQ methods.
    /// </summary>
    [MemoryDiagnoser( true )]
    [Attributes.OperationsPerSecondColumn]
    [GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
    [CategoriesColumn]
    public class AgentToolHelperBenchmarks
    {
        #region Test Data

        private readonly IQueryable<Campus> _campusQry = new Campus[0].AsQueryable();

        private readonly Consumer _consumer = new Consumer();

        private AgentToolHelper _agentToolHelper;

        #endregion


        [GlobalSetup]
        public void Setup()
        {
            var rockContext = new RockContext( "bogus" );

            _agentToolHelper = new AgentToolHelper( rockContext, new AgentRequestContext( null, rockContext ), NullLogger.Instance);
        }

        [Benchmark( Baseline = true )]
        [BenchmarkCategory( "Where" )]
        public object WherePropertyLambda()
        {
            var campusName = "Test";

            return _campusQry.Where( c => c.Name == campusName );
        }

        [Benchmark]
        [BenchmarkCategory( "Where" )]
        public object WherePropertyHelper()
        {
            var campusName = "Test";

            return _agentToolHelper.WhereRequiredProperty( _campusQry, c => c.Name, campusName );
        }

        [Benchmark( Baseline = true )]
        [BenchmarkCategory( "Select" )]
        public void SelectLambda()
        {
            _campusQry
                .Select( c => new
                {
                    c.Id,
                    c.Name,
                    Leader = c.LeaderPersonAlias != null ? new PersonResult
                    {
                        Id = c.LeaderPersonAlias.Person.Id,
                        NickName = c.LeaderPersonAlias.Person.NickName,
                        LastName = c.LeaderPersonAlias.Person.LastName,
                        IncludeAvatarUrl = false,
                    } : null,
                } )
                .Consume( _consumer );
        }

        [Benchmark]
        [BenchmarkCategory( "Select" )]
        public void SelectExpanded()
        {
            _campusQry
                .AsExpandable()
                .Select( c => new
                {
                    c.Id,
                    c.Name,
                    Leader = PersonResult.NameOnly( c.LeaderPersonAlias ),
                } )
                .Consume( _consumer );
        }
    }
}
