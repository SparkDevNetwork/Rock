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

using System.Collections.Generic;

namespace Rock.AI.Agent.Utilities.CommunicationSkill;

/// <summary>
/// The interface for an agent communication medium. Each medium (email, sms, etc.) will implement this interface.
/// </summary>
internal interface IAgentCommunicationMedium
{
    /// <summary>
    /// Validates the recipients for the communication medium.
    /// </summary>
    /// <param name="recipient">The recipients to validate.</param>
    /// <returns>A list of validation error messages. An empty list indicates the recipients are valid.</returns>
    List<string> ValidateRecipients( List<Rock.Model.Person> recipient );

    /// <summary>
    /// Builds a drafting instruction block that is returned to the calling LLM.
    /// The LLM is expected to author the subject/body itself and then call the
    /// draft tool a second time with those values so the communication can be
    /// created as a transient draft.
    /// </summary>
    /// <param name="request">The details of the draft.</param>
    /// <returns>The instruction text the LLM should follow when composing the draft.</returns>
    string BuildDraftingInstructions( DraftRequest request );

    /// <summary>
    /// Produces a human-readable summary of the pending communication that can
    /// be shown to the user for verification before it is sent. The subject
    /// and body themselves are returned in the tool payload, so placeholders
    /// (e.g. <c>[subject]</c>, <c>[body]</c>) may be used here.
    /// </summary>
    /// <param name="currentPerson">The person sending the communication.</param>
    /// <param name="recipients">The recipients that will receive the communication.</param>
    /// <returns>The verification text to include in the tool result.</returns>
    string GetVerificationText( Rock.Model.Person currentPerson, List<Rock.Model.Person> recipients );

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
