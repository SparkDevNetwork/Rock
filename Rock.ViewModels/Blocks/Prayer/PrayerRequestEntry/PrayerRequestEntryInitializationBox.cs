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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Prayer.PrayerRequestEntry
{
    /// <summary>
    /// The box that contains all the initialization information for the Prayer Request Entry block.
    /// </summary>
    public class PrayerRequestEntryInitializationBox : BlockBox
    {
        /// <summary>
        /// The default value for the "Allow Comments" field.
        /// </summary>
        public bool AllowCommentsDefaultValue { get; set; }

        /// <summary>
        /// The prayer request attributes that can be edited.
        /// </summary>
        [TypeScriptType( "Record<string, PublicAttribute> | null", "import { PublicAttribute } from './publicAttribute';" )]
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// The categories the person can choose from when entering their prayer request.
        /// <para>If empty, then categories will not be shown.</para>
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// The number of characters allowed when entering a new prayer request.
        /// <para>There will be no limit if this is not a positive number.</para>
        /// </summary>
        public int CharacterLimit { get; set; }

        /// <summary>
        /// The default category to use for all new prayer requests.
        /// <para>If <see cref="Categories"/> contains this default value, then it will be selected by default.</para>
        /// </summary>
        public Guid? DefaultCategoryGuid { get; set; }

        /// <summary>
        /// The default campus of the person making the prayer request.
        /// <para>Defaults to the campus of the currently authenticated person.</para>
        /// </summary>
        public ListItemBag DefaultCampus { get; set; }

        /// <summary>
        /// The default email of the person making the prayer request.
        /// <para>Defaults to the email of the currently authenticated person.</para>
        /// </summary>
        public string DefaultEmail { get; set; }

        /// <summary>
        /// The default first name of the person making the prayer request.
        /// <para>Defaults to the first name of the currently authenticated person.</para>
        /// </summary>
        public string DefaultFirstName { get; set; }

        /// <summary>
        /// The default last name of the person making the prayer request.
        /// <para>Defaults to the last name of the currently authenticated person.</para>
        /// </summary>
        public string DefaultLastName { get; set; }

        /// <summary>
        /// The default prayer request text.
        /// <para>Defaults to the value of the "Request" page parameter.</para>
        /// </summary>
        public string DefaultRequest { get; set; }

        /// <summary>
        /// Determines if the "Allow Comments" field should be shown.
        /// </summary>
        public bool IsAllowCommentsShown { get; set; }

        /// <summary>
        /// Determines if the campus field is required.
        /// <para>If there is only one active campus, then the campus field will not show. If not show and if this is set to <c>true</c>, then the single campus is automatically used.</para>
        /// </summary>
        public bool IsCampusRequired { get; set; }

        /// <summary>
        /// Determines if the campus field is shown.
        /// <para>If there is only one active campus, then the campus field will not show.</para>
        /// </summary>
        public bool IsCampusShown { get; set; }

        /// <summary>
        /// Determines if the "Is Public" field is shown.
        /// </summary>
        public bool IsIsPublicShown { get; set; }

        /// <summary>
        /// Determines if the last name field is required.
        /// </summary>
        public bool IsLastNameRequired { get; set; }

        /// <summary>
        /// Determines if the parent page will be redirected to on successful save.
        /// </summary>
        public bool IsPageRedirectedToParentOnSave { get; set; }

        /// <summary>
        /// Determines if the current page will be refreshed on successful save.
        /// <para>This is ignored if <see cref="IsPageRedirectedToParentOnSave"/> is <c>true</c>.</para>
        /// </summary>
        public bool IsPageRefreshedOnSave { get; set; }

        /// <summary>
        /// The default value for the "Is Public" field.
        /// </summary>
        public bool IsPublicDefaultValue { get; set; }

        /// <summary>
        /// Determines if the requester info is shown.
        /// </summary>
        public bool IsRequesterInfoShown { get; set; }

        /// <summary>
        /// Determines if the "Is Urgent" field is shown.
        /// </summary>
        public bool IsUrgentShown { get; set; }

        /// <summary>
        /// The parent page URL to redirect to if <see cref="IsPageRedirectedToParentOnSave"/> is <c>true</c>.
        /// </summary>
        public string ParentPageUrl { get; set; }

        /// <summary>
        /// Determines if the "Mobile Phone" field is shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mobile phone shown; otherwise, <c>false</c>.
        /// </value>
        public bool IsMobilePhoneShown { get; set; }
    }
}
