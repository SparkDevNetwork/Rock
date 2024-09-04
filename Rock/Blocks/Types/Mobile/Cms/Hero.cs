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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Common.Mobile.Enums;
using Rock.Mobile;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Displays an image with text overlay on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Hero" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays an image with text overlay on the page." )]
    [IconCssClass( "fa fa-heading" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [TextField( "Title",
        Description = "The main title to display over the image. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Key = AttributeKeys.Title,
        Order = 0 )]

    [TextField( "Subtitle",
        Description = "The subtitle to display over the image. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Key = AttributeKeys.Subtitle,
        Order = 1 )]

    [FileField( SystemGuid.BinaryFiletype.DEFAULT,
        Name = "Background Image - Phone",
        Description = "Recommended size is at least 1024px wide and double the height specified below.",
        IsRequired = true,
        Key = AttributeKeys.BackgroundImagePhone,
        Order = 2 )]

    [FileField( SystemGuid.BinaryFiletype.DEFAULT,
        Name = "Background Image - Tablet",
        Description = "Recommended size is at least 2048px wide and double the height specified below.",
        IsRequired = true,
        Key = AttributeKeys.BackgroundImageTablet,
        Order = 3 )]

    [IntegerField( "Height - Phone",
        IsRequired = true,
        DefaultIntegerValue = 200,
        Key = AttributeKeys.ImageHeightPhone,
        Order = 4 )]

    [IntegerField( "Height - Tablet",
        IsRequired = true,
        DefaultIntegerValue = 350,
        Key = AttributeKeys.ImageHeightTablet,
        Order = 5 )]

    [CustomRadioListField( "Text Align",
        IsRequired = true,
        DefaultValue = "Center",
        ListSource = "Left, Center, Right",
        Key = AttributeKeys.HorizontalTextAlign,
        Order = 6 )]

    [ColorField( "Title Color",
        Description = "Will override the theme's hero title (.hero-title) color.",
        IsRequired = false,
        DefaultValue = "#ffffff",
        Key = AttributeKeys.TitleColor,
        Order = 7 )]

    [ColorField( "Subtitle Color",
        Description = "Will override the theme's hero subtitle (.hero-subtitle) color.",
        IsRequired = false,
        DefaultValue = "#ffffff",
        Key = AttributeKeys.SubtitleColor,
        Order = 8 )]

    [IntegerField( "Padding",
        Description = "The padding around the inside of the image.",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Key = AttributeKeys.Padding,
        Order = 9 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CMS_HERO_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "A8597994-BD47-4A15-8BB1-4B508977665F")]
    public class Hero : RockBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the Hero block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The title key
            /// </summary>
            public const string Title = "Title";

            /// <summary>
            /// The subtitle key
            /// </summary>
            public const string Subtitle = "Subtitle";

            /// <summary>
            /// The phone background image key
            /// </summary>
            public const string BackgroundImagePhone = "BackgroundImagePhone";

            /// <summary>
            /// The tablet background image key
            /// </summary>
            public const string BackgroundImageTablet = "BackgroundImageTablet";

            /// <summary>
            /// The phone image height key
            /// </summary>
            public const string ImageHeightPhone = "ImageHeightPhone";

            /// <summary>
            /// The tablet image height key
            /// </summary>
            public const string ImageHeightTablet = "ImageHeightTablet";

            /// <summary>
            /// The horizontal text alignment key
            /// </summary>
            public const string HorizontalTextAlign = "HorizontalTextAlign";

            /// <summary>
            /// The title color key
            /// </summary>
            public const string TitleColor = "TitleColor";

            /// <summary>
            /// The subtitle color key
            /// </summary>
            public const string SubtitleColor = "SubtitleColor";

            /// <summary>
            /// The padding key
            /// </summary>
            public const string Padding = "Padding";
        }

        #region Attribute Properties

        /// <summary>
        /// Gets the title attribute value.
        /// </summary>
        /// <value>
        /// The title attribute value.
        /// </value>
        protected string Title => GetAttributeValue( AttributeKeys.Title );

        /// <summary>
        /// Gets the subtitle attribute value.
        /// </summary>
        /// <value>
        /// The subtitle attribute value.
        /// </value>
        protected string Subtitle => GetAttributeValue( AttributeKeys.Subtitle );

        /// <summary>
        /// Gets the phone background image attribute value.
        /// </summary>
        /// <value>
        /// The phone background image attribute value.
        /// </value>
        protected Guid BackgroundImagePhone => GetAttributeValue( AttributeKeys.BackgroundImagePhone ).AsGuid();

        /// <summary>
        /// Gets the tablet background image attribute value.
        /// </summary>
        /// <value>
        /// The tablet background image attribute value.
        /// </value>
        protected Guid BackgroundImageTablet => GetAttributeValue( AttributeKeys.BackgroundImageTablet ).AsGuid();

        /// <summary>
        /// Gets the phone image height attribute value.
        /// </summary>
        /// <value>
        /// The phone image height attribute value.
        /// </value>
        protected int ImageHeightPhone => GetAttributeValue( AttributeKeys.ImageHeightPhone ).AsInteger();

        /// <summary>
        /// Gets the tablet image height attribute value.
        /// </summary>
        /// <value>
        /// The tablet image height attribute value.
        /// </value>
        protected int ImageHeightTablet => GetAttributeValue( AttributeKeys.ImageHeightTablet ).AsInteger();

        /// <summary>
        /// Gets the horizontal text alignment attribute value.
        /// </summary>
        /// <value>
        /// The horizontal text alignment attribute value.
        /// </value>
        protected string HorizontalTextAlign => GetAttributeValue( AttributeKeys.HorizontalTextAlign );

        /// <summary>
        /// Gets the color of the title text.
        /// </summary>
        /// <value>
        /// The color of the title text.
        /// </value>
        protected string TitleColor => GetAttributeValue( AttributeKeys.TitleColor );

        /// <summary>
        /// Gets the color of the subtitle text.
        /// </summary>
        /// <value>
        /// The color of the subtitle text.
        /// </value>
        protected string SubtitleColor => GetAttributeValue( AttributeKeys.SubtitleColor );

        /// <summary>
        /// Gets the padding attribute value.
        /// </summary>
        /// <value>
        /// The padding attribute value.
        /// </value>
        protected int Padding => GetAttributeValue( AttributeKeys.Padding ).AsInteger();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <inheritdoc/>
        public override Guid? MobileBlockTypeGuid => new Guid( "7258A210-E936-4260-B573-9FA1193AD9E2" ); // Content block.

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var additionalSettings = BlockCache?.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            var config = new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                Content = string.Empty,
                ProcessLava = additionalSettings.ProcessLavaOnClient,
                CacheDuration = additionalSettings.CacheDuration,
                DynamicContent = additionalSettings.ProcessLavaOnClient || additionalSettings.ProcessLavaOnServer
            };

            if ( !config.DynamicContent )
            {
                config.Content = GetContent();
            }

            return config;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the XAML content.
        /// </summary>
        /// <returns>The XAML content.</returns>
        protected virtual string GetContent()
        {
            var deviceData = RequestContext.GetHeader( "X-Rock-DeviceData" ).FirstOrDefault().FromJsonOrNull<DeviceData>() ?? new DeviceData();
            var additionalSettings = BlockCache?.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();
            var title = Title;
            var subtitle = Subtitle;
            var imageGuid = deviceData.DeviceType == DeviceType.Phone ? BackgroundImagePhone : BackgroundImageTablet;
            var height = deviceData.DeviceType == DeviceType.Phone ? ImageHeightPhone : ImageHeightTablet;
            var imageHeight = height;
            var alignment = "Center";
            var titleColorPlaceholder = TitleColor.IsNotNullOrWhiteSpace() ? $"TextColor=\"{TitleColor}\"" : string.Empty;
            var subtitleColorPlaceholder = SubtitleColor.IsNotNullOrWhiteSpace() ? $"TextColor=\"{SubtitleColor}\"" : string.Empty;

            if ( deviceData.Density > 1 )
            {
                imageHeight = ( int ) ( imageHeight * deviceData.Density );
            }

            if ( HorizontalTextAlign == "Left" )
            {
                alignment = "Start";
            }
            else if ( HorizontalTextAlign == "Right" )
            {
                alignment = "End";
            }

            //
            // Check if we need to render lava on the server.
            //
            if ( additionalSettings.ProcessLavaOnServer )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();

                title = title.ResolveMergeFields( mergeFields, null );
                subtitle = subtitle.ResolveMergeFields( mergeFields, null );
            }

            return $@"
<AbsoluteLayout HorizontalOptions=""Fill""
                StyleClass=""hero""
                HeightRequest=""{height}"">
    <Rock:Image Source=""{MobileHelper.BuildPublicApplicationRootUrl( $"GetImage.ashx?guid={imageGuid}&amp;height={imageHeight}" )}""
                StyleClass=""hero-image""
                Aspect=""AspectFill""
                AbsoluteLayout.LayoutBounds=""0,0,1,1""
                AbsoluteLayout.LayoutFlags=""All"" />
    <StackLayout Padding=""{Padding}""
                 Spacing=""0""
                 AbsoluteLayout.LayoutBounds=""0,0.5,1,AutoSize""
                 AbsoluteLayout.LayoutFlags=""PositionProportional,WidthProportional"">
        <Label StyleClass=""hero-title""
               HorizontalOptions=""{alignment}""
               Text=""{title.EncodeXml( true )}"" {titleColorPlaceholder} />
        <Label StyleClass=""hero-subtitle""
               HorizontalOptions=""{alignment}""
               Text=""{subtitle.EncodeXml( true )}"" {subtitleColorPlaceholder} />
    </StackLayout>
</AbsoluteLayout>";
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The initial content.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = GetContent()
            };
        }

        #endregion
    }
}
