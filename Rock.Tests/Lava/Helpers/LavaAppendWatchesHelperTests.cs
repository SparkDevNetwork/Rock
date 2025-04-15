// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava.Helpers;
using Rock.Tests.Shared;

namespace Rock.Tests.Lava.Helpers
{
    [TestClass]
    public class LavaAppendWatchesHelperTests
    {
        [TestMethod]
        public void GetResumeInformation_WithEmptyString_ReturnsZero()
        {
            var (resumePercentage, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( string.Empty );

            Assert.That.AreEqual( 0, resumePercentage );
            Assert.That.AreEqual( 0, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithNull_ReturnsZero()
        {
            var (resumePercentage, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( null );

            Assert.That.AreEqual( 0, resumePercentage );
            Assert.That.AreEqual( 0, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithEmptySegment_SkipsSegment()
        {
            var watchMap = "851,222,,4601,5720";
            var (_, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 567, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithInvalidCount_SkipsSegment()
        {
            var watchMap = "851,222,23X,4601,5720";
            var (_, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 567, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithInvalidSeconds_SkipsSegment()
        {
            var watchMap = "851,222,XX1,4601,5720";
            var (_, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 567, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithFullyWatchedVideo_ReturnsZeroResumeSeconds()
        {
            var watchMap = "851,222,4601,5722";
            var (_, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 0, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithFullyWatchedVideo_Returns100ResumePercentage()
        {
            var watchMap = "851,222,4601,5722";
            var (resumePercentage, _) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 100, resumePercentage);
        }

        [TestMethod]
        public void GetResumeInformation_WithPartiallyWatchedVideo_ReturnsExpectedResumeSeconds()
        {
            var watchMap = "851,222,4601,5720";
            var (_, resumeLocationInSeconds) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 567, resumeLocationInSeconds );
        }

        [TestMethod]
        public void GetResumeInformation_WithPartiallyWatchedVideo_ReturnsExpectedResumePercentage()
        {
            var watchMap = "851,222,4601,5720";
            var (resumePercentage, _) = LavaAppendWatchesHelper.GetResumeInformation( watchMap );

            Assert.That.AreEqual( 49.78, Math.Round( resumePercentage, 2 ) );
        }
    }
}
// 851,222,4601,5720

