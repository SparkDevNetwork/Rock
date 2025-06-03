using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.SemanticKernel;

using Rock.Enums.AI.Agent;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    [Description( "Used for managing groups, such as adding or removing members, and getting group information." )]
    [AiSkillGuid( "489e96d7-c66c-4683-b76b-92fbfda372f4" )]
    internal class GroupManagerSkill : IRockAiSkill
    {
        private AgentRequestContext _requestContext;

        public GroupManagerSkill( AgentRequestContext requestContext )
        {
            _requestContext = requestContext;
        }

        #region Native Functions

        [KernelFunction( "GetGroupInformation" )]
        [Description( "Gets information for a specific type of group." )]
        [AiFunctionGuid( "14e9d63d-4c37-4b1d-a400-3a4aca79be2b" )]
        public string GetGroupData( string input )
        {
            // Very basic logic for demo purposes
            if ( input.ToLower().Contains( "youth" ) )
            {
                return "Youth Group meets every Wednesday at 6 PM. Led by Sarah. Average attendance: 25.";
            }

            return "Sorry, I couldn't find info on that group.";
        }

        [KernelFunction( "GroupMemberOperations" )]
        [Description( "Adds, modifies or removes a person from a group." )]
        [AiFunctionGuid( "468688c4-86ad-401f-ab46-ac0875de2452" )]
        public string GroupMemberOperations( Kernel kernel, int personId, int groupId,
            [Description( "The operation to preform (Add|Update|Delete)." )] GroupMemberOperation operation, int groupMemberRole = 0 )
        {
            var test = _requestContext.ChatHistory.FirstOrDefault().Content;
            return @"If the role provided does not exist for the group return a list of valid roles. Otherwise
                Confirm that they have been added to the group being sure to state the role that was used.";
        }

        [KernelFunction( "DescribeGroup" )]
        [Description( "Describes a group." )]
        [AiFunctionGuid( "3e1d8c57-8599-48d2-a5dd-a037bc194c00" )]
        public string DescribeGroup( KernelArguments args, string promptSummary )
        {
            return $"You have been {promptSummary}.";
        }

        [KernelFunction( "GetGroup" )]
        [Description( "Gets the group information." )]
        [AiFunctionGuid( "fc857adf-1db7-4eb3-9486-f19c00e4663e" )]
        public string GetGroup( string input )
        {
            return "Please provide the group id.";
        }

        #endregion

        /// <summary>
        /// Listing of semantic functions that this skill provides.
        /// </summary>
        /// <returns></returns>
        public List<AgentFunction> GetSemanticFunctions()
        {
            return new List<AgentFunction> {
                new AgentFunction {
                    Name = "GetGroupInsights",
                    UsageHint = "Describes the group",
                    Prompt = "Say it's cool.",
                    Temperature = 0.3,
                    Role = ModelServiceRole.Default,
                    EnableLavaPreRendering = true
                },
                new AgentFunction {
                    Name = "GetEmojiForGroup",
                    UsageHint = "Returns an emoji for the group.",
                    Prompt = "Return an emoji related to the group name.",
                    EnableLavaPreRendering = false
                },
            };
        }

        public enum GroupMemberOperation { Add, Update, Delete }
    }
}
