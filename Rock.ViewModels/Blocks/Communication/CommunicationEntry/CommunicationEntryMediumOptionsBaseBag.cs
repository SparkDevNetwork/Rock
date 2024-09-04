﻿// <copyright>
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
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing basic information about a communication medium.
    /// </summary>
    public class CommunicationEntryMediumOptionsBaseBag
    {
        /// <summary>
        /// Gets the type of the medium.
        /// </summary>
        /// <value>
        /// The type of the medium.
        /// </value>
        public virtual MediumType MediumType { get; } = MediumType.Unknown;

        /// <summary>
        /// Helper property that can be used when the communication medium type is unknown.
        /// </summary>
        public static readonly CommunicationEntryMediumOptionsBaseBag Unknown = new CommunicationEntryUnknownMediumOptionsBag();

        /// <summary>
        /// Gets or sets the communication templates that can be selected.
        /// </summary>
        /// <value>
        /// The communication templates that can be selected.
        /// </value>
        public List<ListItemBag> Templates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this medium has an active transport.
        /// </summary>
        public bool HasActiveTransport { get; set; }

        /// <summary>
        /// Gets or sets the medium entity type unique identifier.
        /// </summary>
        public virtual Guid MediumEntityTypeGuid { get; set; }

        /// <summary>
        /// Internal for server-side processing only.
        /// </summary>
        internal int? MediumEntityTypeId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this communication medium is unknown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this communication medium is unknown; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnknown => object.ReferenceEquals( this, Unknown ) || this is CommunicationEntryUnknownMediumOptionsBag || this.MediumType == MediumType.Unknown;

        private class CommunicationEntryUnknownMediumOptionsBag : CommunicationEntryMediumOptionsBaseBag
        {
            public override MediumType MediumType => MediumType.Unknown;
        }
    }
}
