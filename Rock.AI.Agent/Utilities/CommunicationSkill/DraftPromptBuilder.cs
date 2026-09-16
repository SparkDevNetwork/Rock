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

namespace Rock.AI.Agent.Utilities.CommunicationSkill;

/// <summary>
/// Builds the drafting instructions returned to the calling LLM during
/// phase 1 of the two-phase draft flow. The LLM composes the subject and
/// body itself and then calls the draft tool a second time with those
/// values so the communication is persisted as a transient draft.
/// </summary>
internal static class DraftPromptBuilder
{
    /// <summary>
    /// Builds the drafting instructions for an email communication.
    /// </summary>
    /// <param name="request">The details of the draft.</param>
    /// <returns>The instruction text the LLM should follow when composing the email.</returns>
    public static string BuildEmailDraftInstructions( DraftRequest request )
    {
        var recipientsSection = "### Recipients\r\n";
        foreach ( var recipient in request.Recipients )
        {
            recipientsSection += $"- {recipient.FullName} ({recipient.Email})\r\n";
        }

        var emailSignatureSection = string.Empty;
        if ( request.EmailClosingPhrase.IsNotNullOrWhiteSpace() )
        {
            emailSignatureSection = $"- Finish the email with the following phrase: {request.EmailClosingPhrase}\r\n";
        }

        return $@"Draft a professional, well-structured email that meets the requirements below. Do not send it yet. Once the draft is ready, call the AddOrUpdateCommunicationDraft tool with the same recipientIdKey and communicationType and pass the composed content in the top-level `draftedSubject` and `draftedBody` tool parameters — do NOT place the composed content inside any other field. The tool will then persist the draft and ask the user to verify it before it is sent.
{recipientsSection}

### Context
- From Name: {request.CurrentPerson.FullName}
- Tone: {request.Tone}
- Guidance: {request.DraftGuidance}
- Subject hint: {request.SubjectHint}

### Relevant Data
{request.RelevantData}

### Requirements
- Write a concise subject line (≤ 70 characters) and return it in the top-level `draftedSubject` tool parameter.
- Return the composed body in the top-level `draftedBody` tool parameter.
{emailSignatureSection}- Greet the recipient by name. If multiple recipients, use a general greeting.
- Match the requested tone.
- Keep paragraphs short and easy to scan.
- Sign the email from the name provided.
- Do **not** include placeholders — the draft will be sent as-is once approved.";
    }

    /// <summary>
    /// Builds the drafting instructions for a push notification communication.
    /// </summary>
    /// <param name="request">The details of the draft.</param>
    /// <returns>The instruction text the LLM should follow when composing the push notification.</returns>
    public static string BuildPushDraftInstructions( DraftRequest request )
    {
        var recipientsSection = "### Recipients\r\n";
        foreach ( var recipient in request.Recipients )
        {
            recipientsSection += $"- {recipient.FullName}\r\n";
        }

        return $@"Draft a concise, engaging push notification that meets the requirements below. Do not send it yet. Once the draft is ready, call the AddOrUpdateCommunicationDraft tool with the same recipientIdKey and communicationType and pass the composed content in the top-level `draftedSubject` and `draftedBody` tool parameters — do NOT place the composed content inside any other field. The tool will then persist the draft and ask the user to verify it before it is sent.
{recipientsSection}

### Context
- From Name: {request.CurrentPerson.FullName}
- Tone: {request.Tone}
- Guidance: {request.DraftGuidance}
- Subject hint: {request.SubjectHint}

### Relevant Data
{request.RelevantData}

### Requirements
- Write a concise title (≤ 70 characters) and return it in the top-level `draftedSubject` tool parameter.
- Keep the notification body short and direct (≤ 200 characters) and return it in the top-level `draftedBody` tool parameter.
- Match the requested tone.
- Avoid greetings, signatures, or extra filler.
- Do **not** include placeholders — the draft will be sent as-is once approved.";
    }

    /// <summary>
    /// Builds the drafting instructions for an SMS communication.
    /// </summary>
    /// <param name="request">The details of the draft.</param>
    /// <param name="fromNumber">The phone number the SMS will be sent from.</param>
    /// <returns>The instruction text the LLM should follow when composing the SMS.</returns>
    public static string BuildSmsDraftInstructions( DraftRequest request, string fromNumber )
    {
        var recipientsSection = "### Recipients\r\n";
        foreach ( var recipient in request.Recipients )
        {
            recipientsSection += $"- {recipient.FullName}\r\n";
        }

        return $@"Draft a concise, engaging SMS message that meets the requirements below. Do not send it yet. Once the draft is ready, call the AddOrUpdateCommunicationDraft tool with the same recipientIdKey and communicationType and pass the composed message in the top-level `draftedBody` tool parameter — do NOT place the composed content inside any other field. SMS has no subject, so `draftedSubject` can be left empty. The tool will then persist the draft and ask the user to verify it before it is sent.
{recipientsSection}

### Context
- From Number: {fromNumber}
- Tone: {request.Tone}
- Guidance: {request.DraftGuidance}
- Subject hint: {request.SubjectHint}

### Relevant Data
{request.RelevantData}

### Requirements
- Keep the message short and direct (≤ 200 characters) and return it in the top-level `draftedBody` tool parameter.
- Match the requested tone.
- Avoid greetings, signatures, or extra filler.
- Do **not** include placeholders — the draft will be sent as-is once approved.";
    }
}
