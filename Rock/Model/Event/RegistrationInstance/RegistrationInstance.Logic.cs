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

using System.Collections.Generic;
using Rock.Communication;
using Rock.Security;

namespace Rock.Model
{
    public partial class RegistrationInstance
    {
        /// <summary>
        /// Gets the contact recipient as either an email to the person that registered, or as an anonymous email to the specified contact email if it is different than the person's email.
        /// </summary>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <returns></returns>
        public RockMessageRecipient GetContactRecipient( Dictionary<string, object> mergeObjects )
        {
            var person = this.ContactPersonAlias?.Person;
            string personEmail = person?.Email;

            var contactEmail = this.ContactEmail;
            if ( personEmail == contactEmail )
            {
                return new RockEmailMessageRecipient( person, mergeObjects );
            }
            else
            {
                return RockEmailMessageRecipient.CreateAnonymous( contactEmail, mergeObjects );
            }
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                return RegistrationTemplate != null ? RegistrationTemplate : base.ParentAuthority;
            }
        }
    }
}
