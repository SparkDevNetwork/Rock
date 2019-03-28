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
    /// Group Role System Guids
    /// </summary>
    public static class GroupRole
    {
        #region Family Members

        /// <summary>
        /// Gets the adult family member role
        /// </summary>
        public const string GROUPROLE_FAMILY_MEMBER_ADULT= "2639F9A5-2AAE-4E48-A8C3-4FFE86681E42";
        
        /// <summary>
        /// Gets the child family member role
        /// </summary>
        public const string GROUPROLE_FAMILY_MEMBER_CHILD= "C8B1814F-6AA7-4055-B2D7-48FE20429CB9";

        #endregion

        #region Known Relationships

        /// <summary>
        /// Gets the Known Relationships owner role.
        /// </summary>
        /// <value>
        /// The role Guid
        /// </value>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_OWNER = "7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42";
        
        /// <summary>
        /// A person that can be checked in by the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN = "DC8E5C35-F37C-4B49-A5C6-DF3D94FC808F";

        /// <summary>
        /// A person that can check in the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_CHECK_IN_BY = "FF9869F1-BC56-4410-8A12-CAFC32C62257";
        
        /// <summary>
        /// A grandparent of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_GRANDPARENT = "567DA89F-3C43-443D-A988-C05BC516EF28";

        /// <summary>
        /// A grandchild of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_GRANDCHILD = "C1A393B2-519D-4E46-A551-E48C36BCAC06";

        /// <summary>
        /// A brother or sister of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_SIBLING = "1D92F0E1-E161-4160-9C63-2D0A901D3C38";

        /// <summary>
        /// A person that was first invited by the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_INVITED = "32E71DAC-B40E-467A-98C9-0AA92AA5025E";

        /// <summary>
        /// The person that first invited the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_INVITED_BY = "03BE336C-CD3D-445C-86EC-0856A51DC926";

        /// <summary>
        /// A step child of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_STEP_CHILD = "EFD2D6D1-A407-4EFB-9086-5DF1F19B7D93";

        /// <summary>
        /// A step parent of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_STEP_PARENT = "D14827EF-5D43-442D-8134-DEB58AAC93C5";

        /// <summary>
        /// An adult child of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_CHILD = "F87DF00F-E86D-4771-A3AE-DBF79B78CF5D";

        /// <summary>
        /// The parent of the owner of this known relationship group
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_PARENT = "6F3FADC4-6320-4B54-9CF6-02EF9586A660";

        /// <summary>
        /// Role to identify former spouses after divorce.
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_PREVIOUS_SPOUSE = "60C6142E-8E00-4678-BC2F-983BB7BDE80B";

        /// <summary>
        /// Role to identify facebook friends
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_FACEBOOK_FRIEND = "AB69816C-4DFA-4A7A-86A5-9BFCBA6FED1E";

        /// <summary>
        /// Role to identify contacts related to a business.
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT = "102E6AF5-62C2-4767-B473-C9C228D75FB6";

        /// <summary>
        /// A role to identify the business a person owns.
        /// </summary>
        public const string GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS = "7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0";

        #endregion

        #region Implied Relationships / Peer Network

        /// <summary>
        /// Gets the Implied Relationships owner role.
        /// </summary>
        /// <value>
        /// The role Guid.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use GROUPROLE_PEER_NETWORK_OWNER instead.", false )]
        public const string GROUPROLE_IMPLIED_RELATIONSHIPS_OWNER= "CB9A0E14-6FCF-4C07-A49A-D7873F45E196";

        /// <summary>
        /// Gets the Peer Network owner role.
        /// </summary>
        public const string GROUPROLE_PEER_NETWORK_OWNER = "CB9A0E14-6FCF-4C07-A49A-D7873F45E196";

        /// <summary>
        /// Gets the Implied Relationships related role.
        /// </summary>
        /// <value>
        /// The role Guid.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use GROUPROLE_PEER_NETWORK_RELATED instead.", false )]
        public const string GROUPROLE_IMPLIED_RELATIONSHIPS_RELATED= "FEA75948-97CB-4DE9-8A0D-43FA2646F55B";

        /// <summary>
        /// Gets the Peer Network related role.
        /// </summary>
        /// <value>
        /// The role Guid.
        /// </value>
        public const string GROUPROLE_PEER_NETWORK_RELATED = "FEA75948-97CB-4DE9-8A0D-43FA2646F55B";

        #endregion

        #region Security Groups

        /// <summary>
        /// Gets the security group member role.
        /// </summary>
        /// <value>
        /// The role Guid.
        /// </value>
        public const string GROUPROLE_SECURITY_GROUP_MEMBER = "00F3AC1C-71B9-4EE5-A30E-4C48C8A0BF1F";

        #endregion

        #region GROUPTYPE_ORGANIZATION_UNIT

        /// <summary>
        /// Gets the Leader group member role for an Organizational Unit
        /// </summary>
        public const string GROUPROLE_ORGANIZATION_UNIT_LEADER = "8438D6C5-DB92-4C99-947B-60E9100F223D";

        /// <summary>
        /// Gets the Staff group member role for an Organizational Unit
        /// </summary>
        public const string GROUPROLE_ORGANIZATION_UNIT_STAFF = "17E516FC-76A4-4BF4-9B6F-0F859B13F563";

        #endregion

        #region GROUPTYPE_FUNDRAISINGOPPORTUNITY

        /// <summary>
        /// The Leader group member roles for Fundraising Opportunity
        /// </summary>
        public const string GROUPROLE_FUNDRAISINGOPPORTUNITY_LEADER = "253973A5-18F2-49B6-B2F1-F8F84294AAB2";

        /// <summary>
        /// The Participant group member roles for Fundraising Opportunity
        /// </summary>
        public const string GROUPROLE_FUNDRAISINGOPPORTUNITY_PARTICIPANT = "F82DF077-9664-4DA8-A3D9-7379B690124D";

        #endregion
    }
}