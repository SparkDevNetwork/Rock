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

namespace Rock.Data
{
    /// <summary>
    /// Indicates a model that supports Analytics and whether it supports Analytic History and dynamically added Attributes in the Analytic Tables.
    /// Specify entityTypeQualifierColumn and entityTypeQualifierValue if attribute support depends on entityTypeQualifierColumn/Value.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class AnalyticsAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsAttribute"/> class.
        /// </summary>
        public AnalyticsAttribute( bool supportsHistory, bool supportsAttributes ) : this( null, null, supportsHistory, supportsAttributes )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsAttribute" /> class.
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="supportsHistory">if set to <c>true</c> [supports history].</param>
        /// <param name="supportsAttributes">if set to <c>true</c> [supports attributes].</param>
        public AnalyticsAttribute( string entityTypeQualifierColumn, string entityTypeQualifierValue, bool supportsHistory, bool supportsAttributes )
        {
            this.EntityTypeQualifierColumn = entityTypeQualifierColumn;
            this.EntityTypeQualifierValue = entityTypeQualifierValue;
            this.SupportsHistory = supportsHistory;
            this.SupportsAttributes = supportsAttributes;
        }

        /// <summary>
        /// Gets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [supports history].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports history]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsHistory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [supports attributes].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports attributes]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsAttributes { get; private set; }
    }
}
