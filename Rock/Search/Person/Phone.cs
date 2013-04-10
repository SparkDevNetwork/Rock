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

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching phones
    /// </summary>
    [Description( "Person Phone Search" )]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Phone")]
    public class Phone : SearchComponent
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "SearchLabel", "Phone" );
                return defaults;
            }
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var phoneNumberService = new PhoneNumberService();

            return phoneNumberService.Queryable().
                Where( n => n.Number.Contains( searchterm ) ).
                OrderBy( n => n.Number ).
                Select( n => n.Number ).Distinct();
        }
    }
}