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

using Newtonsoft.Json.Linq;

using Rock.Mobile;

namespace Rock.Tests.Mobile
{
    /// <summary>
    /// Checks the skip list rule that can be checked without a database: the control panel
    /// may only read and write settings on the list. The builder's half, refusing to write a
    /// key on the list, needs a database and is covered by running Repair.
    /// </summary>
    [TestClass]
    public class PlatformMobileAppChurchSettingsTests
    {
        private static readonly Guid ContentCollectionViewBlockGuid = SystemGuid.PlatformMobileApp.Block.CONTENT_COLLECTION_VIEW.AsGuid();

        [TestMethod]
        public void IsChurchOwned_ContentCollection_IsTrue()
        {
            Assert.IsTrue( PlatformMobileAppChurchSettings.IsChurchOwned( ContentCollectionViewBlockGuid, PlatformMobileAppChurchSettings.BlockKey.ContentCollection ) );
        }

        [TestMethod]
        public void IsChurchOwned_PlatformSettingOnSameBlock_IsFalse()
        {
            Assert.IsFalse( PlatformMobileAppChurchSettings.IsChurchOwned( ContentCollectionViewBlockGuid, "SearchOnLoad" ) );
        }

        [TestMethod]
        public void IsChurchOwned_BlockNotOnList_IsFalse()
        {
            Assert.IsFalse( PlatformMobileAppChurchSettings.IsChurchOwned( SystemGuid.PlatformMobileApp.Block.HOME_MENU.AsGuid(), "Content" ) );
        }

        [TestMethod]
        public void SaveBlockValue_PlatformSetting_IsRefused()
        {
            // Refused before any database access, so no context is needed.
            Assert.ThrowsExactly<InvalidOperationException>( () =>
                PlatformMobileAppChurchSettings.SaveBlockValue( SystemGuid.PlatformMobileApp.Block.HOME_MENU.AsGuid(), "Content", "x", null ) );
        }

        [TestMethod]
        public void SaveBranding_WritesBothPrimaryAndBrandPairs()
        {
            var site = new Rock.Model.Site { AdditionalSettings = "{}" };

            PlatformMobileAppChurchSettings.SaveBranding( site, new PlatformMobileAppBranding
            {
                LightLogoBinaryFileId = 11,
                DarkLogoBinaryFileId = 12,
                ColorStrong = "#123456",
                ColorSoft = "#abcdef"
            } );

            // Read the stored JSON directly; the test project does not reference the style assembly.
            var settings = JObject.Parse( site.AdditionalSettings );
            var colors = settings["DownhillSettings"]["ApplicationColors"];

            Assert.AreEqual( "#123456", ( string ) colors["PrimaryStrong"] );
            Assert.AreEqual( "#123456", ( string ) colors["BrandStrong"] );
            Assert.AreEqual( "#abcdef", ( string ) colors["PrimarySoft"] );
            Assert.AreEqual( "#abcdef", ( string ) colors["BrandSoft"] );
            Assert.AreEqual( 11, site.FavIconBinaryFileId );
            Assert.AreEqual( 12, ( int? ) settings["DarkFavIconBinaryFileId"] );
        }

        [TestMethod]
        public void EnsureSiteUnchanged_ChangedColor_Throws()
        {
            var site = new Rock.Model.Site { AdditionalSettings = "{}" };
            var snapshot = PlatformMobileAppChurchSettings.GetSiteSnapshot( site );

            PlatformMobileAppChurchSettings.SaveBranding( site, new PlatformMobileAppBranding { ColorStrong = "#000000", ColorSoft = "#ffffff" } );

            Assert.ThrowsExactly<InvalidOperationException>( () => PlatformMobileAppChurchSettings.EnsureSiteUnchanged( snapshot, site ) );
        }

        [TestMethod]
        public void EnsureSiteUnchanged_MissingStyleSettingsFilledWithDefaults_DoesNotThrow()
        {
            // The builder creates missing style settings; their default colors are not a church change.
            var site = new Rock.Model.Site { AdditionalSettings = @"{ ""DownhillSettings"": null }" };
            var snapshot = PlatformMobileAppChurchSettings.GetSiteSnapshot( site );

            site.AdditionalSettings = @"{ ""DownhillSettings"": {} }";

            PlatformMobileAppChurchSettings.EnsureSiteUnchanged( snapshot, site );
        }

        [TestMethod]
        public void GetBlockValue_PlatformSetting_IsRefused()
        {
            Assert.ThrowsExactly<InvalidOperationException>( () =>
                PlatformMobileAppChurchSettings.GetBlockValue( ContentCollectionViewBlockGuid, "SearchOnLoad", null ) );
        }
    }
}
