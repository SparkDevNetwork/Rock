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

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Specifies the block type for the class.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class )]
    public class StructuredContentBlockAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the type of the block.
        /// </summary>
        /// <value>
        /// The type of the block.
        /// </value>
        public string BlockType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredContentBlockAttribute"/> class.
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        public StructuredContentBlockAttribute( string blockType )
        {
            BlockType = blockType;
        }
    }
}
