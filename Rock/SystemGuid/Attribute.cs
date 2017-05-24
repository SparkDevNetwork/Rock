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
        /// The allergy attribute
        /// </summary>
        public const string PERSON_ALLERGY = "DBD192C9-0AA1-46EC-92AB-A3DA8E056D31";

        /// <summary>
        /// The person attribute for the the person's giving envelope number
        /// </summary>
        public const string PERSON_GIVING_ENVELOPE_NUMBER = "76C33FBC-8799-4DF1-B2FE-A6C41AC3DD49";

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
    }
}
