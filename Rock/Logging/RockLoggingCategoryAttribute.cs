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
namespace Rock.Logging
{
    /// <summary>
    /// <para>
    /// Identifies a class as supporting logging. This is used to allow the
    /// administrator to easily search for and enable logging categories.
    /// Logging can still happen even without this attribute.
    /// </para>
    /// <para>
    /// The constructor that takes a category name parameter should only
    /// be used for assembly level attributes.
    /// </para>
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Assembly, AllowMultiple = true )]
    public class RockLoggingCategoryAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <value>The name of the category.</value>
        public string CategoryName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggingCategoryAttribute"/> class.
        /// </summary>
        public RockLoggingCategoryAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggingCategoryAttribute"/> class.
        /// This can be used to register a non standard category name such as <c>Rock.Jobs</c>,
        /// which would allow the entire namespace to be configured.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        public RockLoggingCategoryAttribute( string categoryName )
        {
            CategoryName = categoryName;
        }
    }
}
