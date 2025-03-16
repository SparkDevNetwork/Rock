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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// This provides the server check-in data for a single check-in group.
    /// This should not be sent down to clients as it contains additional
    /// data they should not see.
    /// </summary>
    internal class GroupConfigurationData
    {
        #region Properties

        /// <summary>
        /// Determines if this group requires that the person be scheduled
        /// in order to check-in. This is used by the group scheduler.
        /// </summary>        
        public virtual AttendanceRecordRequiredForCheckIn AttendanceRecordRequiredForCheckIn { get; }

        /// <summary>
        /// Gets the minimum age requirement or <c>null</c> if there is no
        /// minimum. The person's age must be greater than or equal to
        /// this value.
        /// </summary>
        /// <value>The minimum age requirement.</value>
        public virtual decimal? MinimumAge { get; }

        /// <summary>
        /// Gets the maximum age requirement or <c>null</c> if there is no
        /// maximum. The person's age must be less than this value.
        /// </summary>
        /// <value>The maximum age requirement.</value>
        public virtual decimal? MaximumAge { get; }

        /// <summary>
        /// Gets the minimum birthdate requirement or <c>null</c> if there
        /// is no minimum. The person's birthdate must be greater than or
        /// equal to this value.
        /// </summary>
        /// <value>The minimum birthdate requirement.</value>
        public virtual DateTime? MinimumBirthdate { get; }

        /// <summary>
        /// Gets the maximum birthdate requirement or <c>null</c> if there
        /// is no maximum. The person's birthdate must be less than this value.
        /// </summary>
        /// <value>The maximum birthdate requirement.</value>
        public virtual DateTime? MaximumBirthdate { get; }

        /// <summary>
        /// Gets the minimum birth month requirement or <c>null</c> if there
        /// is no minimum. The person's birth month (1=January) must be greater
        /// than or equal to this value.
        /// </summary>
        /// <value>The minimum birth month requirement.</value>
        public virtual int? MinimumBirthMonth { get; }

        /// <summary>
        /// Gets the maximum birth month requirement or <c>null</c> if there
        /// is no maximum. The person's birth month (1=January) must be less
        /// than or equal to this value.
        /// </summary>
        /// <value>The maximum grade offset requirement.</value>
        public virtual int? MaximumBirthMonth { get; }

        /// <summary>
        /// Gets the minimum grade offset requirement or <c>null</c> if there
        /// is no minimum. The person's grade offset must be greater than or
        /// equal to this value.
        /// </summary>
        /// <value>The minimum grade offset requirement.</value>
        public virtual int? MinimumGradeOffset { get; }

        /// <summary>
        /// Gets the maximum grade offset requirement or <c>null</c> if there
        /// is no maximum. The person's grade offset must be less than or
        /// equal to this value.
        /// </summary>
        /// <value>The maximum grade offset requirement.</value>
        public virtual int? MaximumGradeOffset { get; }

        /// <summary>
        /// Gets the data view unique identifiers for this group. An individual
        /// must be a member of all of these data views.
        /// </summary>
        /// <value>The data view unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> DataViewGuids { get; }

        /// <summary>
        /// Gets the gender that an individual must be in order to check-in to
        /// this group or <c>null</c> if there is no requirement.
        /// </summary>
        /// <value>The required gender.</value>
        public virtual Gender? Gender { get; }

        /// <summary>
        /// Gets a value that determines if this group is configured as a
        /// special needs group.
        /// </summary>
        public virtual bool IsSpecialNeeds { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfigurationData"/> class.
        /// </summary>
        /// <remarks>
        /// This is meant to be used by unit tests only.
        /// </remarks>
        protected GroupConfigurationData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfigurationData"/> class.
        /// </summary>
        /// <param name="groupCache">The group cache.</param>
        /// <param name="rockContext">The rock context.</param>
        internal GroupConfigurationData( GroupCache groupCache, RockContext rockContext )
        {
            (MinimumAge, MaximumAge) = GetAgeRange( groupCache );
            (MinimumBirthdate, MaximumBirthdate) = GetBirthdateRange( groupCache );
            (MinimumGradeOffset, MaximumGradeOffset) = GetGradeOffsetRange( groupCache, rockContext );
            (MinimumBirthMonth, MaximumBirthMonth) = GetBirthMonthRange( groupCache );
            AttendanceRecordRequiredForCheckIn = groupCache.AttendanceRecordRequiredForCheckIn;
            Gender = groupCache.GetAttributeValue( "Gender" ).ConvertToEnumOrNull<Gender>();
            DataViewGuids = groupCache.GetAttributeValue( "DataView" ).SplitDelimitedValues().AsGuidList();
            IsSpecialNeeds = groupCache.IsSpecialNeeds;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the age range specified by the group attribute value.
        /// </summary>
        /// <param name="groupCache">The group cache.</param>
        /// <returns>A tuple that contains the minimum and maximum values.</returns>
        private static (decimal? MinimumAge, decimal? MaximumAge) GetAgeRange( IHasAttributes groupCache )
        {
            // Get the age ranges.
            var ageRange = groupCache.GetAttributeValue( "AgeRange" ).ToStringSafe();
            var ageRangePair = ageRange.Split( new char[] { ',' }, StringSplitOptions.None );

            if ( ageRangePair.Length != 2 )
            {
                return (null, null);
            }

            return (ageRangePair[0].AsDecimalOrNull(), ageRangePair[1].AsDecimalOrNull());
        }

        /// <summary>
        /// Gets the birthdate range specified by the group attribute value.
        /// </summary>
        /// <param name="groupCache">The group cache.</param>
        /// <returns>A tuple that contains the minimum and maximum values.</returns>
        private static (DateTime? MinimumBirthdate, DateTime? MaximumBirthdate) GetBirthdateRange( IHasAttributes groupCache )
        {
            var birthdateRange = groupCache.GetAttributeValue( "BirthdateRange" ).ToStringSafe();
            var birthdateRangePair = birthdateRange.Split( new char[] { ',' }, StringSplitOptions.None );

            if ( birthdateRangePair.Length != 2 )
            {
                return (null, null);
            }

            return (birthdateRangePair[0].AsDateTime(), birthdateRangePair[1].AsDateTime());
        }

        /// <summary>
        /// Gets the birth month range specified by the group attribute value.
        /// </summary>
        /// <param name="groupCache">The group cache.</param>
        /// <returns>A tuple that contains the minimum and maximum values.</returns>
        private static (int? MinimumBirthdate, int? MaximumBirthdate) GetBirthMonthRange( IHasAttributes groupCache )
        {
            var birthMonthRange = groupCache.GetAttributeValue( "BirthMonthRange" ).ToStringSafe();
            var birthMonthRangePair = birthMonthRange.Split( new char[] { ',' }, StringSplitOptions.None );

            if ( birthMonthRangePair.Length != 2 )
            {
                return (null, null);
            }

            return (birthMonthRangePair[0].AsIntegerOrNull(), birthMonthRangePair[1].AsIntegerOrNull());
        }

        /// <summary>
        /// Gets the grade range specified by the group attribute value.
        /// </summary>
        /// <param name="groupCache">The group cache.</param>
        /// <param name="rockContext">The database context to use when loading data for cache.</param>
        /// <returns>A tuple that contains the minimum and maximum values.</returns>
        private static (int? MinimumGradeOffset, int? MaximumGradeOffset) GetGradeOffsetRange( IHasAttributes groupCache, RockContext rockContext )
        {
            string gradeOffsetRange = groupCache.GetAttributeValue( "GradeRange" ) ?? string.Empty;
            var gradeOffsetRangePair = gradeOffsetRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();

            if ( gradeOffsetRangePair.Length != 2 )
            {
                return (null, null);
            }

            var minGradeDefinedValue = gradeOffsetRangePair[0].HasValue
                ? DefinedValueCache.Get( gradeOffsetRangePair[0].Value, rockContext )
                : null;

            var maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue
                ? DefinedValueCache.Get( gradeOffsetRangePair[1].Value, rockContext )
                : null;

            // NOTE: the grade offsets are actually reversed because the range
            // defined values then specify the grade offset as the value, which
            // is the number of years until graduation. So the UI says "4th to 6th"
            // but the offset numbers are "8 to 6".
            return (maxGradeDefinedValue?.Value.AsIntegerOrNull(), minGradeDefinedValue?.Value.AsIntegerOrNull());
        }

        #endregion
    }
}
