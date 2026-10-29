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

using Rock.Web.UI;

namespace Rock.Tests.Web.UI
{
    /// <summary>
    /// Tests for the impersonation redirect marker helpers on <see cref="RockPage"/>
    /// that stop the post sign in reload from looping for clients without cookies.
    /// </summary>
    [TestClass]
    public class RockPageImpersonationRedirectTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset( 2026, 10, 29, 12, 0, 0, TimeSpan.Zero );

        private static readonly long NowSeconds = Now.ToUnixTimeSeconds();

        #region AddImpersonationRedirectMarker

        [TestMethod]
        public void Add_UrlWithQuery_AppendsMarkerWithAmpersand()
        {
            var url = RockPage.AddImpersonationRedirectMarker( "/page/12?rckipid=abc!2Fdef", Now );

            Assert.AreEqual( $"/page/12?rckipid=abc!2Fdef&ImpersonationRedirect={NowSeconds}", url );
        }

        [TestMethod]
        public void Add_UrlWithoutQuery_AppendsMarkerWithQuestionMark()
        {
            var url = RockPage.AddImpersonationRedirectMarker( "/MyAccount/abc!2Fdef", Now );

            Assert.AreEqual( $"/MyAccount/abc!2Fdef?ImpersonationRedirect={NowSeconds}", url );
        }

        [TestMethod]
        public void Add_UrlAlreadyHasMarker_ReplacesMarker()
        {
            var url = RockPage.AddImpersonationRedirectMarker( "/page/12?ImpersonationRedirect=5&rckipid=abc", Now );

            Assert.AreEqual( $"/page/12?rckipid=abc&ImpersonationRedirect={NowSeconds}", url );
        }

        [TestMethod]
        public void Add_UrlEndingInQuestionMark_DoesNotDoubleSeparator()
        {
            var url = RockPage.AddImpersonationRedirectMarker( "/page/12?", Now );

            Assert.AreEqual( $"/page/12?ImpersonationRedirect={NowSeconds}", url );
        }

        [TestMethod]
        public void Add_ThenRead_IsFresh()
        {
            var url = RockPage.AddImpersonationRedirectMarker( "/page/12?rckipid=abc", Now );
            var prefix = RockPage.ImpersonationRedirectParameterName + "=";
            var value = url.Substring( url.IndexOf( prefix, StringComparison.Ordinal ) + prefix.Length );

            Assert.IsTrue( RockPage.IsImpersonationRedirectMarkerFresh( value, Now ) );
        }

        #endregion AddImpersonationRedirectMarker

        #region IsImpersonationRedirectMarkerFresh

        [TestMethod]
        public void IsFresh_SameTime_ReturnsTrue()
        {
            Assert.IsTrue( RockPage.IsImpersonationRedirectMarkerFresh( NowSeconds.ToString(), Now ) );
        }

        [TestMethod]
        public void IsFresh_FewSecondsOld_ReturnsTrue()
        {
            Assert.IsTrue( RockPage.IsImpersonationRedirectMarkerFresh( ( NowSeconds - 5 ).ToString(), Now ) );
        }

        [TestMethod]
        public void IsFresh_AtWindowEdge_ReturnsTrue()
        {
            Assert.IsTrue( RockPage.IsImpersonationRedirectMarkerFresh( ( NowSeconds - 60 ).ToString(), Now ) );
        }

        [TestMethod]
        public void IsFresh_Stale_ReturnsFalse()
        {
            Assert.IsFalse( RockPage.IsImpersonationRedirectMarkerFresh( ( NowSeconds - 61 ).ToString(), Now ) );
            Assert.IsFalse( RockPage.IsImpersonationRedirectMarkerFresh( ( NowSeconds - 86400 ).ToString(), Now ) );
        }

        [TestMethod]
        public void IsFresh_SlightlyInFuture_ReturnsTrue()
        {
            // Web farm nodes may have slightly different clocks.
            Assert.IsTrue( RockPage.IsImpersonationRedirectMarkerFresh( ( NowSeconds + 10 ).ToString(), Now ) );
        }

        [TestMethod]
        public void IsFresh_FarInFuture_ReturnsFalse()
        {
            Assert.IsFalse( RockPage.IsImpersonationRedirectMarkerFresh( ( NowSeconds + 61 ).ToString(), Now ) );
            Assert.IsFalse( RockPage.IsImpersonationRedirectMarkerFresh( long.MaxValue.ToString(), Now ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "True" )]
        [DataRow( "abc" )]
        [DataRow( "-5" )]
        [DataRow( "+5" )]
        [DataRow( "1.5" )]
        [DataRow( "1,000" )]
        [DataRow( "0x10" )]
        [DataRow( "99999999999999999999999" )]
        public void IsFresh_GarbageValue_ReturnsFalse( string value )
        {
            Assert.IsFalse( RockPage.IsImpersonationRedirectMarkerFresh( value, Now ) );
        }

        [TestMethod]
        public void IsFresh_Zero_ReturnsFalse()
        {
            Assert.IsFalse( RockPage.IsImpersonationRedirectMarkerFresh( "0", Now ) );
        }

        #endregion IsImpersonationRedirectMarkerFresh

        #region RemoveImpersonationRedirectMarker

        [TestMethod]
        public void Remove_MarkerLast_KeepsOtherParameters()
        {
            var url = RockPage.RemoveImpersonationRedirectMarker( "/page/12?rckipid=abc&GroupId=4&ImpersonationRedirect=123" );

            Assert.AreEqual( "/page/12?rckipid=abc&GroupId=4", url );
        }

        [TestMethod]
        public void Remove_MarkerFirst_KeepsOtherParameters()
        {
            var url = RockPage.RemoveImpersonationRedirectMarker( "/page/12?ImpersonationRedirect=123&rckipid=abc&GroupId=4" );

            Assert.AreEqual( "/page/12?rckipid=abc&GroupId=4", url );
        }

        [TestMethod]
        public void Remove_MarkerOnly_DropsQuestionMark()
        {
            var url = RockPage.RemoveImpersonationRedirectMarker( "/MyAccount/abc!2Fdef?ImpersonationRedirect=123" );

            Assert.AreEqual( "/MyAccount/abc!2Fdef", url );
        }

        [TestMethod]
        public void Remove_EncodedValues_KeepsEncodingAsIs()
        {
            var url = RockPage.RemoveImpersonationRedirectMarker( "/page/12?rckipid=a!2Fb!3D&ReturnUrl=%2Fpage%3Fx%3D1&Name=A+B&ImpersonationRedirect=123" );

            Assert.AreEqual( "/page/12?rckipid=a!2Fb!3D&ReturnUrl=%2Fpage%3Fx%3D1&Name=A+B", url );
        }

        [TestMethod]
        public void Remove_DifferentCaseAndEncodedKey_RemovesAll()
        {
            var url = RockPage.RemoveImpersonationRedirectMarker( "/page/12?impersonationredirect=1&rckipid=abc&Impersonation%52edirect=2&IMPERSONATIONREDIRECT" );

            Assert.AreEqual( "/page/12?rckipid=abc", url );
        }

        [TestMethod]
        public void Remove_SimilarKeys_AreKept()
        {
            const string original = "/page/12?ImpersonationRedirectX=1&XImpersonationRedirect=2&Value=ImpersonationRedirect%3D3";

            Assert.AreEqual( original, RockPage.RemoveImpersonationRedirectMarker( original ) );
        }

        [TestMethod]
        public void Remove_NoMarker_ReturnsSameUrl()
        {
            const string original = "/page/12?rckipid=abc&GroupId=4";

            Assert.AreEqual( original, RockPage.RemoveImpersonationRedirectMarker( original ) );
            Assert.AreEqual( "/page/12", RockPage.RemoveImpersonationRedirectMarker( "/page/12" ) );
        }

        [TestMethod]
        public void Remove_NullOrEmpty_ReturnsInput()
        {
            Assert.IsNull( RockPage.RemoveImpersonationRedirectMarker( null ) );
            Assert.AreEqual( string.Empty, RockPage.RemoveImpersonationRedirectMarker( string.Empty ) );
        }

        [TestMethod]
        public void AddThenRemove_ReturnsOriginalUrl()
        {
            const string original = "/MyAccount/a!2Fb?GroupId=4&ReturnUrl=%2Fx";

            var withMarker = RockPage.AddImpersonationRedirectMarker( original, Now );

            Assert.AreEqual( original, RockPage.RemoveImpersonationRedirectMarker( withMarker ) );
        }

        #endregion RemoveImpersonationRedirectMarker
    }
}
