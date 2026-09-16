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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonLeft
{
    /// <summary>
    /// Contains all the initial configuration data required to render the
    /// Check-in Manager Person Profile (limited) block for a single person.
    /// </summary>
    public class PersonLeftInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block content should be
        /// rendered. This is <c>false</c> when no person could be resolved or
        /// the current user is not authorized to view the block.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the person being viewed. Passed back to
        /// the block actions that need to identify the person.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person being viewed.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered <c>&lt;img&gt;</c> tag for the
        /// person's profile photo, sized to 200x200. Rendered with v-html so
        /// the markup matches the WebForms output.
        /// </summary>
        public string PhotoImageTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has an
        /// uploaded photo. Drives whether the photo container is wrapped in
        /// the click-to-enlarge anchor.
        /// </summary>
        public bool HasPhoto { get; set; }

        /// <summary>
        /// Gets or sets the URL used as the click-to-enlarge target when
        /// the person has an uploaded photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the campus name displayed as a highlight label. Null
        /// hides the label.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the absolute URL of the person edit page that the
        /// share button posts to the Web Share API. Null hides the share
        /// button.
        /// </summary>
        public string SharePersonUrl { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered email tag HTML for the person. Null
        /// hides the email row.
        /// </summary>
        public string EmailTagHtml { get; set; }

        /// <summary>
        /// Gets or sets the list of phone numbers to display, in the order
        /// they should be shown.
        /// </summary>
        public List<PersonLeftPhoneNumberBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the adult attribute values shown when the person's
        /// age classification is Adult or Unknown and the block is configured
        /// with an adult attribute category. Empty hides the section.
        /// </summary>
        public List<PersonLeftAttributeBag> AdultAttributes { get; set; }

        /// <summary>
        /// Gets or sets the child attribute values shown when the person's
        /// age classification is Child or Unknown and the block is configured
        /// with a child attribute category. Empty hides the section.
        /// </summary>
        public List<PersonLeftAttributeBag> ChildAttributes { get; set; }

        /// <summary>
        /// Gets or sets the other family members displayed as tiles. Empty
        /// hides the family panel.
        /// </summary>
        public List<PersonLeftRelatedPersonBag> FamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the related people (known-relationship inverses that
        /// can check the person in) displayed as tiles. Empty hides the
        /// related-people panel.
        /// </summary>
        public List<PersonLeftRelatedPersonBag> RelatedPeople { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS send feature is
        /// available for this render. False hides every SMS icon on the phone
        /// list and prevents the modal from being opened.
        /// </summary>
        public bool IsSmsAvailable { get; set; }
    }
}
