using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.AI.Agent.Utilities;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Centralized skill for drafting and sending communications (email and SMS) in Rock.
    /// Provides LLM prompts for drafting messages and tool functions for sending them.
    /// </summary>
    [Description(
        "📩 Sending Communications" + "\r\n" +
        "- Complete the following steps in order:" + "\r\n" +
        "1. Draft an email using the DraftEmail function." + "\r\n" +
        "2. Request user approval on the draft." + "\r\n" +
        "3. Send that EXACT draft using the SendEmail function. Do not re-call the DraftEmail function."
    )]
    [AgentSkillGuid( "37DF3637-9775-4A89-9A77-BF6744232991" )]
    [EntityTypeGuid( "F67D0B02-B59F-475F-A005-8F2A5CCCA91C" )]
    internal sealed class CommunicationSkill : AgentSkillComponent
    {
        private readonly ILogger<CommunicationSkill> _logger;
        private readonly RockContext _rockContext;

        public CommunicationSkill( RockContext rockContext, ILogger<CommunicationSkill> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #region Semantic Functions

        /// <inheritdoc />
        public override IReadOnlyCollection<AgentFunction> GetSemanticFunctions()
        {
            return new List<AgentFunction>
            {
                GetDraftEmailSemanticFunction(),
                GetDraftSmsMessageSemanticFunction()
            };
        }

        private AgentFunction GetDraftEmailSemanticFunction()
        {
            return new AgentFunction
            {
                FunctionType = FunctionType.AIPrompt,
                EnableLavaPreRendering = false,
                Temperature = 0.7,
                Role = ModelServiceRole.Research,
                UsageHint =
                        "Drafts a complete email given recipient details, tone, subject hints, and relevant data. " +
                        "Use this before calling SendEmail.",
                Prompt =
@"You are drafting an email.

Context:
- Recipient Name: {{ $recipientName }}
- From Name: {{ $fromName }}
- Tone: {{ $tone }}
- Subject Hint: {{ $subjectHint }}
- Instruction: {{ $instruction }}

Relevant Data:
{{ $relevantData }}

Requirements:
- Write a clear subject line (<= 70 chars).
- Greet the recipient by name if provided.
- Keep paragraphs short and scannable.
- Close with a friendly sign-off using From Name if provided.
- Output JSON with: { ""subject"": string, ""body"": string }.

Now produce the JSON only.",
                Name = "DraftEmail",
                Guid = new Guid( "9F6E0D73-4F8C-4C77-AC33-4B5B8E58C2B2" ),
            };
        }
        private AgentFunction GetDraftSmsMessageSemanticFunction()
        {
            return new AgentFunction
            {
                FunctionType = FunctionType.AIPrompt,
                EnableLavaPreRendering = false,
                Temperature = 0.4,
                UsageHint =
                        "Drafts an SMS body (concise) given recipient details, tone, and relevant data. " +
                        "Use this before calling SendSmsMessage.",
                // Variables expected:
                // $recipientName (optional), $fromName (optional), $tone (optional),
                // $instruction, $relevantData
                Prompt =
@"You are drafting an SMS.

Context:
- Recipient Name: {{ $recipientName }}
- From Name: {{ $fromName }}
- Tone: {{ $tone }}
- Instruction: {{ $instruction }}

Relevant Data:
{{ $relevantData }}

Requirements:
- Keep it concise (<= 320 chars).
- Make it clear who it's from if needed.
- Avoid links unless explicitly present in Relevant Data.
- Output JSON: { ""body"": string }.

Now produce the JSON only.",
                Name = "DraftSmsMessage",
                Guid = new Guid( "6E160A0F-7D86-4B6A-A0E0-3B4A8B7C7F33" ),
            };

        }

        #endregion

        #region Kernel Functions  


        /// <summary>
        /// Sends an email to either a Person (via PersonKey) or a raw email address.
        /// This is a stubbed function: integrate with Rock's Communication system before enabling in production.
        /// </summary>
        [KernelFunction( "SendEmail" )]
        [AgentFunctionGuid( "0A0CE381-92A8-4FD7-8619-319C7F63AEC7" )]
        public RockFunctionResult SendEmail( SendEmailArguments options )
        {
            // If you don't actually await anything, remove 'async' and use Task.FromResult at the very end.
            // But if you plan to await later (e.g., DB, comms queue), keep async and just 'return' values.

            if ( options == null )
            {
                return RockFunctionResult.Error( "Options are required." );
            }
            else if ( options.PersonKey.IsNullOrWhiteSpace() )
            {
                return RockFunctionResult.Error( "PersonKey is required to send an email." );
            }
            else if ( options.Subject.IsNullOrWhiteSpace() || options.Body.IsNullOrWhiteSpace() )
            {
                return RockFunctionResult.Error( "Subject and Body are required." );
            }
            else if ( !options.HasUserReviewed )
            {
                return RockFunctionResult.Error( "User approval is required to send this email.", instructions: "Prompt for approval from the user with the subject and body displayed." );
            }

            var requestContext = RockRequestContextAccessor.Current;
            var currentPerson = requestContext?.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockFunctionResult.Error( "Current person is not available. Ensure the agent is properly initialized." );
            }

            if( currentPerson.Email.IsNullOrWhiteSpace() )
            {
                return RockFunctionResult.Error( "Current person does not have a valid email address." );
            }

            var personService = new PersonService( _rockContext );
            var communicationService = new CommunicationService( _rockContext );
            var person = personService.Get( options.PersonKey, false );

            if ( person == null )
            {
                return RockFunctionResult.Error( "No valid recipient found. Provide either a valid PersonKey or ToEmail." );
            }

            var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;

            // Create the communication object
            var communication = new Rock.Model.Communication
            {
                Status = CommunicationStatus.Transient,
                Recipients = new List<CommunicationRecipient>
                {
                    new CommunicationRecipient
                    {
                        PersonAliasId = person.PrimaryAliasId,
                        MediumEntityTypeId = emailMediumEntityTypeId,
                    }
                },
                SenderPersonAliasId = currentPerson.PrimaryAliasId,
                CommunicationType = CommunicationType.Email,
                Subject = options.Subject,
                Message = options.Body,
                FromEmail = currentPerson.Email,
            };
            communicationService.Add( communication );

            try
            {
                _rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Failed to save communication." );
                return RockFunctionResult.Error( "Failed to save the communication. Check the logs for details." );
            }

            communication.Status = CommunicationStatus.Approved;
            communication.ReviewedDateTime = RockDateTime.Now;
            communication.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;
            _rockContext.SaveChanges();

            var transactionMsg = new ProcessSendCommunication.Message()
            {
                CommunicationId = communication.Id
            };
            transactionMsg.Send();

            var sendCommResult = new SendCommunicationResult
            {
                CommunicationKey = communication.IdKey,
            };

            string instructions = null;

            if ( IsAuthorizedForRoute( requestContext, "/Communication/{CommunicationId}" ) )
            {
                sendCommResult.CommunicationUrl = ResolveRockUrlIncludeRoot( requestContext, $"/Communication/{communication.Id}" );
                instructions = "The user can view the communication details at the provided link.";
            }

            return RockFunctionResult.Success( sendCommResult, instructions );
        }


        /// <summary>
        /// Sends an SMS message to either a Person (via PersonKey) or a raw phone number.
        /// This is a stubbed function: integrate with your SMS transport before enabling in production.
        /// </summary>
        [KernelFunction( "SendSmsMessage" )]
        [Description(
            "🎯 Purpose:\r\n" +
            "Sends an SMS message using Rock's communication framework or an external provider.\r\n\r\n" +

            "🧭 Usage Guidance:\r\n" +
            "- Prefer PersonKey when available. If both PersonKey and PhoneNumber are provided, PersonKey wins.\r\n" +
            "- Body is required. Provide pre-drafted content from DraftSmsMessage.\r\n" +
            "- Optionally specify FromNumber or a named SMS medium if your environment requires it.\r\n\r\n" +

            "🛡 Guardrails:\r\n" +
            "1) Do not send if neither PersonKey nor PhoneNumber is provided.\r\n" +
            "2) Respect SMS length and compliance rules.\r\n" +
            "3) Do not auto-retry; surface the error for correction.\r\n"
        )]
        [AgentFunctionGuid( "355B3DA7-C3DF-4DD5-941E-C5650AD3D625" )]
        public async Task<RockFunctionResult> SendSmsMessage( SendSmsArguments options )
        {
            if ( options == null )
            {
                return RockFunctionResult.Error( "Options are required." );
            }

            if ( string.IsNullOrWhiteSpace( options.PersonKey ) && string.IsNullOrWhiteSpace( options.PhoneNumber ) )
            {
                return RockFunctionResult.Error( "Provide either PersonKey or ToEmail." );
            }
            await Task.Delay( 20 );
            return RockFunctionResult.Success( "The SMS message has been sent." );
        }

        #endregion

        #region DTOs

        public sealed class SendEmailArguments
        {
            public string PersonKey { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public bool HasUserReviewed { get; set; }
        }

        public sealed class SendSmsArguments
        {
            [Description( "Preferred. The hashed person key to route this SMS to a Rock Person." )]
            public string PersonKey { get; set; }

            [Description( "Optional fallback. E.164 formatted phone number if PersonKey is not available." )]
            public string PhoneNumber { get; set; }

            [Description( "Required. SMS body text (keep concise)." )]
            public string Body { get; set; }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Determines whether the person making the request has access to
        /// the page identified by the route.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="route">The route to be checked.</param>
        /// <returns><c>true</c> if the route was found and the requesting person is authorized; otherwise, <c>false</c>.</returns>
        private static bool IsAuthorizedForRoute( RockRequestContext context, string route )
        {
            try
            {
                // Replace any parameters in the route with fake values.
                route = new Regex( "{[^}]+}" ).Replace( route, "1" );

                // Resolve the route based on the current request.
                route = ResolveRockUrlIncludeRoot( context, route );

                // Try to parse the URL, if we can't then assume they can't
                // access the page.
                if ( !Uri.TryCreate( route, UriKind.Absolute, out var uri ) )
                {
                    return false;
                }

                // Find a page ref based on the uri.
                var pageRef = new Rock.Web.PageReference( uri, "/" );

                if ( pageRef.IsValid )
                {
                    // If a valid pageref was found, check the security of the page
                    var page = PageCache.Get( pageRef.PageId );

                    if ( page != null )
                    {
                        return page.IsAuthorized( Rock.Security.Authorization.VIEW, context.CurrentPerson );
                    }
                }
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex );
                // Log and move on...
            }

            return false;
        }

        /// <summary>
        /// Resolves the rock URL and includes the original scheme and domain
        /// from the request.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="url">The URL to ben resolved.</param>
        /// <returns>A new string resolved to the proper domain.</returns>
        private static string ResolveRockUrlIncludeRoot( RockRequestContext context, string url )
        {
            var virtualPath = context.ResolveRockUrl( url );

            if ( context.RootUrlPath.IsNotNullOrWhiteSpace() )
            {
                return $"{context.RootUrlPath}{virtualPath}";
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
        }


        #endregion
    }
}
