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
    /// System file types.
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// The binaryfiletype filestorage rootpath attribute
        /// </summary>
        public const string BINARYFILETYPE_FILESTORAGE_ROOTPATH = "3CAFA34D-9208-439B-A046-CB727FB729DE";

        /// <summary>
        /// The global email link preference
        /// </summary>
        public const string GLOBAL_EMAIL_LINK_PREFERENCE = "F1BECEF9-1047-E89F-4CC8-8F856750E5D0";

        /// <summary>
        /// The global enabled lava commands
        /// </summary>
        public const string GLOBAL_ENABLED_LAVA_COMMANDS = "933CFB7D-C9E1-BDAE-40AD-231002A91626";

        /// <summary>
        /// The global enable giving envelope feature
        /// </summary>
        public const string GLOBAL_ENABLE_GIVING_ENVELOPE = "805698B0-BED7-4183-8FC6-3BDBF9E49EF1";

        /// <summary>
        /// The default short link site
        /// </summary>
        public const string GLOBAL_DEFAULT_SHORT_LINK_SITE = "DD0E0757-2A01-47BB-A74A-F6E69B0399C8";

        /// <summary>
        /// The global attribute to enable when routes must match the site's domain
        /// </summary>
        public const string GLOBAL_ENABLE_ROUTE_DOMAIN_MATCHING = "0B7DD63E-AD00-445E-8E9D-047956FEAFB3";

        /// <summary>
        /// The Google reCaptcha site key
        /// </summary>
        public const string GLOBAL_GOOGLE_RECAPTCHA_SITE_KEY = "BF1FD484-50F3-4C7E-975C-5E1CEB1F8C72";

        /// <summary>
        /// The Google reCaptcha secret key
        /// </summary>
        public const string GLOBAL_GOOGLE_RECAPTCHA_SECRET_KEY = "D36E5760-05FF-479F-AD1D-C048AE2E99E9";

        /// <summary>
        /// The Facebook link attribute
        /// </summary>
        public const string PERSON_FACEBOOK = "2B8A03D3-B7DC-4DA3-A31E-826D655435D5";

        /// <summary>
        /// The Twitter link attribute
        /// </summary>
        public const string PERSON_TWITTER = "12E9C8A7-03E4-472D-9E20-9EC8F3453B2F";

        /// <summary>
        /// The Instagram link attribute
        /// </summary>
        public const string PERSON_INSTAGRAM = "8796567C-4047-43C1-AF32-2FDBE030BEAC";

        /// <summary>
        /// The SnapChat link attribute
        /// </summary>
        public const string PERSON_SNAPCHAT = "61099377-9EB3-43EA-BA37-75E329E55866";

        /// <summary>
        /// The allergy attribute
        /// </summary>
        public const string PERSON_ALLERGY = "DBD192C9-0AA1-46EC-92AB-A3DA8E056D31";

        /// <summary>
        /// The person attribute for the the person's giving envelope number
        /// </summary>
        public const string PERSON_GIVING_ENVELOPE_NUMBER = "76C33FBC-8799-4DF1-B2FE-A6C41AC3DD49";

        #region Assessment Test Related

        /// <summary>
        /// The attribute that stores the date the person took the DISC test
        /// </summary>
        public const string PERSON_DISC_LAST_SAVE_DATE = "990275DB-611B-4D2E-94EA-3FFA1186A5E1";

        /// <summary>
        /// The person attribute for the DISC profile
        /// </summary>
        public const string PERSON_DISC_PROFILE = "6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D";

        /// <summary>
        /// The person attribute for the dominant gifts
        /// </summary>
        public const string PERSON_DOMINANT_GIFTS = "F76FC75E-B33F-42B8-B360-15BA9A1F0F9A";

        /// <summary>
        /// The person attribute for the supportive gifts
        /// </summary>
        public const string PERSON_SUPPORTIVE_GIFTS = "0499E359-3A7B-4138-A3EE-44CBF9750E33";

        /// <summary>
        /// The person attribute for the other gifts
        /// </summary>
        public const string PERSON_OTHER_GIFTS = "F33EC30E-7E5C-488E-AB48-81977CCFB185";

        /// <summary>
        /// The person attribute for the spiritual gifts
        /// </summary>
        public const string PERSON_SPIRITUAL_GIFTS_LAST_SAVE_DATE = "3668547C-3DC4-450B-B92D-4B98A693A371";

        /// <summary>
        /// The person attribute for the conflict mode: winning
        /// </summary>
        public const string PERSON_CONFLICT_MODE_WINNING = "7147F706-388E-45E6-BE21-893FC7D652AA";

        /// <summary>
        /// The person attribute for the conflict mode: resolving 
        /// </summary>
        public const string PERSON_CONFLICT_MODE_RESOLVING = "5B811EAC-51B2-41F2-A55A-C966D9DB05EE";

        /// <summary>
        /// The person attribute for the conflict mode: compromising  
        /// </summary>
        public const string PERSON_CONFLICT_MODE_COMPROMISING = "817D6B13-E4AA-4E93-8547-FE711A0065F2";

        /// <summary>
        /// The person attribute for the conflict mode: avoiding  
        /// </summary>
        public const string PERSON_CONFLICT_MODE_AVOIDING = "071A8EFA-AD1C-436A-8E1E-23D215617004";

        /// <summary>
        /// The person attribute for the conflict mode: yielding  
        /// </summary>
        public const string PERSON_CONFLICT_MODE_YIELDING = "D30A33AD-7A60-43E0-84DA-E23600156BF7";

        /// <summary>
        /// The person attribute for the conflict engagement profile: accommodating  
        /// </summary>
        public const string PERSON_CONFLICT_THEME_ACCOMMODATING = "404A64FB-7396-4896-9C94-84DE21E995CA";

        /// <summary>
        /// The person attribute for the conflict engagement profile: winning  
        /// </summary>
        public const string PERSON_CONFLICT_THEME_WINNING = "6DE5878D-7CDB-404D-93A7-27CFF5E98C3B";

        /// <summary>
        /// The person attribute for the conflict engagement profile: solving  
        /// </summary>
        public const string PERSON_CONFLICT_THEME_SOLVING = "33235605-D8BB-4C1E-B231-6F085970A14F";

        /// <summary>
        /// The person attribute for the EQ self aware
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_SELF_AWARE = "A5EFCE3E-EA41-4FEC-99F6-DD748A7D5BB5";

        /// <summary>
        /// The person attribute for the EQ self regulate
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_SELF_REGULATE = "149CD0CD-3CD6-44B6-8D84-A17A477A8978";

        /// <summary>
        /// The person attribute for the EQ others aware
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_OTHERS_AWARE = "A6AF0BE5-E93A-49EB-AFEA-3520B7C41C78";

        /// <summary>
        /// The person attribute for the EQ others regulate
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_OTHERS_REGULATE = "129C108E-CE61-4DFB-A9A8-1EBC3462022E";

        /// <summary>
        /// The person attribute for the EQ in problem solving
        /// </summary>
        public const string PERSON_EQ_SCALES_IN_PROBLEM_SOLVING = "B598BF9C-7A0C-467E-B467-13B40DAC9F8D";

        /// <summary>
        /// The person attribute for the EQ under stress
        /// </summary>
        public const string PERSON_EQ_SCALES_UNDER_STRESS = "C3CB8FB5-34A2-48C8-B1FC-7CEBA670C1ED";

        /// <summary>
        /// The person attribute for the Motivator Beliving
        /// </summary>
        public const string PERSON_MOTIVATOR_BELIVING = "2045D752-2B7F-4314-A58D-AE77AE095CA8";

        /// <summary>
        /// The person attribute for the Motivator Caring
        /// </summary>
        public const string PERSON_MOTIVATOR_CARING = "95C6E9B1-4E26-4D7A-8944-3FED076C12B6";

        /// <summary>
        /// The person attribute for the Motivator Expressing
        /// </summary>
        public const string PERSON_MOTIVATOR_EXPRESSING = "79CC222F-ABB0-489B-8DC3-20FA10A29ADE";

        /// <summary>
        /// The person attribute for the Motivator Empowering
        /// </summary>
        public const string PERSON_MOTIVATOR_EMPOWERING = "510523B0-E428-407A-8C6F-216ADD27CCE0";

        /// <summary>
        /// The person attribute for the Motivator Engaging
        /// </summary>
        public const string PERSON_MOTIVATOR_ENGAGING = "A3B93C89-8C89-431D-A408-7E8C209DF62A";

        /// <summary>
        /// The person attribute for the Motivator Adapting
        /// </summary>
        public const string PERSON_MOTIVATOR_ADAPTING = "7E32DC1C-D912-45AA-9C16-F098ED33A0D2";

        /// <summary>
        /// The person attribute for the Motivator Gathering
        /// </summary>
        public const string PERSON_MOTIVATOR_GATHERING = "2E6960AE-9381-457C-9191-C09CDCAC6FBB";

        /// <summary>
        /// The person attribute for the Motivator Innovating
        /// </summary>
        public const string PERSON_MOTIVATOR_INNOVATING = "B907EC74-CC86-4AEC-9A85-F46FA4152993";

        /// <summary>
        /// The person attribute for the Motivator Leading
        /// </summary>
        public const string PERSON_MOTIVATOR_LEADING = "52652A3A-BE69-4956-B86A-40341481A57C";

        /// <summary>
        /// The person attribute for the Motivator Learning
        /// </summary>
        public const string PERSON_MOTIVATOR_LEARNING = "2579C27B-CE3F-4F2F-B413-1131781106BC";

        /// <summary>
        /// The person attribute for the Motivator Maximizing
        /// </summary>
        public const string PERSON_MOTIVATOR_MAXIMIZING = "F0611197-79C6-4AA3-9BB8-3296604CDA2E";

        /// <summary>
        /// The person attribute for the Motivator Organizing
        /// </summary>
        public const string PERSON_MOTIVATOR_ORGANIZING = "8BA793F1-81B3-43D3-A096-79C3CF50D4C6";

        /// <summary>
        /// The person attribute for the Motivator Pacing
        /// </summary>
        public const string PERSON_MOTIVATOR_PACING = "DD20707F-155B-4784-9BEC-76894A2216A3";

        /// <summary>
        /// The person attribute for the Motivator Perceiving
        /// </summary>
        public const string PERSON_MOTIVATOR_PERCEIVING = "33DBCA52-367D-40CB-BB79-DB38D41E7CF4";

        /// <summary>
        /// The person attribute for the Motivator Relating
        /// </summary>
        public const string PERSON_MOTIVATOR_RELATING = "60653130-E82A-472B-984E-11594547B26C";

        /// <summary>
        /// The person attribute for the Motivator Serving
        /// </summary>
        public const string PERSON_MOTIVATOR_SERVING = "C63CEF01-A942-445B-A27D-824FC6197F4E";

        /// <summary>
        /// The person attribute for the Motivator Thinking
        /// </summary>
        public const string PERSON_MOTIVATOR_THINKING = "4AA4D77D-138D-45A8-827E-1644062BA5C2";

        /// <summary>
        /// The person attribute for the Motivator Transforming
        /// </summary>
        public const string PERSON_MOTIVATOR_TRANSFORMING = "EB185628-9F15-4BFB-BE75-9B08DA73CF7B";

        /// <summary>
        /// The person attribute for the Motivator Uniting
        /// </summary>
        public const string PERSON_MOTIVATOR_UNITING = "D7A987CA-7DF6-4539-96F6-A3641C3F1DED";

        /// <summary>
        /// The person attribute for the Motivator Persevering
        /// </summary>
        public const string PERSON_MOTIVATOR_PERSEVERING = "C4361DE6-6F62-446C-B4B3-39CB670AC0E1";

        /// <summary>
        /// The person attribute for the Motivator Venturing
        /// </summary>
        public const string PERSON_MOTIVATOR_RISKING = "04ED7F11-4C01-43B6-9EF7-C5C4820055B0";

        /// <summary>
        /// The person attribute for the Motivator Visioning
        /// </summary>
        public const string PERSON_MOTIVATOR_VISIONING = "C9BC83A0-27D4-4194-A199-56F2EA83363C";

        /// <summary>
        /// The person attribute for the Motivator Growth Propensity
        /// </summary>
        public const string PERSON_MOTIVATOR_GROWTHPROPENSITY = "3BDBF9D3-F4DF-4E4C-A81D-64730217B6EA";

        /// <summary>
        /// The person attribute for the Motivator Cluster Influential
        /// </summary>
        public const string PERSON_MOTIVATOR_CLUSTER_INFLUENTIAL = "CDCBA1D3-4129-43DB-9607-74F57BEBF807";

        /// <summary>
        /// The person attribute for the Motivator Cluster Organizational
        /// </summary>
        public const string PERSON_MOTIVATOR_CLUSTER_ORGANIZATIONAL = "0815E9BE-BC24-4568-AC1A-3ECCDFF44D9F";

        /// <summary>
        /// The person attribute for the Motivator Cluster Intellectual
        /// </summary>
        public const string PERSON_MOTIVATOR_CLUSTER_INTELLECTUAL = "592A5F89-5E8A-43D8-8843-760207D71699";

        /// <summary>
        /// The person attribute for the Motivator Cluster Operational
        /// </summary>
        public const string PERSON_MOTIVATOR_CLUSTER_OPERATIONAL = "075FDF4D-DDEC-4106-B996-F48CD1EFC978";

        #endregion

        /// <summary>
        /// The family attribute for storing a family's checkin identifiers
        /// </summary>
        ///
        [RockObsolete( "1.8" )]
        [Obsolete( "Check-in identifiers are no longer stored as a family attribute. They are stored as a PersonSearchValue.")]
        public const string FAMILY_CHECKIN_IDENTIFIERS = "8F528431-A438-4488-8DC3-CA42E66C1B37";

        #region eRA Attributes

        /// <summary>
        /// The eRA Currently an eRA attribute
        /// </summary>
        public const string PERSON_ERA_CURRENTLY_AN_ERA = "CE5739C5-2156-E2AB-48E5-1337C38B935E";

        /// <summary>
        /// The eRA start date attribute
        /// </summary>
        public const string PERSON_ERA_START_DATE = "A106610C-A7A1-469E-4097-9DE6400FDFC2";

        /// <summary>
        /// The eRA end date attribute
        /// </summary>
        public const string PERSON_ERA_END_DATE = "4711D67E-7526-9582-4A8E-1CD7BBE1B3A2";

        /// <summary>
        /// The eRA first attended attribute
        /// </summary>
        public const string PERSON_ERA_FIRST_CHECKIN = "AB12B3B0-55B8-D6A5-4C1F-DB9CCB2C4342";

        /// <summary>
        /// The eRA last attended attribute
        /// </summary>
        public const string PERSON_ERA_LAST_CHECKIN = "5F4C6462-018E-D19C-4AB0-9843CB21C57E";

        /// <summary>
        /// The eRA last gave attribute
        /// </summary>
        public const string PERSON_ERA_LAST_GAVE = "02F64263-E290-399E-4487-FC236F4DE81F";

        /// <summary>
        /// The eRA first gave attribute
        /// </summary>
        public const string PERSON_ERA_FIRST_GAVE = "EE5EC76A-D4B9-56B5-4B48-29627D945F10";

        /// <summary>
        /// The eRA times attended in the last 16 weeks attribute
        /// </summary>
        public const string PERSON_ERA_TIMES_CHECKEDIN_16 = "45A1E978-DC5B-CFA1-4AF4-EA098A24C914";

        /// <summary>
        /// The eRA times given in last 52 weeks attribute
        /// </summary>
        public const string PERSON_ERA_TIMES_GIVEN_52 = "57700E8F-ED11-D787-415A-04DDF411BB10";

        /// <summary>
        /// The eRA times given in last 6 weeks attribute
        /// </summary>
        public const string PERSON_ERA_TIMES_GIVEN_6 = "AC11EF53-AE55-79A0-4CAD-43721750E988";
        #endregion

        #region Check-in Attributes

        /// <summary>
        /// Group attribute to store the age range of the group
        /// </summary>
        public const string GROUP_AGE_RANGE = "43511B8F-71D9-423A-85BF-D1CD08C1998E";

        /// <summary>
        /// Group attribute to store the birthdate range of the group
        /// </summary>
        public const string GROUP_BIRTHDATE_RANGE = "F1A43EAB-D682-403F-A05E-CCFFBF879F32";

        #endregion

        #region Communication List (group) attributes

        /// <summary>
        /// Group attribute for groups of GroupType CommunicationList to defined additional dataviews that can be used as communication segments
        /// </summary>
        public const string GROUP_COMMUNICATION_LIST_SEGMENTS = "73A53BC1-2178-46A1-8413-C7A4DD49F0B4";

        /// <summary>
        /// Group attribute for groups of GroupType CommunicationList to define category
        /// </summary>
        public const string GROUP_COMMUNICATION_LIST_CATEGORY = "E3810936-182E-2585-4F8E-030A0E18B27A";

        #endregion

        #region Communication List (group member) attributes

        /// <summary>
        /// The groupmember (of a communication list) preferred communication medium
        /// </summary>
        public const string GROUPMEMBER_COMMUNICATION_LIST_PREFERRED_COMMUNICATION_MEDIUM = "D7941908-1F65-CC9B-416C-CCFABE4221B9";

        #endregion

        #region Fundraising Attributes

        /// <summary>
        /// The defined value attribute for storing a fundraising opportunity type's donation button text.
        /// </summary>
        public const string DEFINED_VALUE_FUNDRAISING_DONATE_BUTTON_TEXT = "7ACD6580-0E5B-4407-BC8B-1BBBAF443B1E";

        #endregion

        #region BIO Block

        /// <summary>
        /// The bio block's workflow action attribute Guid
        /// </summary>
        public const string BIO_WORKFLOWACTION = "7197A0FB-B330-43C4-8E62-F3C14F649813";

        #endregion

        #region File Type Lists

        /// <summary>
        /// Global attribute of image file type extensions that should be allowed.
        /// </summary>
        public const string CONTENT_IMAGE_FILETYPE_WHITELIST = "0F842054-7629-419F-BC72-90BDDE9F3676";

        /// <summary>
        /// Global attribute of file type extensions that should never be allowed. Has precedence over other lists.
        /// </summary>
        public const string CONTENT_FILETYPE_BLACKLIST = "9FFB15C1-AA53-4FBA-A480-64C9B348C5E5";

        /// <summary>
        /// The content filetype whitelist. Has a lower precedence than CONTENT_FILETYPE_BLACKLIST
        /// </summary>
        public const string CONTENT_FILETYPE_WHITELIST = "B895B6D7-BA21-45C0-8913-EF47FAAD69B1";

        #endregion File Type Lists
    }
}
