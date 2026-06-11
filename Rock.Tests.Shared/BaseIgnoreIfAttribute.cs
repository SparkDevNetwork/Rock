using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DoNotParallelize]

namespace Rock.Tests.Shared
{
    /// <summary>
    /// Base implementation for attributes that can be used along with
    /// <see cref="ConditionalTestMethodAttribute"/> to conditionally ignore
    /// a test.
    /// </summary>
    public abstract class BaseIgnoreIfAttribute : System.Attribute
    {
        /// <summary>
        /// The details as to why the test was ignored/skipped.
        /// </summary>
        public string SkipDetails { get; protected set; }

        /// <summary>
        /// Determines if the test should be executed. If this returns false,
        /// the test will be skipped.
        /// </summary>
        /// <param name="testMethod">The method that is going to be tested.</param>
        /// <returns><c>true</c> if the test should execute, otherwise <c>false</c>.</returns>
        internal abstract bool ShouldExecute( ITestMethod testMethod );
    }
}
