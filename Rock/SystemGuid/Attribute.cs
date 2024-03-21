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

using Rock.Financial;

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
        /// The Google reCaptcha site key
        /// </summary>
        public const string GLOBAL_GOOGLE_RECAPTCHA_SITE_KEY = "BF1FD484-50F3-4C7E-975C-5E1CEB1F8C72";

        /// <summary>
        /// The Google reCaptcha secret key
        /// </summary>
        public const string GLOBAL_GOOGLE_RECAPTCHA_SECRET_KEY = "D36E5760-05FF-479F-AD1D-C048AE2E99E9";

        /// <summary>
        /// The Google reCaptcha secret key
        /// </summary>
        public const string GLOBAL_PUBLIC_APPLICATION_ROOT = "49AD7AD6-9BAC-4743-B1E8-B917F6271924";

        /// <summary>
        /// The Liquid Framework used to parse and render Lava.
        /// </summary>
        public const string GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK = "9CBDD352-A4F5-47D6-9EFE-6115774B2DFE";

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
        /// The Person legal note attribute
        /// </summary>
        public const string PERSON_LEGAL_NOTE = "F832AB6F-B684-4EEA-8DB4-C54B895C79ED";

        /// <summary>
        /// The person attribute for the person's giving envelope number
        /// </summary>
        public const string PERSON_GIVING_ENVELOPE_NUMBER = "76C33FBC-8799-4DF1-B2FE-A6C41AC3DD49";

        /// <summary>
        /// The same site cookie setting
        /// </summary>
        public const string SAME_SITE_COOKIE_SETTING = "03F55022-C1E0-45F3-84E1-C2BE8C38E22B";

        #region Observability

        /// <summary>
        /// The Active attribute for the observability HTTP filter.
        /// </summary>
        public const string HTTP_MODULE_OBSERVABILITY_ACTIVE = "DAC40EF2-1616-4E15-A897-8CCD7CF2C588";

        #endregion

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
        public const string PERSON_EQ_CONSTRUCTS_SELF_AWARENESS = "A5EFCE3E-EA41-4FEC-99F6-DD748A7D5BB5";

        /// <summary>
        /// The person attribute for the EQ self regulate
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_SELF_REGULATING = "149CD0CD-3CD6-44B6-8D84-A17A477A8978";

        /// <summary>
        /// The person attribute for the EQ others aware
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_OTHERS_AWARENESS = "A6AF0BE5-E93A-49EB-AFEA-3520B7C41C78";

        /// <summary>
        /// The person attribute for the EQ others regulate
        /// </summary>
        public const string PERSON_EQ_CONSTRUCTS_OTHERS_REGULATING = "129C108E-CE61-4DFB-A9A8-1EBC3462022E";

        /// <summary>
        /// The person attribute for the EQ in problem solving
        /// </summary>
        public const string PERSON_EQ_SCALES_PROBLEM_SOLVING = "B598BF9C-7A0C-467E-B467-13B40DAC9F8D";

        /// <summary>
        /// The person attribute for the EQ under stress
        /// </summary>
        public const string PERSON_EQ_SCALES_UNDER_STRESS = "C3CB8FB5-34A2-48C8-B1FC-7CEBA670C1ED";

        /// <summary>
        /// The person attribute for the Motivator Believing
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
        /// The person attribute to hold the top 5 motivators.
        /// </summary>
        public const string PERSON_MOTIVATOR_TOP_5_MOTIVATORS = "402308F6-44BB-46CF-ADF9-6F62406C9923";

        /// <summary>
        /// The person attribute for the Motivator Growth Propensity
        /// </summary>
        public const string PERSON_MOTIVATOR_GROWTHPROPENSITY = "3BDBF9D3-F4DF-4E4C-A81D-64730217B6EA";

        /// <summary>
        /// The person attribute for the Motivator Relational Theme
        /// </summary>
        public const string PERSON_MOTIVATORS_RELATIONAL_THEME = "CDCBA1D3-4129-43DB-9607-74F57BEBF807";

        /// <summary>
        /// The person attribute for the Motivator Directional Theme
        /// </summary>
        public const string PERSON_MOTIVATOR_DIRECTIONAL_THEME = "0815E9BE-BC24-4568-AC1A-3ECCDFF44D9F";

        /// <summary>
        /// The person attribute for the Motivator Intellectual Theme
        /// </summary>
        public const string PERSON_MOTIVATORS_INTELLECTUAL_THEME = "592A5F89-5E8A-43D8-8843-760207D71699";

        /// <summary>
        /// The person attribute for the Motivator Positional Theme
        /// </summary>
        public const string PERSON_MOTIVATOR_POSITIONAL_THEME = "075FDF4D-DDEC-4106-B996-F48CD1EFC978";

        #endregion

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

        #region Communication Module

        /// <summary>
        /// Communication Entry Wizard Block configuration setting.
        /// </summary>
        public const string COMMUNICATION_ENTRY_WIZARD_DEFAULT_AS_BULK = "23C883A6-AD9B-4C91-BAE6-16E0076C5D67";

        /// <summary>
        /// Communication Entry Block configuration setting.
        /// </summary>
        public const string COMMUNICATION_ENTRY_DEFAULT_AS_BULK = "679E5FBB-AB03-4DE4-BB24-1C7CEFEACD3E";

        /// <summary>
        /// System Communication List/Detail Page configuration setting.
        /// </summary>
        public const string SYSTEM_COMMUNICATION_LIST_DETAIL_PAGE = "9880C186-F079-4113-99B6-EF53AB4FE92D";

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
        [RockObsolete( "1.10" )]
        [Obsolete( "This value is no longer used. The Communication Preference on the Group Member should be used.", true )]
        public const string GROUPMEMBER_COMMUNICATION_LIST_PREFERRED_COMMUNICATION_MEDIUM = "D7941908-1F65-CC9B-416C-CCFABE4221B9";

        #endregion

        #region Communication Transport Attributes

        /// <summary>
        /// The communication transport SMTP server Attribute Guid
        /// </summary>
        public const string COMMUNICATION_TRANSPORT_SMTP_SERVER = "6CFFDF99-E93A-49B8-B440-0EF93878A51F";

        #endregion

        #region Communication Medium Attributes

        /// <summary>
        /// The communication medium Email CSS Inlining Enabled Attribute Guid
        /// </summary>
        public const string COMMUNICATION_MEDIUM_EMAIL_CSS_INLINING_ENABLED = "1D5E06A4-79BD-4554-AB63-DD6F1F815594";

        #endregion

        #region DefinedType Attributes

        /// <summary>
        /// The Template DefinedType > TemplateBlock DefinedValue Attribute Guid
        /// </summary>
        public const string DEFINED_TYPE_TEMPLATE_TEMPLATE_BLOCK = "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D";

        /// <summary>
        /// The Template DefinedType > Icon DefinedValue Attribute Guid
        /// </summary>
        public const string DEFINED_TYPE_TEMPLATE_ICON = "831403EB-262E-4BC5-8B5E-F16153493BF5";

        /// <summary>
        /// The Currency type DefinedType > IconCssClass DefinedValue Attribute Guid
        /// </summary>
        public const string DEFINED_TYPE_CURRENCY_TYPE_ICONCSSCLASS = "CB1E9401-E1FD-4DBB-B15F-4E6994602723";

        /// <summary>
        /// The Transaction Source DefinedType > IconCssClass DefinedValue Attribute Guid
        /// </summary>
        public const string DEFINED_TYPE_TRANSACTION_SOURCE_ICONCSSCLASS = "9617D1DC-6561-4314-83EB-7F0ACBA2E259";

        /// <summary>
        /// The Financial Frequency DefinedType > Interval Days DefinedValue Attribute Guid
        /// </summary>
        public const string DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS = "E18CB1D2-08A9-4D12-BCEA-33369193C869";

        #endregion

        #region Device Type Attributes

        /// <summary>
        /// The defined value attribute for storing if a device type supports cameras.
        /// </summary>
        public const string DEFINED_VALUE_DEVICE_TYPE_SUPPORTS_CAMERAS = "79D1D843-4641-458D-A20B-37E0D7B4AEBE";

        #endregion

        #region Country Attributes

        /// <summary>
        /// Country - Locality Label
        /// </summary>
        public const string COUNTRY_LOCALITY_LABEL = "1C234A6D-007F-4410-814E-13E9AE8654B4";

        /// <summary>
        /// Country Address Requirement Level for Address Line 1.
        /// </summary>
        public const string COUNTRY_ADDRESS_LINE_1_REQUIREMENT = "8B7410F4-7EFB-4ABC-BFD0-B8A9A7ADB27D";

        /// <summary>
        /// Country Address Requirement Level for Address Line 2.
        /// </summary>
        public const string COUNTRY_ADDRESS_LINE_2_REQUIREMENT = "0FCBA54C-0B88-45A8-9303-E9783F2A2D0E";

        /// <summary>
        /// Country Address Requirement Level for City.
        /// </summary>
        public const string COUNTRY_ADDRESS_CITY_REQUIREMENT = "EE9B4454-ACBF-416F-8080-2885A3CD6CA6";

        /// <summary>
        /// Country Address Requirement Level for Locality.
        /// </summary>
        public const string COUNTRY_ADDRESS_LOCALITY_REQUIREMENT = "A0B0B033-9DA9-45F4-A593-B40EDCDB2D00";

        /// <summary>
        /// Country Address Requirement Level for State.
        /// </summary>
        public const string COUNTRY_ADDRESS_STATE_REQUIREMENT = "E488AE1D-FC76-44FE-A48D-271E6DF44C24";

        /// <summary>
        /// Country Address Requirement Level for Postal Code.
        /// </summary>
        public const string COUNTRY_ADDRESS_POSTCODE_REQUIREMENT = "0037453D-0D26-4F02-8AB3-6AD675D85AAE";

        #endregion

        #region Fundraising Attributes

        /// <summary>
        /// The defined value attribute for storing a fundraising opportunity type's donation button text.
        /// </summary>
        public const string DEFINED_VALUE_FUNDRAISING_DONATE_BUTTON_TEXT = "7ACD6580-0E5B-4407-BC8B-1BBBAF443B1E";

        /// <summary>
        /// The group attribute for the type of participation in a Fundraising Opportunity group.
        /// </summary>
        public const string PARTICIPATION_TYPE = "EFA9F0D0-54CE-4B88-BC91-8BD110DEE0FC";

        /// <summary>
        /// The attribute for Transaction Header in the Fundraising Transaction Entry block.
        /// </summary>
        public const string FUNDRAISING_TRANSACTION_HEADER = "65FB0B9A-670E-4AB9-9666-77959B4B702E";

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

        #region JWT Config Attributes

        /// <summary>
        /// The defined value attribute for JWT issuer
        /// </summary>
        public const string DEFINED_VALUE_JWT_ISSUER = "4B89D006-0523-4C77-A46B-7ECD042FDE99";

        /// <summary>
        /// The defined value attribute for JWT audience
        /// </summary>
        public const string DEFINED_VALUE_JWT_AUDIENCE = "6F9D9BFB-433F-4D77-8758-FBDB2011FB27";

        /// <summary>
        /// The defined value attribute for JWT person search key
        /// </summary>
        public const string DEFINED_VALUE_JWT_SEARCH_KEY = "DEECB6D1-E596-4A15-B0DB-B2947B5DB784";

        #endregion JWT Config Attributes

        #region Workflow Action Attributes

        /// <summary>
        /// The Campus workflow action attribute guid for PersonGetCampusTeamMember
        /// </summary>
        public const string WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS = "B07F920E-8450-4D1F-985D-6241E4F5E5CB";

        /// <summary>
        /// The Campus Role workflow action attribute guid for PersonGetCampusTeamMember
        /// </summary>
        public const string WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS_ROLE = "5F8F5E6B-5888-4834-B47B-36664FB3A96C";

        /// <summary>
        /// The Campus Team Member workflow action attribute guid for PersonGetCampusTeamMember
        /// </summary>
        public const string WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS_TEAM_MEMBER = "7CFEDCB2-EA8F-421F-BA5E-B0D8BD10EA92";

        /// <summary>
        /// The Person workflow action attribute guid for PersonGetCampusTeamMember
        /// </summary>
        public const string WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_PERSON = "C10C4C89-2B91-4D9A-8D5F-A3E65758A878";

        /// <summary>
        /// The Send Email workflow action attribute guid for FromEmailAddress
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_FROM_EMAIL_ADDRESS = "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC";

        /// <summary>
        /// The Send Email workflow action attribute guid for SendToEmailAddresses
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_SEND_TO_EMAIL_ADDRESSES = "0C4C13B8-7076-4872-925A-F950886B5E16";

        /// <summary>
        /// The Send Email workflow action attribute guid for SendToGroupRole
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_SEND_TO_GROUP_ROLE = "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F";

        /// <summary>
        /// The Send Email workflow action attribute guid for Subject
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_SUBJECT = "5D9B13B6-CD96-4C7C-86FA-4512B9D28386";

        /// <summary>
        /// The Send Email workflow action attribute guid for Body
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_BODY = "4D245B9E-6B03-46E7-8482-A51FBA190E4D";

        /// <summary>
        /// The Send Email workflow action attribute guid for CcEmailAddresses
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_CC_EMAIL_ADDRESSES = "99FFD423-2AB6-481B-8749-B4793A16B620";

        /// <summary>
        /// The Send Email workflow action attribute guid for BccEmailAddresses
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_BCC_EMAIL_ADDRESSES = "3A131021-CB73-44A8-A142-B42832B77F60";

        /// <summary>
        /// The Send Email workflow action attribute guid for AttachmentOne
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_ATTACHMENT_ONE = "C2C7DA55-3018-4645-B9EE-4BCD11855F2C";

        /// <summary>
        /// The Send Email workflow action attribute guid for AttachmentTwo
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_ATTACHMENT_TWO = "FFD9193A-451F-40E6-9776-74D5DCAC1450";

        /// <summary>
        /// The Send Email workflow action attribute guid for AttachmentThree
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_ATTACHMENT_THREE = "A059767A-5592-4926-948A-1065AF4E9748";

        /// <summary>
        /// The Send Email workflow action attribute guid for SaveCommunicationHistory
        /// </summary>
        public const string WORKFLOW_ACTION_SEND_EMAIL_SAVE_COMMUNICATION_HISTORY = "65E69B78-37D8-4A88-B8AC-71893D2F75EF";

        #endregion Workflow Action Attributes

        #region Workflow Entry Block Attributes

        /// <summary>
        /// The Workflow Entry Block Attribute that disables passing the WorkflowTypeID.
        /// </summary>
        public const string WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWTYPEID = "BA7D9988-E6C9-467E-8F08-E0282FE6F7CB";

        /// <summary>
        /// The Workflow Entry Block Attribute that disables passing the WorkflowID.
        /// </summary>
        public const string WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWID = "890676BC-18D3-445F-A6FA-CC2F515F1930";

        #endregion Workflow Entry Block Attributes

        /// <summary>
        /// The defined value logging domains to log
        /// </summary>
        public const string DEFINED_VALUE_LOGGING_DOMAINS_TO_LOG = "9BEA544F-0636-485E-8772-B63180E529F9";

        /// <summary>
        /// The defined value log system settings
        /// </summary>
        public const string DEFINED_VALUE_LOG_SYSTEM_SETTINGS = "B9D4A315-8672-4214-B5D3-01A06C3CAD9F";

        /// <summary>
        /// The system security settings.
        /// </summary>
        public const string SYSTEM_SECURITY_SETTINGS = "450EF9DC-66F6-43A3-BE22-9CB8B1C42477";

        /// <summary>
        /// The phone number lookup title
        /// </summary>
        public const string PHONE_NUMBER_LOOKUP_TITLE = "7FD2383A-A2E8-4158-8E78-2E2E0C6CBA11";

        /// <summary>
        /// The phone number initial instructions
        /// </summary>
        public const string PHONE_NUMBER_INITIAL_INSTRUCTIONS = "D9DFC4E4-A1F7-435C-8CF4-AD67CFA3F26E";

        /// <summary>
        /// The phone number verification instructions
        /// </summary>
        public const string PHONE_NUMBER_VERIFICATION_INSTRUCTIONS = "B14CA36A-CC8F-4858-A2B8-AE0966EDEF2D";

        /// <summary>
        /// The phone number individual selection instructions
        /// </summary>
        public const string PHONE_NUMBER_INDIVIDUAL_SELECTION_INSTRUCTIONS = "1DFA50D7-0819-4659-96DE-D25F80B880E5";

        /// <summary>
        /// The phone number not found message
        /// </summary>
        public const string PHONE_NUMBER_NOT_FOUND_MESSAGE = "F752712B-C2CA-4541-8347-A632652E0764";

        /// <summary>
        /// The phone number authentication level
        /// </summary>
        public const string PHONE_NUMBER_AUTHENTICATION_LEVEL = "92C72C91-8670-4B1B-B529-F744EEE38B5A";

        /// <summary>
        /// The phone number verification time limit
        /// </summary>
        public const string PHONE_NUMBER_VERIFICATION_TIME_LIMIT = "4569E05C-DE8F-40D4-8DF7-4DE6A564FF6E";

        /// <summary>
        /// The phone number ip throttle limit
        /// </summary>
        public const string PHONE_NUMBER_IP_THROTTLE_LIMIT = "2D148814-418A-45A3-9A98-1498363759E7";

        /// <summary>
        /// The phone number SMS number
        /// </summary>
        public const string PHONE_NUMBER_SMS_NUMBER = "AE2979DF-EDE5-4389-ACBA-0FF7680BFE52";

        /// <summary>
        /// The phone number text message template
        /// </summary>
        public const string PHONE_NUMBER_TEXT_MESSAGE_TEMPLATE = "7F12E9B4-0CD1-42C8-AE68-457212B0B459";

        /// <summary>
        /// The oidc scope list detail page
        /// </summary>
        public const string OIDC_SCOPE_LIST_DETAIL_PAGE = "4F4943D5-80D7-4472-B1D6-0AEA14B13CE1";

        /// <summary>
        /// The oidc client list detail page
        /// </summary>
        public const string OIDC_CLIENT_LIST_DETAIL_PAGE = "B889F2F5-800B-455C-A6E3-28E1AB6BE7BA";

        /// <summary>
        /// The oidc client list scope page
        /// </summary>
        public const string OIDC_CLIENT_LIST_SCOPE_PAGE = "EF07798E-48D4-4261-97B2-501A8AD54E15";

        /// <summary>
        /// The Content Channel View Enable Archive Summary Attribute
        /// </summary>
        public const string ENABLE_ARCHIVE_SUMMARY = "753217FB-D519-44CC-83FC-C451E37E553F";

        #region Group Attendance Detail
        /// <summary>
        /// The attendance type label
        /// </summary>
        public const string ATTENDANCE_TYPE_LABEL = "6916359C-C168-4DBA-A893-365526C9F4C4";

        /// <summary>
        /// The configured attendance types
        /// </summary>
        public const string CONFIGURED_ATTENDANCE_TYPES = "2CD11610-775B-44D4-BC0C-063563AC07E5";
        #endregion

        #region Group Attendance List
        /// <summary>
        /// The display attendance type
        /// </summary>
        public const string DISPLAY_ATTENDANCE_TYPE = "41D650B3-78B8-4F02-AD58-B914914A72AE";
        #endregion

        #region Attendance Self Entry
        /// <summary>
        /// The configured attendance type
        /// </summary>
        public const string CONFIGURED_ATTENDANCE_TYPE = "D449AC5B-AC7A-457C-AD0F-D1DB1F73FC19";
        #endregion

        #region Giving Analytics

        /// <summary>
        /// Preferred Currency - Defined Type
        /// </summary>
        public const string PERSON_GIVING_PREFERRED_CURRENCY = "77A5F7DE-9096-45C8-9051-9D8EE50E3C2F";

        /// <summary>
        /// Preferred Source - Defined Type
        /// </summary>
        public const string PERSON_GIVING_PREFERRED_SOURCE = "0567B279-1F4D-4573-9AA7-927A7278443E";

        /// <summary>
        /// Frequency Label. See <seealso cref="FinancialGivingAnalyticsFrequencyLabel"/>.
        /// </summary>
        public const string PERSON_GIVING_FREQUENCY_LABEL = "1A58F7AA-238B-46E5-B1DC-0A5BC1F213A5";

        /// <summary>
        /// Percent of Gifts Scheduled - Integer
        /// </summary>
        public const string PERSON_GIVING_PERCENT_SCHEDULED = "98373264-0E65-4C79-B75B-4F8477AA647E";

        /// <summary>
        /// Gift Amount: Median - Currency
        /// </summary>
        public const string PERSON_GIVING_AMOUNT_MEDIAN = "327F1CFF-A013-42B5-80A7-5922A40480EC";

        /// <summary>
        /// Gift Amount: IQR - Currency
        /// IQR = Interquartile Range calculated from the past 12 months of giving
        /// </summary>
        public const string PERSON_GIVING_AMOUNT_IQR = "CE129112-4BA9-4FC1-A67C-2A5C69140DA7";

        /// <summary>
        /// Gift Frequency Days: Mean -  Decimal
        /// </summary>
        public const string PERSON_GIVING_FREQUENCY_MEAN_DAYS = "88E59B38-044C-4AE4-A455-A0D3A33DDEDA";

        /// <summary>
        /// Gift Frequency Days: Standard Deviation - Decimal
        /// </summary>
        public const string PERSON_GIVING_FREQUENCY_STD_DEV_DAYS = "1D5E4356-DC66-4067-BEF1-3560E61150BD";

        /// <summary>
        /// Giving Bin - Integer
        /// </summary>
        public const string PERSON_GIVING_BIN = "7FBB63CC-F4FC-4F7E-A8C5-44DC3D0F0720";

        /// <summary>
        /// Giving Percentile - Integer - This will be rounded to the nearest percent and stored as a whole number (15 vs .15)
        /// </summary>
        public const string PERSON_GIVING_PERCENTILE = "D03ACAB8-EB0C-4835-A04C-4C357014D935";

        /// <summary>
        /// Next Expected Gift Date - Date
        /// </summary>
        public const string PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE = "65D7CF79-BD80-44B2-9F5F-96D81B9B4990";

        /// <summary>
        /// Last Classification Run Date Time - Date - sets the date time of then the giving unit was last classified.
        /// Classification is updated after each new gift, but if they stop giving we’ll use this to occasionally update the classification.
        /// </summary>
        public const string PERSON_GIVING_LAST_CLASSIFICATION_DATE = "7220B230-03CE-4D1E-985B-26AA28BE06F8";

        /// <summary>
        /// Giving History JSON - Code - gets the JSON array of giving data by month objects.
        /// [{ Year: 2020, Month: 1, AccountId: 1, Amount: 550.67 }, ...]
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "This is now calculated dynamically" )]
        public const string PERSON_GIVING_HISTORY_JSON = "3BF34F25-4D50-4417-B436-37FEA3FA5473";

        /// <summary>
        /// Giving Total past 12 months - Currency
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "This is now calculated dynamically" )]
        public const string PERSON_GIVING_12_MONTHS = "ADD9BE86-49CA-46C4-B4EA-547F2F277294";

        /// <summary>
        /// Giving Total past 90 days - Currency
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "This is now calculated dynamically" )]
        public const string PERSON_GIVING_90_DAYS = "0DE95B77-D26E-4513-9A71-92A7FD5C4B7C";

        /// <summary>
        /// Giving Total prior 90 days (90-180 days ago) - Currency
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "This is now calculated dynamically" )]
        public const string PERSON_GIVING_PRIOR_90_DAYS = "0170A267-942A-480A-A9CF-E4EA60CAA529";

        /// <summary>
        /// Gift count 12 month - Integer
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "This is now calculated dynamically" )]
        public const string PERSON_GIVING_12_MONTHS_COUNT = "23B6A7BD-BBBB-4F2D-9695-2B1E03B3013A";

        /// <summary>
        /// Gift count 90 days - Integer
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "This is now calculated dynamically" )]
        public const string PERSON_GIVING_90_DAYS_COUNT = "356B8F0B-AA54-4F44-8513-F8A5FF592F18";

        /// <summary>
        /// Giving Journey - Current <see cref="GivingJourneyStage">Giving Journey Stage</see>
        /// </summary>
        public const string PERSON_GIVING_CURRENT_GIVING_JOURNEY_STAGE = "13C55AEA-6D88-4470-B3AE-EE5138F044DF";

        /// <summary>
        /// Giving Journey - Previous <see cref="GivingJourneyStage">Giving Journey Stage</see>
        /// </summary>
        public const string PERSON_GIVING_PREVIOUS_GIVING_JOURNEY_STAGE = "B35CE867-6017-484E-9EC7-AEB93CD4B2D8";

        /// <summary>
        /// Giving Journey - Change Date of <see cref="GivingJourneyStage">Giving Journey Stage</see>
        /// </summary>
        public const string PERSON_GIVING_GIVING_JOURNEY_STAGE_CHANGE_DATE = "8FFE3554-43F2-40D8-8803-446559D2B1F7";

        #endregion Giving Analytics

        #region Language Defined Type

        /// <summary>
        /// Language in ISO639-1
        /// </summary>
        public const string ISO639_1 = "F5E8B6D2-6483-0F8D-4C20-07C51E7548AD";

        /// <summary>
        /// Language in ISO639
        /// </summary>
        public const string ISO639_2 = "09225D47-9A4D-D391-49E4-5A99A1DB47B8";

        /// <summary>
        /// The native language name
        /// </summary>
        public const string NativeLanguageName = "55256C99-DAC9-1AB4-4FD2-7CBFE3170245";

        #endregion

        /// <summary>
        /// The currency code symbol
        /// </summary>
        public const string CURRENCY_CODE_SYMBOL = "1268AD58-5459-4C1C-A036-B7A6D948198F";

        /// <summary>
        /// The currency code position
        /// </summary>
        public const string CURRENCY_CODE_POSITION = "909B35DA-5B14-42FF-90E5-328033A07415";

        /// <summary>
        /// The currency code decimal places
        /// </summary>
        public const string CURRENCY_CODE_DECIMAL_PLACES = "98699FDB-DFD3-4015-AB25-ABCB91EE35EB";

        /// <summary>
        /// The organization currency code
        /// </summary>
        public const string ORGANIZATION_CURRENCY_CODE = "60B61A30-3FE8-4158-8848-D4D95DBC64CD";

        /// <summary>
        /// The person do not send giving statement
        /// </summary>
        public const string PERSON_DO_NOT_SEND_GIVING_STATEMENT = "B767F2CF-A4F0-45AA-A2E9-8270F31B307B";

        /// <summary>
        /// The accumulative achievement streak type
        /// </summary>
        public const string ACCUMULATIVE_ACHIEVEMENT_STREAK_TYPE = "BEDD14D0-450E-475C-8D9F-404DDE350530";

        /// <summary>
        /// The accumulative achievement number to accumulate
        /// </summary>
        public const string ACCUMULATIVE_ACHIEVEMENT_NUMBER_TO_ACCUMULATE = "E286F5E1-356F-473A-AB80-A3BA3063703F";

        /// <summary>
        /// The accumulative achievement time span in days
        /// </summary>
        public const string ACCUMULATIVE_ACHIEVEMENT_TIME_SPAN_IN_DAYS = "1C0F4BE1-81E9-4974-A24E-2DFBA8320AE5";

        /// <summary>
        /// The streak achievement streak type
        /// </summary>
        public const string STREAK_ACHIEVEMENT_STREAK_TYPE = "E926DAAE-980A-4BEE-9CF8-C3BF52F28D9D";

        /// <summary>
        /// The streak achievement number to achieve
        /// </summary>
        public const string STREAK_ACHIEVEMENT_NUMBER_TO_ACHIEVE = "302BDD9E-5EAA-423B-AC1A-7E2067E70C19";

        /// <summary>
        /// The streak achievement time span in days
        /// </summary>
        public const string STREAK_ACHIEVEMENT_TIME_SPAN_IN_DAYS = "80030537-ED8E-41BA-BF61-AF242B9073CC";

        /// <summary>
        /// The statement generator configuration
        /// </summary>
        public const string STATEMENT_GENERATOR_CONFIG = "3C6B81A5-63AB-4EA7-A671-836505B9E444";

        /// <summary>
        /// The category treeview search results
        /// </summary>
        public const string CATEGORY_TREEVIEW_SEARCH_RESULTS = "7287F9CD-CDB2-43BA-8E80-E5F7A618415E";

        #region Sign-Up Group (GroupType) Attributes

        /// <summary>
        /// The Sign-up Group - Group Type's project type attribute
        /// </summary>
        public const string GROUPTYPE_SIGNUP_GROUP_PROJECT_TYPE = "46FFBB10-6E4B-4B3A-A560-61B36C6B0E09";

        #endregion
    }
}
