using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Linq.SqlClient;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Search;

namespace org.newpointe.ExtraSearch
{
    /// <summary>
    /// Searches for pages with matching titles
    /// </summary>
    [Description( "Page Title Search" )]
    [Export( typeof( SearchComponent ) )]
    [ExportMetadata( "ComponentName", "Page Title" )]
    public class PageTitle : SearchComponent
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults => new Dictionary<string, string> { { "SearchLabel", "Page Title" } };

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            return SearchPages(searchterm).Select(p => p.PageTitle);
        }

        public static IQueryable<Page> SearchPages(string searchterm)
        {
            var likeExp = "%" + searchterm.Replace( "%", "[%]" ).Replace( " ", "%" ) + "%";
            return new PageService(new RockContext()).Queryable().Where( p => SqlMethods.Like(p.PageTitle, likeExp) );
        }
    }
}
