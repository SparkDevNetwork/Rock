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

using Rock.Attribute;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// A Financial Gateway that has an Obsidian control
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public interface IObsidianHostedGatewayComponent : IPaymentTokenGateway
    {
        /// <summary>
        /// Gets the obsidian control file URL.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        string GetObsidianControlFileUrl( FinancialGateway financialGateway );

        /// <summary>
        /// Gets the obsidian control settings.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="options">The hosted control options.</param>
        /// <returns></returns>
        object GetObsidianControlSettings( FinancialGateway financialGateway, HostedPaymentInfoControlOptions options );
    }
}
