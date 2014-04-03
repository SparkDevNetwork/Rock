// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Data.Entity;

namespace Rock.Data
{
    /// <summary>
    /// Class used when services need to share the same DbContext
    /// </summary>
    [Obsolete("Getting rid of this soon")]
    public class UnitOfWorkScope : IDisposable
    {
        [ThreadStatic]
        private static UnitOfWorkScope currentScope;
        private bool isDisposed;

        /// <summary>
        /// The object context
        /// </summary>
        public readonly DbContext DbContext;

        /// <summary>
        /// Gets the current object context.
        /// </summary>
        internal static DbContext CurrentObjectContext
        {
            get { return currentScope != null ? currentScope.DbContext : null; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWorkScope"/> class.
        /// </summary>
        public UnitOfWorkScope() 
        {
            if ( currentScope == null )
            {
                DbContext = new RockContext();
                isDisposed = false;
                currentScope = this;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWorkScope"/> class using a custom dbContext
        /// </summary>
        public UnitOfWorkScope(DbContext context)
        {
            if ( currentScope == null )
            {
                DbContext = context;
                isDisposed = false;
                currentScope = this;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !isDisposed )
            {
                if ( disposing )
                {
                    if ( DbContext != null )
                    {
                        DbContext.Dispose();
                        currentScope = null;
                    }
                }

                isDisposed = true;
            }
        }
    }
}
