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

using Rock.Net;

namespace Rock.Tests.Net
{
    [TestClass]
    public class CrawlerUserAgentsTests
    {
        /// <summary>
        /// Guards the packaging step. If the crawler-user-agents.json embedded
        /// resource is dropped from Rock.csproj or fails to parse, crawler
        /// detection silently falls back to the legacy keyword list and most
        /// bots start being recorded as page views again. This test is the only
        /// thing that makes that failure loud.
        /// </summary>
        [TestMethod]
        public void EmbeddedDataset_IsLoaded()
        {
            Assert.IsFalse( CrawlerUserAgents.IsUsingFallbackList, "The crawler-user-agents.json embedded resource could not be loaded, so crawler detection has fallen back to the legacy keyword list. Check the EmbeddedResource entry in Rock.csproj." );
            Assert.IsGreaterThan( 1000, CrawlerUserAgents.PatternCount, $"Expected the crawler dataset to contain more than 1000 patterns but found {CrawlerUserAgents.PatternCount}." );
        }

        [TestMethod]
        [DataRow( "Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)" )]
        [DataRow( "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)" )]
        [DataRow( "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko); compatible; GPTBot/1.0; +https://openai.com/gptbot" )]
        [DataRow( "Mozilla/5.0 (compatible; ClaudeBot/1.0; +claudebot@anthropic.com)" )]
        [DataRow( "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/126.0.0.0 Safari/537.36" )]
        [DataRow( "python-requests/2.31.0" )]
        [DataRow( "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Chrome-Lighthouse" )]
        public void IsCrawler_ReturnsTrueForKnownCrawlers( string userAgent )
        {
            Assert.IsTrue( CrawlerUserAgents.IsCrawler( userAgent ), $"Expected to be detected as a crawler: {userAgent}" );
        }

        [TestMethod]
        // A CUBOT phone. The legacy expression's bare "bot" token matched this.
        [DataRow( "Mozilla/5.0 (Linux; Android 10; CUBOT_X30 Build/QP1A.190711.020) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Mobile Safari/537.36" )]
        [DataRow( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36" )]
        [DataRow( "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.5 Mobile/15E148 Safari/604.1" )]
        [DataRow( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0" )]
        [DataRow( "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.5 Safari/605.1.15" )]
        [DataRow( "" )]
        [DataRow( null )]
        public void IsCrawler_ReturnsFalseForHumanTraffic( string userAgent )
        {
            Assert.IsFalse( CrawlerUserAgents.IsCrawler( userAgent ), $"Expected NOT to be detected as a crawler: {userAgent}" );
        }
    }
}
