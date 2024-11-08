﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// All unit tests that require database access should inherit from
    /// this class. It will abstract how away how a database is provided
    /// to each test. Currently this is done with docker images.
    /// </summary>
    public abstract class DatabaseTestsBase
    {
        /// <summary>
        /// Initialize the test database container.
        /// </summary>
        /// <param name="container"></param>
        public static void InitializeContainer( ITestDatabaseContainer container )
        {
            _container = container;

            IsContainersEnabled = ( _container != null );
        }

        /// <summary>
        /// <c>true</c> if running the database in a docker container
        /// is enabled; otherwise the RockContext connection string
        /// defined in the app settings will be used instead.
        /// </summary>
        public static bool IsContainersEnabled { get; set; } = true;

        /// <summary>
        /// <c>true</c> if we need to destroy the test container after the
        /// current test has finished.
        /// </summary>
        private bool _testWantsIsolatedDatabase = false;

        /// <summary>
        /// The current container providing the database.
        /// </summary>
        private static ITestDatabaseContainer _container;

        /// <summary>
        /// <c>true</c> if the container has been used by a test already.
        /// </summary>
        private static bool _containerIsDirty = false;

        /// <summary>
        /// Set by the test framework to indicate which test is running.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Methods

        /// <summary>
        /// Starts a new container, after stopping the previous container,
        /// for a fresh database.
        /// </summary>
        /// <returns>A task that indicates when the operation has completed.</returns>
        private static async Task StartNewContainer()
        {
            if ( !IsContainersEnabled )
            {
                return;
            }

            await DisposeDatabaseInstanceAsync();

            // Initialize a new database instance in the container.
            LogHelper.Log( $"Starting Database Instance... [Container={_container.GetType().Name}]" );

            try
            {
                await _container.StartAsync();

                if ( !_container.HasCurrentInstance )
                {
                    throw new Exception( "The container failed to provide a valid database instance." );
                }
            }
            catch ( Exception ex )
            {
                LogHelper.LogError( ex, "The database instance failed to start." );
                throw ex;
            }

            LogHelper.Log( $"Starting Database Instance: completed." );

            _containerIsDirty = false;
        }

        /// <summary>
        /// Called before any test is executed in the entire class.
        /// </summary>
        /// <param name="context">The context of the first test that will be run.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        [ClassInitialize( InheritanceBehavior.BeforeEachDerivedClass )]
        public static Task ContainerClassInitialize( TestContext context )
        {
            return StartNewContainer();
        }

        /// <summary>
        /// Called after every test in the class has finished executing.
        /// </summary>
        /// <returns>A task that indicates when the cleanup has completed.</returns>
        [ClassCleanup( InheritanceBehavior.BeforeEachDerivedClass )]
        public static async Task ContainerClassCleanup()
        {
            // Make sure we shut down the container at the end of all tests
            // in this class.
            await DisposeDatabaseInstanceAsync();
        }

        /// <summary>
        /// Called before each test in the class is executed. This will
        /// also be called before each data row of the test is executed.
        /// So if a test has 20 data rows and requests a new database, we
        /// will spin up a new database 20 times.
        /// </summary>
        /// <returns>A task that indicates when the initialization has completed.</returns>
        [TestInitialize]
        public async Task ContainerTestInitialize()
        {
            if ( !IsContainersEnabled )
            {
                return;
            }

            var method = GetType().GetMethod( TestContext.TestName );

            _testWantsIsolatedDatabase = method.GetCustomAttribute<IsolatedTestDatabaseAttribute>() != null
                || GetType().GetCustomAttribute<IsolatedTestDatabaseAttribute>() != null;

            if ( !_container.HasCurrentInstance || ( _testWantsIsolatedDatabase && _containerIsDirty ) )
            {
                await StartNewContainer();
            }
        }

        /// <summary>
        /// Called after each test in the class has completed.
        /// </summary>
        /// <returns>A task that indicates when cleanup is completed.</returns>
        [TestCleanup]
        public async Task ContainerTestCleanup()
        {
            _containerIsDirty = true;

            // If the test indicated that it needed an isolated database
            // then shut it down so the next test gets a fresh one.
            if ( _testWantsIsolatedDatabase )
            {
                await DisposeDatabaseInstanceAsync();
            }
        }

        private static async Task DisposeDatabaseInstanceAsync()
        {
            if ( _container == null )
            {
                return;
            }

            if ( _container.HasCurrentInstance )
            {
                // Dispose the current database instance held in the container.
                await _container.DisposeAsync();
            }
        }

        #endregion
    }
}
