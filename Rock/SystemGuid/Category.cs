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

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Categories
    /// </summary>
    public class Category
    {
        #region CMS Categories

        /// <summary>
        /// Marketing Category Guid
        /// </summary>
        public const string MARKETING = "FE0AE3EB-B648-4264-A222-96CEA3A25E20";

        #endregion

        #region Dataview Categories

        /// <summary>
        /// Communication Segments Dataview Category Guid
        /// </summary>
        public const string DATAVIEW_COMMUNICATION_SEGMENTS = "FF7081F8-7223-43D4-BE28-CB030DC4E13B";

        #endregion DataView Categories

        #region Defined Type Categories

        /// <summary>
        /// Campus Category
        /// </summary>
        public const string DEFINEDTYPE_CAMPUS = "4503C83E-4171-4EAE-99C1-A3647E065181";

        /// <summary>
        /// CMS Category
        /// </summary>
        public const string DEFINEDTYPE_CMS = "6B1B3106-B7F5-BBA2-4F81-8300F21F6661";

        /// <summary>
        /// CMS Settings Category
        /// </summary>
        public const string DEFINEDTYPE_CMS_SETTINGS = "262313F6-5D66-41CE-9B6F-D36567D9AB9D";

        #endregion

        #region History Categories

        /// <summary>
        /// History changes for an event registration
        /// </summary>
        public const string HISTORY_EVENT_REGISTRATION = "813DF1A5-ADBD-481C-AC1D-884F0FA7AE77";

        /// <summary>
        /// History changes for person
        /// </summary>
        public const string HISTORY_PERSON = "6F09163D-7DDD-4E1E-8D18-D7CAA04451A7";

        /// <summary>
        /// The history attendance changes for a person
        /// </summary>
        public const string HISTORY_ATTENDANCE_CHANGES = "BF6ABCD3-AD41-4D54-998F-B83C302756E3";

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
        /// History of connection request
        /// </summary>
        public const string HISTORY_PERSON_CONNECTION_REQUEST = "4B472C6A-1ACF-481E-A2D4-9C44436BBCF5";

        /// <summary>
        /// History of step
        /// </summary>
        public const string HISTORY_PERSON_STEP = "517BB2D3-0A50-4132-818E-63BB3C81EAE9";

        /// <summary>
        /// History of person communications
        /// </summary>
        public const string HISTORY_PERSON_COMMUNICATIONS = "F291034B-7581-48F3-B522-E31B8534D529";

        /// <summary>
        /// History of person activity
        /// </summary>
        public const string HISTORY_PERSON_ACTIVITY = "0836845E-5ED8-4ABE-8787-3B61EF2F0FA5";

        /// <summary>
        /// History of person registering or being registered
        /// </summary>
        public const string HISTORY_PERSON_REGISTRATION = "DA9C0CC7-7B31-4E1E-BBA5-50405B2D9EFE";

        /// <summary>
        /// History of changes to batches
        /// </summary>
        public const string HISTORY_FINANCIAL_BATCH = "AF6A8CFF-F24F-4AA8-B126-94B6903961C0";

        /// <summary>
        /// History of changes to transaction and/or transaction details
        /// </summary>
        public const string HISTORY_FINANCIAL_TRANSACTION = "477EE3BE-C68F-48BD-B218-FAFC99AF56B3";

        /// <summary>
        /// The history person analytics
        /// </summary>
        public const string HISTORY_PERSON_ANALYTICS = "C1524D2E-3E8F-3D83-45F8-526B749D79F0";

        /// <summary>
        /// Parent group for Group History
        /// </summary>
        public const string HISTORY_GROUP = "180C5767-8769-4C51-865F-FEE29AEF80A0";

        /// <summary>
        /// History of group changes
        /// </summary>
        public const string HISTORY_GROUP_CHANGES = "089EB47D-D0EF-493E-B867-DC51BCDEF319";

        /// <summary>
        /// History of connection request changes
        /// </summary>
        public const string HISTORY_CONNECTION_REQUEST = "A8542DD2-91B1-4CCA-873A-D052BCD6EE06";

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

        /// <summary>
        /// Gets the DataView Persisted Schedule category guid
        /// </summary>
        public const string SCHEDULE_PERSISTED_DATAVIEWS = "EEC7A935-BEF2-4450-9CBF-B85CEC6F7FEA";

        /// <summary>
        /// Gets the DataView Persisted Schedule category guid
        /// </summary>
        public const string SCHEDULE_PERSISTED_DATASETS = "7D152006-C47D-47E6-BF0B-09C3C2D0AE84";

        #endregion

        #region Person Attributes

        /// <summary>
        /// The category for person attributes Social guid
        /// </summary>
        public const string PERSON_ATTRIBUTES_SOCIAL = "DD8F467D-B83C-444F-B04C-C681167046A1";

        /// <summary>
        /// The category for person attribute for ERA
        /// </summary>
        public const string PERSON_ATTRIBUTES_ERA = "266A1EA8-425C-7BB0-4191-C2E234D60086";

        /// <summary>
        /// The category for person attribute for Finance Internal
        /// </summary>
        public const string PERSON_ATTRIBUTES_FINANCE_INTERNAL = "3030B3E2-91EE-4833-BACE-47F775FA98A0";

        /// <summary>
        /// The category for person attributes for DISC
        /// </summary>
        public const string PERSON_ATTRIBUTES_DISC = "0B187C81-2106-4875-82B6-FBF1277AE23B";

        /// <summary>
        /// The category for the Check-in Manager Roster Alert Icons
        /// </summary>
        public const string PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON = "367571D1-62D3-4948-B588-C0FDCE00CF27";

        /// <summary>
        /// The category for person attributes for Personality Assessment Data
        /// </summary>
        public const string PERSON_ATTRIBUTES_PERSONALITY_ASSESSMENT_DATA = "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969";

        /// <summary>
        /// Obsolete. Use <see cref="PERSON_ATTRIBUTES_GIVING_OVERVIEW"/> instead
        /// </summary>
        [RockObsolete("1.13")]
        [Obsolete( "Use PERSON_ATTRIBUTES_GIVING_OVERVIEW instead" )]
        public const string PERSON_ATTRIBUTES_GIVING_ANALYTICS = PERSON_ATTRIBUTES_GIVING_OVERVIEW;

        /// <summary>
        /// Person Attribute Category for Giving Overview
        /// </summary>
        public const string PERSON_ATTRIBUTES_GIVING_OVERVIEW = "61823196-8EA1-4C2B-A7DF-1654BD085667";

        #endregion

        #region PowerBI 

        /// <summary>
        /// The category for defined types for PowerBI
        /// </summary>
        public const string POWERBI_DEFINED_TYPE = "FF0B8B72-C1A3-BB99-4D51-78BC670ADB9C";

        #endregion 

        #region Group Categories (for a specific group type)

        /// <summary>
        /// The 'Public' category for Communication List groups
        /// </summary>
        public const string GROUPTYPE_COMMUNICATIONLIST_PUBLIC = "A0889E77-67D9-418C-B301-1B3924692058";

        #endregion

        #region System Email Categories

        /// <summary>
        /// The System Email Workflow Category guid
        /// </summary>
        [Obsolete( "Use SYSTEM_COMMUNICATION_WORKFLOW instead.", true )]
        [RockObsolete("1.10")]
        public const string SYSTEM_EMAIL_WORKFLOW = "C7B9B5F1-9D90-485F-93E4-5D7D81EC2B12";

        /// <summary>
        /// The System Communication Workflow Category guid
        /// </summary>
        public const string SYSTEM_COMMUNICATION_WORKFLOW = "D8EC958D-ADD5-48FC-B539-CB919F5C9D32";

        /// <summary>
        /// The System Communication RSVP Confirmation Category guid
        /// </summary>
        public const string SYSTEM_COMMUNICATION_RSVP_CONFIRMATION = "19024818-E456-4642-8858-F50C8B6DB5ED";

        /// <summary>
        /// The system communication Sign-Up Group confirmation Category guid
        /// </summary>
        public const string SYSTEM_COMMUNICATION_SIGNUP_GROUP_CONFIRMATION = "CB279EE1-9A12-4837-9A14-1F36B6F7CDAF";

        #endregion

        #region Merge Template Categories

        /// <summary>
        /// The Personal Merge Template Category guid
        /// </summary>
        public const string PERSONAL_MERGE_TEMPLATE = "A9F2F544-660B-4176-ACAD-88898416A66E";

        #endregion

        #region Registration Attributes

        /// <summary>
        /// The registration attribute category that indicates Registration Attributes that should be prompted for at the start of the registration process.
        /// </summary>
        public const string REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION = "27808664-97E7-432B-A1E7-D02F6DE5977A";

        /// <summary>
        /// The registration attribute category that indicates Registration Attributes that should be prompted for at the end of the registration process.
        /// </summary>
        public const string REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION = "4648196E-CEB8-4D76-8F27-273EBBC35A08";

        #endregion

        #region Metric Categories

        /// <summary>
        /// Hosting Metrics Metric Category Guid
        /// </summary>
        public const string METRIC_HOSTING_METRICS = "370FBBD8-7766-4B3F-81A9-F13EE819A832";

        /// <summary>
        /// The insights metrics Category Guid
        /// </summary>
        public const string INSIGHTS = "CEC746EE-76D6-477F-B7CF-374542F92041";

        #endregion

        #region WorkflowType Categories

        /// <summary>
        /// The GUID for the WorkflowType category "Check-in"
        /// </summary>
        public const string WORKFLOW_TYPE_CHECKIN = "8F8B272D-D351-485E-86D6-3EE5B7C84D99";

        /// <summary>
        /// The GUID for the WorkflowType category "Data Integrity"
        /// </summary>
        public const string WORKFLOW_TYPE_DATA_INTEGRITY = "BBAE05FD-8192-4616-A71E-903A927E0D90";

        /// <summary>
        /// The GUID for the WorkflowType category "Requests"
        /// </summary>
        public const string WORKFLOW_TYPE_REQUESTS = "78E38655-D951-41DB-A0FF-D6474775CFA1";

        /// <summary>
        /// The GUID for the WorkflowType category "Safety &amp; Security"
        /// </summary>
        public const string WORKFLOW_TYPE_SAFETY_AND_SECURITY = "6F8A431C-BEBD-4D33-AAD6-1D70870329C2";

        /// <summary>
        /// The GUID for the WorkflowType "Category Samples"
        /// </summary>
        public const string WORKFLOW_TYPE_SAMPLES = "CB99421E-9ADC-488E-8C71-94BB14F27F56";

        /// <summary>
        /// The GUID for the WorkflowType category "Staff Forms"
        /// </summary>
        public const string WORKFLOW_TYPE_STAFF_FORMS = "25E6844F-59BA-4C0D-9284-ED2A558E18EC";

        #endregion FormBuilder Categories

        #region Lava Shortcode Categories

        /// <summary>
        /// Lava Shortcode Category Guid
        /// </summary>
        public const string LAVA_SHORTCODE_AI = "125FB37E-5540-48A6-4A74-A49D1C1324F8";

        #endregion
    }
}