﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rock;
using Rock.Data;
using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode.
    /// NOTE on IDisposable: We'd like to be able to use IDisposble to perform automatic Cleanup() after
    /// each test method but we can't do this until we can safely run tests that have a db (ie, we don't
    /// want to break other teams CI environments that are running these tests w/o a db.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class AttendanceCodeTests : IDisposable
    {
        private static List<string> noGood = new List<string> {
            "4NL", "4SS", "5CK", "5HT", "5LT", "5NM", "5TD", "5XX", "666", "BCH", "CLT", "CNT", "D4M", "D5H", "DCK", "DMN", "DSH", "F4G", "FCK", "FGT", "G4Y", "GZZ", "H8R",
            "JNK", "JZZ", "KKK", "KLT", "KNT", "L5D", "LCK", "LSD", "MFF", "MLF", "ND5", "NDS", "NDZ", "NGR", "P55", "PCP", "PHC", "PHK", "PHQ", "PM5", "PMS", "PN5", "PNS",
            "PRC", "PRK", "PRN", "PRQ", "PSS", "RCK", "SCK", "S3X", "SHT", "SLT", "SNM", "STD", "SXX", "THC", "V4G", "WCK", "XTC", "XXX", "911",
            "1XL", "2XL", "3XL", "4XL", "5XL", "6XL", "7XL", "8XL", "9XL", "XXL"
        };

        /// <summary>
        /// Setup test which cleans the AttendanceCode table for these tests.
        /// </summary>
        public AttendanceCodeTests()
        {
            // Someday when we can run these tests on all systems without the [Skip...
            // we can uncomment these cleanup calls, and remove the Cleanup calls
            // from each test below.
            //Cleanup();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Someday when we can run these tests on all systems without the [Skip...
            // we can uncomment these cleanup calls, and remove the Cleanup calls
            // from each test below.
            //Cleanup();
        }

        /// <summary>
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        private void Cleanup()
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new AttendanceCodeService( rockContext );

                DateTime today = RockDateTime.Today;
                DateTime tomorrow = today.AddDays( 1 );
                var todaysCodes = service.Queryable()
                        .Where( c => c.IssueDateTime >= today && c.IssueDateTime < tomorrow )
                        .ToList();
                if ( todaysCodes.Any() )
                {
                    service.DeleteRange( todaysCodes );
                    rockContext.SaveChanges();
                }
            }
            AttendanceCodeService.FlushTodaysCodes();
        }

        #region Tests that don't require a database/context

        /// <summary>
        /// Avoids the triple six.  Note: Does not use the database.
        /// </summary>
        [Fact]
        public void AvoidTripleSix()
        {
            int alphaNumericLength = 0;
            int alphaLength = 0;
            int numericLength = 4;
            bool isRandomized = false;
            string lastCode = "0665";

            string code = AttendanceCodeService.GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
            Assert.Equal( "0667", code );
        }

        #endregion

        #region Alpha-numeric codes

        [Fact( Skip = "Requires a db" )]
        public void AlphaNumericCodesShouldSkipBadCodes()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 6000; i++ )
            {
                code = AttendanceCodeService.GetNew( 3, 0, 0, false );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => noGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.False( hasMatchIsBad );
        }

        #endregion

        #region Numeric only codes

        [Fact( Skip = "Requires a db" )]
        public void CheckThreeChar002Code()
        {
            Cleanup();

            AttendanceCode code = null;
            for ( int i = 0; i < 2; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
            }

            Assert.Equal( "002", code.Code );
        }

        [Fact( Skip = "Requires a db" )]
        public void NumericCodesShouldSkip911And666()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 2000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 4, false );
                codeList.Add( code.Code );
            }

            Assert.DoesNotContain( codeList, s => s == "911" );
            Assert.DoesNotContain( codeList, s => s == "666" );
        }

        /// <summary>
        /// Numeric only code with length of 2 should not go beyond 99.
        /// Attempting to create one should not be allowed so throwing a
        /// timeout exception is acceptable to let the admin know there is a
        /// configuration problem.
        /// </summary>
        [Fact( Skip = "Requires a db" )]
        public void NumericCodeWithLengthOf2ShouldNotGoBeyond99()
        {
            Cleanup();

            try
            {
                var codeList = new List<string>();
                AttendanceCode code = null;
                for ( int i = 0; i < 101; i++ )
                {
                    code = AttendanceCodeService.GetNew( 0, 0, 2, false );
                    codeList.Add( code.Code );
                }

                // should not be longer than 4 characters
                // This is a known bug in v7.4 and earlier, and possibly fixed via PR #3071
                Assert.True( codeList.Last().Length == 4 );
            }
            catch ( TimeoutException )
            {
                // An exception in this case is considered better than hanging (since there is 
                // no actual solution).
                Assert.True( true );
            }
        }

        /// <summary>
        /// Numerics codes should not repeat.  This is/was a known bug in v7.4 and earlier
        /// </summary>
        [Fact( Skip = "Requires a db" )]
        public void NumericCodesShouldNotRepeat()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 999; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
                codeList.Add( code.Code );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.True( duplicates.Count() == 0 );
        }

        /// <summary>
        /// Numerics codes should not repeat.  This is/was a known bug in v7.4 and earlier
        /// </summary>
        [Fact( Skip = "Requires a db" )]
        public void RandomNumericCodesShouldNotRepeat()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 999; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, true );
                codeList.Add( code.Code );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.True( duplicates.Count() == 0 );
        }

        /// <summary>
        /// Requestings the more codes than are possible should throw exception...
        /// because there's really nothing else we could do in that situation, right.
        /// </summary>
        [Fact( Skip = "Requires a db" )]
        public async void RequestingMoreCodesThanPossibleShouldThrowException()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;

            // Generate 99 codes (the maximum number of valid codes).
            for ( int i = 0; i < 100; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 2, true );
                codeList.Add( code.Code );
            }

            // Now try to generate one more...
            try
            {
                // We'll give this test only 30 seconds to complete, otherwise it's considered a failure.
                // We'll prevent this call from hanging even if there is an infinite loop in the GetNew(...) call.
                using ( var source = new CancellationTokenSource() )
                {
                    source.CancelAfter( TimeSpan.FromSeconds( 30 ) );
                    var completionSource = new TaskCompletionSource<object>();
                    source.Token.Register( () => completionSource.TrySetCanceled() );

                    // call the hundredth time (which typically hangs in v7.4 and earlier)
                    var task = Task<AttendanceCode>.Factory.StartNew( () => AttendanceCodeService.GetNew( 0, 0, 2, true ), source.Token );
                    await Task.WhenAny( task, completionSource.Task );

                    // We can't check the task's Result property or else it will block again until there is a result...
                    // ...but there is Result in the impossible case, so we're going to ignore the result anyhow.
                    // code = task.Result; // <-- don't do this

                    // If the task is still running, then it's a fail.
                    if ( task.Status != TaskStatus.RanToCompletion )
                    {
                        // ... and I'd like to abort the task, but I've read this is a bad idea in production
                        // environments so we will call Cancel():
                        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/cancel-async-tasks-after-a-period-of-time
                        source.Cancel();

                        Assert.True( task.Status == TaskStatus.Faulted );
                    }
                    else
                    {
                        // This should never happen. If it does, it means we're not really 
                        // attempting to generate more codes than are possible -- so it's a Fail
                        // too.
                        Assert.True( true );
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                // An exception in this case is considered better than hanging (since there is 
                // no actual solution).
                Assert.True( true );
            }
            catch ( TimeoutException )
            {
                // An exception in this case is considered better than hanging (since there is 
                // no actual solution).
                Assert.True( true );
            }
        }

        [Fact( Skip = "Requires a db" )]
        public void Increment100SequentialNumericCodes()
        {
            Cleanup();

            AttendanceCode code = null;
            for ( int i = 0; i < 100; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
            }

            Assert.Equal( "100", code.Code );
        }

        #endregion

        #region Alpha only codes

        [Fact( Skip = "Requires a db" )]
        public void AlphaOnlyCodesShouldSkipBadCodes()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 1000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 3, 0, true );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => noGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.False( hasMatchIsBad );
        }

        /// <summary>
        /// Alpha codes should not repeat.
        /// </summary>
        [Fact( Skip = "Requires a db" )]
        public void AlphaOnlyCodesShouldNotRepeat()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;

            // 4847 (17*17*17 minus ~50 badcodes) possible combinations of 17 letters
            for ( int i = 0; i < 4860; i++ )
            {
                //System.Diagnostics.Debug.WriteIf( i > 4700, "code number " + i + " took... " );
                code = AttendanceCodeService.GetNew( 0, 3, 0, false );
                codeList.Add( code.Code );
                //System.Diagnostics.Debug.WriteLineIf( i > 4700, "" );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.True( duplicates.Count() == 0 );
        }

        #endregion

        #region Alpha-numeric + numeric only codes

        /// <summary>
        /// NOTE: This appears to be a current bug in v8.0 and earlier.  It cazn only generate 100 codes
        /// Two character alpha numeric codes (codeCharacters) has possible 24*24 (576) combinations
        /// plus two character numeric codes has a possible 10*10 (100) for a total set of
        /// 676 combinations.  Removing the noGood (~60) codes leaves us with a valid set of
        /// 616 codes.
        /// There should be no bad codes in this list either even though
        /// individually each part has no bad codes.
        /// </summary>
        [Fact( Skip = "Requires a db" )]
        public void AlphaNumericWithNumericCodesShouldSkipBadCodes()
        {
            Cleanup();
            int attemptCombination = 0;

            try
            {
                var codeList = new List<string>();
                AttendanceCode code = null;
                for ( int i = 0; i < 676; i++ )
                {
                    attemptCombination = i;
                    code = AttendanceCodeService.GetNew( 2, 0, 2, true );
                    codeList.Add( code.Code );
                }

                bool hasMatchIsBad = codeList.Where( c => noGood.Any( ng => c.Contains( ng ) ) ).Any();

                Assert.False( hasMatchIsBad );

            }
            catch( TimeoutException )
            {
                // If an infinite loop was detected, but we tried at least 616 codes then
                // we'll consider this a pass.
                Assert.True( attemptCombination >= 616 );
            }
        }

        #endregion

        #region Alpha only + numeric only codes

        [Fact( Skip = "Requires a db" )]
        public void AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes()
        {
            Cleanup();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 6000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 3, 4, true );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => noGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.False( hasMatchIsBad );
        }
        
        #endregion

    }
}
