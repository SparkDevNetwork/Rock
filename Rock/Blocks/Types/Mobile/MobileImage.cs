using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    [DisplayName( "Mobile Image" )]
    [Category( "Mobile" )]
    [Description( "Places an image on the mobile device screen." )]
    [IconCssClass( "fa fa-image" )]

    #region Block Attributes

    [CodeEditorField( "Image Url",
        "The URL to use for displaying the image. <span class='tip tip-lava'></span>",
        Web.UI.Controls.CodeEditorMode.Lava )]

    #endregion
    public class MobileImage : RockBlockType, IRockMobileBlockType
    {
        private static string Xaml = @"<Rock:RockImage ImageUrl=""{Binding ConfigurationValues[Url]}"" />";

        public int RequiredMobileAbiVersion => 1;

        public string MobileBlockType => "Rock.Mobile.Blocks.XamlContent";

        public object GetMobileConfigurationValues()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            return new Dictionary<string, object>
            {
                { "Xaml", Xaml },
                { "Url", GetAttributeValue( "ImageUrl" ).ResolveMergeFields( mergeFields, null ) },
            };
        }
    }
}
