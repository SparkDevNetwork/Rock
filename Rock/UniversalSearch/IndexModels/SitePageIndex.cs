using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    public class SitePageIndex : IndexModelBase
    {
        [RockIndexField]
        public int SiteId { get; set; }

        [RockIndexField( Index = IndexType.NotIndexed )]
        public string SiteName { get; set; }

        [RockIndexField( Boost = 4 )]
        public string PageTitle { get; set; }

        [RockIndexField( Boost = 2 )]
        public string PageSummary { get; set; }

        [RockIndexField( Boost = 8 )]
        public string PageKeywords { get; set; }

        [RockIndexField( Index = IndexType.NotIndexed )]
        public string Url { get; set; }

        [RockIndexField]
        public string Content { get; set; }

        [RockIndexField( Type = IndexFieldType.Date )]
        public DateTime LastIndexedDateTime { get; set; }

        [RockIndexField( Index = IndexType.NotIndexed )]
        public override string IconCssClass
        {
            get
            {
                return "fa fa-desktop";
            }
        }

        public static SitePageIndex LoadByModel( Site site )
        {
            // site page does not have a Rock model
            return null;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null )
        {

            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = string.Format( "<a href='{0}'>{1}</a>", this.Url, this.PageTitle ) };
        }
    }
}
