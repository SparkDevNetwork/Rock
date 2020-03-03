using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DotLiquid;
using Xunit;

namespace Rock.Tests.DotLiquidTests
{
    public class TemplateTests
    {
        [Theory( Skip = "Outcome dependent upon system resources" )]
        [InlineData(false)]
        [InlineData(true)]
        public void Render_IsThreadSafePropertyValue_DictatesThreadSafety( bool isThreadSafe )
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
                        Assert.Equal( expected[i % 2], template.Render( Hash.FromAnonymousObject( input[i % 2] ) ) );
                    }
                    catch
                    {
                        failures.Add( DateTime.Now );
                    }
                }
            );

            if ( !isThreadSafe )
            {
                Assert.True( failures.Count > 0 );
            }
            else
            {
                Assert.True( failures.Count == 0 );
            }
        }
    }
}
