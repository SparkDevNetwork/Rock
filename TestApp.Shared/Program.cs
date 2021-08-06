using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;

#if NET5_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;

namespace TestApp
{
    class Program
    {
#if NET5_0_OR_GREATER
        private static string _directoryPrefix = "efcore";
#else
        private static string _directoryPrefix = "ef6";
#endif

        static void Main( string[] args )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new DefinedValueService( rockContext ).Queryable().AsNoTracking().ToList();

                //var y = items[5].DefinedType?.Id;
                new DefinedValueService( rockContext ).Queryable().ToList();
            }

            System.Threading.Thread.Sleep( 5000 );

            double trackingMs = 0;
            double noTrackingMs = 0;
            for ( int i = 0; i < 10; i++ )
            {
                using ( var rockContext = new RockContext() )
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    new DefinedValueService( rockContext ).Queryable().ToList();
                    sw.Stop();
                    Console.WriteLine( $"Took {sw.ElapsedMilliseconds}ms with tracking." );
                    trackingMs += sw.Elapsed.TotalMilliseconds;
                }

                using ( var rockContext = new RockContext() )
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    new DefinedValueService( rockContext ).Queryable().AsNoTracking().ToList();
                    sw.Stop();
                    Console.WriteLine( $"Took {sw.ElapsedMilliseconds}ms without tracking." );
                    noTrackingMs += sw.Elapsed.TotalMilliseconds;
                }
            }

            Console.WriteLine( $"Tracking Avg: {trackingMs / 10}; No Tracking Avg: {noTrackingMs / 10}" );
            Console.ReadLine();

            using ( var rockContext = new RockContext() )
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var definedValues = new DefinedValueService( rockContext ).Queryable();
                var count = definedValues.Count();
                var entityTypeId = Rock.Web.Cache.EntityTypeCache.Get( Guid.Parse( Rock.SystemGuid.EntityType.GROUP ) ).Id;
                sw.Stop();
                Console.WriteLine( $"Got {count} defined values." );
                Console.WriteLine( $"Group Entity Type Id = {entityTypeId}." );
                Console.WriteLine( $"Took {sw.ElapsedMilliseconds}ms." );
                //var groupTypeService = new GroupTypeService( rockContext );
                //var groupType = new GroupType
                //{
                //    Name = "Daniel Test"
                //};

                //groupTypeService.Add( groupType );
                //rockContext.SaveChanges();

                return;
            }

            var entityTypes = GetEntityTypes();

            if ( !Directory.Exists( $"{_directoryPrefix}-test" ) )
            {
                Directory.CreateDirectory( $"{_directoryPrefix}-test" );
            }

            Console.WriteLine( "Counting..." );
            var entityCounts = EntityTester.CountAllEntities( entityTypes );

            using ( var countsWriter = new StreamWriter( $"{_directoryPrefix}-test/_counts.txt", false ) )
            {
                countsWriter.WriteLine( entityCounts );
                countsWriter.Flush();
            }

            Console.WriteLine( "Dumping..." );
            var entityDump = EntityTester.DumpEntities( entityTypes );

            foreach ( var d in entityDump )
            {
                using ( var dumpWriter = new StreamWriter( $"{_directoryPrefix}-test/{d.Key}.txt", false ) )
                {
                    try
                    {
                        dumpWriter.WriteLine( JsonConvert.SerializeObject( d.Value, Formatting.Indented ) );
                    }
                    catch ( Exception ex )
                    {
                        dumpWriter.WriteLine( $"Exception: {ex.Message}" );
                    }

                    dumpWriter.Flush();
                }
            }
        }

        static IEnumerable<Type> GetEntityTypes()
        {
            return Rock.Reflection.FindTypes( typeof( IEntity ) )
                .Values
                .Where( t => !t.IsAbstract )
                .Where( t => t.GetCustomAttribute<NotMappedAttribute>() == null )
                .OrderBy( v => v.FullName );
        }
    }
}
