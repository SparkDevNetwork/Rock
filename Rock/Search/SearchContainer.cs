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
    /// <summary>
    /// 
    /// </summary>
    public class SearchContainer : ContainerManaged<SearchComponent, IComponentData>
    {
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// Prevents a default instance of the <see cref="SearchContainer" /> class from being created.
        /// </summary>
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