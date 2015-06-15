// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System file types.
    /// </summary>
    public class Category
    {

        #region History Categories

        /// <summary>
        /// History changes for person
        /// </summary>
        public const string HISTORY_PERSON = "6F09163D-7DDD-4E1E-8D18-D7CAA04451A7";

        /// <summary>
        /// History of person demographic changes
        /// </summary>
        public const string HISTORY_PERSON_DEMOGRAPHIC_CHANGES = "51D3EC5A-D079-45ED-909E-B0AB2FD06835";

        /// <summary>
        /// History of Family changes
        /// </summary>
        public const string HISTORY_PERSON_FAMILY_CHANGES = "5C4CCE5A-D7D0-492F-A241-96E13A3F7DF8";

        /// <summary>
        /// history of group membership
        /// </summary>
        public const string HISTORY_PERSON_GROUP_MEMBERSHIP = "325278A4-FACA-4F38-A405-9C090B3BAA34";

        /// <summary>
        /// History of person communications
        /// </summary>
        public const string HISTORY_PERSON_COMMUNICATIONS = "F291034B-7581-48F3-B522-E31B8534D529";

        /// <summary>
        /// History of person activity
        /// </summary>
        public const string HISTORY_PERSON_ACTIVITY = "0836845E-5ED8-4ABE-8787-3B61EF2F0FA5";

        /// <summary>
        /// History of changes to batches
        /// </summary>
        public const string HISTORY_FINANCIAL_BATCH = "AF6A8CFF-F24F-4AA8-B126-94B6903961C0";

        /// <summary>
        /// History of changes to transaction and/or transaction details
        /// </summary>
        public const string HISTORY_FINANCIAL_TRANSACTION = "477EE3BE-C68F-48BD-B218-FAFC99AF56B3";

        #endregion

        #region Schedule Categories

        /// <summary>
        /// Gets the Service Times schedule category guid
        /// </summary>
        public const string SCHEDULE_SERVICE_TIMES = "4FECC91B-83F9-4269-AE03-A006F401C47E";

        /// <summary>
        /// Gets the Metrics schedule category guid
        /// </summary>
        public const string SCHEDULE_METRICS = "5A794741-5444-43F0-90D7-48E47276D426";

        #endregion

        #region Person Attributes

        /// <summary>
        /// The person attributes Social guid
        /// </summary>
        public const string PERSON_ATTRIBUTES_SOCIAL = "DD8F467D-B83C-444F-B04C-C681167046A1";

        #endregion

        #region System Email Categories

        /// <summary>
        /// The System Email Workflow Category guid
        /// </summary>
        public const string SYSTEM_EMAIL_WORKFLOW = "C7B9B5F1-9D90-485F-93E4-5D7D81EC2B12";

        #endregion

        #region Merge Template Categories

        /// <summary>
        /// The Personal Merge Template Category guid
        /// </summary>
        public const string PERSONAL_MERGE_TEMPLATE = "A9F2F544-660B-4176-ACAD-88898416A66E";
        
        #endregion

    }
}