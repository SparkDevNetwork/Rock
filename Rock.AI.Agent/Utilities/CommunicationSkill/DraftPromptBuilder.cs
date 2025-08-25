using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    internal static class DraftPromptBuilder
    {
        public static string BuildEmailDraftPrompt(
            DraftRequest request )
            {
                return
        $@"You are an assistant that drafts professional, well-structured emails.

    ### Context
    - Recipient Name: {request.Recipient.FullName}
    - From Name: {request.CurrentPerson.FullName}
    - Tone: {request.Tone}
    - Guidance: {request.DraftGuidance}
    - Subject hint: {request.SubjectHint}

    ### Relevant Data
    {request.RelevantData}

    ### Requirements
    - Write a concise subject line (≤ 70 characters).
    - Greet the recipient by name.
    - Match the requested tone.
    - Keep paragraphs short and easy to scan.
    - Sign the email from name provided.
    - Do **not** include explanations, notes, or extra text.
    - Respond with **valid JSON only** in the following format:
    {{
      ""subject"": ""string"",
      ""body"": ""string""
    }}";
        }
    }
}
