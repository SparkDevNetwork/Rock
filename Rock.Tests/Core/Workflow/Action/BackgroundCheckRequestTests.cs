using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;
using Rock.Workflow.Action;

namespace Rock.Tests.Core.Workflow.Action
{
    [TestClass]
    [TestCategory( "Core.Workflow" )]
    public class BackgroundCheckRequestTests
    {
        [TestMethod]
        public void CanBeInstantiated()
        {
            Assert.IsNotNull( new BackgroundCheckRequest() );
        }

        [TestMethod]
        public void HasExecuteMethod()
        {
            var BackgroundAction = new BackgroundCheckRequest();
            Assert.IsTrue( BackgroundAction.GetType().GetMethod( "Execute" ) != null );
        }

        [TestMethod]
        public void ReturnsFalse()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();

            var BackgroundAction = new BackgroundCheckRequest();
            var action = new WorkflowAction
            {
                ActionType = new WorkflowActionType()
                {
                    // Add in the expected objects so we don't make a db request.
                    Attributes = new Dictionary<string, AttributeCache>(),
                    AttributeValues = new Dictionary<string, AttributeValueCache>()
                }
            };
            var errors = new List<string>();
            var result = BackgroundAction.Execute( rockContext, action, null, out errors );
            Assert.IsFalse( result );
        }

        [TestMethod]
        public void InvalidProviderGuidError()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();

            var BackgroundAction = new BackgroundCheckRequest();
            var action = new WorkflowAction
            {
                ActionType = new WorkflowActionType()
                {
                    // Add in the expected objects so we don't make a db request.
                    Attributes = new Dictionary<string, AttributeCache>(),
                    AttributeValues = new Dictionary<string, AttributeValueCache>()
                }
            };
            var errors = new List<string>();
            var result = BackgroundAction.Execute( rockContext, action, null, out errors );
            Assert.ContainsSingle( errors );
            Assert.AreEqual( "Invalid Background Check Provider Guid!", errors[0] );
        }
    }
}
