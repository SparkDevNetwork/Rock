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
using System.Collections.Generic;

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The options that will be provided to the Checkout check-in REST
    /// endpoint.
    /// </summary>
    public class CheckoutOptionsBag
    {
        /// <summary>
        /// Gets or sets the check-in configuration template unique identifier.
        /// </summary>
        /// <value>The check-in configuration template unique identifier.</value>
        public Guid TemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the kiosk unique identifier.
        /// </summary>
        /// <value>The kiosk unique identifier.</value>
        public Guid? KioskGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance unique identifiers that will be checked
        /// out for this operation.
        /// </summary>
        /// <value>The attendance unique identifiers.</value>
        public List<Guid> AttendanceGuids { get; set; }

        /// <summary>
        /// Gets or sets the details about the check-in session that apply
        /// to the checkout operation.
        /// </summary>
        /// <value>The session details.</value>
        public AttendanceSessionRequestBag Session { get; set; }
    }
}
