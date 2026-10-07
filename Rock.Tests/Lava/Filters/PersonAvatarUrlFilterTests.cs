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

using Rock.Lava;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    public class PersonAvatarUrlFilterTests
    {
        private const string Query = "GetAvatar.ashx?fileIdKey=Z9NB6v3Bo0&e=2026-10-13T00%3A00%3A00&t=abc";

        [TestMethod]
        public void ResolveAvatarUrl_SiteRelativeUrl_IsUnchanged()
        {
            var url = LavaFilters.ResolveAvatarUrl( $"/{Query}", "/", null );

            Assert.AreEqual( $"/{Query}", url );
        }

        [TestMethod]
        public void ResolveAvatarUrl_VirtualPath_ResolvesToSiteRoot()
        {
            var url = LavaFilters.ResolveAvatarUrl( $"~/{Query}", "/", null );

            Assert.AreEqual( $"/{Query}", url );
        }

        [TestMethod]
        public void ResolveAvatarUrl_VirtualPathUnderAppFolder_ResolvesToAppFolder()
        {
            var url = LavaFilters.ResolveAvatarUrl( $"~/{Query}", "/rock", null );

            Assert.AreEqual( $"/rock/{Query}", url );
        }

        [TestMethod]
        public void ResolveAvatarUrl_MissingVirtualRoot_ResolvesToSiteRoot()
        {
            var url = LavaFilters.ResolveAvatarUrl( $"~/{Query}", null, null );

            Assert.AreEqual( $"/{Query}", url );
        }

        [TestMethod]
        public void ResolveAvatarUrl_WithPublicRoot_IsAbsolute()
        {
            var url = LavaFilters.ResolveAvatarUrl( $"/{Query}", "/", "https://www.example.org/" );

            Assert.AreEqual( $"https://www.example.org/{Query}", url );
        }

        [TestMethod]
        public void ResolveAvatarUrl_VirtualPathWithPublicRoot_IsAbsolute()
        {
            var url = LavaFilters.ResolveAvatarUrl( $"~/{Query}", "/", "https://www.example.org" );

            Assert.AreEqual( $"https://www.example.org/{Query}", url );
        }
    }
}
