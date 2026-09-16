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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceSendPaymentReminder
{
    /// <summary>
    /// Request payload posted when the user toggles the email body into
    /// Preview mode so the server can resolve its merge fields against a
    /// sample registration.
    /// </summary>
    public class ResolvePreviewRequestBag
    {
        /// <summary>
        /// Gets or sets the current Lava source entered in the message body
        /// editor. The server resolves this against a sample registration and
        /// returns the rendered HTML.
        /// </summary>
        public string MessageBody { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the registration to use as the sample
        /// merge-field source. Supplied by the client from the first row of
        /// the grid so the server can avoid re-scanning the full outstanding-
        /// balance list on every preview toggle. When null or empty, the
        /// server falls back to discovering a sample registration itself.
        /// </summary>
        public string SampleRegistrationKey { get; set; }
    }
}
