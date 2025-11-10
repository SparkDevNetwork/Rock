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
namespace Rock.SystemKey
{
    /// <summary>
    /// Global person preference keys. 
    /// </summary>
    public class PersonPreferenceKey
    {
        /// <summary>
        /// The source phone type the user would like to originate calls to
        /// </summary>
        public const string ORIGINATE_CALL_SOURCE = "Core_OriginateCallSource";

        /// <summary>
        /// The default SMS phone number to use when sending SMS messages. This
        /// is stored as the integer identifier of the <see cref="Model.SystemPhoneNumber"/>.
        /// </summary>
        public const string DEFAULT_SMS_PHONE_NUMBER = "default-sms-phone-number";

        /// <summary>
        /// The closing phrase when drafting e-mails automatically.
        /// </summary>
        public const string EMAIL_CLOSING_PHRASE = "email-closing-phrase";
    }
}