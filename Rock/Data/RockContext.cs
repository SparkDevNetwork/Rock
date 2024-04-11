// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

using Rock.Model;
using Rock.Utility.Settings;

namespace Rock.Data
{
    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public class RockContext : Rock.Data.DbContext
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether timing metrics should be captured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [metrics enabled]; otherwise, <c>false</c>.
        /// </value>
        public QueryMetricDetailLevel QueryMetricDetailLevel { get; set; } = QueryMetricDetailLevel.Off;

        /// <summary>
        /// Gets or sets the metric query count.
        /// </summary>
        /// <value>
        /// The query count.
        /// </value>
        public int QueryCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the metric details.
        /// </summary>
        /// <value>
        /// The metric details.
        /// </value>
        public List<QueryMetricDetail> QueryMetricDetails { get; private set; } = new List<QueryMetricDetail>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockContext"/> class.
        /// Use this if you need to specify a connection string other than the default
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public RockContext( string nameOrConnectionString )
            : base( nameOrConnectionString )
        {
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="RockContext"/> sub-class using the same <see cref="ObjectContext"/> as regular RockContext.
        /// This is for internal use by <see cref="RockContextReadOnly"/> and <see cref="RockContextAnalytics"/>. 
        /// </summary>
        /// <param name="objectContext">The object context.</param>
        /// <param name="dbContextOwnsObjectContext">if set to <c>true</c> [database context owns object context].</param>
        /// <inheritdoc />
        internal protected RockContext( ObjectContext objectContext, bool dbContextOwnsObjectContext ) :
            base( objectContext, dbContextOwnsObjectContext )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockContext"/> class.
        /// </summary>
        public RockContext()
            : base( RockInstanceConfig.Database.ConnectionString )
        {
        }

        /// <summary>
        /// Use SqlBulkInsert to quickly insert a large number records.
        /// NOTE: This bypasses the Rock and a bunch of the EF Framework and automatically commits the changes to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">The records.</param>
        /// <param name="useSqlBulkCopy">if set to <c>true</c> [use SQL bulk copy].</param>
        public void BulkInsert<T>( IEnumerable<T> records, bool useSqlBulkCopy ) where T : class, IEntity
        {
            if ( useSqlBulkCopy )
            {
                this.BulkInsert( records );
            }
            else
            {
                this.Configuration.ValidateOnSaveEnabled = false;
                this.Set<T>().AddRange( records );
                this.SaveChanges( true );
            }
        }

        /// <summary>
        /// This method is called when the context has been initialized, but
        /// before the model has been locked down and used to initialize the context.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            ContextHelper.AddConfigurations( modelBuilder );

            try
            {
                //// dynamically add plugin entities so that queryables can use a mixture of entities from different plugins and core
                //// from http://romiller.com/2012/03/26/dynamically-building-a-model-with-code-first/, but using the new RegisterEntityType in 6.1.3

                // look for IRockStoreModelConvention classes
                var modelConventionList = Reflection.FindTypes( typeof( Rock.Data.IRockStoreModelConvention<System.Data.Entity.Core.Metadata.Edm.EdmModel> ) )
                    .Where( a => !a.Value.IsAbstract )
                    .OrderBy( a => a.Key ).Select( a => a.Value );

                foreach ( var modelConventionType in modelConventionList )
                {
                    var convention = ( IConvention ) Activator.CreateInstance( modelConventionType );
                    modelBuilder.Conventions.Add( convention );
                }

                // look for IRockEntity classes
                var entityTypeList = Reflection.FindTypes( typeof( Rock.Data.IRockEntity ) )
                    .Where( a => !a.Value.IsAbstract && ( a.Value.GetCustomAttribute<NotMappedAttribute>() == null ) && ( a.Value.GetCustomAttribute<System.Runtime.Serialization.DataContractAttribute>() != null ) )
                    .OrderBy( a => a.Key ).Select( a => a.Value );

                foreach ( var entityType in entityTypeList )
                {
                    try
                    {
                        modelBuilder.RegisterEntityType( entityType );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Exception occurred when Registering Entity Type {entityType} to RockContext", ex ), null );
                    }
                }

                // add configurations that might be in plugin assemblies
                foreach ( var assembly in entityTypeList.Select( a => a.Assembly ).Distinct() )
                {
                    try
                    {
                        modelBuilder.Configurations.AddFromAssembly( assembly );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Exception occurred when adding Plugin Entity Configurations from {assembly} to RockContext", ex ), null );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Exception occurred when adding Plugin Entities to RockContext", ex ), null );
            }
        }
    }

    /// <summary>
    /// Enum for determining the level of query metrics to capture.
    /// </summary>
    public enum QueryMetricDetailLevel
    {
        /// <summary>
        /// No metrics will be captured (default)
        /// </summary>
        Off = 0,

        /// <summary>
        /// Just the number of queries will be captured.
        /// </summary>
        Count = 1,

        /// <summary>
        /// All metrics will the captured (count and a copy of the SQL)
        /// </summary>
        Full = 2
    }

    /// <summary>
    /// POCO for storing metrics about the context
    /// </summary>
    public class QueryMetricDetail
    {
        /// <summary>
        /// Gets or sets the SQL.
        /// </summary>
        /// <value>
        /// The SQL.
        /// </value>
        public string Sql { get; set; }

        /// <summary>
        /// Gets or sets the duration in ticks (not fleas).
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public long Duration { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public string Database { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public static class ContextHelper
    {
        /// <summary>
        /// Adds the configurations.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        public static void AddConfigurations( DbModelBuilder modelBuilder )
        {
            modelBuilder.Conventions.Add<DecimalPrecisionAttributeConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.AddFromAssembly( typeof( RockContext ).Assembly );
        }
    }
}