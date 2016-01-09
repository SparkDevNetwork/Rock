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
using System.Collections.Generic;

namespace Rock.Communication
{
    /// <summary>
    /// Simple recipient information
    /// </summary>
    public class RecipientData
    {
        /// <summary>
        /// Gets or sets to.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the merge fields.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public Dictionary<string, object> MergeFields { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipientData"/> class.
        /// </summary>
        public RecipientData()
        {
            MergeFields = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipientData"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        public RecipientData( string to )
            : this()
        {
            To = to;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipientData"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="mergeFields">The merge fields.</param>
        public RecipientData( string to, Dictionary<string, object> mergeFields )
            : this( to )
        {
            MergeFields = mergeFields;
        }
    }
}
