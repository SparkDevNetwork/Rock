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

namespace Rock.Workflow
{
    /// <summary>
    /// Custom attribute used to decorate workflow actions with a category name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class )]
    public class ActionCategoryAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        /// <value>
        /// The name of the category.
        /// </value>
        public string CategoryName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCategoryAttribute"/> class.
        /// </summary>
        public ActionCategoryAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCategoryAttribute"/> class.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        public ActionCategoryAttribute( string categoryName )
        {
            CategoryName = categoryName;
        }

    }
}