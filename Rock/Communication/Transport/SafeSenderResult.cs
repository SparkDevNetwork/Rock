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
using System.Net.Mail;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// 
    /// </summary>
    public class SafeSenderResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is unsafe domain.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unsafe domain; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnsafeDomain { get; set; }

        /// <summary>
        /// Gets or sets the safe from address.
        /// </summary>
        /// <value>
        /// The safe from address.
        /// </value>
        public MailAddress SafeFromAddress { get; set; }

        /// <summary>
        /// Gets or sets the reply to address.
        /// </summary>
        /// <value>
        /// The reply to address.
        /// </value>
        public MailAddress ReplyToAddress { get; set; }
    }
}
