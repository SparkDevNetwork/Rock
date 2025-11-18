using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// A test method attribute that will conditionally skip a test if one or more
    /// [IgnoreIf] type attributes specify it should be skipped at runtime.
    /// </summary>
    public class ConditionalTestMethodAttribute : TestMethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalTestMethodAttribute"/> class.
        /// </summary>
        public ConditionalTestMethodAttribute( [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1 )
            : base( callerFilePath, callerLineNumber )
        {
        }

        /// <inheritdoc/>
        public override Task<TestResult[]> ExecuteAsync( ITestMethod testMethod )
        {
            if ( !ShouldExecute( testMethod, out var message ) )
            {
                return Task.FromResult( new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        TestFailureException = new AssertInconclusiveException( message ),
                    }
                } );
            }

            return base.ExecuteAsync( testMethod );
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
            foreach ( var attribute in testMethod.GetAttributes<BaseIgnoreIfAttribute>() )
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
