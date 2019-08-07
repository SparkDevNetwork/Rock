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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Financial
{
    /// <summary>
    /// 
    /// </summary>
    public class HostedPaymentInfoControlOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable ach].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable ach]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableACH { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [enable credit card].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable credit card]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableCreditCard { get; set; } = true;
    }
}
