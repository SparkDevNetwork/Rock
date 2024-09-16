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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;
using Rock.Enums.Crm;
using Rock.Lava;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Person
    {
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group">giving group</see> id.  If an individual would like their giving to be grouped with the rest of their family,
        /// this will be the id of their family group.  If they elect to contribute on their own, this value will be null.
        /// </summary>
        /// <value>
        /// The giving group id.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int? GivingGroupId
        {
            get
            {
                return _givingGroupId;
            }

            set
            {
                _givingGroupId = value;
                GivingId = _givingGroupId.HasValue ? $"G{_givingGroupId.Value}" : $"P{Id}";
            }
        }

        private int? _givingGroupId;

        /// <summary>
        /// Gets the Full Name of the Person using the NickName LastName Suffix format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Full Name of a Person using the NickName LastName Suffix format.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string FullName
        {
            get
            {
                // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so
                // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                return FormatFullName( NickName, LastName, SuffixValueId, this.RecordTypeValueId );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the initials for the person based on the nick name and last name.
        /// </summary>
        /// /// <value>
        /// A <see cref="System.String"/> representing the initials of a Person using the NickName and LastName.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string Initials
        {
            get
            {
                return $"{NickName.Truncate( 1, false )}{LastName.Truncate( 1, false )}";
            }
            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Determines whether the <see cref="RecordTypeValue"/> of this Person is Business
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBusiness()
        {
            return IsBusiness( this.RecordTypeValueId );
        }

        /// <summary>
        /// Gets a value indicating the specified recordTypeValueId is the record type of a <see cref="SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS" /> record type
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </value>
        private static bool IsBusiness( int? recordTypeValueId )
        {
            if ( recordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                return recordTypeValueId.Value == recordTypeValueIdBusiness;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="RecordTypeValue"/> of this Person is Nameless
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is nameless; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNameless()
        {
            int recordTypeValueIdNameless = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
            return this.RecordTypeValueId.HasValue && this.RecordTypeValueId.Value == recordTypeValueIdNameless;
        }

        /// <summary>
        /// Gets a value indicating the specified recordTypeValueId is the record type of a <see cref="SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS"/> record type
        /// </summary>
        /// <param name="recordTypeValueId">The record type value identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified record type value identifier is nameless; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNameless( int? recordTypeValueId )
        {
            if ( recordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
                return recordTypeValueId.Value == recordTypeValueIdBusiness;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the full name of the Person using the LastName, FirstName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of a Person using the LastName, FirstName format
        /// </value>
        [NotMapped]
        public virtual string FullNameReversed
        {
            get
            {
                return FormatFullNameReversed( this.LastName, this.NickName, this.SuffixValueId, this.RecordTypeValueId );
            }
        }

        /// <summary>
        /// Gets the full name reversed.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="nickName">Name of the nick.</param>
        /// <param name="suffixValueId">The suffix value identifier.</param>
        /// <param name="recordTypeValueId">The record type value identifier.</param>
        /// <returns></returns>
        public static string FormatFullNameReversed( string lastName, string nickName, int? suffixValueId, int? recordTypeValueId )
        {
            if ( IsBusiness( recordTypeValueId ) )
            {
                return lastName;
            }

            var fullName = new StringBuilder();

            fullName.Append( lastName );

            // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so
            // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
            if ( suffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Get( suffixValueId.Value );
                if ( suffix != null )
                {
                    fullName.AppendFormat( " {0}", suffix.Value );
                }
            }

            fullName.AppendFormat( ", {0}", nickName );
            return fullName.ToString();
        }

        /// <summary>
        /// Gets the Full Name of the Person using the Title FirstName LastName Suffix format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Full Name of a Person using the Title FirstName LastName Suffix format.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string FullNameFormal
        {
            get
            {
                if ( IsBusiness( this.RecordTypeValueId ) )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();

                fullName.AppendFormat( "{0} {1} {2}", TitleValue, FirstName, LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Value ) )
                {
                    fullName.AppendFormat( " {0}", SuffixValue.Value );
                }

                return fullName.ToString();
            }
        }

        /// <summary>
        /// Gets the full name of the Person using the LastName, FirstName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of a Person using the LastName, FirstName format
        /// </value>
        [NotMapped]
        public virtual string FullNameFormalReversed
        {
            get
            {
                if ( IsBusiness( this.RecordTypeValueId ) )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();
                fullName.Append( LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Value ) )
                {
                    fullName.AppendFormat( " {0}", SuffixValue.Value );
                }

                fullName.AppendFormat( ", {0}", FirstName );
                return fullName.ToString();
            }
        }

        /// <summary>
        /// Gets the day of the week the person's birthday falls on for the current year.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the day of the week the person's birthday falls on for the current year.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string BirthdayDayOfWeek
        {
            get
            {
                var thisYearsBirthdate = ThisYearsBirthdate;

                if ( !thisYearsBirthdate.HasValue )
                {
                    return string.Empty;
                }

                return thisYearsBirthdate.Value.ToString( "dddd" );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the day of the week the person's birthday falls on for the current year as a shortened string (e.g. Wed.)
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the shortened day of the week the person's birthday falls on for the current year.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string BirthdayDayOfWeekShort
        {
            get
            {
                var thisYearsBirthdate = ThisYearsBirthdate;

                if ( !thisYearsBirthdate.HasValue )
                {
                    return string.Empty;
                }

                return thisYearsBirthdate.Value.ToString( "ddd" );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the date that represents this person's birthday in the current year.
        /// </summary>
        internal DateTime? ThisYearsBirthdate
        {
            get
            {
                if ( !BirthMonth.HasValue || !BirthDay.HasValue )
                {
                    return null;
                }

                try
                {
                    if ( BirthMonth == 2 && BirthDay == 29 && !DateTime.IsLeapYear( RockDateTime.Now.Year ) )
                    {
                        // if their birthdate is 2/29 and the current year is NOT a leapyear, have their birthday be 2/28
                        return new DateTime( RockDateTime.Now.Year, BirthMonth.Value, 28, 0, 0, 0 );
                    }
                    else
                    {
                        return new DateTime( RockDateTime.Now.Year, BirthMonth.Value, BirthDay.Value, 0, 0, 0 );
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the URL of the person's photo.
        /// </summary>
        /// <value>
        /// URL of the photo
        /// </value>
        [LavaVisible]
        [NotMapped]
        public virtual string PhotoUrl
        {
            get
            {
                return Person.GetPersonPhotoUrl( this );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the Person's birth date. Note: Use <see cref="SetBirthDate(DateTime?)"/> set the Birthdate
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the Person's birthdate.  If no birthdate is available, null is returned. If the year is not available then the birthdate is returned with the DateTime.MinValue.Year.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? BirthDate
        {
            get
            {
                _birthDate = CalculateBirthDate();
                return _birthDate;
            }

            private set
            {
                _birthDate = value;
            }
        }

        private DateTime? _birthDate;

        /// <summary>
        /// Calculates the birthdate from the BirthYear, BirthMonth, and BirthDay.
        /// Will return null if BirthMonth or BirthDay is null.
        /// If BirthYear is null then DateTime.MinValue.Year (Year = 1) is used.
        /// </summary>
        /// <returns></returns>
        private DateTime? CalculateBirthDate()
        {
            if ( BirthDay == null || BirthMonth == null )
            {
                return null;
            }
            else
            {
                if ( BirthMonth <= 12 )
                {
                    if ( BirthDay <= DateTime.DaysInMonth( BirthYear ?? DateTime.MinValue.Year, BirthMonth.Value ) )
                    {
                        return new DateTime( BirthYear ?? DateTime.MinValue.Year, BirthMonth.Value, BirthDay.Value );
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Sets the birth date, which will set the BirthMonth, BirthDay, and BirthYear values
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetBirthDate( DateTime? value )
        {
            if ( value.HasValue )
            {
                BirthMonth = value.Value.Month;
                BirthDay = value.Value.Day;
                if ( value.Value.Year != DateTime.MinValue.Year )
                {
                    BirthYear = value.Value.Year;
                }
                else
                {
                    BirthYear = null;
                }
            }
            else
            {
                BirthMonth = null;
                BirthDay = null;
                BirthYear = null;
            }
        }

        /// <summary>
        /// Gets the age of the person in years. For infants under the age of 1, the value returned would be 0.
        /// To print the age as a string use <see cref="FormatAge(bool)"/> 
        /// </summary>
        /// <param name="birthDate">The birth date.</param>
        /// <param name="deceasedDate">The deceased date.</param>
        /// <returns></returns>
        public static int? GetAge( DateTime? birthDate, DateTime? deceasedDate )
        {
            if ( birthDate.HasValue && birthDate.Value.Year != DateTime.MinValue.Year )
            {
                DateTime asOfDate = deceasedDate.HasValue ? deceasedDate.Value : RockDateTime.Today;
                int age = asOfDate.Year - birthDate.Value.Year;
                if ( birthDate.Value > asOfDate.AddYears( -age ) )
                {
                    // their birthdate is after today's date, so they aren't a year older yet
                    age--;
                }

                return age;
            }

            return null;
        }

        /// <summary>
        /// Gets the age.
        /// </summary>
        /// <param name="birthDate">The birth date.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use GetAge( birthDate, deceasedDate ) instead." )]
        public static int? GetAge( DateTime? birthDate )
        {
            if ( birthDate.HasValue && birthDate.Value.Year != DateTime.MinValue.Year )
            {
                DateTime today = RockDateTime.Today;
                int age = today.Year - birthDate.Value.Year;
                if ( birthDate.Value > today.AddYears( -age ) )
                {
                    // their birthdate is after today's date, so they aren't a year older yet
                    age--;
                }

                return age;
            }

            return null;
        }

        /// <summary>
        /// Formats the age with unit (year, month, day) suffix depending on the age of the individual.
        /// </summary>
        /// <param name="condensed">if set to <c>true</c> age in years is returned without a unit suffix.</param>
        /// <returns></returns>
        public string FormatAge( bool condensed = false )
        {
            if (BirthDate > DateTime.Now)
            {
                return string.Empty;
            }

            var age = Age ?? GetAge( BirthDate, DeceasedDate );
            if ( age != null )
            {
                if ( condensed )
                {
                    return age.ToString();
                }

                if ( age > 0 )
                {
                    return age + ( age == 1 ? " yr" : " years" );
                }
                else if ( age < -1 )
                {
                    return string.Empty;
                }
            }

            // For infants less than an year old
            var asOfDate = DeceasedDate.HasValue ? DeceasedDate.Value : RockDateTime.Today;
            if ( BirthYear != null && BirthMonth != null )
            {
                int months = asOfDate.Month - BirthMonth.Value;
                if ( BirthYear < asOfDate.Year )
                {
                    months = months + 12;
                }

                if ( BirthDay > asOfDate.Day )
                {
                    months--;
                }

                if ( months > 0 )
                {
                    return months + ( months == 1 ? " mo" : " mos" );
                }
            }

            if ( BirthYear != null && BirthMonth != null && BirthDay != null )
            {
                int days = asOfDate.Day - BirthDay.Value;
                if ( days < 0 )
                {
                    // Add the number of days in the birth month
                    var birthMonth = new DateTime( BirthYear.Value, BirthMonth.Value, 1 );
                    days = days + birthMonth.AddMonths( 1 ).AddDays( -1 ).Day;
                }

                return days + ( days == 1 ? " day" : " days" );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the next birth day.
        /// </summary>
        /// <value>
        /// The next birth day.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual DateTime? NextBirthDay
        {
            get
            {
                if ( BirthMonth.HasValue && BirthDay.HasValue )
                {
                    var today = RockDateTime.Today;
                    var nextBirthDay = RockDateTime.New( today.Year, BirthMonth.Value, BirthDay.Value );
                    if ( nextBirthDay.HasValue && nextBirthDay.Value.CompareTo( today ) < 0 )
                    {
                        nextBirthDay = RockDateTime.New( today.Year + 1, BirthMonth.Value, BirthDay.Value );
                    }

                    return nextBirthDay;
                }

                return null;
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the number of days until the Person's birthday. This is an in-memory calculation. If needed in a LinqToSql query
        /// use DaysUntilBirthday property instead
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of days until the Person's birthday. If the person's birthdate is
        /// not available returns Int.MaxValue
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual int DaysToBirthday
        {
            get
            {
                if ( BirthDay.HasValue && BirthMonth.HasValue )
                {
                    if ( BirthDay.Value >= 1 && BirthDay.Value <= 31 && BirthMonth.Value >= 1 && BirthMonth.Value <= 12 )
                    {
                        var today = RockDateTime.Today;

                        int day = BirthDay.Value;
                        int month = BirthMonth.Value;
                        int year = today.Year;
                        if ( month < today.Month || ( month == today.Month && day < today.Day ) )
                        {
                            year++;
                        }

                        DateTime bday = DateTime.MinValue;
                        while ( !DateTime.TryParse( BirthMonth.Value.ToString() + "/" + day.ToString() + "/" + year.ToString(), out bday ) && day > 28 )
                        {
                            day--;
                        }

                        if ( bday != DateTime.MinValue )
                        {
                            return Convert.ToInt32( bday.Subtract( today ).TotalDays );
                        }
                    }
                }

                return int.MaxValue;
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the Person's precise age (includes the fraction of the year).
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> representing the Person's age (including fraction of year)
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual double? AgePrecise
        {
            get
            {
                DateTime? bday = this.BirthDate;
                if ( this.BirthYear.HasValue && bday.HasValue )
                {
                    // Calculate years
                    DateTime today = RockDateTime.Today;
                    int years = today.Year - bday.Value.Year;
                    if ( bday > today.AddYears( -years ) )
                    {
                        years--;
                    }

                    // Calculate days between last and next bday (differs on leap years).
                    DateTime lastBday = bday.Value.AddYears( years );
                    DateTime nextBday = lastBday.AddYears( 1 );
                    double daysInYear = nextBday.Subtract( lastBday ).TotalDays;

                    // Calculate days since last bday
                    double days = today.Subtract( lastBday ).TotalDays;

                    return years + ( days / daysInYear );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the number of days until the Person's anniversary. This is an in-memory calculation. If needed in a LinqToSql query
        /// use DaysUntilAnniversary property instead
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of days until the Person's anniversary. If the person's anniversary
        /// is not available returns Int.MaxValue
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual int DaysToAnniversary
        {
            get
            {
                if ( AnniversaryDate.HasValue )
                {
                    var today = RockDateTime.Today;

                    int day = AnniversaryDate.Value.Day;
                    int month = AnniversaryDate.Value.Month;
                    int year = today.Year;
                    if ( month < today.Month || ( month == today.Month && day < today.Day ) )
                    {
                        year++;
                    }

                    DateTime aday = DateTime.MinValue;
                    while ( !DateTime.TryParse( month.ToString() + "/" + day.ToString() + "/" + year.ToString(), out aday ) && day > 28 )
                    {
                        day--;
                    }

                    if ( aday != DateTime.MinValue )
                    {
                        return Convert.ToInt32( aday.Subtract( today ).TotalDays );
                    }
                }

                return int.MaxValue;
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the next anniversary.
        /// </summary>
        /// <value>
        /// The next anniversary.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual DateTime? NextAnniversary
        {
            get
            {
                if ( AnniversaryDate.HasValue )
                {
                    var today = RockDateTime.Today;
                    var nextAnniversary = RockDateTime.New( today.Year, AnniversaryDate.Value.Month, AnniversaryDate.Value.Day );
                    if ( nextAnniversary.HasValue && nextAnniversary.Value.CompareTo( today ) < 0 )
                    {
                        nextAnniversary = RockDateTime.New( today.Year + 1, AnniversaryDate.Value.Month, AnniversaryDate.Value.Day );
                    }

                    return nextAnniversary;
                }

                return null;
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets or sets the number of days until their next anniversary. This is a computed column and can be used
        /// in LinqToSql queries, but there is no in-memory calculation. Avoid using property outside of
        /// a linq query. Use DaysToAnniversary instead.
        /// NOTE: If their anniversary is Feb 29, and this isn't a leap year, it'll treat Feb 28th as their anniversary when doing this calculation
        /// </summary>
        /// <value>
        /// The number of days until their next anniversary
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public int? DaysUntilAnniversary { get; set; }

        /// <summary>
        /// Gets or sets the grade offset, which is the number of years until their graduation date.  This is used to determine which Grade (Defined Value) they are in
        /// </summary>
        /// <value>
        /// The grade offset.
        /// </value>
        [NotMapped]
        [DataMember]
        [RockClientInclude( "The Grade Offset of the person, which is the number of years until their graduation date. See GradeFormatted to see their current Grade. [Readonly]" )]
        public virtual int? GradeOffset
        {
            get
            {
                return GradeOffsetFromGraduationYear( GraduationYear );
            }

            set
            {
                GraduationYear = GraduationYearFromGradeOffset( value );
            }
        }

        /// <summary>
        /// Gets the has graduated.
        /// </summary>
        /// <value>
        /// The has graduated.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual bool? HasGraduated
        {
            get
            {
                return HasGraduatedFromGradeOffset( GradeOffset );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the grade string.
        /// </summary>
        /// <value>
        /// The grade string.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual string GradeFormatted
        {
            get
            {
                return GradeFormattedFromGradeOffset( GradeOffset );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the impersonation parameter.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string ImpersonationParameter
        {
            get
            {
                return this.GetImpersonationParameter();
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// NOTE: Use the GetImpersonationParameter(...) methods to specify an expiration date, usage limit or pageid
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> that represents a URL friendly version of the entity's unique key.
        /// </value>
        [NotMapped]
        public override string EncryptedKey
        {
            get
            {
                // in the case of Person, use an encrypted PersonToken instead of the base.UrlEncodedKey
                return this.GetImpersonationToken();
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// NOTE: Use the GetImpersonationParameter(...) methods to specify an expiration date, usage limit or pageid
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> that represents a URL friendly version of the entity's unique key.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public override string UrlEncodedKey
        {
            get
            {
                // in the case of Person, use an encrypted PersonToken instead of the base.UrlEncodedKey
                return this.GetImpersonationToken();
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.PersonAlias">primary alias</see>.
        /// </summary>
        /// <value>
        /// The primary alias.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual PersonAlias PrimaryAlias
        {
            get
            {
                return Aliases.FirstOrDefault( a => a.AliasPersonId == Id );
            }
        }

        #region Methods

        /// <summary>
        /// Gets the <see cref="Rock.Model.UserLogin"/> of the user being impersonated.
        /// </summary>
        /// <value>
        /// Th <see cref="Rock.Model.UserLogin"/> of the user being impersonated.
        /// </value>
        public virtual UserLogin GetImpersonatedUser()
        {
            UserLogin user = new UserLogin();
            user.UserName = this.FullName;
            user.PersonId = this.Id;
            user.Person = this;
            return user;
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// </summary>
        /// <returns></returns>
        public virtual string GetImpersonationToken()
        {
            return GetImpersonationToken( null, null, null );
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the impersonation parameter.
        /// </value>
        public virtual string GetImpersonationParameter()
        {
            return GetImpersonationParameter( null, null, null );
        }

        /// <summary>
        /// Builds an encrypted action identifier string for the person instance.
        /// Returns the encrypted URLEncoded identifier"
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the person action identifier.
        /// </value>
        public virtual string GetPersonActionIdentifier( string action )
        {
            var encryptedToken = Rock.Security.Encryption.EncryptString( $"{Guid}>{action}" );

            // do a Replace('%', '!') after we UrlEncode it (to make it more safely embeddable in HTML and cross browser compatible)
            return System.Web.HttpUtility.UrlEncode( encryptedToken ).Replace( '%', '!' );
        }

        /// <summary>
        /// Builds an encrypted action identifier stringfor the person instance.
        /// Returns the encrypted URLEncoded Token along with the identifier key in the form of "rckipid={PersonActionIdentifier}"
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the person action identifier.
        /// </value>
        public virtual string GetPersonActionIdentifierParameter( string action )
        {
            return $"rckid={GetPersonActionIdentifier( action )}";
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the impersonation parameter.
        /// </value>
        public virtual string GetImpersonationParameter( DateTime? expireDateTime, int? usageLimit, int? pageId )
        {
            return $"rckipid={GetImpersonationToken( expireDateTime, usageLimit, pageId )}";
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// </summary>
        /// <returns></returns>
        public virtual string GetImpersonationToken( DateTime? expireDateTime, int? usageLimit, int? pageId )
        {
            return PersonToken.CreateNew( this.PrimaryAlias, expireDateTime, usageLimit, pageId );
        }

        /// <summary>
        /// Gets an anchor tag for linking to person profile
        /// </summary>
        /// <param name="rockUrlRoot">The rock URL root.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <returns></returns>
        public string GetAnchorTag( string rockUrlRoot, string cssClass = "" )
        {
            return string.Format( "<a class='{0}' href='{1}Person/{2}'>{3}</a>", cssClass, rockUrlRoot, Id, FullName );
        }

        /// <summary>
        /// Gets an anchor tag to send person a communication
        /// </summary>
        /// <param name="rockUrlRoot">The rock URL root.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <param name="preText">The pre text.</param>
        /// <param name="postText">The post text.</param>
        /// <param name="styles">The styles.</param>
        /// <returns></returns>
        /// <value>
        /// The email tag.
        /// </value>
        public string GetEmailTag( string rockUrlRoot, string cssClass = "", string preText = "", string postText = "", string styles = "" )
        {
            return GetEmailTag( rockUrlRoot, null, cssClass, preText, postText, styles );
        }

        /// <summary>
        /// Uploads the person's photo from the specified url and sets it as the person's Photo using the default BinaryFileType.
        /// </summary>
        /// <param name="photoUri">The photo URI.</param>
        public void SetPhotoFromUrl( Uri photoUri )
        {
            this.SetPhotoFromUrl( photoUri, Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
        }

        /// <summary>
        /// Uploads the person's photo from the specified url and sets it as the person's Photo using the specified BinaryFileType.
        /// </summary>
        /// <param name="photoUri">The photo URI.</param>
        /// <param name="binaryFileTypeGuid">The binary file type unique identifier.</param>
        public void SetPhotoFromUrl( Uri photoUri, Guid binaryFileTypeGuid )
        {
            try
            {
                HttpWebRequest imageRequest = ( HttpWebRequest ) HttpWebRequest.Create( photoUri );
                HttpWebResponse imageResponse = ( HttpWebResponse ) imageRequest.GetResponse();
                var imageStream = imageResponse.GetResponseStream();
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileType = new BinaryFileTypeService( rockContext ).GetNoTracking( binaryFileTypeGuid );
                    using ( MemoryStream photoData = new MemoryStream() )
                    {
                        imageStream.CopyTo( photoData );
                        var fileName = this.FullName.RemoveSpaces().MakeValidFileName();
                        if ( fileName.IsNullOrWhiteSpace() )
                        {
                            fileName = "PersonPhoto";
                        }

                        var binaryFile = new BinaryFile()
                        {
                            FileName = fileName,
                            MimeType = imageResponse.ContentType,
                            BinaryFileTypeId = binaryFileType.Id,
                            IsTemporary = true
                        };

                        binaryFile.SetStorageEntityTypeId( binaryFileType.StorageEntityTypeId );

                        byte[] photoDataBytes = photoData.ToArray();
                        binaryFile.FileSize = photoDataBytes.Length;
                        binaryFile.ContentStream = new MemoryStream( photoDataBytes );

                        var binaryFileService = new BinaryFileService( rockContext );
                        binaryFileService.Add( binaryFile );
                        rockContext.SaveChanges();

                        this.PhotoId = binaryFile.Id;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Unable to set photo from {photoUri},", ex );
            }
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object.</returns>
        public override Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.TryAdd( "Age", AgePrecise );
            dictionary.TryAdd( "DaysToBirthday", DaysToBirthday );
            dictionary.TryAdd( "DaysToAnniversary", DaysToAnniversary );
            dictionary.TryAdd( "FullName", FullName );
            dictionary.TryAdd( "PrimaryAliasId", this.PrimaryAliasId );
            return dictionary;
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( person != null && person.Guid.Equals( this.Guid ) )
            {
                return true;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// Gets the campus.
        /// </summary>
        /// <returns></returns>
        public Campus GetCampus()
        {
            // If PrimaryCampus has been calculated, use that. Otherwise, retrieve the campus of the primary family.
            if ( this.PrimaryCampus != null )
            {
                return this.PrimaryCampus;
            }
            else
            {
                var primaryFamily = this.GetFamily();

                return primaryFamily != null ? primaryFamily.Campus : null;
            }
        }

        /// <summary>
        /// Gets the campus ids for all the families that a person belongs to.
        /// </summary>
        /// <returns></returns>
        public List<int> GetCampusIds()
        {
            return this.GetFamilies()
                .Where( f => f.CampusId.HasValue )
                .Select( f => f.CampusId.Value )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Calculates the top-most signal and updates the person properties.
        /// </summary>
        public void CalculateSignals()
        {
            if ( Signals != null )
            {
                var rockContext = new RockContext();
                var topSignal = Signals
                    .Where( s => !s.ExpirationDate.HasValue || s.ExpirationDate >= RockDateTime.Now )
                    .Select( s => new
                    {
                        Id = s.Id,
                        SignalType = SignalTypeCache.Get( s.SignalTypeId )
                    } )
                    .OrderBy( s => s.SignalType.Order )
                    .ThenBy( s => s.SignalType.Id )
                    .FirstOrDefault();

                TopSignalId = topSignal?.Id;
                TopSignalIconCssClass = topSignal?.SignalType.SignalIconCssClass;
                TopSignalColor = topSignal?.SignalType.SignalColor;
            }
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        /// <param name="phoneType">Type of the phone.</param>
        /// <returns></returns>
        public PhoneNumber GetPhoneNumber( Guid phoneType )
        {
            int numberTypeValueId = DefinedValueCache.GetId( phoneType ) ?? 0;
            return PhoneNumbers?.FirstOrDefault( n => n.NumberTypeValueId == numberTypeValueId );
        }

        /// <summary>
        /// Determines whether this Person can receive emails.
        /// </summary>
        /// <param name="isBulk">if set to <c>true</c> this method will validate that this Person can receive bulk emails.</param>
        /// <returns>
        ///   <c>true</c> if this Person can receive emails; otherwise, <c>false</c>.
        /// </returns>
        public bool CanReceiveEmail( bool isBulk = true )
        {
            var userAllowsBulk = EmailPreference != EmailPreference.NoMassEmails;

            return Email.IsNotNullOrWhiteSpace()
                    && IsEmailActive
                    && EmailPreference != EmailPreference.DoNotEmail
                    && ( !isBulk || userAllowsBulk );
        }
        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Gets the family salutation.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="includeChildren">if set to <c>true</c> [include children].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="useFormalNames">if set to <c>true</c> [use formal names].</param>
        /// <param name="finalSeparator">The final separator.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        [RockObsolete( "1.12.4" )]
        [Obsolete( "Use Person.PrimaryFamily.GroupSalutation instead" )]
        public static string GetFamilySalutation( Person person, bool includeChildren = false, bool includeInactive = true, bool useFormalNames = false, string finalSeparator = "&", string separator = "," )
        {
            var args = new CalculateFamilySalutationArgs( includeChildren )
            {
                IncludeInactive = includeInactive,
                UseFormalNames = useFormalNames,
                FinalSeparator = finalSeparator,
                Separator = separator,
                LimitToPersonIds = null
            };

            return CalculateFamilySalutation( person, args );
        }

        /// <summary>
        ///
        /// </summary>
        public sealed class CalculateFamilySalutationArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CalculateFamilySalutationArgs"/> class.
            /// </summary>
            /// <param name="includeChildren">if set to <c>true</c> [include children].</param>
            public CalculateFamilySalutationArgs( bool includeChildren )
            {
                IncludeChildren = includeChildren;
            }

            /// <summary>
            /// Gets or sets a value indicating whether [include children].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include children]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeChildren { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether [include inactive].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeInactive { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether [use formal names].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [use formal names]; otherwise, <c>false</c>.
            /// </value>
            public bool UseFormalNames { get; set; } = false;

            /// <summary>
            /// Gets or sets the final separator.
            /// </summary>
            /// <value>
            /// The final separator.
            /// </value>
            public string FinalSeparator { get; set; } = "&";

            /// <summary>
            /// Gets or sets the separator.
            /// </summary>
            /// <value>
            /// The separator.
            /// </value>
            public string Separator { get; set; } = ",";

            /// <summary>
            /// Gets or sets the limit to person ids.
            /// </summary>
            /// <value>
            /// The limit to person ids.
            /// </value>
            public int[] LimitToPersonIds { get; set; } = null;

            /// <summary>
            /// Gets or sets the rock context.
            /// </summary>
            /// <value>
            /// The rock context.
            /// </value>
            public RockContext RockContext { get; set; } = null;
        }

        /// <summary>
        /// Calculates the family salutation.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="calculateFamilySalutationArgs">The calculate family salutation arguments.</param>
        /// <returns></returns>
        public static string CalculateFamilySalutation( Person person, CalculateFamilySalutationArgs calculateFamilySalutationArgs )
        {
            string familySalutation = null;
            if ( person.PrimaryFamilyId.HasValue )
            {
                var primaryFamily = person.PrimaryFamily ?? new GroupService( new RockContext() ).Get( person.PrimaryFamilyId.Value );
                if ( primaryFamily != null )
                {
                    familySalutation = GroupService.CalculateFamilySalutation( primaryFamily, calculateFamilySalutationArgs );
                }
            }

            if ( familySalutation.IsNullOrWhiteSpace() )
            {
                // This shouldn't happen, but if somehow there are no family members, just return the specified person's name
                familySalutation = $"{( calculateFamilySalutationArgs.UseFormalNames ? person.FirstName : person.NickName )} {person.LastName}";
            }

            return familySalutation;
        }

        /// <summary>
        /// Gets the person photo URL.
        /// </summary>
        /// <param name="person">The person to get the photo for.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <returns></returns>
        public static string GetPersonPhotoUrl( Person person, int? maxWidth = null, int? maxHeight = null )
        {
            // Convert maxsizes to size by selecting the greater of the values of maxwidth and maxheight
            int? size = maxWidth;

            if ( maxHeight.HasValue )
            {
                if ( size.HasValue && size.Value < maxHeight.Value )
                {
                    size = maxHeight.Value;
                }
            }

            return GetPersonPhotoUrl( person.Initials, person.PhotoId, person.Age, person.Gender, person.RecordTypeValueId, person.AgeClassification, size );
        }

        /// <summary>
        /// Gets the person photo URL from a person id (warning this will cause a database lookup).
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <returns></returns>
        public static string GetPersonPhotoUrl( int personId, int? maxWidth = null, int? maxHeight = null )
        {
            // Convert maxsizes to size by selecting the greater of the values of maxwidth and maxheight
            int? size = maxWidth;

            if ( maxHeight.HasValue )
            {
                if ( size.HasValue && size.Value < maxHeight.Value )
                {
                    size = maxHeight.Value;
                }
            }

            using ( RockContext rockContext = new RockContext() )
            {
                Person person = new PersonService( rockContext ).Get( personId );
                return GetPersonPhotoUrl( person, size );
            }
        }

        /// <summary>
        /// Gets the 'NoPictureUrl' for the person based on their Gender, Age, and RecordType (Person or Business)
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPersonNoPictureUrl( Person person, int? maxWidth = null, int? maxHeight = null )
        {
            // Convert maxsizes to size by selecting the greater of the values of maxwidth and maxheight
            int? size = maxWidth;

            if ( maxHeight.HasValue )
            {
                if ( size.HasValue && size.Value < maxHeight.Value )
                {
                    size = maxHeight.Value;
                }
            }

            return GetPersonPhotoUrl( person.Initials, null, person.Age, person.Gender, person.RecordTypeValueId, person.AgeClassification, size );
        }

        /// <summary>
        /// Gets the photo path based on the gender and adult/child status.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <param name="isAdult">if set to <c>true</c> is adult.</param>
        /// <returns></returns>
        private static string GetPhotoPath( Gender gender, bool isAdult )
        {
            if ( isAdult )
            {
                switch ( gender )
                {
                    case Gender.Female:
                        {
                            return "Assets/Images/person-no-photo-female.svg?";
                        }

                    case Gender.Male:
                        {
                            return "Assets/Images/person-no-photo-male.svg?";
                        }

                    default:
                        {
                            return "Assets/Images/person-no-photo-unknown.svg?";
                        }
                }
            }

            switch ( gender )
            {
                case Gender.Female:
                    {
                        return "Assets/Images/person-no-photo-child-female.svg?";
                    }

                case Gender.Male:
                    {
                        return "Assets/Images/person-no-photo-child-male.svg?";
                    }

                default:
                    {
                        return "Assets/Images/person-no-photo-child-unknown.svg?";
                    }
            }
        }

        /// <summary>
        /// Gets the HTML markup to use for displaying the top-most signal icon for this person.
        /// </summary>
        /// <returns>A string that represents the Icon to display or an empty string if no signal is active.</returns>
        public string GetSignalMarkup()
        {
            return Person.GetSignalMarkup( TopSignalColor, TopSignalIconCssClass );
        }

        /// <summary>
        /// Gets the HTML markup to use for displaying the given signal color and icon.
        /// </summary>
        /// <returns>A string that represents the Icon to display or an empty string if no signal color was provided.</returns>
        public static string GetSignalMarkup( string signalColor, string signalIconCssClass )
        {
            if ( !string.IsNullOrWhiteSpace( signalColor ) )
            {
                return string.Format(
                    "<i class='{1}' style='color: {0};'></i>",
                    signalColor,
                    !string.IsNullOrWhiteSpace( signalIconCssClass ) ? signalIconCssClass : "fa fa-flag" );
            }

            return string.Empty;
        }

        /// <summary>
        /// Adds the related person to the selected person's known relationships with a role of 'Can check in' which
        /// is typically configured to allow check-in.  If an inverse relationship is configured for 'Can check in'
        /// (i.e. 'Allow check in by'), that relationship will also be created.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32" /> representing the Id of the Person.</param>
        /// <param name="relatedPersonId">A <see cref="System.Int32" /> representing the Id of the related Person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void CreateCheckinRelationship( int personId, int relatedPersonId, RockContext rockContext = null )
        {
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) );
            if ( canCheckInRole != null )
            {
                rockContext = rockContext ?? new RockContext();
                var groupMemberService = new GroupMemberService( rockContext );
                groupMemberService.CreateKnownRelationship( personId, relatedPersonId, canCheckInRole.Id );
            }
        }

        /// <summary>
        /// Formats the full name.
        /// </summary>
        /// <param name="nickName">The nick name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns></returns>
        public static string FormatFullName( string nickName, string lastName, string suffix )
        {
            var fullName = new StringBuilder();

            fullName.AppendFormat( "{0} {1}", nickName, lastName );

            if ( !string.IsNullOrWhiteSpace( suffix ) )
            {
                fullName.AppendFormat( " {0}", suffix );
            }

            return fullName.ToString();
        }

        /// <summary>
        /// Formats the full name.
        /// </summary>
        /// <param name="nickName">The nick name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="suffixValueId">The suffix value identifier.</param>
        /// <param name="recordTypeValueId">The record type value identifier.</param>
        /// <returns></returns>
        public static string FormatFullName( string nickName, string lastName, int? suffixValueId, int? recordTypeValueId = null )
        {
            if ( IsNameless( recordTypeValueId ) )
            {
                return "Nameless Person";
            }

            if ( IsBusiness( recordTypeValueId ) )
            {
                return lastName;
            }

            if ( suffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Get( suffixValueId.Value );
                if ( suffix != null )
                {
                    return FormatFullName( nickName, lastName, suffix.Value );
                }
            }

            return FormatFullName( nickName, lastName, string.Empty );
        }

        /// <summary>
        /// Given a grade offset, returns the graduation year
        /// </summary>
        /// <param name="gradeOffset"></param>
        /// <returns></returns>
        public static int? GraduationYearFromGradeOffset( int? gradeOffset )
        {
            if ( gradeOffset.HasValue && gradeOffset.Value >= 0 )
            {
                return PersonService.GetCurrentGraduationYear() + gradeOffset.Value;
            }

            return null;
        }

        /// <summary>
        /// Given a graduation year returns the grade offset
        /// </summary>
        /// <param name="graduationYear">The graduation year.</param>
        /// <returns></returns>
        public static int? GradeOffsetFromGraduationYear( int? graduationYear )
        {
            if ( !graduationYear.HasValue )
            {
                return null;
            }
            else
            {
                return graduationYear.Value - PersonService.GetCurrentGraduationYear();
            }
        }

        /// <summary>
        /// Determines whether person has graduated based on grade offset
        /// </summary>
        /// <param name="gradeOffset">The grade offset.</param>
        /// <returns></returns>
        public static bool? HasGraduatedFromGradeOffset( int? gradeOffset )
        {
            if ( gradeOffset.HasValue )
            {
                return gradeOffset < 0;
            }

            return null;
        }

        /// <summary>
        /// Formats the grade based on graduation year
        /// </summary>
        /// <param name="graduationYear">The graduation year.</param>
        /// <returns></returns>
        public static string GradeFormattedFromGraduationYear( int? graduationYear )
        {
            return GradeFormattedFromGradeOffset( GradeOffsetFromGraduationYear( graduationYear ) );
        }

        /// <summary>
        /// Gets the grade abbreviation attribute based on graduation year.
        /// </summary>
        /// <param name="graduationYear">The graduation year.</param>
        /// <returns>
        /// Returns a string of the abbreviation attribute.
        /// </returns>
        internal static string GradeAbbreviationFromGraduationYear( int? graduationYear )
        {
            return GradeAbbreviationFromGradeOffset( GradeOffsetFromGraduationYear( graduationYear ) );
        }

        /// <summary>
        /// Formats the grade based on grade offset
        /// </summary>
        /// <param name="gradeOffset">The grade offset.</param>
        /// <returns></returns>
        public static string GradeFormattedFromGradeOffset( int? gradeOffset )
        {
            // If the grade offset does not have a value or it is less than zero, return an empty string.
            if ( !gradeOffset.HasValue || gradeOffset < 0 )
            {
                return string.Empty;
            }

            var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
            if ( schoolGrades == null )
            {
                return string.Empty;
            }

            var sortedGradeValues = schoolGrades.DefinedValues.OrderBy( a => a.Value.AsInteger() );
            var schoolGradeValue = sortedGradeValues.Where( a => a.Value.AsInteger() >= gradeOffset.Value ).FirstOrDefault();
            if ( schoolGradeValue == null )
            {
                return string.Empty;
            }

            return schoolGradeValue.Description;
        }

        /// <summary>
        /// Gets the grade abbreviation attribute based on grade offset.
        /// </summary>
        /// <param name="gradeOffset">The grade offset.</param>
        /// <returns>
        /// Returns a string of the abbreviation attribute.
        /// </returns>
        internal static string GradeAbbreviationFromGradeOffset( int? gradeOffset )
        {
            // If the grade offset does not have a value or it is less than zero, return an empty string.
            if ( !gradeOffset.HasValue || gradeOffset < 0 )
            {
                return string.Empty;
            }

            var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
            if ( schoolGrades == null )
            {
                return string.Empty;
            }

            var sortedGradeValues = schoolGrades.DefinedValues.OrderBy( a => a.Value.AsInteger() );
            var schoolGradeValue = sortedGradeValues.Where( a => a.Value.AsInteger() >= gradeOffset.Value ).FirstOrDefault();
            if ( schoolGradeValue == null )
            {
                return string.Empty;
            }

            // If there is an abbreviation, return it.  Otherwise, return an empty string.
            return schoolGradeValue.Attributes.ContainsKey( "Abbreviation" ) ? schoolGradeValue.GetAttributeValue( "Abbreviation" ) : string.Empty;
        }

        /// <summary>
        /// Gets the home locations for all the person id's passed in. If a person is in
        /// more than one family or that family has more than one home address a single
        /// location is provided.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Dictionary<int, Location> GetHomeLocations( List<int> personIds, RockContext rockContext = null )
        {
            var personHomeAddresses = new Dictionary<int, Location>();

            if ( personIds != null )
            {
                rockContext = rockContext ?? new RockContext();

                Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                Guid? familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

                if ( homeAddressGuid.HasValue && familyGuid.HasValue )
                {
                    var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                    var familyGroupType = GroupTypeCache.Get( familyGuid.Value );
                    if ( homeAddressDv != null && familyGroupType != null )
                    {
                        var personLocations = new GroupMemberService( rockContext ).Queryable()
                                .Where( m =>
                                     personIds.Contains( m.PersonId )
                                     && m.Group.GroupTypeId == familyGroupType.Id )
                                .Select( m => new
                                {
                                    m.PersonId,
                                    Location = m.Group.GroupLocations
                                                                     .Where( gl => gl.GroupLocationTypeValueId == homeAddressDv.Id )
                                                                     .Select( gl => gl.Location )
                                                                     .FirstOrDefault()
                                } ).ToList();

                        foreach ( var personLocation in personLocations )
                        {
                            if ( !personHomeAddresses.ContainsKey( personLocation.PersonId ) )
                            {
                                personHomeAddresses.Add( personLocation.PersonId, personLocation.Location );
                            }
                        }
                    }
                }
            }

            return personHomeAddresses;
        }

        /// <summary>
        /// Gets the age bracket.
        /// </summary>
        /// <param name="age">The age.</param>
        /// <returns></returns>
        public static AgeBracket GetAgeBracket( int? age )
        {
            if ( age == null )
            {
                return AgeBracket.Unknown;
            }

            if ( age >= 0 && age <= 5 )
            {
                return Enums.Crm.AgeBracket.ZeroToFive;
            }
            else if ( age >= 6 && age <= 12 )
            {
                return Enums.Crm.AgeBracket.SixToTwelve;
            }
            else if ( age >= 13 && age <= 17 )
            {
                return Enums.Crm.AgeBracket.ThirteenToSeventeen;
            }
            else if ( age >= 18 && age <= 24 )
            {
                return Enums.Crm.AgeBracket.EighteenToTwentyFour;
            }
            else if ( age >= 25 && age <= 34 )
            {
                return Enums.Crm.AgeBracket.TwentyFiveToThirtyFour;
            }
            else if ( age >= 35 && age <= 44 )
            {
                return Enums.Crm.AgeBracket.ThirtyFiveToFortyFour;
            }
            else if ( age >= 45 && age <= 54 )
            {
                return Enums.Crm.AgeBracket.FortyFiveToFiftyFour;
            }
            else if ( age >= 55 && age <= 64 )
            {
                return Enums.Crm.AgeBracket.FiftyFiveToSixtyFour;
            }
            else
            {
                return Enums.Crm.AgeBracket.SixtyFiveOrOlder;
            }
        }

        #endregion

        #region Indexing Methods

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            List<PersonIndex> indexablePersonList = new List<PersonIndex>();

            var recordTypePersonId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordTypeBusinessId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            RockContext rockContext = new RockContext();

            // return people
            var people = new PersonService( rockContext ).Queryable().AsNoTracking()
                                .Where( p => p.RecordTypeValueId == recordTypePersonId );

            int recordCounter = 0;

            foreach ( var person in people )
            {
                recordCounter++;

                var indexablePerson = PersonIndex.LoadByModel( person );
                indexablePersonList.Add( indexablePerson );

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexablePersonList );
                    indexablePersonList = new List<PersonIndex>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexablePersonList );

            // return businesses
            var businesses = new PersonService( rockContext ).Queryable().AsNoTracking()
                                .Where( p =>
                                     p.IsSystem == false
                                     && p.RecordTypeValueId == recordTypeBusinessId );

            List<BusinessIndex> indexableBusinessList = new List<BusinessIndex>();

            foreach ( var business in businesses )
            {
                var indexableBusiness = BusinessIndex.LoadByModel( business );
                indexableBusinessList.Add( indexableBusiness );

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableBusinessList );
                    indexableBusinessList = new List<BusinessIndex>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableBusinessList );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<PersonIndex>();
            IndexContainer.DeleteDocumentsByType<BusinessIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( PersonIndex );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            var personEntity = new PersonService( new RockContext() ).Get( id );

            if ( personEntity != null )
            {
                if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )
                {
                    var indexItem = PersonIndex.LoadByModel( personEntity );
                    IndexContainer.IndexDocument( indexItem );
                }
                else if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    var indexItem = BusinessIndex.LoadByModel( personEntity );
                    IndexContainer.IndexDocument( indexItem );
                }
            }
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            var personEntity = new PersonService( new RockContext() ).Get( id );

            if ( personEntity != null )
            {
                if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )
                {
                    Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.PersonIndex" );
                    IndexContainer.DeleteDocumentById( indexType, id );
                }
                else if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.PersonIndex" );
                    IndexContainer.DeleteDocumentById( indexType, id );
                }
            }
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            return new ModelFieldFilterConfig() { FilterLabel = string.Empty, FilterField = string.Empty };
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return false;
        }

        #endregion
    }
}
