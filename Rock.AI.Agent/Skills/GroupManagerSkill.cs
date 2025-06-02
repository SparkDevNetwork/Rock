using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.SemanticKernel;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Skills
{
    [Description( "Used for managing groups, such as adding or removing members, and getting group information." )]
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
        public string GroupMemberOperations( Kernel kernel, int personId, int groupId,
            [Description( "The operation to preform (Add|Update|Delete)." )] GroupMemberOperation operation, int groupMemberRole = 0 )
        {
            var test = _requestContext.ChatHistory.FirstOrDefault().Content;
            return @"If the role provided does not exist for the group return a list of valid roles. Otherwise
                Confirm that they have been added to the group being sure to state the role that was used.";
        }

        [KernelFunction( "DescribeGroup" )]
        [Description( "Describes a group." )]
        public string DescribeGroup( KernelArguments args, string promptSummary )
        {
            return $"You have been {promptSummary}.";
        }

        [KernelFunction( "GetGroup" )]
        [Description( "Gets the group information." )]
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
