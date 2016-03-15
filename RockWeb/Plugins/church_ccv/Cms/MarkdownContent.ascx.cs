using System.ComponentModel;
using System.Diagnostics;
using Rock.Attribute;
using Rock;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Markdown Content" )]
    [Category( "CCV > Cms" )]
    [Description( "Block for editing content using Markdown" )]

    [TextField( "MarkdownContent", category: "CustomSetting" )]
    public partial class MarkdownContent : church.ccv.Utility.Web.BaseContentBlock
    {
        /// <summary>
        /// Implement to show the content. For example, simply have it do "lContent.Text = this.GetContentHtml();"
        /// </summary>
        public override void ShowContent()
        {
            mdEdit.Text = this.GetAttributeValue( "MarkdownContent" );
            lContent.Text = this.GetContentHtml();
        }

        /// <summary>
        /// Gets the content template.
        /// </summary>
        /// <returns></returns>
        public override string GetContentTemplate()
        {

            var markdown = this.GetAttributeValue( "MarkdownContent" ) ?? string.Empty;
            if ( string.IsNullOrWhiteSpace( markdown.Trim() ) )
            {
                markdown = "<small>Edit</small>";
            }

            return markdown.ConvertMarkdownToHtml( true );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, System.EventArgs e )
        {

            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, System.EventArgs e )
        {
            FlushCachedContent();
            this.SetAttributeValue( "MarkdownContent", mdEdit.Text );
            this.SaveAttributeValues();
            ShowContent();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, System.EventArgs e )
        {
            ShowContent();
        }
    }
}