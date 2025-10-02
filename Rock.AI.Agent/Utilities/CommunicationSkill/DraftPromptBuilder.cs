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

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    internal static class DraftPromptBuilder
    {
        public static string BuildEmailDraftPrompt(
            DraftRequest request )
        {

            string recipientsSection = "### Recipients\r\n";
            foreach ( var recipient in request.Recipients )
            {
                recipientsSection += $"- {recipient.FullName} ({recipient.Email})\r\n";
            }

            string emailSignatureSection = string.Empty;
            if( request.EmailClosingPhrase.IsNotNullOrWhiteSpace() )
            {
                emailSignatureSection = $"- Finish the email with the following phrase: {request.EmailClosingPhrase}\r\n";
            }

            return $@"You are an assistant that drafts professional, well-structured emails.
{recipientsSection}

### Context
- From Name: {request.CurrentPerson.FullName}
- Tone: {request.Tone}
- Guidance: {request.DraftGuidance}
- Subject hint: {request.SubjectHint}

### Relevant Data
{request.RelevantData}

### Requirements
- Write a concise subject line (≤ 70 characters).
{emailSignatureSection}
- Greet the recipient by name. If multiple recipients, use a general greeting.
- Match the requested tone.
- Keep paragraphs short and easy to scan.
- Sign the email from name provided.
- Do **not** include explanations, notes, or extra text.
- This email is intended to be directly sent, so ensure there are no placeholders.
- Respond with **valid JSON only** in the following format:
{{
    ""subject"": ""string"",
    ""body"": ""string""
}}";
        }

        public static string BuildPushDraftPrompt(
           DraftRequest request )
        {
            string recipientsSection = "### Recipients\r\n";
            foreach ( var recipient in request.Recipients )
            {
                recipientsSection += $"- {recipient.FullName}\r\n";
            }

            return
$@"You are an assistant that drafts concise, engaging push notifications.
{recipientsSection}

### Context
- From Name: {request.CurrentPerson.FullName}
- Tone: {request.Tone}
- Guidance: {request.DraftGuidance}
- Subject hint: {request.SubjectHint}

### Relevant Data
{request.RelevantData}

### Requirements
- Write a concise subject line (≤ 70 characters).
- Keep the notification short and direct (≤ 200 characters).
- Match the requested tone.
- Avoid greetings, signatures, or extra filler.
- Do **not** include explanations, notes, or extra text.
- This push notification is intended to be directly sent, so ensure there are no placeholders.
    - Respond with **valid JSON only** in the following format:
    {{
      ""subject"": ""string"",
      ""body"": ""string""
    }}";
        }

        public static string BuildSmsDraftPrompt(
   DraftRequest request, string fromNumber )
        {
            string recipientsSection = "### Recipients\r\n";
            foreach ( var recipient in request.Recipients )
            {
                recipientsSection += $"- {recipient.FullName}\r\n";
            }

            return
$@"You are an assistant that drafts concise, engaging SMS messages.
{recipientsSection}

### Context
- From Number: {fromNumber}
- Tone: {request.Tone}
- Guidance: {request.DraftGuidance}
- Subject hint: {request.SubjectHint}

### Relevant Data
{request.RelevantData}

### Requirements
- Keep the message short and direct (≤ 200 characters).
- Match the requested tone.
- Avoid greetings, signatures, or extra filler.
- Do **not** include explanations, notes, or extra text.
- This message is intended to be directly sent, so ensure there are no placeholders.
- Respond with **valid JSON only** in the following format:
{{
    ""subject"": ""string"",
    ""body"": ""string""
}}";
        }
    }
}
