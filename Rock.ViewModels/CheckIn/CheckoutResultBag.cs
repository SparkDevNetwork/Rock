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
using System.Collections.Generic;

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// The result of a checkout operation.
    /// </summary>
    public class CheckoutResultBag
    {
        /// <summary>
        /// Gets or sets the messages for this check-in operation.
        /// </summary>
        /// <value>The messages for this check-in operation.</value>
        public List<string> Messages { get; set; }

        /// <summary>
        /// Gets or sets the attendance details for the records that were
        /// checked out.
        /// </summary>
        /// <value>The list of attendance records.</value>
        public List<AttendanceBag> Attendances { get; set; }
    }
}
