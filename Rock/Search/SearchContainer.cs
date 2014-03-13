// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
    public class SearchContainer : Container<SearchComponent, IComponentData>
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

        public static SearchComponent GetComponent( Type searchComponentType )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.TypeName == searchComponentType.FullName )
                {
                    return component;
                }
            }

            return null;
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( SearchComponent ) )]
        protected override IEnumerable<Lazy<SearchComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}