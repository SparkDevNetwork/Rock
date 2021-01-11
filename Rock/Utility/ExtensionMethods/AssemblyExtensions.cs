using System;
using System.Collections.Generic;
using System.Reflection;

using Rock.Model;

namespace Rock.Utility.ExtensionMethods
{
    /// <summary>
    /// Assembly Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the types from an Assembly using System.Reflection.Assembly.GetTypes. If an excepion occurs then it is logged and an empty array is returned.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static Type[] GetTypesSafe( this Assembly assembly )
        {
            try
            {
                return assembly.GetTypes();
            }
            catch ( ReflectionTypeLoadException ex )
            {
                var exceptions = new List<Exception>();

                // The stacktrace in ex will not be complete, so add the current one to the message.
                exceptions.Add( new Exception( $@"{ex.Message}{Environment.NewLine}StackTrace: {Environment.StackTrace}", ex ) );
                
                foreach ( var e in ex.LoaderExceptions )
                {
                    exceptions.Add( e );
                }

                ExceptionLogService.LogException( new AggregateException( $"Error getting Types for assembly {assembly.FullName}", exceptions ) );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Error getting Types for assembly {assembly.FullName}", ex ) );
            }

            return new Type[] { };
        }
    }
}
