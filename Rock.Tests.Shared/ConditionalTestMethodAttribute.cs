using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// A test method attribute that will conditionally skip a test if one or more
    /// [IgnoreIf] type attributes specify it should be skipped at runtime.
    /// </summary>
    public class ConditionalTestMethodAttribute : TestMethodAttribute
    {
        /// <inheritdoc/>
        public override TestResult[] Execute( ITestMethod testMethod )
        {
            if ( !ShouldExecute( testMethod, out var message ) )
            {
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        TestFailureException = new AssertInconclusiveException( message ),
                    }
                };
            }

            return base.Execute( testMethod );
        }

        /// <summary>
        /// Determines if the test should be executed. This will evaluate all
        /// attributes that are applied to the test or class.
        /// </summary>
        /// <param name="testMethod">The test method to be evaluated.</param>
        /// <param name="message">The message for why the test was skipped if <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the test method should be executed, <c>false</c> otherwise.</returns>
        private bool ShouldExecute( ITestMethod testMethod, out string message )
        {
            foreach ( var attribute in testMethod.GetAttributes<BaseIgnoreIfAttribute>( inherit: true ) )
            {
                if ( !attribute.ShouldExecute( testMethod ) )
                {
                    message = attribute.SkipDetails;
                    return false;
                }
            }

            foreach ( var attribute in testMethod.MethodInfo.ReflectedType.GetCustomAttributes<BaseIgnoreIfAttribute>( inherit: true ) )
            {
                if ( !attribute.ShouldExecute( testMethod ) )
                {
                    message = attribute.SkipDetails;
                    return false;
                }
            }

            message = null;
            return true;
        }
    }
}
