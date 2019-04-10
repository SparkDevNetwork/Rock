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
using System.Linq;

using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class for getting check-in configuration settings from the group type
    /// </summary>
    public class CheckinType
    {
        /// <summary>
        /// The checkin type identifier
        /// </summary>
        private int _checkinTypeId;

        /// <summary>
        /// Gets the type of the checkin.
        /// </summary>
        /// <value>
        /// The type of the checkin.
        /// </value>
        private GroupTypeCache _checkinType => GroupTypeCache.Get( _checkinTypeId );

        /// <summary>
        /// Gets the type of checkin.
        /// </summary>
        /// <value>
        /// The type of checkin.
        /// </value>
        public TypeOfCheckin TypeOfCheckin => GetSetting( "core_checkin_CheckInType" ) == "1" ? TypeOfCheckin.Family : TypeOfCheckin.Individual;

        /// <summary>
        /// Gets a value indicating whether [enable manager option].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable manager option]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableManagerOption => GetSetting( "core_checkin_EnableManagerOption" ).AsBoolean( true );

        /// <summary>
        /// Gets a value indicating whether [enable override].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable override]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableOverride => GetSetting( "core_checkin_EnableOverride" ).AsBoolean( true );

        /// <summary>
        /// Gets the length of the security code alpha numeric.
        /// </summary>
        /// <value>
        /// The length of the security code alpha numeric.
        /// </value>
        public int SecurityCodeAlphaNumericLength => GetSetting( "core_checkin_SecurityCodeLength" ).AsIntegerOrNull() ?? 3;

        /// <summary>
        /// Gets the length of the security code alpha.
        /// </summary>
        /// <value>
        /// The length of the security code alpha.
        /// </value>
        public int SecurityCodeAlphaLength => GetSetting( "core_checkin_SecurityCodeAlphaLength" ).AsIntegerOrNull() ?? 0;

        /// <summary>
        /// Gets the length of the security code numeric.
        /// </summary>
        /// <value>
        /// The length of the security code numeric.
        /// </value>
        public int SecurityCodeNumericLength => GetSetting( "core_checkin_SecurityCodeNumericLength" ).AsIntegerOrNull() ?? 0;

        /// <summary>
        /// Gets a value indicating whether [security code numeric random].
        /// </summary>
        /// <value>
        /// <c>true</c> if [security code numeric random]; otherwise, <c>false</c>.
        /// </value>
        public bool SecurityCodeNumericRandom => GetSetting( "core_checkin_SecurityCodeNumericRandom" ).AsBooleanOrNull() ?? true;

        /// <summary>
        /// Gets a value indicating whether [reuse same code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reuse same code]; otherwise, <c>false</c>.
        /// </value>
        public bool ReuseSameCode => GetSetting( "core_checkin_ReuseSameCode" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [allow checkout].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow checkout]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowCheckout => GetSetting( "core_checkin_AllowCheckout" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [use same options].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use same options]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSameOptions => GetSetting( "core_checkin_UseSameOptions" ).AsBoolean( false );

        /// <summary>
        /// Gets the type of the search.
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        public DefinedValueCache SearchType => DefinedValueCache.Get( GetSetting( "core_checkin_SearchType" ).AsGuid() );

        /// <summary>
        /// Gets the regular expression filter.
        /// </summary>
        /// <value>
        /// The regular expression filter.
        /// </value>
        public string RegularExpressionFilter => GetSetting( "core_checkin_RegularExpressionFilter" );

        /// <summary>
        /// Gets the maximum search results.
        /// </summary>
        /// <value>
        /// The maximum search results.
        /// </value>
        public int MaxSearchResults => GetSetting( "core_checkin_MaxSearchResults" ).AsIntegerOrNull() ?? 100;

        /// <summary>
        /// Gets the minimum length of the phone search.
        /// </summary>
        /// <value>
        /// The minimum length of the phone search.
        /// </value>
        public int MinimumPhoneSearchLength => GetSetting( "core_checkin_MinimumPhoneSearchLength" ).AsIntegerOrNull() ?? 4;

        /// <summary>
        /// Gets the maximum length of the phone search.
        /// </summary>
        /// <value>
        /// The maximum length of the phone search.
        /// </value>
        public int MaximumPhoneSearchLength => GetSetting( "core_checkin_MaximumPhoneSearchLength" ).AsIntegerOrNull() ?? 10;

        /// <summary>
        /// Gets the type of the phone search.
        /// </summary>
        /// <value>
        /// The type of the phone search.
        /// </value>
        public PhoneSearchType PhoneSearchType => GetSetting( "core_checkin_PhoneSearchType" ) == "0" ? PhoneSearchType.Contains : PhoneSearchType.EndsWith;

        /// <summary>
        /// Gets the refresh interval.
        /// </summary>
        /// <value>
        /// The refresh interval.
        /// </value>
        public int RefreshInterval => GetSetting( "core_checkin_RefreshInterval" ).AsIntegerOrNull() ?? 10;

        /// <summary>
        /// Gets a value indicating whether [age required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [age required]; otherwise, <c>false</c>.
        /// </value>
        public bool AgeRequired => GetSetting( "core_checkin_AgeRequired" ).AsBoolean( true );

        /// <summary>
        /// Gets a value indicating whether [grade is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [age required]; otherwise, <c>false</c>.
        /// </value>
        public bool GradeRequired => GetSetting( "core_checkin_GradeRequired" ).AsBoolean( true );

        /// <summary>
        /// Gets a value indicating whether [hide photos].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide photos]; otherwise, <c>false</c>.
        /// </value>
        public bool HidePhotos => GetSetting( "core_checkin_HidePhotos" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [prevent duplicate checkin].
        /// </summary>
        /// <value>
        /// <c>true</c> if [prevent duplicate checkin]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventDuplicateCheckin => GetSetting( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [prevent inactive people]. Obsolete as of 1.7.0.
        /// </summary>
        /// <value>
        /// <c>true</c> if [prevent inactive people]; otherwise, <c>false</c>.
        /// </value>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use PreventInactivePeople instead.", true )]
        public bool PreventInactivePeopele => GetSetting( "core_checkin_PreventInactivePeople" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [prevent inactive people].
        /// </summary>
        /// <value>
        /// <c>true</c> if [prevent inactive people]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventInactivePeople => GetSetting( "core_checkin_PreventInactivePeople" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [display location count].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display location count]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayLocationCount => GetSetting( "core_checkin_DisplayLocationCount" ).AsBoolean( true );

        /// <summary>
        /// Gets the automatic select days back.
        /// </summary>
        /// <value>
        /// The automatic select days back.
        /// </value>
        public int AutoSelectDaysBack => GetSetting( "core_checkin_AutoSelectDaysBack" ).AsIntegerOrNull() ?? 10;

        /// <summary>
        /// Gets or sets the automatic select options.
        /// </summary>
        /// <value>
        /// The automatic select options.
        /// </value>
        public int? AutoSelectOptions => GetSetting( "core_checkin_AutoSelectOptions" ).AsIntegerOrNull();

        /// <summary>
        /// Gets the start lava template.
        /// </summary>
        /// <value>
        /// The start lava template.
        /// </value>
        public string StartLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the family select lava template.
        /// </summary>
        /// <value>
        /// The family select lava template.
        /// </value>
        public string FamilySelectLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the success lava template.
        /// </summary>
        /// <value>
        /// The success lava template.
        /// </value>
        public string SuccessLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE );

        #region registration

        /// <summary>
        /// Gets the CheckinType settings related to Registration
        /// </summary>
        /// <value>
        /// The registration.
        /// </value>
        public RegistrationSettings Registration { get; private set; }

        /// <summary>
        /// CheckinType settings related to Registration
        /// </summary>
        public class RegistrationSettings
        {
            /// <summary>
            /// Gets or sets the type of the checkin.
            /// </summary>
            /// <value>
            /// The type of the checkin.
            /// </value>
            private CheckinType _checkinType { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationSettings"/> class.
            /// </summary>
            /// <param name="checkinType">Type of the checkin.</param>
            public RegistrationSettings( CheckinType checkinType )
            {
                _checkinType = checkinType;
            }

            /// <summary>
            /// Gets a value indicating whether [display alternate identifier fieldfor adults].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [display alternate identifier fieldfor adults]; otherwise, <c>false</c>.
            /// </value>
            public bool DisplayAlternateIdFieldforAdults => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS ).AsBoolean();

            /// <summary>
            /// Gets a value indicating whether [display alternate identifier fieldfor children].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [display alternate identifier fieldfor children]; otherwise, <c>false</c>.
            /// </value>
            public bool DisplayAlternateIdFieldforChildren => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN ).AsBoolean();

            /// <summary>
            /// Determines if the family should continue on the check-in path after being registered, or if they should be directed to a different kiosk after registration (take then back to search in that case)
            /// </summary>
            /// <value>
            ///   <c>true</c> if [enable check in after registration]; otherwise, <c>false</c>.
            /// </value>
            public bool EnableCheckInAfterRegistration => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION ).AsBoolean();

            /// <summary>
            /// Gets the default person connection status identifier.
            /// </summary>
            /// <value>
            /// The default person connection status identifier.
            /// </value>
            public int? DefaultPersonConnectionStatusId
            {
                get
                {
                    var personConnectionStatusGuid = _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS ).AsGuidOrNull();
                    if ( !personConnectionStatusGuid.HasValue )
                    {
                        personConnectionStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid();
                    }

                    if ( personConnectionStatusGuid.HasValue )
                    {
                        return DefinedValueCache.Get( personConnectionStatusGuid.Value )?.Id;
                    }

                    return null;
                }
            }

            /// <summary>
            /// Gets the required attributes for adults.
            /// </summary>
            /// <value>
            /// The required attributesfor adults.
            /// </value>
            public List<AttributeCache> RequiredAttributesForAdults
            {
                get
                {
                    return GetAttributesForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS );
                }
            }

            /// <summary>
            /// Gets the optional attributes for adults.
            /// </summary>
            /// <value>
            /// The optional attributes for adults.
            /// </value>
            public List<AttributeCache> OptionalAttributesForAdults
            {
                get
                {
                    var optionalAttributes = GetAttributesForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS );

                    // just in case an attribute is specified as Optional AND Required, don't include it if it is also a Required attribute
                    return optionalAttributes.Where( a => !this.RequiredAttributesForAdults.Any( r => r.Id == a.Id ) ).ToList();
                }
            }

            /// <summary>
            /// Gets the required attributes for children.
            /// </summary>
            /// <value>
            /// The required attributes for children.
            /// </value>
            public List<AttributeCache> RequiredAttributesForChildren
            {
                get
                {
                    return GetAttributesForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN );
                }
            }

            /// <summary>
            /// Gets the optional attributes for children.
            /// </summary>
            /// <value>
            /// The optional attributes for children.
            /// </value>
            public List<AttributeCache> OptionalAttributesForChildren
            {
                get
                {
                    var optionalAttributes = GetAttributesForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN );

                    // just in case an attribute is specified as Optional AND Required, don't include it if it is also a Required attribute
                    return optionalAttributes.Where( a => !this.RequiredAttributesForChildren.Any( r => r.Id == a.Id ) ).ToList();
                }
            }

            /// <summary>
            /// Gets the required attributes for children.
            /// </summary>
            /// <value>
            /// The required attributes for children.
            /// </value>
            public List<AttributeCache> RequiredAttributesForFamilies
            {
                get
                {
                    return GetAttributesForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES );
                }
            }

            /// <summary>
            /// Gets the optional attributes for children.
            /// </summary>
            /// <value>
            /// The optional attributes for children.
            /// </value>
            public List<AttributeCache> OptionalAttributesForFamilies
            {
                get
                {
                    var optionalAttributes = GetAttributesForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES );

                    // just in case an attribute is specified as Optional AND Required, don't include it if it is also a Required attribute
                    return optionalAttributes.Where( a => !this.RequiredAttributesForFamilies.Any( r => r.Id == a.Id ) ).ToList();
                }
            }

            /// <summary>
            /// Gets a Dictionary of GroupTypeRoleId and Name for the known relationship group type roles that are defined for Registration (where 0 means Child/Adult in Family)
            /// </summary>
            /// <value>
            /// The known relationship group type roles.
            /// </value>
            public Dictionary<int, string> KnownRelationships
            {
                get
                {
                    List<int> groupTypeRoleIds = _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().AsIntegerList();
                    var knownRelationShipRoles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ).Roles;

                    var result = new Dictionary<int, string>();
                    if ( groupTypeRoleIds.Contains( 0 ) )
                    {
                        result.Add( 0, "Child" );
                    }

                    foreach ( var role in knownRelationShipRoles.Where( a => groupTypeRoleIds.Contains( a.Id ) ).ToList() )
                    {
                        result.Add( role.Id, role.Name );
                    }

                    return result;
                }
            }

            /// <summary>
            /// Gets a Dictionary of GroupTypeRoleId and Name for the known relationship group type roles that indicate that the person is in the primary fmily
            /// </summary>
            /// <value>
            /// The known relationships same family.
            /// </value>
            public Dictionary<int, string> KnownRelationshipsSameFamily
            {
                get
                {
                    List<int> groupTypeRoleIds = _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().AsIntegerList();
                    var knownRelationShipRoles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ).Roles;

                    var result = new Dictionary<int, string>();
                    if ( groupTypeRoleIds.Contains( 0 ) )
                    {
                        result.Add( 0, "Child" );
                    }

                    foreach ( var role in knownRelationShipRoles.Where( a => groupTypeRoleIds.Contains( a.Id ) ).ToList() )
                    {
                        result.Add( role.Id, role.Name );
                    }

                    return result;
                }
            }

            /// <summary>
            /// Gets a Dictionary of GroupTypeRoleId and Name for the known relationship group type roles that indicate that the person is not in the primary family (just "Can Checkin", etc)
            /// </summary>
            /// <value>
            /// The known relationships can checkin.
            /// </value>
            public Dictionary<int, string> KnownRelationshipsCanCheckin
            {
                get
                {
                    List<int> groupTypeRoleIds = _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().AsIntegerList();
                    var knownRelationShipRoles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ).Roles;

                    var result = new Dictionary<int, string>();
                    if ( groupTypeRoleIds.Contains( 0 ) )
                    {
                        result.Add( 0, "Child" );
                    }

                    foreach ( var role in knownRelationShipRoles.Where( a => groupTypeRoleIds.Contains( a.Id ) ).ToList() )
                    {
                        result.Add( role.Id, role.Name );
                    }

                    return result;
                }
            }

            /// <summary>
            /// WorkflowTypes that should be queued after adding a new family
            /// </summary>
            /// <value>
            /// The add family workflow types.
            /// </value>
            public List<WorkflowTypeCache> AddFamilyWorkflowTypes
            {
                get
                {
                    var workflowTypeGuids = _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES )?.SplitDelimitedValues().AsGuidList() ?? new List<Guid>();

                    return workflowTypeGuids.Select( g => WorkflowTypeCache.Get( g ) ).Where( a => a != null ).ToList();
                }
            }

            /// <summary>
            /// WorkflowTypes that should be queued after adding a new person
            /// </summary>
            /// <value>
            /// The add person workflow types.
            /// </value>
            public List<WorkflowTypeCache> AddPersonWorkflowTypes
            {
                get
                {
                    var workflowTypeGuids = _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES )?.SplitDelimitedValues().AsGuidList() ?? new List<Guid>();

                    return workflowTypeGuids.Select( g => WorkflowTypeCache.Get( g ) ).Where( a => a != null ).ToList();
                }
            }

            /// <summary>
            /// Gets the attributes that are specified for the GroupType attribute key.
            /// </summary>
            /// <param name="groupTypeAttributeKey">The group type attribute key.</param>
            /// <returns></returns>
            private List<AttributeCache> GetAttributesForAttributeKey( string groupTypeAttributeKey )
            {
                return _checkinType.GetSetting( groupTypeAttributeKey ).SplitDelimitedValues().AsGuidList().Select( g => AttributeCache.Get( g ) ).Where( a => a != null ).ToList();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckinType"/> class.
        /// </summary>
        /// <param name="checkinTypeId">The checkin type identifier.</param>
        public CheckinType( int checkinTypeId )
        {
            _checkinTypeId = checkinTypeId;

            Registration = new RegistrationSettings( this );
        }

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSetting( string key )
        {
            if ( _checkinType != null )
            {
                return _checkinType.GetAttributeValue( key );
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// The type of checkin
    /// </summary>
    public enum TypeOfCheckin
    {
        /// <summary>
        /// The individual
        /// </summary>
        Individual = 0,

        /// <summary>
        /// The family
        /// </summary>
        Family = 1
    }

    /// <summary>
    /// The type of phone search
    /// </summary>
    public enum PhoneSearchType
    {
        /// <summary>
        /// The ends with
        /// </summary>
        EndsWith = 0,

        /// <summary>
        /// The contains
        /// </summary>
        Contains = 1
    }
}
