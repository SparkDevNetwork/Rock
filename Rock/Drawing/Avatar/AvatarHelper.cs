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
using System.IO;
using Rock.Model;
using Rock.Web.Cache;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Web;

namespace Rock.Drawing.Avatar
{
    /// <summary>
    /// Helper class for building avatars
    /// </summary>
    public static class AvatarHelper
    {
        #region Reusable Mask Properties
        /// <summary>
        /// Mask for adult males
        /// </summary>
        public static Image AdultMaleMask { get; } = new Image<Rgba32>( 1, 1 );

        /// <summary>
        /// Mask for adult females
        /// </summary>
        public static Image AdultFemaleMask { get; } = new Image<Rgba32>( 1, 1 );

        /// <summary>
        /// Mask for male children
        /// </summary>
        public static Image ChildMaleMask { get; } = new Image<Rgba32>( 1, 1 );

        /// <summary>
        /// Mask for female children
        /// </summary>
        public static Image ChildFemaleMask { get; } = new Image<Rgba32>( 1, 1 );

        /// <summary>
        /// Mask for unknown genders
        /// </summary>
        public static Image UnknownGenderMask { get; } = new Image<Rgba32>( 1, 1 );

        /// <summary>
        /// Mask for businesses
        /// </summary>
        public static Image BusinessMask { get; } = new Image<Rgba32>( 1, 1 );

        #endregion

        #region Public Properties
        /// <summary>
        /// List of security roles that are allowed to clear cached avatars
        /// </summary>
        public static List<Guid> AuthorizedRefreshCacheRoleGuids { get; } = new List<Guid>();
        #endregion

        #region Private Members
        /// <summary>
        /// Collection of fonts to be used for avatars
        /// </summary>
        private static FontCollection _fontCollection = null;

        /// <summary>
        /// The record type id for businesses
        /// </summary>
        private static int BusinessRecordTypeId { get; } = 0;

        /// <summary>
        /// The record type id for nameless people
        /// </summary>
        private static int NamelessPersonRecordTypeId { get; } = 0;

        /// <summary>
        /// The record type id for people
        /// </summary>
        private static int PersonRecordTypeId { get; } = 0;
        #endregion

        static AvatarHelper()
        {

            // Load icon masks. 
            var folderPath = System.Web.Hosting.HostingEnvironment.MapPath( "~\\App_Data\\Avatar\\" );

            // Load role guids that are allowed to refresh the cache
            AuthorizedRefreshCacheRoleGuids.Add( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
            AuthorizedRefreshCacheRoleGuids.Add( Rock.SystemGuid.Group.GROUP_WEB_ADMINISTRATORS.AsGuid() );

            try
            {
                AdultMaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\adult-male.png" );
                AdultFemaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\adult-female.png" );
                ChildMaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\child-male.png" );
                ChildFemaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\child-female.png" );
                UnknownGenderMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\unknown-gender.png" );
                BusinessMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\business.png" );

                // Load Fonts
                _fontCollection = new FontCollection();
                _fontCollection.Add( folderPath + "\\Fonts\\Inter-Regular.ttf" );
                _fontCollection.Add( folderPath + "\\Fonts\\Inter-Bold.ttf" );
            }
            catch { }

            // Load cache of record types
            BusinessRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
            NamelessPersonRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
            PersonRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
        }

        /// <summary>
        /// Creates the avatar based on the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static Stream CreateAvatar( AvatarSettings settings )
        {
            Image avatar = null;

            // If there is a photo in the settings we will always use that
            if ( settings.PhotoId.HasValue )
            {
                try
                {
                    // Get image from the binary file
                    avatar = RockImage.GetPersonImageFromBinaryFileService( settings.PhotoId.Value );
                }
                catch ( Exception )
                {
                    return null;
                }

                // Resize image
                avatar?.CropResize( settings.Size, settings.Size );
            }

            // If not photo was provided then we'll create a fallback
            if ( avatar == null )
            {
                switch ( settings.AvatarStyle )
                {
                    case AvatarStyle.Icon:
                        {
                            avatar = CreateIconAvatar( settings );
                            break;
                        }
                    case AvatarStyle.Initials:
                        {
                            avatar = CreateInitialsAvatar( settings );
                            break;
                        }
                }
            }

            // We should always have a image, but just in case
            if ( avatar == null )
            {
                return null;
            }

            // Apply the requested styling
            if ( settings.CornerRadius != 0 )
            {
                avatar.Mutate( o => o.ApplyRoundedCorners( settings.CornerRadius ) );
            }

            if ( settings.IsCircle )
            {
                avatar.Mutate( o => o.ApplyCircleCorners() );
            }


            // Cache the image to the file system
            try
            {
                avatar.SaveAsPng( $"{settings.CachePath}{settings.CacheKey}.png" );
            }
            catch ( Exception ) { }

            var outputStream = new MemoryStream();
            avatar.SaveAsPng( outputStream );

            outputStream.Position = 0;

            return outputStream;
        }

        private static Image CreateIconAvatar( AvatarSettings settings )
        {
            // Get the top layer which is the icon
            var topLayer = CreateIconAvatarTopLayer( settings );

            // Get the background layer which is solid
            var bottomLayer = RockImage.CreateSolidImage( settings.Size, settings.Size, settings.AvatarColors.BackgroundColor );

            // Return the two images merged
            bottomLayer.Mutate( i => i
                        .DrawImage( topLayer, new Point( 0, 0 ), 1f )
                        .DrawImage( bottomLayer, new Point( 0, 0 ), 1f ) );

            return bottomLayer;
        }

        /// <summary>
        /// Gets the top layer for the icon avatar
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static Image CreateIconAvatarTopLayer( AvatarSettings settings )
        {
            // Get the correct mask to use based off the settings
            var mask = GetIconMask( settings );

            // Resize the mask
            mask.Mutate( o => o.Resize( settings.Size, settings.Size, KnownResamplers.Lanczos3 ) );

            var topLayer = RockImage.CreateMaskImage( mask, settings.AvatarColors.ForegroundColor );

            return topLayer;
        }

        /// <summary>
        /// Gets the correct icon mask based on the settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static Image GetIconMask( AvatarSettings settings )
        {
            // Business
            if ( settings.RecordTypeId == AvatarHelper.BusinessRecordTypeId )
            {
                return AvatarHelper.BusinessMask.CloneAs<Rgba32>();
            }

            // Nameless person
            if ( settings.RecordTypeId == AvatarHelper.NamelessPersonRecordTypeId )
            {
                return AvatarHelper.UnknownGenderMask.CloneAs<Rgba32>();
            }

            // Child male
            if ( settings.Gender == Gender.Male && settings.AgeClassification == AgeClassification.Child )
            {
                return AvatarHelper.ChildMaleMask.CloneAs<Rgba32>();
            }

            // Child female
            if ( settings.Gender == Gender.Female && settings.AgeClassification == AgeClassification.Child )
            {
                return AvatarHelper.ChildFemaleMask.CloneAs<Rgba32>();
            }

            // Male
            if ( settings.Gender == Gender.Male )
            {
                return AvatarHelper.AdultMaleMask.CloneAs<Rgba32>();
            }

            // Female
            if ( settings.Gender == Gender.Female )
            {
                return AvatarHelper.AdultFemaleMask.CloneAs<Rgba32>();
            }

            // Default is unknown
            return AvatarHelper.UnknownGenderMask.CloneAs<Rgba32>();
        }

        /// <summary>
        /// Creates the avatar based on the initials in the Text setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static Image CreateInitialsAvatar( AvatarSettings settings )
        {
            // Create the background
            var backgroundImage = new Image<Rgba32>( settings.Size, settings.Size, Color.ParseHex( settings.AvatarColors.BackgroundColor ) );

            // Calculate spacing
            var fontSize = ( settings.Size / 3 ); // font size is 1/3 of the height

            // Get font
            var fontWeight = FontStyle.Regular;

            if ( settings.IsBold )
            {
                fontWeight = FontStyle.Bold;
            }

            try
            {
                var fontFamily = _fontCollection.Get( "Inter" );
                var font = fontFamily.CreateFont( ( float ) fontSize, fontWeight );

                // Write text
                // https://github.com/SixLabors/ImageSharp/issues/185
                // https://www.adamrussell.com/adding-image-watermark-text-in-c-with-imagesh
                var textOptions = new TextOptions( font )
                {
                    Dpi = ( float ) backgroundImage.Metadata.VerticalResolution
                };

                // Measure the size of the text
                var textSize = TextMeasurer.Measure( settings.Text, textOptions );

                // Calculate margins
                var leftMargin = ( settings.Size - textSize.Width ) / 2;
                var topMargin = ( settings.Size - textSize.Height ) / 2;

                // We needed to create the TextOptions above to be able to measure the size of the Text. One
                // would think you could update that object's Origin property to tell it where to place the text
                // but you can't. The Origin will appear to be updated, but the text will be place at the original
                // Origin (possible bug? 🤷‍). To get around that we'll need to create a new TextOption.

                var renderTextOptions = new TextOptions( font )
                {
                    Dpi = ( float ) backgroundImage.Metadata.VerticalResolution,
                    Origin = new System.Numerics.Vector2( leftMargin, topMargin )
                };

                backgroundImage.Mutate( o => o.DrawText( renderTextOptions, settings.Text, Color.ParseHex( settings.AvatarColors.ForegroundColor ) ) );
                // alignment testing box backgroundImage.Mutate( o => o.Draw( Color.Red, 1, new RectangleF( new PointF( textOptions.Origin.X, textOptions.Origin.Y ), new SizeF( textSize.Width , textSize.Height ) ) ) ); 

            }
            catch { }

            return backgroundImage;
        }
    }
}
