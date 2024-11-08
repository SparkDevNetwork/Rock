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

namespace Rock.Blocks
{
    /// <summary>
    /// Defines the initial height of a web block while it waits for
    /// JavaScript to initialize.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class InitialBlockHeightAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the initial height of the block in pixels.
        /// </summary>
        /// <value>The initial height of the block in pixels.</value>
        public int? Height { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialBlockHeightAttribute"/> class.
        /// </summary>
        /// <param name="height">The initial height of the block in pixels. A value less than <c>0</c> will disable.</param>
        public InitialBlockHeightAttribute( int height )
        {
            Height = height >= 0 ? height : ( int? ) null;
        }
    }
}
