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

using System.Collections.Generic;

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunitySignup
{
    /// <summary>
    /// A bag containing the required information to render a Connection Opportunity Signup block.
    /// </summary>
    public class ConnectionOpportunitySignupInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the pre-filled first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the pre-filled last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the pre-filled email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the pre-filled home phone.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the pre-filled mobile phone.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets whether to display the home phone field.
        /// </summary>
        public bool DisplayHomePhone { get; set; }

        /// <summary>
        /// Gets or sets whether to display the mobile phone field.
        /// </summary>
        public bool DisplayMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets whether to disable captcha support for the block.
        /// </summary>
        public bool DisableCaptchaSupport { get; set; }

        /// <summary>
        /// Gets or sets the available campuses for the opportunity.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the pre-selected campus ID.
        /// </summary>
        public int? SelectedCampusId { get; set; }

        /// <summary>
        /// Gets or sets the label to display for the comment field on the signup form.
        /// </summary>
        public string CommentFieldLabel { get; set; }

        /// <summary>
        /// Gets or sets the public editable attributes for the connection request.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
    }
}
