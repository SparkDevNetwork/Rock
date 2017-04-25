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
using System.Text;
using System.Threading.Tasks;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class for getting check-in configuratoin settings from the group type
    /// </summary>
    public class CheckinType
    {
        private GroupTypeCache _checkinType;

        /// <summary>
        /// Gets the type of checkin.
        /// </summary>
        /// <value>
        /// The type of checkin.
        /// </value>
        public TypeOfCheckin TypeOfCheckin { get { return GetSetting( "core_checkin_CheckInType" ) == "1" ? TypeOfCheckin.Family : TypeOfCheckin.Individual; } }

        /// <summary>
        /// Gets a value indicating whether [enable manager option].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable manager option]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableManagerOption { get { return GetSetting( "core_checkin_EnableManagerOption" ).AsBoolean( true ); } }

        /// <summary>
        /// Gets a value indicating whether [enable override].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable override]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableOverride { get { return GetSetting( "core_checkin_EnableOverride" ).AsBoolean( true ); } }

        /// <summary>
        /// Gets the length of the security code.
        /// </summary>
        /// <value>
        /// The length of the security code.
        /// </value>
        [Obsolete("Use SecurityCodeAlphaNumericLength instead.")]
        public int SecurityCodeLength { get { return GetSetting( "core_checkin_SecurityCodeLength" ).AsIntegerOrNull() ?? 3; } }

        /// <summary>
        /// Gets the length of the security code alpha numeric.
        /// </summary>
        /// <value>
        /// The length of the security code alpha numeric.
        /// </value>
        public int SecurityCodeAlphaNumericLength { get { return GetSetting( "core_checkin_SecurityCodeLength" ).AsIntegerOrNull() ?? 3; } }

        /// <summary>
        /// Gets the length of the security code alpha.
        /// </summary>
        /// <value>
        /// The length of the security code alpha.
        /// </value>
        public int SecurityCodeAlphaLength { get { return GetSetting( "core_checkin_SecurityCodeAlphaLength" ).AsIntegerOrNull() ?? 0; } }

        /// <summary>
        /// Gets the length of the security code numeric.
        /// </summary>
        /// <value>
        /// The length of the security code numeric.
        /// </value>
        public int SecurityCodeNumericLength { get { return GetSetting( "core_checkin_SecurityCodeNumericLength" ).AsIntegerOrNull() ?? 0; } }

        /// <summary>
        /// Gets a value indicating whether [security code numeric random].
        /// </summary>
        /// <value>
        /// <c>true</c> if [security code numeric random]; otherwise, <c>false</c>.
        /// </value>
        public bool SecurityCodeNumericRandom { get { return GetSetting( "core_checkin_SecurityCodeNumericRandom" ).AsBooleanOrNull() ?? true; } }

        /// <summary>
        /// Gets a value indicating whether [reuse same code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reuse same code]; otherwise, <c>false</c>.
        /// </value>
        public bool ReuseSameCode { get { return GetSetting( "core_checkin_ReuseSameCode" ).AsBoolean( false ); } }

        /// <summary>
        /// Gets a value indicating whether [use same options].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use same options]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSameOptions { get { return GetSetting( "core_checkin_UseSameOptions" ).AsBoolean( false ); } }

        /// <summary>
        /// Gets the type of the search.
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        public DefinedValueCache SearchType { get { return DefinedValueCache.Read( GetSetting( "core_checkin_SearchType" ).AsGuid() ); } }

        /// <summary>
        /// Gets the regular expression filter.
        /// </summary>
        /// <value>
        /// The regular expression filter.
        /// </value>
        public string RegularExpressionFilter { get { return GetSetting( "core_checkin_RegularExpressionFilter" ); } }

        /// <summary>
        /// Gets the maximum search results.
        /// </summary>
        /// <value>
        /// The maximum search results.
        /// </value>
        public int MaxSearchResults { get { return GetSetting( "core_checkin_MaxSearchResults" ).AsIntegerOrNull() ?? 100; } }

        /// <summary>
        /// Gets the minimum length of the phone search.
        /// </summary>
        /// <value>
        /// The minimum length of the phone search.
        /// </value>
        public int MinimumPhoneSearchLength { get { return GetSetting( "core_checkin_MinimumPhoneSearchLength" ).AsIntegerOrNull() ?? 4; } }

        /// <summary>
        /// Gets the maximum length of the phone search.
        /// </summary>
        /// <value>
        /// The maximum length of the phone search.
        /// </value>
        public int MaximumPhoneSearchLength { get { return GetSetting( "core_checkin_MaximumPhoneSearchLength" ).AsIntegerOrNull() ?? 10; } }

        /// <summary>
        /// Gets the type of the phone search.
        /// </summary>
        /// <value>
        /// The type of the phone search.
        /// </value>
        public PhoneSearchType PhoneSearchType { get { return GetSetting( "core_checkin_PhoneSearchType" ) == "0" ? PhoneSearchType.Contains : PhoneSearchType.EndsWith; } }

        /// <summary>
        /// Gets the refresh interval.
        /// </summary>
        /// <value>
        /// The refresh interval.
        /// </value>
        public int RefreshInterval { get { return GetSetting( "core_checkin_RefreshInterval" ).AsIntegerOrNull() ?? 10; } }

        /// <summary>
        /// Gets a value indicating whether [age required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [age required]; otherwise, <c>false</c>.
        /// </value>
        public bool AgeRequired { get { return GetSetting( "core_checkin_AgeRequired" ).AsBoolean( true ); } }

        /// <summary>
        /// Gets a value indicating whether [grade is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [age required]; otherwise, <c>false</c>.
        /// </value>
        public bool GradeRequired { get { return GetSetting( "core_checkin_GradeRequired" ).AsBoolean( true ); } }

        /// <summary>
        /// Gets a value indicating whether [hide photos].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide photos]; otherwise, <c>false</c>.
        /// </value>
        public bool HidePhotos { get { return GetSetting( "core_checkin_HidePhotos" ).AsBoolean( false ); } }

        /// <summary>
        /// Gets a value indicating whether [prevent duplicate checkin].
        /// </summary>
        /// <value>
        /// <c>true</c> if [prevent duplicate checkin]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventDuplicateCheckin { get { return GetSetting( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( false ); } }
        
        /// <summary>
        /// Gets a value indicating whether [prevent inactive peopele].
        /// </summary>
        /// <value>
        /// <c>true</c> if [prevent inactive peopele]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventInactivePeopele { get { return GetSetting( "core_checkin_PreventInactivePeople" ).AsBoolean( false ); } }

        /// <summary>
        /// Gets a value indicating whether [display location count].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display location count]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayLocationCount { get { return GetSetting( "core_checkin_DisplayLocationCount" ).AsBoolean( true ); } }

        /// <summary>
        /// Gets the automatic select days back.
        /// </summary>
        /// <value>
        /// The automatic select days back.
        /// </value>
        public int AutoSelectDaysBack { get { return GetSetting( "core_checkin_AutoSelectDaysBack" ).AsIntegerOrNull() ?? 10; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckinType"/> class.
        /// </summary>
        /// <param name="checkinTypeId">The checkin type identifier.</param>
        public CheckinType( int checkinTypeId )
        {
            _checkinType = GroupTypeCache.Read( checkinTypeId );
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
