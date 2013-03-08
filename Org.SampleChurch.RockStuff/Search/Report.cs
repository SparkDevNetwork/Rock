//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Model;
using Rock.Search;

namespace Org.SampleChurch.RockStuff.Search
{
    /// <summary>
    /// Searches for people with matching phones
    /// </summary>
    [Description( "Report Search" )]
    [Export( typeof( SearchComponent ) )]
    [ExportMetadata( "ComponentName", "Report" )]
    public class Report : SearchComponent
    {
        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var reportService = new ReportService();

            return reportService.Queryable().
                Where( r => r.Name.Contains( searchterm ) ).
                OrderBy( r => r.Name  ).
                Select( r => r.Name ).Distinct();
        }
    }
}