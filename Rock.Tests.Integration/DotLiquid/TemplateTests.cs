using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.DotLiquidTests
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        [Ignore( "Outcome dependent upon system resources" )]
        public void Render_MakeThreadSafeMethod_ProvidesThreadSafety()
        {
            Render_IsThreadSafePropertyValue_DictatesThreadSafety( true );
        }

        [TestMethod]
        [Ignore( "Outcome dependent upon system resources" )]
        public void Render_MakeThreadSafeMethod_IgnoresThreadSafety()
        {
            Render_IsThreadSafePropertyValue_DictatesThreadSafety( false );
        }

        private void Render_IsThreadSafePropertyValue_DictatesThreadSafety( bool isThreadSafe )
        {
            // This test is based on the following:
            // issue: https://github.com/dotliquid/dotliquid/issues/206
            // fix: https://github.com/dotliquid/dotliquid/pull/220

            // Arrange
            var template = Template.Parse( "{% assign value = Value %}{{ value }}" );

            if ( isThreadSafe )
            {
                template.MakeThreadSafe();
            }

            var input = new[]
            {
                new { Value = 1 },
                new { Value = 2 }
            };

            var expected = new[]
            {
                template.Render( Hash.FromAnonymousObject( input[0] ) ),
                template.Render( Hash.FromAnonymousObject( input[1] ) ),
            };

            // Act & Assert
            var failures = new ConcurrentBag<DateTime>();

            Parallel.For( 0, 2000, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
                {
                    try
                    {
                        Assert.That.Equal( expected[i % 2], template.Render( Hash.FromAnonymousObject( input[i % 2] ) ) );
                    }
                    catch
                    {
                        failures.Add( DateTime.Now );
                    }
                }
            );

            if ( !isThreadSafe )
            {
                Assert.That.True( failures.Count > 0 );
            }
            else
            {
                Assert.That.True( failures.Count == 0 );
            }
        }
    }
}
