using System;
using System.Collections.Generic;

using Moq;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Provides a mock object that supports deferred initialization of object
    /// property values. This allows a helper method to create a mock object and
    /// return the mocking instance to the caller for further setup and still
    /// set some property values when it has been initialized.
    /// </summary>
    /// <typeparam name="T">The type to be mocked.</typeparam>
    internal class LazyMock<T> : Mock<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// Determines if the object has been initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// The one-time initializers to call after the object has been
        /// initialized.
        /// </summary>
        private readonly List<Action<T>> _initializers = new List<Action<T>>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the LazyMock class.
        /// </summary>
        /// <param name="behavior">The behavior to use for the mock instance.</param>
        public LazyMock( MockBehavior behavior )
            : base( behavior )
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override object OnGetObject()
        {
            var value = base.OnGetObject();

            if ( !_isInitialized )
            {
                foreach ( var initializer in _initializers )
                {
                    initializer( value as T );
                }

                _isInitialized = true;
            }

            return value;
        }

        /// <inheritdoc/>
        public void SetupInitializer( Action<T> initializer )
        {
            if ( _isInitialized )
            {
                throw new InvalidOperationException( "Cannot add initializer after the mock has been initialized." );
            }

            _initializers.Add( initializer );
        }

        #endregion
    }
}
