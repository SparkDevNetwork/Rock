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

namespace Rock.Communication
{
    /// <summary>
    /// This should be used by any transport that as a webhook that rock needs to know about. This is currently used by ths Sms Pipeline Details block to show the web hook path.
    /// </summary>
    public interface ISmsPipelineWebhook
    {
        /// <summary>
        /// Gets the sms pipeline webhook path that should be used by this transport.
        /// </summary>
        /// <value>
        /// The sms pipeline webhook path.
        /// </value>
        /// <note>
        /// This should be from the application root (https://www.rocksolidchurch.com/).
        /// In other words you don't need a leading forward slash.
        /// For example, you can just return Webhooks/TwilioSms.ashx
        /// </note>
        string SmsPipelineWebhookPath { get; }
    }
}
