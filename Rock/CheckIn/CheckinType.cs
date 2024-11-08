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
    /// Helper class for getting check-in configuration settings from the group type (Checkin Area)
    /// </summary>
    public class CheckinType
    {
        /// <summary>
        /// Gets the type of the checkin.
        /// </summary>
        /// <value>
        /// The type of the checkin.
        /// </value>
        private GroupTypeCache _checkinType => GroupTypeCache.Get( this.Id );

        /// <summary>
        /// Gets the CheckinTypeId (GroupTypeCache.Id) for this CheckinType
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public readonly int Id;

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
        /// Gets the <see cref="AchievementTypeCache" >Achievement Types</see> that are enabled for Checkin Celebrations.
        /// </summary>
        /// <value>
        /// The achievement types.
        /// </value>
        public AchievementTypeCache[] AchievementTypes
        {
            get
            {
                var achievementTypeGuids = GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES ).SplitDelimitedValues().AsGuidList();
                return achievementTypeGuids.Select( a => AchievementTypeCache.Get( a ) ).Where( a => a != null ).ToArray();
            }
        }

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
        [RockObsolete( "1.11" )]
        [Obsolete( "Use CheckinState.AllowCheckout instead" )]
        public bool AllowCheckout => AllowCheckoutDefault;

        /// <summary>
        /// Returns the AllowCheckout setting for this Checkout Type
        /// Note: Use <see cref="CheckInState.AllowCheckout"/> to see if Checkout is allowed in the current state
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow checkout default]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowCheckoutDefault => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating whether [enable presence].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable presence]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePresence => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean( false );

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
        /// Gets a value indicating whether [prevent inactive people].
        /// </summary>
        /// <value>
        /// <c>true</c> if [prevent inactive people]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventInactivePeople => GetSetting( "core_checkin_PreventInactivePeople" ).AsBoolean( false );

        /// <summary>
        /// Gets a value indicating the ability level determination if the AbilityLevelSelect checking block should be skipped or not.
        /// 0 = (Ask) show the ability level
        /// 1 = (Don't Ask) Do not show the ability level
        /// 2 = (Don't Ask If There Is No Ability Level) Do not show the ability level if the person does not have one.
        /// </summary>
        /// <value>
        /// The ability level determination.
        /// </value>
        public AbilityLevelDeterminationOptions AbilityLevelDetermination
        {
            get
            {
                var value = ( AbilityLevelDeterminationOptions ) GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION ).AsInteger();

                // Ensure the next-gen only value isn't used by v1 check-in.
                if ( value != AbilityLevelDeterminationOptions.DoNotAsk && value != AbilityLevelDeterminationOptions.DoNotAskIfThereIsNoAbilityLevel )
                {
                    value = AbilityLevelDeterminationOptions.Ask;
                }

                return value;
            }
        }

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
        /// Gets the person select additional information template.
        /// </summary>
        /// <value>
        /// The person select additional information template.
        /// </value>
        public string PersonSelectAdditionalInfoLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the <see cref="SuccessLavaTemplate" /> display mode.
        /// </summary>
        /// <value>
        /// The success lava template display mode.
        /// </value>
        public SuccessLavaTemplateDisplayMode SuccessLavaTemplateDisplayMode => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE ).ConvertToEnumOrNull<SuccessLavaTemplateDisplayMode>() ?? SuccessLavaTemplateDisplayMode.Never;

        /// <summary>
        /// Gets the success lava template. By default, this is no longer used,
        /// and will be rendered based on CheckinCelebration and CheckinSuccess display logic. But this behavior
        /// can be overridden using the <see cref="SuccessLavaTemplateDisplayMode"/> setting.
        /// </summary>
        /// <value>
        /// The success lava template.
        /// </value>
        public string SuccessLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the action select header lava template.
        /// </summary>
        /// <value>
        /// The action select header lava template.
        /// </value>
        public string ActionSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the checkout person select header lava template.
        /// </summary>
        /// <value>
        /// The checkout person select header lava template.
        /// </value>
        public string CheckoutPersonSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the person select header lava template.
        /// </summary>
        /// <value>
        /// The person select header lava template.
        /// </value>
        public string PersonSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the multi person select header lava template.
        /// </summary>
        /// <value>
        /// The multi person select header lava template.
        /// </value>
        public string MultiPersonSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the group type select header lava template.
        /// </summary>
        /// <value>
        /// The group type select header lava template.
        /// </value>
        public string GroupTypeSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the time select header lava template.
        /// </summary>
        /// <value>
        /// The time select header lava template.
        /// </value>
        public string TimeSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the ability level select header lava template.
        /// </summary>
        /// <value>
        /// The ability level select header lava template.
        /// </value>
        public string AbilityLevelSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the location select header lava template.
        /// </summary>
        /// <value>
        /// The location select header lava template.
        /// </value>
        public string LocationSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE );

        /// <summary>
        /// Gets the group select header lava template.
        /// </summary>
        /// <value>
        /// The group select header lava template.
        /// </value>
        public string GroupSelectHeaderLavaTemplate => GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE );

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
            /// Gets a value indicating whether [display alternate identifier field for adults].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [display alternate identifier field for adults]; otherwise, <c>false</c>.
            /// </value>
            public bool DisplayAlternateIdFieldforAdults => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS ).AsBoolean();

            /// <summary>
            /// Gets a value indicating whether [display alternate identifier field for children].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [display alternate identifier field for children]; otherwise, <c>false</c>.
            /// </value>
            public bool DisplayAlternateIdFieldforChildren => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN ).AsBoolean();

            /// <summary>
            /// Gets a value indicating whether [display SMS button].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [display SMS button]; otherwise, <c>false</c>.
            /// </value>
            public bool DisplaySmsButton => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON ).AsBoolean();

            /// <summary>
            /// Gets a value indicating whether [default SMS enabled].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [default SMS enabled]; otherwise, <c>false</c>.
            /// </value>
            public bool DefaultSmsEnabled => _checkinType.GetSetting( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED ).AsBoolean();

            /// <summary>
            /// Determines if the family should continue on the check-in path after being registered, or if they should be directed to a different kiosk after registration (take them back to search in that case)
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
            /// The required attributes for adults.
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
            /// Gets the display birthdate on adults.
            /// </summary>
            /// <value>
            /// The display birthdate on adults.
            /// </value>
            public string DisplayBirthdateOnAdults
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS );
                }
            }

            /// <summary>
            /// Gets the display birthdate on children attribute.
            /// </summary>
            /// <value>
            /// The display birthdate on children attribute.
            /// </value>
            public string DisplayBirthdateOnChildren
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN );
                }
            }

            /// <summary>
            /// Gets the display grade on children attribute.
            /// </summary>
            /// <value>
            /// The display grade on children attribute.
            /// </value>
            public string DisplayGradeOnChildren
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN );
                }
            }

            /// <summary>
            /// Gets the display race on children attribute.
            /// </summary>
            /// <value>
            /// The display grade on children attribute.
            /// </value>
            public string DisplayRaceOnChildren
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN );
                }
            }

            /// <summary>
            /// Gets the display ethnicity on children attribute.
            /// </summary>
            /// <value>
            /// The display grade on children attribute.
            /// </value>
            public string DisplayEthnicityOnChildren
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN );
                }
            }

            /// <summary>
            /// Gets the display race on adults attribute.
            /// </summary>
            /// <value>
            /// The display grade on children attribute.
            /// </value>
            public string DisplayRaceOnAdults
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS );
                }
            }

            /// <summary>
            /// Gets the display ethnicity on adults attribute.
            /// </summary>
            /// <value>
            /// The display grade on children attribute.
            /// </value>
            public string DisplayEthnicityOnAdults
            {
                get
                {
                    return GetAttributeForAttributeKey( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS );
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
            /// Gets a Dictionary of GroupTypeRoleId and Name for the known relationship group type roles that indicate that the person is in the primary family
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

            /// <summary>
            /// Gets the attribute that is specified for the GroupType attribute key.
            /// </summary>
            /// <param name="groupTypeAttributeKey">The group type attribute key.</param>
            /// <returns></returns>
            private string GetAttributeForAttributeKey( string groupTypeAttributeKey )
            {
                return _checkinType.GetSetting( groupTypeAttributeKey );
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckinType"/> class.
        /// </summary>
        /// <param name="checkinTypeId">The checkin type identifier.</param>
        public CheckinType( int checkinTypeId )
        {
            this.Id = checkinTypeId;

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

    /// <summary>
    /// Available options to determine how check-in should gather the individuals current ability level.
    /// </summary>
    public enum AbilityLevelDeterminationOptions
    {
        /// <summary>
        /// The individual will be asked as a part of each check-in
        /// </summary>
        Ask = 0,

        /// <summary>
        /// Trust that there is another process in place to gather ability level information and the individual will not be asked for their level during check-in.
        /// </summary>
        DoNotAsk = 1,

        /// <summary>
        /// The individuals without an ability level will not be asked as part of each check-in.
        /// If the person has an ability level: show the ability selection screen (to possibly change it) and then allow selecting a group that matches the ability level.
        /// If the person does not have an ability level: Bypass the ability screen then allow selection of a group that has no ability levels.
        /// </summary>
        DoNotAskIfThereIsNoAbilityLevel = 2
    }

    /// <summary>
    /// Determines how the custom Success Lava Template is used. By default,
    /// it <see href="Never">won't be used</see> and the Success Block will display the default results
    /// which may include Achievements and other logic.
    /// </summary>
    public enum SuccessLavaTemplateDisplayMode
    {
        /// <summary>
        /// Hide the custom success template (default).
        /// </summary>
        Never = 0,

        /// <summary>
        /// Replace the current success content with the template.
        /// </summary>
        Replace = 1,

        /// <summary>
        /// Place the success template content under the existing content
        /// </summary>
        Append = 2
    }

    /// <summary>
    /// Determines how controls are displayed in the block.
    /// </summary>
    public static class ControlOptions
    {
        /// <summary>
        /// Hides the field from being displayed.
        /// </summary>
        public const string HIDE = "Hide";

        /// <summary>
        /// The field is display and the value is optional.
        /// </summary>
        public const string OPTIONAL = "Optional";

        /// <summary>
        /// The field is displayed and the value is required.
        /// </summary>
        public const string REQUIRED = "Required";
    }
}
