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
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

using Rock.Utility.Settings;

namespace Rock.Data
{
    /// <summary>
    /// <para>
    /// <c>WARNING</c>: Experimental. For internal use only. This is intended only for use in environments with
    /// a read only replica of the production DB.
    /// </para>
    /// <para>
    /// This is special RockContext that can be used to connect to a readonly copy of the RockContext database
    /// as specified as a connection named <see cref="RockContextReadOnly"/> in web.connectionstrings.config.
    /// Operations that write to the database are not allowed.
    /// </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal class</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public classes. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    public class RockContextReadOnly : RockContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockContextReadOnly"/> class.
        /// </summary>
        public RockContextReadOnly()
            : this( new RockContext( RockInstanceConfig.Database.ReadOnlyConnectionString ) )
        {
            /*  2021-09-28 MDP

            In order for RockContextReadOnly to use the same compiled EF Model as RockContext,
            We need to use the DbContext constructor that takes an ObjectContext parameter.

            If we don't do this, EF will try to compile a new set of DbModels that will
            conflict with the DbModel that RockContext uses. 
             
            */
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockContextReadOnly"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        internal protected RockContextReadOnly( RockContext rockContext )
            : base( ( rockContext as IObjectContextAdapter ).ObjectContext, true )
        {
            // The ObjectContext constructor needs to know whether we own the ObjectContext
            // or if it should take care of disposing itself.
            // In this case, we want to use the instance of the ObjectContext
            // that a normal RockContext would use.
            _ownedRockContext = rockContext;
        }

        private readonly RockContext _ownedRockContext = null;

        /// <inheritdoc />
        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            _ownedRockContext?.Dispose();
        }

        /// <summary>
        /// Not Supported by <see cref="RockContextReadOnly"/> or <see cref="RockContextAnalytics"/>.
        /// </summary>
        /// <exception cref="WriteOperationsNotSupportedException" />
        public override int SaveChanges()
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <inheritdoc cref="RockContextReadOnly.SaveChanges()" />
        public override int SaveChanges( bool disablePrePostProcessing )
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <inheritdoc cref="RockContextReadOnly.SaveChanges()" />
        public override SaveChangesResult SaveChanges( SaveChangesArgs args )
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <inheritdoc cref="RockContextReadOnly.SaveChanges()" />
        public override int BulkDelete<T>( IQueryable<T> queryable, int? batchSize = null )
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <inheritdoc cref="RockContextReadOnly.SaveChanges()" />
        public override void BulkInsert<T>( IEnumerable<T> records )
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <inheritdoc cref="RockContextReadOnly.SaveChanges()" />
        public override void BulkInsertWithConditionalCacheUse<T>( IEnumerable<T> records, bool canUseCache )
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <inheritdoc cref="RockContextReadOnly.SaveChanges()" />
        public override int BulkUpdate<T>( IQueryable<T> queryable, Expression<Func<T, T>> updateFactory )
        {
            throw new WriteOperationsNotSupportedException( this );
        }

        /// <summary>
        /// </summary>
        /// <seealso cref="System.NotSupportedException" />
        public class WriteOperationsNotSupportedException : NotSupportedException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WriteOperationsNotSupportedException"/> class.
            /// </summary>
            /// <param name="rockContextReadOnly">The rock context read only.</param>
            public WriteOperationsNotSupportedException( RockContextReadOnly rockContextReadOnly )
                : base( $"{rockContextReadOnly.GetType().Name} can only be used for ReadOnly operations." )
            {
            }
        }
    }
}