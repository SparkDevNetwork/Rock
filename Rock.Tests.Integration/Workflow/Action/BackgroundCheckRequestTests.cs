﻿using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow.Action;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Workflow.Action
{
    public class BackgroundCheckRequestTests
    {
        [TestMethod]
        public void CanBeInstantiated()
        {
            Assert.That.True( new BackgroundCheckRequest() != null );
        }

        [TestMethod]
        public void HasExecuteMethod()
        {
            var BackgroundAction = new BackgroundCheckRequest();
            Assert.That.True( BackgroundAction.GetType().GetMethod( "Execute" ) != null );
        }

        [TestMethod]
        [Ignore( "new way of mocking RockContext")]
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
            Assert.That.False( result );
        }

        [TestMethod]
        [Ignore( "need way of mocking" )]
        public void CallsLoadAttributesOnActionType()
        {

        }

        [TestMethod]
        [Ignore( "new way of mocking RockContext" )]
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
            Assert.That.Single( errors );
            Assert.That.Equal( "Invalid Background Check Provider Guid!", errors[0] );
        }
    }
}