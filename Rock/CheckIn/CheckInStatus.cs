﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Runtime.Serialization;

using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// The status of the currently active check-in.  Contains all the available options
    /// and the values selected for check-in
    /// </summary>
    [DataContract]
    public class CheckInStatus 
    {
        /// <summary>
        /// Gets or sets a value indicating whether the search value was entered by a user (vs. scanned)
        /// </summary>
        /// <value>
        ///   <c>true</c> if user entered search; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UserEnteredSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if a single family result should be confirmed 
        /// by user.  Usually the user entered values will need to be confirmed, while the 
        /// scanned values are more unique and will not need to be confirmed
        /// </summary>
        /// <value>
        ///   <c>true</c> if confirm single family; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ConfirmSingleFamily { get; set; }

        /// <summary>
        /// Gets or sets the type of value that was scanned or entered (i.e. "Barcode",  
        /// "Phone Number", etc)
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        [DataMember]
        public DefinedValueCache SearchType { get; set; }

        /// <summary>
        /// Gets or sets the search value that was scanned or entered by user
        /// </summary>
        /// <value>
        /// The search value.
        /// </value>
        [DataMember]
        public string SearchValue { get; set; }

        /// <summary>
        /// Gets or sets the families that match the search value
        /// </summary>
        /// <value>
        /// The families.
        /// </value>
        [DataMember]
        public List<CheckInFamily> Families { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInStatus" /> class.
        /// </summary>
        public CheckInStatus()
            : base()
        {
            Families = new List<CheckInFamily>();
        }

    }
}