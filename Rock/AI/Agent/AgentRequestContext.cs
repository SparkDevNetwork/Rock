﻿// <copyright>
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

using Rock.Data;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Provides access to information about the current agent request.
    /// </summary>
    public abstract class AgentRequestContext
    {
        #region Properties

        /// <summary>
        /// The identifier of the <see cref="Model.AIAgent"/> that this request
        /// is being processed by.
        /// </summary>
        public abstract int? AgentId { get; }

        /// <summary>
        /// The unique identifier of the <see cref="Model.AIAgent"/> that this
        /// request is being processed by.
        /// </summary>
        public abstract Guid? AgentGuid { get; }

        /// <inheritdoc cref="Model.AIAgent.Name"/>
        public abstract string AgentName { get; }

        /// <inheritdoc cref="Model.AIAgent.AgentType"/>
        public abstract AgentType AgentType { get; }

        /// <inheritdoc cref="Model.AIAgent.AudienceType"/>
        public abstract AudienceType AudienceType { get; }

        /// <summary>
        /// The Person that is logged in and interacting with the agent.
        /// </summary>
        public abstract Model.Person CurrentPerson { get; }

        /// <summary>
        /// The root URL, such as <c>https://www.rocksolidchurch.com</c>, of
        /// the related web request. If a web request is not available then
        /// this will be either the public application root or the internal
        /// application root.
        /// </summary>
        public abstract string RootUrlPath { get; }

        /// <summary>
        /// The <see cref="RockContext"/> that can be used to query the database.
        /// This context is automatically disposed after the request is completed.
        /// This context should not be used to save changes to the database. To
        /// save changes create a new context by calling
        /// <c>RockApp.Current.CreateRockContext()</c>.
        /// </summary>
        public abstract RockContext RockContext { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of <see cref="AgentRequestContext"/>. The
        /// constructor is internal to prevent plugins from inheriting. Unit
        /// tests construct a derived context through
        /// <c>Rock.Tests.Shared.TestAccess.AI.Agent.TestAgentRequestContext</c>,
        /// which has access to this constructor; keep that type in sync with
        /// this class.
        /// </summary>
        internal AgentRequestContext()
        {
        }

        #endregion
    }
}
