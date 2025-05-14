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

        public string RegistrationInstanceIdKey { get; set; }

        public string RegistrationInstanceName { get; set; }

        public Guid? RegistrationInstanceGuid { get; set; }

        public string RegistrationTemplateIdKey { get; set; }

        public Guid? RegistrationTemplateGuid { get; set; }

        public PersonBag Person { get; set; }

        //public Dictionary<string, ListItemBag> Fees { get; set; }
    }
}
