// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "Registration" )]
    [DataContract]
    public partial class Registration : Model<Registration>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [Required]
        [DataMember]
        public int RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email.
        /// </summary>
        /// <value>
        /// The confirmation email.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [MaxLength( 100 )]
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        [DataMember]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [DataMember]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [DataMember]
        public virtual RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationRegistrant> Registrants
        {
            get { return _registrants ?? ( _registrants = new Collection<RegistrationRegistrant>() ); }
            set { _registrants = value; }
        }
        private ICollection<RegistrationRegistrant> _registrants;

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalCost
        {
            get
            {
                if ( Registrants != null )
                {
                    return Registrants.Sum( r => r.TotalCost );
                }

                return 0.0M;
            }
        }

        /// <summary>
        /// Gets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal DiscountedCost
        {
            get
            {
                var discountedCost = 0.0m;
                if ( Registrants != null )
                {
                    foreach ( var registrant in Registrants )
                    {
                        discountedCost += registrant.DiscountedCost( DiscountPercentage, DiscountAmount );
                    }
                }
                return discountedCost;
            }
        }
        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <value>
        /// The total paid.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalPaid
        {
            get
            {
                return this.GetTotalPaid();
            }
        }

        /// <summary>
        /// Gets the balance due.
        /// </summary>
        /// <value>
        /// The balance due.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal BalanceDue
        {
            get
            {
                return DiscountedCost - TotalPaid;
            }
        }

        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <value>
        /// The payments.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual IQueryable<FinancialTransactionDetail> Payments
        {
            get
            {
                return this.GetPayments();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a summary of the registration
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public string GetSummary( RegistrationInstance registrationInstance = null )
        {
            var result = new StringBuilder();
            result.Append("Event registration payment");

            var instance = registrationInstance ?? RegistrationInstance;
            if ( instance != null )
            {
                result.AppendFormat( " for {0} [ID:{1}]", instance.Name, instance.Id );
                if ( instance.RegistrationTemplate != null )
                {
                    result.AppendFormat( " (Template: {0} [ID:{1}])", instance.RegistrationTemplate.Name, instance.RegistrationTemplate.Id );
                }
            }

            string registrationPerson = PersonAlias != null && PersonAlias.Person != null ?
                PersonAlias.Person.FullName : 
                string.Format( "{0} {1}", FirstName, LastName );
            result.AppendFormat( @".
Registration By: {0} Total Cost/Fees:{1:C2}
", registrationPerson, DiscountedCost );

            var registrantPersons = new List<string>();
            if ( Registrants != null )
            {
                foreach( var registrant in Registrants.Where( r => r.PersonAlias != null && r.PersonAlias.Person != null ) )
                {
                    registrantPersons.Add( string.Format( "{0} Cost/Fees:{1:C2}", 
                        registrant.PersonAlias.Person.FullName,
                        registrant.DiscountedCost( DiscountPercentage, DiscountAmount ) ) );
                }
            }
            result.AppendFormat( "Registrants: {0}", registrantPersons.AsDelimited( ", " ) );

            return result.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PersonAlias != null && PersonAlias.Person != null )
            {
                return PersonAlias.Person.FullName;
            }

            string personName = string.Format( "{0} {1}", FirstName, LastName );
            return string.IsNullOrWhiteSpace( personName ) ? "Registration" : personName.Trim();

        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationConfiguration : EntityTypeConfiguration<Registration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationConfiguration"/> class.
        /// </summary>
        public RegistrationConfiguration()
        {
            this.HasRequired( r => r.RegistrationInstance ).WithMany( t => t.Registrations ).HasForeignKey( r => r.RegistrationInstanceId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Group ).WithMany().HasForeignKey( r => r.GroupId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Extension method class for Registration
    /// </summary>
    public static partial class RegistrationExtensionMethods
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<FinancialTransactionDetail> GetPayments( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetPayments( registration != null ? registration.Id : 0 );
        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static decimal GetTotalPaid( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetTotalPayments( registration != null ? registration.Id : 0 );
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Registration Helper Class
    /// </summary>
    [Serializable]
    public class RegistrationInfo
    {
        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int? RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets your first name.
        /// </summary>
        /// <value>
        /// Your first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets your last name.
        /// </summary>
        /// <value>
        /// Your last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email.
        /// </summary>
        /// <value>
        /// The confirmation email.
        /// </value>
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the previous payment total.
        /// </summary>
        /// <value>
        /// The previous payment total.
        /// </value>
        public decimal PreviousPaymentTotal { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        /// <value>
        /// The payment amount.
        /// </value>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        public List<RegistrantInfo> Registrants { get; set; }

        /// <summary>
        /// Gets the registrant count.
        /// </summary>
        /// <value>
        /// The registrant count.
        /// </value>
        public int RegistrantCount
        {
            get { return Registrants.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        public RegistrationInfo()
        {
            Registrants = new List<RegistrantInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public RegistrationInfo( Person person )
            : this()
        {
            if ( person != null )
            {
                FirstName = person.NickName;
                LastName = person.LastName;
                ConfirmationEmail = person.Email;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo" /> class.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        public RegistrationInfo( Registration registration, RockContext rockContext )
            : this()
        {
            if ( registration != null )
            {
                RegistrationId = registration.Id;
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    FirstName = registration.PersonAlias.Person.NickName;
                    LastName = registration.PersonAlias.Person.LastName;
                    ConfirmationEmail = registration.ConfirmationEmail;
                }

                DiscountCode = registration.DiscountCode.Trim();
                DiscountPercentage = registration.DiscountPercentage;
                DiscountAmount = registration.DiscountAmount;
                TotalCost = registration.TotalCost;
                DiscountedCost = registration.DiscountedCost;

                foreach ( var registrant in registration.Registrants )
                {
                    Registrants.Add( new RegistrantInfo( registrant, rockContext ) );
                }
            }
        }

        /// <summary>
        /// Gets the options that should be available for additional registrants to specify the family they belong to
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="currentRegistrantIndex">Index of the current registrant.</param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetFamilyOptions( RegistrationTemplate template, int currentRegistrantIndex )
        {
            // Return a dictionary of family group guid, and the formated name (i.e. "Ted & Cindy Decker" )
            var result = new Dictionary<Guid, string>();

            // Get all the registrants prior to the current registrant
            var familyRegistrants = new Dictionary<Guid, List<RegistrantInfo>>();
            for ( int i = 0; i < currentRegistrantIndex; i++ )
            {
                if ( Registrants != null && Registrants.Count > i )
                {
                    var registrant = Registrants[i];
                    familyRegistrants.AddOrIgnore( registrant.FamilyGuid, new List<RegistrantInfo>() );
                    familyRegistrants[registrant.FamilyGuid].Add( registrant );
                }
                else
                {
                    break;
                }
            }

            // Loop through those registrants
            foreach ( var keyVal in familyRegistrants )
            {
                // Find all the people and group them by same last name
                var lastNames = new Dictionary<string, List<string>>();
                foreach ( var registrant in keyVal.Value )
                {
                    string firstName = registrant.GetFirstName( template );
                    string lastName = registrant.GetLastName( template );
                    lastNames.AddOrIgnore( lastName, new List<string>() );
                    lastNames[lastName].Add( firstName );
                }

                // Build a formated output for each unique last name
                var familyNames = new List<string>();
                foreach ( var lastName in lastNames )
                {
                    familyNames.Add( string.Format( "{0} {1}", lastName.Value.AsDelimited( " & " ), lastName.Key ) );
                }

                // Join each of the formated values for each unique last name for the current family
                result.Add( keyVal.Key, familyNames.AsDelimited( " and " ) );
            }

            return result;
        }

    }

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
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

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
        public Dictionary<int, object> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets the fee values.
        /// </summary>
        /// <value>
        /// The fee values.
        /// </value>
        public Dictionary<int, List<FeeInfo>> FeeValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo"/> class.
        /// </summary>
        public RegistrantInfo()
        {
            Guid = Guid.NewGuid();
            PersonAliasGuid = Guid.Empty;
            FamilyGuid = Guid.Empty;
            FieldValues = new Dictionary<int, object>();
            FeeValues = new Dictionary<int, List<FeeInfo>>();
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
                RegistrationId = registrant.RegistrationId;
                Cost = registrant.Cost;

                Person person = null;
                Group family = null;

                if ( registrant.PersonAlias != null )
                {
                    PersonAliasGuid = registrant.PersonAlias.Guid;
                    person = registrant.PersonAlias.Person;

                    if ( person != null )
                    {
                        PersonName = person.FullName;
                        family = person.GetFamilies( rockContext ).FirstOrDefault();
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
                            FieldValues.Add( field.Id, dbValue );
                        }
                    }

                    foreach ( var fee in registrant.Fees )
                    {
                        FeeValues.AddOrIgnore( fee.RegistrationTemplateFeeId, new List<FeeInfo>() );
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
        /// <param name="Field">The field.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public object GetRegistrantValue( RegistrationRegistrant registrant, Person person, Group family,
            RegistrationTemplateFormField Field, RockContext rockContext )
        {
            if ( Field.FieldSource == RegistrationFieldSource.PersonField )
            {
                if ( person != null )
                {
                    DefinedValueCache dvPhone = null;

                    switch ( Field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.FirstName: return person.NickName;
                        case RegistrationPersonFieldType.LastName: return person.LastName;
                        case RegistrationPersonFieldType.Campus:
                            {
                                if ( family != null )
                                {
                                    return family.CampusId;
                                }
                                break;
                            }
                        case RegistrationPersonFieldType.Address:
                            {
                                var location = person.GetHomeLocation( rockContext );
                                if ( location != null )
                                {
                                    return location.Clone();
                                }
                                break;
                            }
                        case RegistrationPersonFieldType.Email: return person.Email;
                        case RegistrationPersonFieldType.Birthdate: return person.BirthDate;
                        case RegistrationPersonFieldType.Gender: return person.Gender;
                        case RegistrationPersonFieldType.MaritalStatus: return person.MaritalStatusValueId;
                        case RegistrationPersonFieldType.MobilePhone:
                            {
                                dvPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                                break;
                            }
                        case RegistrationPersonFieldType.HomePhone:
                            {
                                dvPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                                break;
                            }
                        case RegistrationPersonFieldType.WorkPhone:
                            {
                                dvPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                                break;
                            }
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
                var attribute = AttributeCache.Read( Field.AttributeId ?? 0 );
                if ( attribute != null )
                {
                    switch ( Field.FieldSource )
                    {
                        case RegistrationFieldSource.PersonAttribute:
                            {
                                if ( person != null )
                                {
                                    if ( person.Attributes == null )
                                    {
                                        person.LoadAttributes();
                                    }
                                    return person.GetAttributeValue( attribute.Key );
                                }
                                break;
                            }

                        case RegistrationFieldSource.GroupMemberAttribute:
                            {
                                if ( registrant.GroupMember != null )
                                {
                                    if ( registrant.GroupMember.Attributes == null )
                                    {
                                        registrant.GroupMember.LoadAttributes();
                                    }
                                    return registrant.GroupMember.GetAttributeValue( attribute.Key );
                                }
                                break;
                            }

                        case RegistrationFieldSource.RegistrationAttribute:
                            {
                                if ( registrant.Attributes == null )
                                {
                                    registrant.LoadAttributes();
                                }
                                return registrant.GetAttributeValue( attribute.Key );
                            }
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
            return value != null ? value.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetLastName( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.LastName );
            return value != null ? value.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetEmail( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.Email );
            return value != null ? value.ToString() : string.Empty;
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

                return FieldValues.ContainsKey( fieldId ) ? FieldValues[fieldId] : null;
            }

            return null;
        }
    }

    /// <summary>
    /// Registrant  Fee Helper Class
    /// </summary>
    [Serializable]
    public class FeeInfo
    {
        /// <summary>
        /// Gets or sets the option.
        /// </summary>
        /// <value>
        /// The option.
        /// </value>
        public string Option { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost
        {
            get { return Cost * Quantity; }
        }

        /// <summary>
        /// Gets or sets the previous cost.
        /// </summary>
        /// <value>
        /// The previous cost.
        /// </value>
        public decimal PreviousCost { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        public FeeInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="cost">The cost.</param>
        public FeeInfo( string option, int quantity, decimal cost )
            : this()
        {
            Option = option;
            Quantity = quantity;
            Cost = cost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        /// <param name="fee">The fee.</param>
        public FeeInfo( RegistrationRegistrantFee fee )
            : this()
        {
            Option = fee.Option;
            Quantity = fee.Quantity;
            Cost = fee.Cost;
            PreviousCost = fee.Cost;
        }
    }

    /// <summary>
    /// Helper class for creating summary of cost/fee information to bind to summary grid
    /// </summary>
    public class RegistrationCostSummaryInfo
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public RegistrationCostSummaryType Type { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the minimum payment.
        /// </summary>
        /// <value>
        /// The minimum payment.
        /// </value>
        public decimal MinPayment { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RegistrationCostSummaryType
    {
        /// <summary>
        /// a cost
        /// 
        /// </summary>
        Cost,

        /// <summary>
        /// a fee
        /// </summary>
        Fee,

        /// <summary>
        /// The discount
        /// </summary>
        Discount,

        /// <summary>
        /// The total
        /// </summary>
        Total
    }

    #endregion

}
