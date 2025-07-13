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

using Rock.Model;
using Rock.ViewModels.Blocks.Group.GroupPlacement;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Event.RegistrationEntry
{
    /// <summary>
    /// Details about a registramt record that is transmitted over
    /// the RealTime engine.
    /// </summary>
    public class RegistrationRegistrantUpdatedMessageBag
    {
        /// <summary>
        /// Gets or sets the registrant unique identifier.
        /// </summary>
        /// <value>The registrant unique identifier.</value>
        public Guid RegistrantGuid { get; set; }

        /// <summary>
        /// Gets or sets the registrant encrypted identifier.
        /// </summary>
        /// <value>The registrant encrypted identifier.</value>
        public string RegistrantIdKey { get; set; }

        /// <summary>
        /// Gets or sets the registration instance encrypted identifier.
        /// </summary>
        /// <value>The registration instance ID key.</value>
        public string RegistrationInstanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the registration instance.
        /// </summary>
        /// <value>The registration instance name.</value>
        public string RegistrationInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the registration instance.
        /// </summary>
        /// <value>The registration instance GUID.</value>
        public Guid? RegistrationInstanceGuid { get; set; }

        /// <summary>
        /// Gets or sets the registration template encrypted identifier.
        /// </summary>
        /// <value>The registration template ID key.</value>
        public string RegistrationTemplateIdKey { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the registration template.
        /// </summary>
        /// <value>The registration template GUID.</value>
        public Guid? RegistrationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the person associated with the registrant.
        /// </summary>
        /// <value>The person data.</value>
        public PersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets the list of fees associated with the registrant.
        /// </summary>
        /// <value>The registrant's fees.</value>
        public Dictionary<string, ListItemBag> Fees { get; set; }
    }
}
