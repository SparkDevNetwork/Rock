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

using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Registrant Helper Class
    /// </summary>
    [Serializable]
    public class RegistrantInfo
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the person alias unique identifier.
        /// </summary>
        /// <value>
        /// The person alias unique identifier.
        /// </value>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [on wait list].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on wait list]; otherwise, <c>false</c>.
        /// </value>
        public bool OnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [discount applies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [discount applies]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountApplies
        {
            get { return _discountApplies; }
            set { _discountApplies = value; }
        }

        private bool _discountApplies = true;

        /// <summary>
        /// Gets the cost with fees.
        /// </summary>
        /// <value>
        /// The cost with fees.
        /// </value>
        public virtual decimal TotalCost
        {
            get
            {
                if ( OnWaitList )
                {
                    return 0.0M;
                }

                var cost = Cost;
                if ( FeeValues != null )
                {
                    cost += FeeValues
                        .Where( f => f.Value != null )
                        .SelectMany( f => f.Value )
                        .Sum( f => f.TotalCost );
                }

                return cost;
            }
        }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public Dictionary<int, FieldValueObject> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of FeeValues (key is RegistrationTemplateFeeId)
        /// </summary>
        /// <value>
        /// The fee values.
        /// </value>
        public Dictionary<int, List<FeeInfo>> FeeValues { get; set; }

        /// <summary>
        /// Gets or sets the signature document Id.
        /// </summary>
        /// <value>
        /// The signature document Id.
        /// </value>
        public int? SignatureDocumentId { get; set; }

        /// <summary>
        /// Gets or sets the signature document key.
        /// </summary>
        /// <value>
        /// The signature document key.
        /// </value>
        public string SignatureDocumentKey { get; set; }

        /// <summary>
        /// Gets or sets the signature document last sent.
        /// </summary>
        /// <value>
        /// The signature document last sent.
        /// </value>
        public DateTime? SignatureDocumentLastSent { get; set; }

        /// <summary>
        /// Gets or sets the signature document signed by.
        /// </summary>
        /// <value>
        /// The signature document signed by.
        /// </value>
        public string SignatureDocumentSignedName { get; set; }

        /// <summary>
        /// Gets or sets the signature document signed at.
        /// </summary>
        /// <value>
        /// The signature document signed at.
        /// </value>
        public DateTime? SignatureDocumentSignedDateTime { get; set; }

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <param name="discountAmount">The discount amount.</param>
        /// <returns></returns>
        public virtual decimal DiscountedCost( decimal discountPercent, decimal discountAmount )
        {
            if ( OnWaitList )
            {
                return 0.0M;
            }

            var discountedCost = Cost - ( DiscountApplies ? ( Cost * discountPercent ) : 0.0M );
            discountedCost = discountedCost - ( DiscountApplies ? discountAmount : 0.0M );

            return discountedCost > 0.0m ? discountedCost : 0.0m;
        }

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <param name="discountAmount">The discount amount.</param>
        /// <returns></returns>
        public virtual decimal DiscountedTotalCost( decimal discountPercent, decimal discountAmount )
        {
            if ( OnWaitList )
            {
                return 0.0M;
            }

            var discountedCost = Cost - ( DiscountApplies ? ( Cost * discountPercent ) : 0.0M );
            if ( FeeValues != null )
            {
                foreach ( var fee in FeeValues.SelectMany( f => f.Value ) )
                {
                    discountedCost += DiscountApplies ? fee.DiscountedCost( discountPercent ) : fee.TotalCost;
                }
            }

            discountedCost -= DiscountApplies ? discountAmount : 0.0M;

            return discountedCost > 0.0m ? discountedCost : 0.0m;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo"/> class.
        /// </summary>
        public RegistrantInfo()
        {
            Guid = Guid.NewGuid();
            PersonAliasGuid = Guid.Empty;
            PersonId = null;
            GroupMemberId = null;
            GroupName = string.Empty;
            FamilyGuid = Guid.Empty;
            FieldValues = new Dictionary<int, FieldValueObject>();
            FeeValues = new Dictionary<int, List<FeeInfo>>();
            OnWaitList = false;
            DiscountApplies = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo" /> class.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="person">The person.</param>
        public RegistrantInfo( RegistrationInstance registrationInstance, Person person )
            : this()
        {
            if ( person != null )
            {
                PersonId = person.Id;

                using ( var rockContext = new RockContext() )
                {
                    PersonName = person.FullName;
                    var family = person.GetFamily( rockContext );

                    if ( registrationInstance != null &&
                        registrationInstance.RegistrationTemplate != null )
                    {
                        var templateFields = registrationInstance.RegistrationTemplate.Forms
                            .SelectMany( f => f.Fields )
                            .Where( f => !f.IsInternal && f.ShowCurrentValue );

                        foreach ( var field in templateFields )
                        {
                            object dbValue = GetRegistrantValue( null, person, family, field, rockContext );
                            if ( dbValue != null )
                            {
                                FieldValues.Add( field.Id, new FieldValueObject( field, dbValue ) );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo" /> class.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="rockContext">The rock context.</param>
        public RegistrantInfo( RegistrationRegistrant registrant, RockContext rockContext )
            : this()
        {
            if ( registrant != null )
            {
                Id = registrant.Id;
                Guid = registrant.Guid;
                GroupMemberId = registrant.GroupMemberId;
                GroupName = registrant.GroupMember != null && registrant.GroupMember.Group != null ?
                    registrant.GroupMember.Group.Name : string.Empty;
                RegistrationId = registrant.RegistrationId;
                Cost = registrant.Cost;
                DiscountApplies = registrant.DiscountApplies;
                OnWaitList = registrant.OnWaitList;

                Person person = null;
                Group family = null;

                if ( registrant.PersonAlias != null )
                {
                    PersonId = registrant.PersonAlias.PersonId;
                    PersonAliasGuid = registrant.PersonAlias.Guid;
                    person = registrant.PersonAlias.Person;

                    if ( person != null )
                    {
                        PersonName = person.FullName;
                        family = person.GetFamily( rockContext );
                        if ( family != null )
                        {
                            FamilyGuid = family.Guid;
                        }
                    }
                }

                if ( registrant.Registration != null &&
                    registrant.Registration.RegistrationInstance != null &&
                    registrant.Registration.RegistrationInstance.RegistrationTemplate != null )
                {
                    var templateFields = registrant.Registration.RegistrationInstance.RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields );
                    foreach ( var field in templateFields )
                    {
                        object dbValue = GetRegistrantValue( registrant, person, family, field, rockContext );
                        if ( dbValue != null )
                        {
                            FieldValues.Add( field.Id, new FieldValueObject( field, dbValue ) );
                        }
                    }

                    foreach ( var fee in registrant.Fees )
                    {
                        FeeValues.TryAdd( fee.RegistrationTemplateFeeId, new List<FeeInfo>() );
                        FeeValues[fee.RegistrationTemplateFeeId].Add( new FeeInfo( fee ) );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the existing value for a specific field for the given registrant.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="person">The person.</param>
        /// <param name="family">The family.</param>
        /// <param name="field">The field.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public object GetRegistrantValue( RegistrationRegistrant registrant, Person person, Group family, RegistrationTemplateFormField field, RockContext rockContext )
        {
            if ( field.FieldSource == RegistrationFieldSource.PersonField )
            {
                if ( person != null )
                {
                    DefinedValueCache dvPhone = null;

                    switch ( field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.FirstName:
                            return person.NickName;

                        case RegistrationPersonFieldType.MiddleName:
                            return person.MiddleName;

                        case RegistrationPersonFieldType.LastName:
                            return person.LastName;

                        case RegistrationPersonFieldType.Campus:
                            if ( family != null )
                            {
                                return family.CampusId;
                            }

                            break;

                        case RegistrationPersonFieldType.Address:
                            var location = person.GetHomeLocation( rockContext );
                            if ( location != null )
                            {
                                return location.Clone();
                            }

                            break;

                        case RegistrationPersonFieldType.Email:
                            return person.Email;

                        case RegistrationPersonFieldType.Birthdate:
                            return person.BirthDate;

                        case RegistrationPersonFieldType.Grade:
                            return person.GraduationYear;

                        case RegistrationPersonFieldType.Gender:
                            return person.Gender;

                        case RegistrationPersonFieldType.MaritalStatus:
                            return person.MaritalStatusValueId;

                        case RegistrationPersonFieldType.AnniversaryDate:
                            return person.AnniversaryDate;

                        case RegistrationPersonFieldType.MobilePhone:
                            dvPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                            break;

                        case RegistrationPersonFieldType.HomePhone:
                            dvPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                            break;

                        case RegistrationPersonFieldType.WorkPhone:
                            dvPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                            break;

                        case RegistrationPersonFieldType.ConnectionStatus:
                            return person.ConnectionStatusValueId;
                    }

                    if ( dvPhone != null )
                    {
                        var phoneNumber = new PersonService( rockContext ).GetPhoneNumber( person, dvPhone );
                        if ( phoneNumber != null )
                        {
                            return phoneNumber.Clone();
                        }
                    }
                }
            }
            else
            {
                var attribute = AttributeCache.Get( field.AttributeId ?? 0 );
                if ( attribute != null )
                {
                    switch ( field.FieldSource )
                    {
                        case RegistrationFieldSource.PersonAttribute:
                            if ( person != null )
                            {
                                if ( person.Attributes == null )
                                {
                                    person.LoadAttributes();
                                }

                                return person.GetAttributeValue( attribute.Key );
                            }

                            break;

                        case RegistrationFieldSource.GroupMemberAttribute:
                            if ( registrant != null && registrant.GroupMember != null )
                            {
                                if ( registrant.GroupMember.Attributes == null )
                                {
                                    registrant.GroupMember.LoadAttributes();
                                }

                                return registrant.GroupMember.GetAttributeValue( attribute.Key );
                            }

                            break;

                        case RegistrationFieldSource.RegistrantAttribute:
                            if ( registrant != null )
                            {
                                if ( registrant.Attributes == null )
                                {
                                    registrant.LoadAttributes();
                                }

                                return registrant.GetAttributeValue( attribute.Key );
                            }

                            break;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetFirstName( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.FirstName );
            if ( value == null )
            {
                // if FirstName isn't prompted for in a registration form, and using an existing Person, get the person's FirstName/NickName from the database
                if ( this.PersonId.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( this.PersonId.Value, s => s.NickName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetLastName( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.LastName );
            if ( value == null )
            {
                // if LastName isn't prompted for in a registration form, and using an existing Person, get the person's lastname from the database
                if ( this.PersonId.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( this.PersonId.Value, s => s.LastName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetEmail( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.Email );
            if ( value == null )
            {
                // if Email isn't prompted for in a registration form, and using an existing Person, get the person's email from the database
                if ( this.PersonId.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( this.PersonId.Value, s => s.Email ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a person field value.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="personFieldType">Type of the person field.</param>
        /// <returns></returns>
        public object GetPersonFieldValue( RegistrationTemplate template, RegistrationPersonFieldType personFieldType )
        {
            if ( template != null && template.Forms != null )
            {
                var fieldId = template.Forms
                    .SelectMany( t => t.Fields
                        .Where( f =>
                            f.FieldSource == RegistrationFieldSource.PersonField &&
                            f.PersonFieldType == personFieldType )
                        .Select( f => f.Id ) )
                    .FirstOrDefault();

                return FieldValues.ContainsKey( fieldId ) ? FieldValues[fieldId].FieldValue : null;
            }

            return null;
        }
    }
}
