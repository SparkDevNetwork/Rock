namespace InteractiveExperienceLoadTest
{
    /// <summary>
    /// Helper methods for doing things in parallel.
    /// </summary>
    internal static class Parallel
    {
        /// <summary>
        /// Simple helper method to execute a function for a set of items in parallel
        /// with a limit on the number of concurrent executions.
        /// </summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="items">The items to be processed</param>
        /// <param name="process">The function that will process a single item.</param>
        /// <param name="maxConcurrency">The maximum number of concurrent items to process.</param>
        /// <param name="cancellationToken">A token to signal the early termination of the processing.</param>
        /// <returns>A task that represents the operation.</returns>
        public static async Task ForEachAsync<T>( IEnumerable<T> items, Func<T, Task> process, int maxConcurrency = 5, CancellationToken cancellationToken = default )
        {
            var messages = new List<string>();
            var tasks = new List<Task>();
            
            using var concurrencySemaphore = new SemaphoreSlim( maxConcurrency );

            foreach ( var item in items )
            {
                if ( cancellationToken.IsCancellationRequested )
                {
                    break;
                }

                await concurrencySemaphore.WaitAsync( cancellationToken );

                var t = await Task.Factory.StartNew( async () =>
                {
                    try
                    {
                        await process( item );
                    }
                    finally
                    {
                        concurrencySemaphore.Release();
                    }
                } );

                tasks.Add( t );
            }

            await Task.WhenAll( tasks.ToArray() );
        }
    }
}
