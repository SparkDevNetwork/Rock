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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Mobile
{
    /// <summary>
    /// The skip list: the one table of church owned settings on the platform mobile
    /// application. <see cref="PlatformMobileAppBuilder"/> never writes a setting on this
    /// list, and the control panel writes only settings on it, so the two can never collide.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Church owned values live where a normal Rock Mobile app keeps them. A church's pick
    /// for a block setting is that block's own attribute value, written with the same value
    /// format the block settings UI would write. The logos and the color pair live on the
    /// Site and in its settings blob, where the normal Styles tab writes them.
    /// </para>
    /// <para>
    /// Build leaves every setting here empty, the way a new Rock Mobile app starts. The
    /// screens stay empty until the church picks in the control panel.
    /// </para>
    /// </remarks>
    internal static class PlatformMobileAppChurchSettings
    {
        #region Fields

        /// <summary>
        /// The church owned block settings: attribute keys by registry block Guid.
        /// </summary>
        private static readonly IReadOnlyDictionary<Guid, IReadOnlyList<string>> BlockAttributeKeys = new Dictionary<Guid, IReadOnlyList<string>>
        {
            [SystemGuid.PlatformMobileApp.Block.CONTENT_COLLECTION_VIEW.AsGuid()] = new[] { BlockKey.ContentCollection }
        };

        #endregion Fields

        #region Keys

        /// <summary>
        /// The attribute keys of church owned block settings.
        /// </summary>
        internal static class BlockKey
        {
            /// <summary>
            /// The Content Collection View block's collection.
            /// </summary>
            public const string ContentCollection = "ContentCollection";
        }

        #endregion Keys

        #region Site Methods

        /*
            9/30/2026 - CLAUDE

            The spec describes the logo as two Site fields, but only the light logo is a
            Site column (FavIconBinaryFileId). The dark logo, like the palette, lives in the
            Site's AdditionalSettings blob (DarkFavIconBinaryFileId), which is where
            MobileApplicationDetail keeps it and where MobileHelper reads it for the
            package. Following "as normal as possible", both stay where Rock keeps them.

            Reason: The dark logo is a blob field, not a Site column.
        */

        /// <summary>
        /// Reads the church owned values stored on the Site: the two logos and the brand
        /// color pair.
        /// </summary>
        /// <param name="site">The platform Site.</param>
        /// <returns>The church's branding.</returns>
        public static PlatformMobileAppBranding GetBranding( Site site )
        {
            var settings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
            var colors = ( settings.DownhillSettings ?? new DownhillCss.DownhillSettings() ).ApplicationColors;

            return new PlatformMobileAppBranding
            {
                LightLogoBinaryFileId = site.FavIconBinaryFileId,
                DarkLogoBinaryFileId = settings.DarkFavIconBinaryFileId,
                ColorStrong = colors.PrimaryStrong,
                ColorSoft = colors.PrimarySoft
            };
        }

        /// <summary>
        /// Writes the church's branding onto the Site, the way the normal Styles tab does:
        /// the light logo into the Site's logo field, the dark logo into the settings blob,
        /// and Strong and Soft into both the Primary and Brand colors, so the two pairs are
        /// always equal. The caller saves the context and then runs the builder, which
        /// deploys.
        /// </summary>
        /// <param name="site">The platform Site.</param>
        /// <param name="branding">The church's branding.</param>
        public static void SaveBranding( Site site, PlatformMobileAppBranding branding )
        {
            var settings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();

            if ( settings.DownhillSettings == null )
            {
                settings.DownhillSettings = new DownhillCss.DownhillSettings
                {
                    Platform = DownhillCss.DownhillPlatform.Mobile
                };
            }

            var colors = settings.DownhillSettings.ApplicationColors;

            colors.PrimaryStrong = branding.ColorStrong;
            colors.BrandStrong = branding.ColorStrong;
            colors.PrimarySoft = branding.ColorSoft;
            colors.BrandSoft = branding.ColorSoft;

            settings.DarkFavIconBinaryFileId = branding.DarkLogoBinaryFileId;
            site.FavIconBinaryFileId = branding.LightLogoBinaryFileId;
            site.AdditionalSettings = settings.ToJson();
        }

        /// <summary>
        /// Takes a snapshot of every church owned value on the Site, so the builder can
        /// prove it left them alone.
        /// </summary>
        /// <param name="site">The platform Site.</param>
        /// <returns>The snapshot.</returns>
        public static string GetSiteSnapshot( Site site )
        {
            var settings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
            var colors = ( settings.DownhillSettings ?? new DownhillCss.DownhillSettings() ).ApplicationColors;

            // Strings, not objects: on .NET Framework the object overload of string.Join
            // returns "" whenever its first value is null, which a missing logo always is.
            var values = new[]
            {
                site.FavIconBinaryFileId?.ToString(),
                settings.DarkFavIconBinaryFileId?.ToString(),
                colors.PrimaryStrong,
                colors.PrimarySoft,
                colors.BrandStrong,
                colors.BrandSoft
            };

            return string.Join( "|", values );
        }

        /// <summary>
        /// Fails the builder run if any church owned value on the Site changed since the
        /// snapshot. The builder never writes these; this makes that a checked rule rather
        /// than a convention.
        /// </summary>
        /// <param name="snapshot">The snapshot taken before the builder touched the Site.</param>
        /// <param name="site">The platform Site, after the builder's changes.</param>
        /// <exception cref="InvalidOperationException">A church owned value changed.</exception>
        public static void EnsureSiteUnchanged( string snapshot, Site site )
        {
            if ( GetSiteSnapshot( site ) != snapshot )
            {
                throw new InvalidOperationException( "The builder changed the church's logo or colors, which are church owned and must never be written by the builder." );
            }
        }

        #endregion Site Methods

        #region Block Methods

        /// <summary>
        /// Determines whether a block setting is church owned.
        /// </summary>
        /// <param name="blockGuid">The registry Guid of the block.</param>
        /// <param name="attributeKey">The attribute key of the setting.</param>
        /// <returns><c>true</c> if the setting is on the skip list.</returns>
        public static bool IsChurchOwned( Guid blockGuid, string attributeKey )
        {
            return BlockAttributeKeys.TryGetValue( blockGuid, out var keys )
                && keys.Contains( attributeKey );
        }

        /// <summary>
        /// Gets the church's value for a church owned block setting.
        /// </summary>
        /// <param name="blockGuid">The registry Guid of the block.</param>
        /// <param name="attributeKey">The attribute key of the setting.</param>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <returns>The value, or <c>null</c> if the block does not exist yet.</returns>
        /// <exception cref="InvalidOperationException">The setting is not church owned.</exception>
        public static string GetBlockValue( Guid blockGuid, string attributeKey, RockContext rockContext )
        {
            EnsureChurchOwned( blockGuid, attributeKey );

            var block = new BlockService( rockContext ).Get( blockGuid );

            if ( block == null )
            {
                return null;
            }

            block.LoadAttributes( rockContext );

            return block.GetAttributeValue( attributeKey );
        }

        /// <summary>
        /// Saves the church's value for a church owned block setting. The caller runs the
        /// builder afterwards, which deploys the app and refreshes anything platform owned
        /// that depends on the value.
        /// </summary>
        /// <param name="blockGuid">The registry Guid of the block.</param>
        /// <param name="attributeKey">The attribute key of the setting.</param>
        /// <param name="value">The value to save.</param>
        /// <param name="rockContext">The Rock context to use.</param>
        /// <exception cref="InvalidOperationException">The setting is not church owned, or the block does not exist.</exception>
        public static void SaveBlockValue( Guid blockGuid, string attributeKey, string value, RockContext rockContext )
        {
            EnsureChurchOwned( blockGuid, attributeKey );

            var block = new BlockService( rockContext ).Get( blockGuid )
                ?? throw new InvalidOperationException( "The platform mobile application has not been built." );

            block.LoadAttributes( rockContext );

            if ( !block.Attributes.ContainsKey( attributeKey ) )
            {
                throw new InvalidOperationException( $"Block '{block.Name}' has no setting '{attributeKey}'." );
            }

            block.SetAttributeValue( attributeKey, value ?? string.Empty );
            block.SaveAttributeValues( rockContext );
        }

        /// <summary>
        /// Refuses a setting that is not on the skip list, so the control panel can never
        /// write a platform owned value.
        /// </summary>
        /// <param name="blockGuid">The registry Guid of the block.</param>
        /// <param name="attributeKey">The attribute key of the setting.</param>
        /// <exception cref="InvalidOperationException">The setting is not church owned.</exception>
        private static void EnsureChurchOwned( Guid blockGuid, string attributeKey )
        {
            if ( !IsChurchOwned( blockGuid, attributeKey ) )
            {
                throw new InvalidOperationException( $"Setting '{attributeKey}' on block {blockGuid} is platform owned and cannot be changed from the control panel." );
            }
        }

        #endregion Block Methods
    }

    /// <summary>
    /// The church's branding of the platform mobile application: its in-app logos and the
    /// Strong and Soft shades of its color.
    /// </summary>
    internal sealed class PlatformMobileAppBranding
    {
        /// <summary>
        /// Gets or sets the logo shown over a light background, or <c>null</c> for none.
        /// </summary>
        public int? LightLogoBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the logo shown over a dark background, or <c>null</c> for none.
        /// </summary>
        public int? DarkLogoBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the strong shade, written into Primary Strong and Brand Strong.
        /// Dark mode swaps it with the soft shade.
        /// </summary>
        public string ColorStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft shade, written into Primary Soft and Brand Soft.
        /// </summary>
        public string ColorSoft { get; set; }
    }
}
