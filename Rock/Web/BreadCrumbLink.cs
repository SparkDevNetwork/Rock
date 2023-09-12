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
namespace Rock.Web
{
    /// <summary>
    /// Helper class to work with page navigation
    /// </summary>
    public class BreadCrumbLink : IBreadCrumb
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Url { get; set; }

        /// <inheritdoc/>
        public bool Active { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadCrumbLink" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="active">if set to <c>true</c> [active].</param>
        public BreadCrumbLink( string name, bool active = false )
        {
            Name = name;
            Active = active;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadCrumbLink" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="url">The URL.</param>
        /// <param name="active">if set to <c>true</c> [active].</param>
        public BreadCrumbLink( string name, string url, bool active = false )
            : this( name, active )
        {
            Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadCrumbLink" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pageReference">The page reference.</param>
        /// <param name="active">if set to <c>true</c> [active].</param>
        public BreadCrumbLink( string name, PageReference pageReference, bool active = false )
            : this( name, active )
        {
            Url = pageReference.BuildUrl();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}