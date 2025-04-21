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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.PersonSignalList
{
    /// <summary>
    /// Describes the data sent to and from remote systems to allow editing
    /// of Person Signal.
    /// </summary>
    public class PersonSignalBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the existing person signal. If
        /// this is a new attribute the value should be <c>null</c>.
        /// </summary>
        /// <value>The unique identifier of the existing signal type.</value>
        public Guid? Guid { get; set; }
        /// <summary>
        /// Gets or sets the key that identifies the person signal.
        /// </summary>
        /// <value>The key that identifies the person signal.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the signal type of the person signal.
        /// </summary>
        /// <value>The signal type of the person signal.</value>
        public ListItemBag SignalType { get; set; }

        /// <summary>
        /// Gets or sets the owner of the person signal.
        /// </summary>
        /// <value>The owner of the person signal.</value>
        public ListItemBag Owner { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the person signal.
        /// </summary>
        /// <value>The expiration date of the person signal.</value>
        public System.DateTimeOffset? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the note on the person signal.
        /// </summary>
        /// <value>The note on the person signal.</value>
        public string Note { get; set; }
    }
}
