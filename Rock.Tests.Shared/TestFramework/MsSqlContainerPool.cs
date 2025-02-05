using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Testcontainers.MsSql;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Simple pool of <see cref="MsSqlContainer"/> images. It takes a few
    /// seconds for the database image to start. Once it is started it takes
    /// another couple seconds to initialize the information before the test
    /// can use the database. Using a pool lets us shave off a few seconds
    /// from each container because we keep a few started and sitting around.
    /// </summary>
    class MsSqlContainerPool
    {
        /// <summary>
        /// The pool of contains that have started, or are starting.
        /// </summary>
        private readonly List<Task<MsSqlContainer>> _pool = new List<Task<MsSqlContainer>>();

        /// <summary>
        /// The name of the image to initialize the container from.
        /// </summary>
        private readonly string _imageName;

        /// <summary>
        /// The number of containers to keep running.
        /// </summary>
        private readonly int _poolSize;

        private int _takeCount;

        /// <summary>
        /// A lock object to provide synchronization.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new instance of <see cref="MsSqlContainerPool"/>.
        /// </summary>
        /// <param name="imageName">The name of the image to initialize the container from.</param>
        public MsSqlContainerPool( string imageName )
        {
            var poolSize = Environment.GetEnvironmentVariable( "CONTAINER_POOL_SIZE" ).AsIntegerOrNull();

            if ( !poolSize.HasValue )
            {
                // If the pool size was not specified by environment variable
                // then make an educated guess. Get the amount of available
                // physical memory. Each image takes about 1.2GB so give some
                // buffer to get the number of images we can run. Then cap it
                // to the number of processors.
                try
                {
                    var availableMemory = OSInfo.GetAvailablePhysicalMemory();

                    poolSize = ( int ) Math.Floor( availableMemory / 1.5 );
                    poolSize = Math.Min( poolSize.Value, Environment.ProcessorCount );

                    // Keep at most 4 images.
                    poolSize = Math.Min( 4, poolSize.Value );
                }
                catch
                {
                    poolSize = 1;
                }
            }

            _imageName = imageName;

            // Subtract 1 because the image being used by the test does not
            // count towards the pool size, so if we want 4 images total in
            // memory then we only need 3 in the pool.
            _poolSize = Math.Max( 0, poolSize.Value - 1 );

            LogHelper.Log( $"Initialized Docker Container standby pool size of {_poolSize}." );
        }

        /// <summary>
        /// Obtain the lock and fill the pool to the proper size.
        /// </summary>
        private void FillPool()
        {
            lock ( _lock )
            {
                FillPoolNoLock();
            }
        }

        /// <summary>
        /// Fill the pool to the proper size. This must be called within a lock
        /// on <see cref="_lock"/>.
        /// </summary>
        private void FillPoolNoLock()
        {
            for ( int i = _pool.Count; i < _poolSize; i++ )
            {
                var task = Task.Run( () =>
                {
                    return StartContainerAsync();
                } );

                _pool.Add( task );
            }
        }

        private async Task<MsSqlContainer> StartContainerAsync()
        {
            var container = new MsSqlBuilder()
                .WithImage( _imageName )
                .Build();

            await container.StartAsync();

            return container;
        }

        /// <summary>
        /// Take one item from the pool. The Task may throw an exception if
        /// the container failed to start.
        /// </summary>
        /// <returns>A task that represents the container.</returns>
        public Task<MsSqlContainer> TakeAsync()
        {
            if ( _poolSize == 0 )
            {
                return StartContainerAsync();
            }

            lock ( _lock )
            {
                // If this is the first container being requested, then just
                // start it without the pool. This way if we are running just
                // a single test we don't spin up a bunch of extra containers.
                if ( _takeCount == 0 )
                {
                    return StartContainerAsync();
                }

                _takeCount++;

                if ( _pool.Count == 0 )
                {
                    FillPool();
                }

                var task = _pool[0];

                _pool.RemoveAt( 0 );
                FillPoolNoLock();

                return task;
            }
        }

        class OSInfo
        {
            [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
            private class MemoryStatusEx
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;

                public MemoryStatusEx()
                {
                    this.dwLength = ( uint ) Marshal.SizeOf( typeof( MemoryStatusEx ) );
                }
            }

            [return: MarshalAs( UnmanagedType.Bool )]
            [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
            private static extern bool GlobalMemoryStatusEx( [In, Out] MemoryStatusEx lpBuffer );

            /// <summary>
            /// Gets the available physical memory in GB.
            /// </summary>
            /// <returns>The number of GB.</returns>
            public static double GetAvailablePhysicalMemory()
            {
                var memoryStatus = new MemoryStatusEx();

                if ( GlobalMemoryStatusEx( memoryStatus ) )
                {
                    return memoryStatus.ullAvailPhys / 1024 / 1024 / 1024.0;
                }

                return 0;
            }
        }
    }
}
