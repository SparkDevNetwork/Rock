using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Rock.Attribute;
using Rock;
using HtmlAgilityPack;
using System.Text;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Markdown Content" )]
    [Category( "CCV > Cms" )]
    [Description( "Block for editing content using Markdown" )]

    [TextField( "MarkdownContent", category: "CustomSetting" )]
    public partial class MarkdownContent : church.ccv.Web.Cms.BaseContentBlock
    {
        /// <summary>
        /// Implement to show the content. For example, simply have it do "lContent.Text = this.GetContentHtml();"
        /// </summary>
        public override void ShowContent()
        {
            mdEdit.Text = this.GetAttributeValue( "MarkdownContent" );
            lView.Text = this.GetContentHtml();
        }

        /// <summary>
        /// Gets the content template.
        /// </summary>
        /// <returns></returns>
        public override string GetContentTemplate()
        {
            var markdown = this.GetAttributeValue( "MarkdownContent" ) ?? string.Empty;
            var html = markdown.ConvertMarkdownToHtml( true );
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml( html );

            if ( doc.ParseErrors.Count() > 0 && !nbInvalidHtml.Visible )
            {
                var reasons = doc.ParseErrors.Select( r => r.Reason ).ToList();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "Warning: The HTML has the following errors:<ul>" );
                foreach ( var reason in reasons )
                {
                    sb.AppendLine( string.Format( "<li>{0}</li>", reason.EncodeHtml() ) );
                }

                nbInvalidHtml.Text = sb.ToString();
                nbInvalidHtml.Visible = true;
                return string.Empty;
            }

            return html;
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