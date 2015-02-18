// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// A JsonTextWriter that is aware of SerializeInSimpleMode
    /// AttributeCacheJsonConverter and AttributeValueJsonConverter use this to figure out how they should serialize
    /// </summary>
    public class RockJsonTextWriter : Newtonsoft.Json.JsonTextWriter
    {
        /// <summary>
        /// Gets or sets a value indicating whether [serialize in simple mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [serialize in simple mode]; otherwise, <c>false</c>.
        /// </value>
        public bool SerializeInSimpleMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJsonTextWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="serializeInSimpleMode">if set to <c>true</c> [serialize in simple mode].</param>
        public RockJsonTextWriter( System.IO.TextWriter textWriter, bool serializeInSimpleMode )
            : base( textWriter )
        {
            SerializeInSimpleMode = serializeInSimpleMode;
        }
    }
}
