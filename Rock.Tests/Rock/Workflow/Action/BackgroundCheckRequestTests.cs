using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow.Action;
using System.Collections.Generic;
using Xunit;

namespace Rock.Tests.Workflow.Action
{
    public class BackgroundCheckRequestTests
    {
        [Fact]
        public void CanBeInstantiated()
        {
            Assert.True( new BackgroundCheckRequest() != null );
        }

        [Fact]
        public void HasExecuteMethod()
        {
            var BackgroundAction = new BackgroundCheckRequest();
            Assert.True( BackgroundAction.GetType().GetMethod( "Execute" ) != null );
        }

        [Fact (Skip = "NS - Skipping because of error with APPVYR-WIN user login")]
        public void ReturnsFalse()
        {
            var BackgroundAction = new BackgroundCheckRequest();
            var action = new WorkflowAction
            {
                ActionType = new WorkflowActionType()
                {
                    // add in the expected objects so we don't make a db request
                    // in the future the db connection should be mocked
                    Attributes = new Dictionary<string, AttributeCache>(),
                    AttributeValues = new Dictionary<string, AttributeValueCache>()
                }
            };
            var errors = new List<string>();
            var result = BackgroundAction.Execute( new RockContext(), action, null, out errors );
            Assert.False( result );
        }

        [Fact (Skip = "need way of mocking")]
        public void CallsLoadAttributesOnActionType()
        {

        }

        [Fact( Skip = "NS - Skipping because of error with APPVYR-WIN user login" )]
        public void InvalidProviderGuidError()
        {
            var BackgroundAction = new BackgroundCheckRequest();
            var action = new WorkflowAction
            {
                ActionType = new WorkflowActionType()
                {
                    // add in the expected objects so we don't make a db request
                    Attributes = new Dictionary<string, AttributeCache>(),
                    AttributeValues = new Dictionary<string, AttributeValueCache>()
                }
            };
            var errors = new List<string>();
            var result = BackgroundAction.Execute( new RockContext(), action, null, out errors );
            Assert.Equal( errors.Count, 1 );
            Assert.Equal( errors[0], "Invalid Background Check Provider Guid!" );
        }
    }
}