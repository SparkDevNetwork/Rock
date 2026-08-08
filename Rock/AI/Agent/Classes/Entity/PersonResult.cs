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
using System.Linq.Expressions;
using System.Text.Json.Serialization;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// A common POCO for storing information about a person. Apply only the properties that are needed for the specific use case.
    /// Null properties will not be serialized.
    /// </summary>
    public class PersonResult : EntityResultBase
    {
        #region Ignored Properties
        // These properties exist to help with internal logic but they should not be serialized to JSON.

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        [JsonIgnore]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the record type value identifier.
        /// </summary>
        [JsonIgnore]
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Determines if the avatar URL should be included in the return.
        /// </summary>
        [JsonIgnore]
        public bool IncludeAvatarUrl { get; set; } = true;

        /// <summary>
        /// Determines if the internal profile should be included in the return.
        /// </summary>
        [JsonIgnore]
        public bool IncludeProfileLink { get; set; }

        #endregion

        #region Common Properties

        /// <summary>
        /// Gets or sets the person's first/given name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's nickname.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the person's last/family name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's name suffix.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the person's e-mail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets the URL for the person's avatar image.
        /// </summary>
        public string AvatarUrl
        {
            get
            {
                if ( !IncludeAvatarUrl )
                {
                    return null;
                }

                string initials = null;

                if ( FirstName.IsNotNullOrWhiteSpace() )
                {
                    initials = FirstName.Left( 1 ) + LastName.Left( 1 );
                }
                else if ( NickName.IsNotNullOrWhiteSpace() )
                {
                    initials = NickName.Left( 1 ) + LastName.Left( 1 );
                }

                var url = Person.GetPersonPhotoUrl(
                    initials,
                    PhotoId,
                    Age,
                    Gender,
                    RecordTypeValueId,
                    AgeClassification );

                return RockApp.Current.ResolveRockUrl( url );
            }
        }

        /// <summary>
        /// The URL to the person's internal profile.
        /// </summary>
        public string InternalProfileUrl
        {
            get
            {
                if ( !IncludeProfileLink )
                {
                    return null;
                }

                return $"{GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ).EnsureTrailingForwardslash()}person/{IdKey}";
            }
        }

        /// <summary>
        /// Gets or sets the age classification.
        /// </summary>
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
        public AgeClassification AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the birth month (1-12).
        /// </summary>
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the birth day of month (1-31).
        /// </summary>
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the birth year.
        /// </summary>
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the anniversary date.
        /// </summary>
        public DateTime? AnniversaryDate { get; set; }

        #endregion

        #region Constructor Expressions

        /// <summary>
        /// The expression for constructing a <see cref="PersonResult"/> with
        /// only the name of the individual.
        /// </summary>
        private static readonly Expression<Func<Person, PersonResult>> _nameOnlyExpression = person => person != null
            ? new PersonResult
            {
                Id = person.Id,
                LastName = person.LastName,
                NickName = person.NickName,
                IncludeAvatarUrl = false,
            }
            : null;

        /// <summary>
        /// The compiled function representing <see cref="_nameOnlyExpression"/>.
        /// </summary>
        private static readonly Lazy<Func<Person, PersonResult>> _nameOnlyFunc = new Lazy<Func<Person, PersonResult>>( () => _nameOnlyExpression.Compile() );

        /// <summary>
        /// The expression for constructing a <see cref="PersonResult"/> with
        /// only the name of the individual.
        /// </summary>
        private static readonly Expression<Func<PersonAlias, PersonResult>> _nameOnlyPersonAliasExpression = personAlias => personAlias != null
            ? new PersonResult
            {
                Id = personAlias.Person.Id,
                LastName = personAlias.Person.LastName,
                NickName = personAlias.Person.NickName,
                IncludeAvatarUrl = false,
            }
            : null;

        /// <summary>
        /// The compiled function representing <see cref="_nameOnlyPersonAliasExpression"/>.
        /// </summary>
        private static readonly Lazy<Func<PersonAlias, PersonResult>> _nameOnlyPersonAliasFunc = new Lazy<Func<PersonAlias, PersonResult>>( () => _nameOnlyPersonAliasExpression.Compile() );

        /// <summary>
        /// The expression for constructing a <see cref="PersonResult"/> with
        /// basic information about the individual.
        /// </summary>
        private static readonly Expression<Func<Person, PersonResult>> _basicExpression = person => person != null
            ? new PersonResult
            {
                Id = person.Id,
                LastName = person.LastName,
                NickName = person.NickName,
                FirstName = person.FirstName,
                PhotoId = person.PhotoId,
                Age = person.Age,
                AgeClassification = person.AgeClassification,
                Gender = person.Gender,
                RecordTypeValueId = person.RecordTypeValueId,
            }
            : null;

        /// <summary>
        /// The compiled function representing <see cref="_basicExpression"/>.
        /// </summary>
        private static readonly Lazy<Func<Person, PersonResult>> _basicFunc = new Lazy<Func<Person, PersonResult>>( () => _basicExpression.Compile() );

        /// <summary>
        /// The expression for constructing a <see cref="PersonResult"/> with
        /// basic information about the individual.
        /// </summary>
        private static readonly Expression<Func<PersonAlias, PersonResult>> _basicPersonAliasExpression = personAlias => personAlias != null
            ? new PersonResult
            {
                Id = personAlias.Person.Id,
                LastName = personAlias.Person.LastName,
                NickName = personAlias.Person.NickName,
                FirstName = personAlias.Person.FirstName,
                PhotoId = personAlias.Person.PhotoId,
                Age = personAlias.Person.Age,
                AgeClassification = personAlias.Person.AgeClassification,
                Gender = personAlias.Person.Gender,
                RecordTypeValueId = personAlias.Person.RecordTypeValueId,
            }
            : null;

        /// <summary>
        /// The compiled function representing <see cref="_basicPersonAliasExpression"/>.
        /// </summary>
        private static readonly Lazy<Func<PersonAlias, PersonResult>> _basicPersonAliasFunc = new Lazy<Func<PersonAlias, PersonResult>>( () => _basicPersonAliasExpression.Compile() );

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with only information about
        /// the individual's name.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> object to construct the result from.</param>
        /// <returns>An instance of <see cref="PersonResult"/> or <c>null</c> if <paramref name="person"/> is <c>null</c>.</returns>
        [Expandable( nameof( NameOnlyExpression ) )]
        public static PersonResult NameOnly( Person person )
        {
            return _nameOnlyFunc.Value( person );
        }

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with only information about
        /// the individual's name.
        /// </summary>
        /// <returns>An expression that can be used to project a <see cref="Person"/> to a <see cref="PersonResult"/>.</returns>
        private static Expression<Func<Person, PersonResult>> NameOnlyExpression()
        {
            return _nameOnlyExpression;
        }

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with only information about
        /// the individual's name.
        /// </summary>
        /// <param name="personAlias">The <see cref="PersonAlias"/> object to construct the result from.</param>
        /// <returns>An instance of <see cref="PersonResult"/> or <c>null</c> if <paramref name="personAlias"/> is <c>null</c>.</returns>
        [Expandable( nameof( NameOnlyPersonAliasExpression ) )]
        public static PersonResult NameOnly( PersonAlias personAlias )
        {
            return _nameOnlyPersonAliasFunc.Value( personAlias );
        }

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with only information about
        /// the individual's name.
        /// </summary>
        /// <returns>An expression that can be used to project a <see cref="PersonAlias"/> to a <see cref="PersonResult"/>.</returns>
        private static Expression<Func<PersonAlias, PersonResult>> NameOnlyPersonAliasExpression()
        {
            return _nameOnlyPersonAliasExpression;
        }

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with basic information
        /// about the individual, such as name, age, gender and photo.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> object to construct the result from.</param>
        /// <returns>An instance of <see cref="PersonResult"/> or <c>null</c> if <paramref name="person"/> is <c>null</c>.</returns>
        [Expandable( nameof( BasicExpression ) )]
        public static PersonResult Basic( Person person )
        {
            return _basicFunc.Value.Invoke( person );
        }


        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with basic information
        /// about the individual, such as name, age, gender and photo.
        /// </summary>
        /// <returns>An expression that can be used to project a <see cref="Person"/> to a <see cref="PersonResult"/>.</returns>
        private static Expression<Func<Person, PersonResult>> BasicExpression()
        {
            return _basicExpression;
        }

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with basic information
        /// about the individual, such as name, age, gender and photo.
        /// </summary>
        /// <param name="personAlias">The <see cref="PersonAlias"/> object to construct the result from.</param>
        /// <returns>An instance of <see cref="PersonResult"/> or <c>null</c> if <paramref name="personAlias"/> is <c>null</c>.</returns>
        [Expandable( nameof( BasicPersonAliasExpression ) )]
        public static PersonResult Basic( PersonAlias personAlias )
        {
            return _basicPersonAliasFunc.Value( personAlias );
        }

        /// <summary>
        /// Constructs a <see cref="PersonResult"/> with basic information
        /// about the individual, such as name, age, gender and photo.
        /// </summary>
        /// <returns>An expression that can be used to project a <see cref="PersonAlias"/> to a <see cref="PersonResult"/>.</returns>
        private static Expression<Func<PersonAlias, PersonResult>> BasicPersonAliasExpression()
        {
            return _basicPersonAliasExpression;
        }

        #endregion
    }
}
