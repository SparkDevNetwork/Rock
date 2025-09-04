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
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

using Rock.Data;

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    /// <summary>
    /// The interface for an agent communication medium. Each medium (email, sms, etc.) will implement this interface.
    /// </summary>
    internal interface IAgentCommunicationMedium
    {
        /// <summary>
        /// Validates the recipients for the communication medium.
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns></returns>
        List<string> ValidateRecipients( List<Rock.Model.Person> recipient );

        /// <summary>
        /// Drafts the commmunication content by invoking an internal prompt via the kernel.
        /// </summary>
        /// <param name="kernel">The kernel to execute the prompt on.</param>
        /// <param name="request">The details of the draft.</param>
        /// <returns></returns>
        Task<DraftResult> DraftAsync(
            IChatAgent agent,
            DraftRequest request );

        /// <summary>
        /// Structures the communication entity from the draft content.
        /// </summary>
        /// <param name="request">The request associated with the draft.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="content">The content of the draft.</param>
        /// <returns></returns>
        Rock.Model.Communication BuildCommunication(
            DraftRequest request,
            List<Rock.Model.Person> recipients,
            DraftResult content );

        /// <summary>
        /// Updates the communication entity from the draft content.
        /// </summary>
        /// <param name="request">The request associated with the draft.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="content">The content of the draft.</param>
        /// <returns></returns>
        Rock.Model.Communication UpdateCommunication(
            DraftRequest request,
            List<Rock.Model.Person> recipients,
            Rock.Model.Communication communication,
            DraftResult content );
    }
}
