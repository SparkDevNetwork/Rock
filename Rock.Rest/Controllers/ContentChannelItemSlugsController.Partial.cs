// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Cache;
using Rock.Security;
using System.Web.Http;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ContentChannelItemSlugsController
    {
        /// <summary>
        /// Posts the content slug.
        /// </summary>
        /// <param name="contentChannelItemSlug">The content channel item slug.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/ContentChannelItemSlugs/SaveContentSlug" )]
        public SaveSlugResponse SaveContentSlug( ContentChannelItemSlug contentChannelItemSlug )
        {
            return new SaveSlugResponse();
        }

        /// <summary>
        /// Return object of the SaveContentSlug action
        /// </summary>
        public class SaveSlugResponse
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="SaveSlugResponse"/> is success.
            /// </summary>
            /// <value>
            ///   <c>true</c> if success; otherwise, <c>false</c>.
            /// </value>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets the slug.
            /// </summary>
            /// <value>
            /// The slug.
            /// </value>
            public string Slug { get; set; }

            /// <summary>
            /// Gets or sets the Id.
            /// </summary>
            /// <value>
            /// The Id.
            /// </value>
            public Guid? Id { get; set; }
        }

    }
}
