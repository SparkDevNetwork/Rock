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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight
{
    /// <summary>
    /// Response from the label-printing block actions.
    /// </summary>
    public class PersonRightPrintResultBag
    {
        /// <summary>
        /// Gets or sets the HTML-safe message rendered in the reprint result
        /// notification below the panel header. Server callers may
        /// concatenate multiple lines with &lt;br&gt;.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the JSON payload of next-gen labels that must be
        /// printed by the WebView native bridge on the client side. When
        /// non-empty, the client should invoke
        /// <c>window.RockCheckinNative.PrintV2Labels(...)</c> with this
        /// string. Null / empty for legacy label prints.
        /// </summary>
        public string ClientLabelsJson { get; set; }
    }
}
