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
namespace Rock.ViewModels.Blocks.Administration.SparkConnectedServices
{
    /// <summary>
    /// What the chat card shows. The church's signing key is delivered by the
    /// same call that fills this in and is deliberately absent from it: a bag
    /// that carries a key is a bag that can leak one.
    /// </summary>
    public class ChatConfigurationBag
    {
        /// <summary>
        /// Whether this organization has enabled chat.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The organization's identifier on the chat platform, shown so an
        /// administrator can quote it to support.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The chat platform project this organization talks to.
        /// </summary>
        public string ProjectUrl { get; set; }

        /// <summary>
        /// Whether this organization was enabled and this Rock server cannot read the
        /// chat credentials it was given, so chat cannot run and enabling again is not
        /// offered.
        /// </summary>
        public bool IsCredentialUnreadable { get; set; }
    }
}
