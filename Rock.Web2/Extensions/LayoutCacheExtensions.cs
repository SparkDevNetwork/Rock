using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Rock.Web.Cache;

namespace Rock.Web2
{
    static class LayoutCacheExtensions
    {
        /// <summary>
        /// Gets the HTML document for the layout.
        /// </summary>
        /// <param name="layout">The layout whose Html Document is to be retrieved.</param>
        /// <param name="rockPage">The rock page associated with the layout. TODO: Maybe should be changed to a mergeFields dictionary?</param>
        /// <returns>An IHtmlDocument that contains the DOM.</returns>
        public static async Task<IHtmlDocument> GetHtmlDocumentAsync( this LayoutCache layout, Rock.Web2.UI.RockPage rockPage )
        {
            var mergeFields = new Dictionary<string, object>
            {
                { "CurrentPage", rockPage }
            };

            var themeFilename = $"../RockWeb/Themes/{layout.Site.Theme}/Layouts/{layout.FileName}.lava";

            var layoutContent = await File.ReadAllTextAsync( themeFilename );

            var documentContent = layoutContent.ResolveMergeFields( mergeFields );

            var context = BrowsingContext.New( AngleSharp.Configuration.Default );

            return await new HtmlParser( new HtmlParserOptions(), context ).ParseDocumentAsync( documentContent );
        }
    }
}
