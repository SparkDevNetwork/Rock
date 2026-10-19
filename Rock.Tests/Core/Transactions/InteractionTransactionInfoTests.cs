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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Transactions;

namespace Rock.Tests.Core.Transactions
{
    /// <summary>
    /// Tests for <see cref="InteractionTransactionInfo"/>.
    /// </summary>
    [TestClass]
    public class InteractionTransactionInfoTests
    {
        /// <summary>
        /// A page title that a block HTML encoded (such as the Person Bio block's
        /// person name) is stored as plain text, after the site name is removed.
        /// </summary>
        [TestMethod]
        [DataRow( "Sean O&#39;Brien", "Sean O'Brien" )]
        [DataRow( "Jo &amp; Al", "Jo & Al" )]
        [DataRow( "Jos&#233;", "José" )]
        [DataRow( "&lt;b&gt;Name&lt;/b&gt;", "<b>Name</b>" )]
        [DataRow( "Jo &amp; Al | Rock RMS", "Jo & Al" )]
        public void GetInteractionSummaryFromPageTitle_EncodedTitle_ReturnsPlainText( string title, string expected )
        {
            Assert.AreEqual( expected, InteractionTransactionInfo.GetInteractionSummaryFromPageTitle( title ) );
        }

        /// <summary>
        /// Plain titles are unchanged, and the site name is still removed.
        /// </summary>
        [TestMethod]
        [DataRow( "Q&A", "Q&A" )]
        [DataRow( "People", "People" )]
        [DataRow( "Welcome | Rock RMS", "Welcome" )]
        [DataRow( "AT&T", "AT&T" )]
        [DataRow( "", "" )]
        [DataRow( "   ", "   " )]
        [DataRow( null, null )]
        public void GetInteractionSummaryFromPageTitle_PlainTitle_IsUnchangedExceptSiteName( string title, string expected )
        {
            Assert.AreEqual( expected, InteractionTransactionInfo.GetInteractionSummaryFromPageTitle( title ) );
        }
    }
}
