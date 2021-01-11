// This is sourced from Microsoft here:
// https://github.com/aspnet/AspNetIdentity/blob/a24b776676f12cf7f0e13944783cf8e379b3ef70/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// This has helper methods that allow you to call an asynchronous method synchronously.
    /// </summary>
    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default );

        /// <summary>
        /// Runs the synchronize.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>( Func<Task<TResult>> func )
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew( () =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            } ).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs the synchronize.
        /// </summary>
        /// <param name="func">The function.</param>
        public static void RunSync( Func<Task> func )
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew( () =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            } ).Unwrap().GetAwaiter().GetResult();
        }
    }
}
