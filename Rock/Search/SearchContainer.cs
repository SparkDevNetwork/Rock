//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Web;

using Rock.Extension;

namespace Rock.Search
{
    public class SearchContainer : Container<SearchComponent, IComponentData>
    {
        private static SearchContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static SearchContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new SearchContainer();
                return instance;
            }
        }

        private SearchContainer()
        {
            Refresh();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( SearchComponent ) )]
        protected override IEnumerable<Lazy<SearchComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}