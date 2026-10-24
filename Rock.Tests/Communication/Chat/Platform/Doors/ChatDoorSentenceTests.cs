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
using System.Globalization;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Doors;

namespace Rock.Tests.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The doors' refusal sentences that spell out a limit as a word.
    /// </summary>
    /// <remarks>
    /// The sentences are English, as every other chat sentence is, so the number in them must be
    /// an English word whatever culture the Rock server runs in.
    /// </remarks>
    [TestClass]
    public class ChatDoorSentenceTests
    {
        [TestMethod]
        public void TheGroupSizeSentencesAreEnglishUnderAnotherCulture()
        {
            var english = Under( "en-US", () => new[] { ChatDoorHelper.ChooseBetweenSentence(), ChatDoorHelper.ConversationHoldsAtMostSentence() } );
            var spanish = Under( "es-ES", () => new[] { ChatDoorHelper.ChooseBetweenSentence(), ChatDoorHelper.ConversationHoldsAtMostSentence() } );

            Assert.AreEqual( english[0], spanish[0], "the add's size refusal" );
            Assert.AreEqual( english[1], spanish[1], "the conversation's size refusal" );
            StringAssert.StartsWith( spanish[0], "Choose between one and " );
            StringAssert.StartsWith( spanish[1], "A conversation holds at most " );
        }

        /// <summary>
        /// Runs a step with the thread's culture and UI culture set to the named one, as a request
        /// on a server in that culture would.
        /// </summary>
        /// <param name="cultureName">The culture to run under.</param>
        /// <param name="step">The step.</param>
        /// <returns>What the step answered.</returns>
        private static string[] Under( string cultureName, Func<string[]> step )
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            var uiCulture = Thread.CurrentThread.CurrentUICulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo( cultureName );
                Thread.CurrentThread.CurrentUICulture = new CultureInfo( cultureName );
                return step();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = uiCulture;
            }
        }
    }
}
