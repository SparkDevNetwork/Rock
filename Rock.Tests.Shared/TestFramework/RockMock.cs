using System;
using System.Collections.Generic;

using Moq;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// <para>
    /// This is a custom subclass of Mock&lt;T&gt; that provides additional
    /// features. These features make it easier to mock objects in Rock.
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// Support for deferred initialization of object property values. This
    /// allows a helper method to create a mock object and return the mocking
    /// instance to the caller for further setup and still set some property
    /// values when it has been initialized.
    /// </item>
    /// <item>
    /// Store custom data values on the mock itself. These can be used to share
    /// information between methods about the mocked object.
    /// </item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">The type to be mocked.</typeparam>
    public class RockMock<T> : Mock<T>
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

        #region Properties

        /// <summary>
        /// A dictionary of custom data values associated with this mock. These
        /// have no structure and can be anything that needs to be stored and
        /// later retrieved.
        /// </summary>
        public Dictionary<string, object> CustomData { get; } = new Dictionary<string, object>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the LazyMock class.
        /// </summary>
        /// <param name="behavior">The behavior to use for the mock instance.</param>
        public RockMock( MockBehavior behavior )
            : base( behavior )
        {
        }

        public RockMock( MockBehavior behavior, params object[] args )
            : base( behavior, args )
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
