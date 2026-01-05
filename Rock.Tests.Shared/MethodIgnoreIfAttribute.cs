using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// An attribute that can be applied to a test method to conditionally
    /// ignore it based on the result of a static method. The method should
    /// return <c>true</c> to run the test or <c>false</c> to skip the test.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true )]
    public class MethodIgnoreIfAttribute : BaseIgnoreIfAttribute
    {
        /// <summary>
        /// The name of the method to call to determine if the test should run.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodIgnoreIfAttribute"/> class.
        /// </summary>
        /// <param name="methodName">The name of the static method to call to determine if the test should be ignored.</param>
        /// <param name="skipDetails">The details about why the test was skipped.</param>
        public MethodIgnoreIfAttribute( string methodName, string skipDetails = null )
        {
            MethodName = methodName;
            SkipDetails = skipDetails ?? "Conditional test run was prevented.";
        }

        /// <inheritdoc/>
        internal override bool ShouldExecute( ITestMethod testMethod )
        {
            var method = testMethod.MethodInfo.DeclaringType.GetMethod( MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic );

            if ( method == null || method.ReturnType != typeof( bool ) || method.GetParameters().Length > 0 )
            {
                throw new InvalidOperationException( $"The conditional method '{MethodName}' specified in the '{GetType().Name}' attribute was not found or is not valid. This method must be declared static, return a boolean and take no parameters." );
            }

            return ( bool ) method.Invoke( null, null );
        }
    }
}
