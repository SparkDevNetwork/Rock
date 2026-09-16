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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
using Rock.Configuration;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaApplicationBuilderSkill
{
    #region Fields

    /// <summary>
    /// The most security roles returned in the full role list. Enough to
    /// cover any realistic instance; the count is reported so truncation is
    /// visible.
    /// </summary>
    private const int MaxListedSecurityRoles = 100;

    /// <summary>
    /// The most suggested matches returned for one description.
    /// </summary>
    private const int MaxSuggestedAudiences = 10;

    /// <summary>
    /// The lowest score a role can have and still be suggested. Below this
    /// the overlap is a single incidental word.
    /// </summary>
    private const int MinSuggestionScore = 15;

    /// <summary>
    /// Phrases in a description that mean the page is for everyone,
    /// including people who are not logged in.
    /// </summary>
    private static readonly Regex PublicAudiencePattern = new Regex(
        @"\b(everyone|everybody|anyone|anybody|public|anonymous|visitors?|guests?|the whole church|not logged in|unauthenticated|no login)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled );

    /// <summary>
    /// Phrases in a description that mean the page is for anyone with a
    /// login, regardless of role.
    /// </summary>
    private static readonly Regex AuthenticatedAudiencePattern = new Regex(
        @"\b(logged.?in|signed.?in|authenticated|anyone with (an account|a login)|any(one)? (registered )?user|all users|registered)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled );

    /// <summary>
    /// Words that carry no meaning when matching a description to a role
    /// name. "the staff team" should match a role named "Staff", so the
    /// filler is removed before tokens are compared.
    /// </summary>
    private static readonly HashSet<string> AudienceStopWords = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
    {
        "a", "an", "and", "any", "are", "as", "at", "be", "by", "can", "for", "from", "group", "groups", "in", "is", "it",
        "members", "of", "on", "only", "or", "our", "people", "person", "role", "roles", "security", "should", "that", "the",
        "their", "them", "they", "this", "to", "view", "who", "will", "with"
    };

    #endregion

    #region Tool(s)

    /*
        9/8/2026 - CLAUDE

        AddOrUpdateLavaApplication already answers a wrong role name with the
        list of roles to choose from, and that was meant to be all the
        discovery the agent needed. It was not. The user rarely names a
        role; they describe people ("the worship team leaders", "staff and
        campus pastors"), and the agent had to invent a role name to earn
        the recovery hint, then pick from a list with no indication of which
        entry the description meant. This tool does the mapping up front:
        it detects the two keyword audiences, scores every role's name and
        description against the words of the description, and returns the
        ranked candidates alongside the full role list so the agent can
        confirm a match with the user rather than guess.

        The scoring is deliberately simple string overlap rather than
        anything semantic. The agent is the semantic layer; this tool's job
        is to hand it exact, valid values and the evidence for each, so a
        low score is visible and gets confirmed instead of silently applied.

        Reason: Map a plain-English audience onto exact audience values.
    */
    [Description( "Lists this instance's security roles and maps a plain-English description of who a page is for onto the exact audience values AddOrUpdateLavaApplication accepts." )]
    [AgentToolPreamble( "Matching the audience to security roles." )]
    [AgentUsage( "Call this when the user describes the people a page is for rather than naming a security role, and before passing audiences to AddOrUpdateLavaApplication. Pass the user's own words as audienceDescription." )]
    [AgentUsage( "suggestedAudiences is ranked; a score of 100 is an exact role name, and anything under 50 is a guess. Confirm guesses with the user, naming the role and its description, before granting it. When several suggestions each cover part of the description (for example 'staff and campus pastors' matching two roles), pass all of them in the audiences list." )]
    [AgentUsage( "securityRoles is the complete list of valid role names. If no suggestion fits, choose from it with the user rather than passing a name that is not in the list." )]
    [AgentUsage( "A 'Public' suggestion means the description sounded like everyone, including anonymous visitors. Treat it as a question for the user, not a decision; only the user can choose to make read endpoints public." )]
    [AgentToolGuid( "248182EA-2B76-4127-A864-BE48F8E15585" )]
    public AgentToolResult ResolveAudience(
        [Description( "Who the page is for, in the user's own words, such as 'the worship team leaders' or 'anyone who is logged in'. Optional; omit to list the security roles without matching." )]
        string audienceDescription = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        // No authorization gate beyond the skill's own security: this reads
        // role names and descriptions, which the AddOrUpdateLavaApplication
        // error path already discloses to the same caller.
        var roles = GetSecurityRoles( rockContext );

        var keywordAudiences = new List<AudienceMatchResult>
        {
            new AudienceMatchResult
            {
                Audience = PublicAudienceKeyword,
                Description = "everyone, including anonymous visitors",
                Score = 0,
                Reason = "Always available. Use only when the user explicitly wants the data visible without logging in."
            },
            new AudienceMatchResult
            {
                Audience = AllAuthenticatedAudienceKeyword,
                Description = "anyone who is logged in",
                Score = 0,
                Reason = "Always available. Use when the page is for any logged-in person regardless of role."
            }
        };

        var suggestions = audienceDescription.IsNotNullOrWhiteSpace()
            ? GetAudienceSuggestions( audienceDescription, roles )
            : new List<AudienceMatchResult>();

        var result = new AudienceResolutionResult
        {
            AudienceDescription = audienceDescription,
            SuggestedAudiences = suggestions,
            KeywordAudiences = keywordAudiences,
            SecurityRoles = roles
                .Take( MaxListedSecurityRoles )
                .Select( r => new SecurityRoleResult
                {
                    Name = r.Name,
                    Description = r.Description
                } )
                .ToList(),
            TotalSecurityRoleCount = roles.Count
        };

        var toolResult = Success( result );

        if ( audienceDescription.IsNullOrWhiteSpace() )
        {
            return toolResult.WithInstructions( $"No description was given, so nothing was matched. Choose from securityRoles or the keyword audiences and pass the exact values to {nameof( AddOrUpdateLavaApplication )}." );
        }

        if ( !suggestions.Any() )
        {
            return toolResult.WithInstructions( $"Nothing in this instance resembled the description. Show the user the securityRoles list and ask which roles the page is for, or whether it should be open to anyone who is logged in. Do not choose 'Public' for them." );
        }

        var bestScore = suggestions.Max( s => s.Score );
        var confidence = bestScore >= 50
            ? "The top suggestion is a strong match. State it to the user as your plan and proceed unless they object."
            : "Every suggestion is a guess. Ask the user which of these roles the page is for before granting any of them.";

        return toolResult.WithInstructions( $"{confidence} Pass the chosen audience values, exactly as returned, in the audiences list of {nameof( AddOrUpdateLavaApplication )}. If the description covers more than one group, pass every matching role." );
    }

    #endregion

    #region Support Methods

    /// <summary>
    /// Scores the keyword audiences and every security role against the
    /// description and returns the ranked candidates.
    /// </summary>
    /// <param name="audienceDescription">The user's description of the audience.</param>
    /// <param name="roles">The active security roles.</param>
    /// <returns>The candidates worth showing, highest score first.</returns>
    private static List<AudienceMatchResult> GetAudienceSuggestions( string audienceDescription, List<SecurityRole> roles )
    {
        var suggestions = new List<AudienceMatchResult>();
        var trimmedDescription = audienceDescription.Trim();

        if ( PublicAudiencePattern.IsMatch( trimmedDescription ) )
        {
            suggestions.Add( new AudienceMatchResult
            {
                Audience = PublicAudienceKeyword,
                Description = "everyone, including anonymous visitors",
                Score = 90,
                Reason = "The description sounds like everyone, including people who are not logged in. Confirm with the user before making read endpoints public."
            } );
        }

        if ( AuthenticatedAudiencePattern.IsMatch( trimmedDescription ) )
        {
            suggestions.Add( new AudienceMatchResult
            {
                Audience = AllAuthenticatedAudienceKeyword,
                Description = "anyone who is logged in",
                Score = 90,
                Reason = "The description sounds like any logged-in person, regardless of role."
            } );
        }

        var descriptionTokens = GetAudienceTokens( trimmedDescription );

        foreach ( var role in roles )
        {
            var score = ScoreRole( trimmedDescription, descriptionTokens, role, out var reason );

            if ( score < MinSuggestionScore )
            {
                continue;
            }

            suggestions.Add( new AudienceMatchResult
            {
                Audience = role.Name,
                Description = $"members of the '{role.Name}' security role",
                Score = score,
                Reason = reason
            } );
        }

        return suggestions
            .OrderByDescending( s => s.Score )
            .ThenBy( s => s.Audience )
            .Take( MaxSuggestedAudiences )
            .ToList();
    }

    /// <summary>
    /// Scores one role against the description. Exact and whole-phrase
    /// matches on the name outrank word overlap, and overlap with the name
    /// outranks overlap with the role's description, because the name is
    /// what the user is most likely echoing.
    /// </summary>
    /// <param name="description">The trimmed audience description.</param>
    /// <param name="descriptionTokens">The meaningful words of the description.</param>
    /// <param name="role">The role to score.</param>
    /// <param name="reason">The evidence for the score, for the agent to relay.</param>
    /// <returns>A score from 0 to 100.</returns>
    private static int ScoreRole( string description, HashSet<string> descriptionTokens, SecurityRole role, out string reason )
    {
        reason = null;

        if ( role.Name.Equals( description, StringComparison.OrdinalIgnoreCase ) )
        {
            reason = "The description is exactly this role's name.";

            return 100;
        }

        // The whole description inside the name, or the whole name inside
        // the description, is as good as it gets short of an exact match:
        // "staff workers" for "RSR - Staff Workers", or "the RSR - Staff
        // Workers role" for the same.
        if ( role.Name.IndexOf( description, StringComparison.OrdinalIgnoreCase ) >= 0
            || description.IndexOf( role.Name, StringComparison.OrdinalIgnoreCase ) >= 0 )
        {
            reason = $"The description contains, or is contained in, the role name '{role.Name}'.";

            return 80;
        }

        if ( descriptionTokens.Count == 0 )
        {
            return 0;
        }

        var nameTokens = GetAudienceTokens( role.Name );
        var roleDescriptionTokens = GetAudienceTokens( role.Description );

        var nameOverlap = descriptionTokens.Count( t => nameTokens.Contains( t ) );
        var descriptionOverlap = descriptionTokens.Count( t => roleDescriptionTokens.Contains( t ) );

        if ( nameOverlap == 0 && descriptionOverlap == 0 )
        {
            return 0;
        }

        // Name overlap is worth up to 60, description overlap up to 20, each
        // scaled by how much of the user's description was accounted for.
        var score = ( int ) Math.Round(
            60.0 * nameOverlap / descriptionTokens.Count
            + 20.0 * descriptionOverlap / descriptionTokens.Count );

        var matchedWords = descriptionTokens
            .Where( t => nameTokens.Contains( t ) || roleDescriptionTokens.Contains( t ) )
            .OrderBy( t => t )
            .ToList();

        reason = nameOverlap > 0
            ? $"The role name shares the word(s) {string.Join( ", ", matchedWords.Select( w => $"'{w}'" ) )} with the description."
            : $"Only the role's description shares the word(s) {string.Join( ", ", matchedWords.Select( w => $"'{w}'" ) )} with the description.";

        return Math.Min( score, 79 );
    }

    /// <summary>
    /// Splits text into the lower-cased words that matter for matching,
    /// dropping punctuation, short fragments and filler words.
    /// </summary>
    /// <param name="text">The text to tokenize, or <c>null</c>.</param>
    /// <returns>The distinct meaningful words.</returns>
    private static HashSet<string> GetAudienceTokens( string text )
    {
        var tokens = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

        if ( text.IsNullOrWhiteSpace() )
        {
            return tokens;
        }

        foreach ( Match match in Regex.Matches( text, @"[A-Za-z0-9]+" ) )
        {
            var token = match.Value;

            if ( token.Length < 3 || AudienceStopWords.Contains( token ) )
            {
                continue;
            }

            // A crude plural fold so "pastors" meets "Pastor".
            if ( token.EndsWith( "s", StringComparison.OrdinalIgnoreCase ) && token.Length > 3 )
            {
                token = token.Substring( 0, token.Length - 1 );
            }

            tokens.Add( token );
        }

        return tokens;
    }

    #endregion
}
