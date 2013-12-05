//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Data.Entity;

namespace Rock.Data
{
    /// <summary>
    /// Class used when services need to share the same DbContext
    /// </summary>
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
