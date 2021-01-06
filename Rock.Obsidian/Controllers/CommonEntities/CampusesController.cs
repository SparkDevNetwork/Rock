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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Obsidian.Controllers.CommonEntities
{
    /// <summary>
    /// Campus Controller
    /// This controller is NOT SECURED by default. DO NOT send sensitive data in responses.
    /// </summary>
    [System.Web.Http.Route( "api/obsidian/v1/commonentities/campuses" )]
    public class ObsidianCampusesController : CachedModelController<CampusCache, Campus>
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        protected override object GetViewModel( CampusCache cache )
        {
            return new CampusViewModel
            {
                Guid = cache.Guid,
                Name = cache.Name,
                IsActive = cache.IsActive
            };
        }

        /// <summary>
        /// Campus View Model
        /// </summary>
        public sealed class CampusViewModel
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            public bool? IsActive { get; set; }
        }
    }
}
