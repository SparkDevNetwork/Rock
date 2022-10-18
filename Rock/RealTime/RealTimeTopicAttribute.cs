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

namespace Rock.RealTime
{
    /// <summary>
    /// Identifies a topic to be registered in the RealTime system.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class RealTimeTopicAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the identifier of the topic.
        /// </summary>
        /// <value>The identifier of the topic.</value>
        public string Identifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeTopicAttribute"/> class
        /// with the class namespace and name as its identifier.
        /// </summary>
        public RealTimeTopicAttribute()
        {
            Identifier = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeTopicAttribute"/> class.
        /// </summary>
        /// <param name="identifier">The identifier that uniquely represents this topic.</param>
        public RealTimeTopicAttribute( string identifier )
        {
            Identifier = identifier;
        }
    }
}
