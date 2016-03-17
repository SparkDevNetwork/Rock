using System.ComponentModel;
using System.Diagnostics;
using Rock.Attribute;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Generic Content" )]
    [Category( "CCV > Cms" )]
    [Description( "Block that can use a Lava Template to render HTML content and supports 'lava-aware' caching" )]

    [CodeEditorField( "Template", "The Lava template to render the content.", Rock.Web.UI.Controls.CodeEditorMode.Lava, order: 1 )]
    public partial class GenericContent : church.ccv.Web.Cms.BaseContentBlock
    {
        /// <summary>
        /// Implement to show the content. For example, simply have it do "lContent.Text = this.GetContentHtml();"
        /// </summary>
        public override void ShowContent()
        {
            lContent.Text = this.GetContentHtml();
        }

        /// <summary>
        /// Gets the content template.
        /// </summary>
        /// <returns></returns>
        public override string GetContentTemplate()
        {
            return this.GetAttributeValue( "Template" ) ?? string.Empty;
        }
    }
}