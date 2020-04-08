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

namespace Rock.Store
{
    /// <summary>
    /// Represents a store category for packages.
    /// </summary>
    public class PackageVersionRating : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package Category.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the rating. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the rating value.
        /// </value>
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets the review text. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing text of the review.
        /// </value>
        public string Review { get; set; }

        /// <summary>
        /// Gets or sets the date added. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the rating was added.
        /// </value>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Gets or sets the helpful count.
        /// </summary>
        /// <value>
        /// The helpful count.
        /// </value>
        public int HelpfulCount { get; set; }

        /// <summary>
        /// Gets or sets the was helpful count.
        /// </summary>
        /// <value>
        /// The was helpful count.
        /// </value>
        public int WasHelpfulCount { get; set; }

        /// <summary>
        /// Gets or sets the person id of the reviewer. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing person id of the reviewer.
        /// </value>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the reviewer. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> representing the reviewer.
        /// </value>
        public PersonAlias PersonAlias { get; set; }
    }
}
