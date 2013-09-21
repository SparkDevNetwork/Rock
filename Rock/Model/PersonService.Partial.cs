//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.Person"/> entity objects.
    /// </summary>
    public partial class PersonService 
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities.
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities.</returns>
        public override IQueryable<Person> Queryable()
        {
            return Queryable( false );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities. If includeDeceased is <c>false</c>, deceased individuals will be excluded.
        /// </summary>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.Person"/> should be included. If <c>true</c>
        /// deceased individuals will be included, otherwise <c>false</c> and they will be excluded.
        /// </param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities, with deceased individuals either included or excluded based on the provided value.</returns>
        public IQueryable<Person> Queryable( bool includeDeceased )
        {
            return base.Repository.AsQueryable().Where( p => includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value );
        }

        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.Person"/> entities with eager loading of the properties that are included in the includes parameter.
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a comma delimited list of properties that should support eager loading.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities with properties that support eager loading.</returns>
        public override IQueryable<Person> Queryable( string includes )
        {
            return Queryable( includes, false );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities with eager loading of properties that are included in the includes parameter.
        /// If includeDeceased is <c>false</c>, deceased individuals will be excluded
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> value indicating if deceased <see cref="Rock.Model.Person"/> should be included. If <c>true</c>
        /// deceased individuals will be included, otherwise <c>false</c> and they will be excluded.
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities with properties that support eager loading, with deceased individuals either included or excluded based on the provided value. </returns>
        public IQueryable<Person> Queryable( string includes, bool includeDeceased )
        {
            return base.Repository.AsQueryable( includes ).Where( p => includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value );
        }

        #region Get People

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities by email address.
        /// </summary>
        /// <param name="email">A <see cref="System.String"/> representing the email address to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in the search results, if
        /// <c>true</c> then they will be included, otherwise <c>false</c>. Default value is false.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByEmail( string email, bool includeDeceased = false )
        {
            return Repository.Find( t => 
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value) &&
                ( t.Email == email || ( email == null && t.Email == null ) )
            );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities by martial status <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <param name="maritalStatusId">An <see cref="System.Int32"/> representing the Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByMaritalStatusId( int? maritalStatusId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.MaritalStatusValueId == maritalStatusId || ( maritalStatusId == null && t.MaritalStatusValueId == null ) )
            );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by the the Person's Person Status <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="personStatusId">A <see cref="System.Int32"/> representing the Id of the Person Status <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByPersonStatusId( int? personStatusId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.PersonStatusValueId == personStatusId || ( personStatusId == null && t.PersonStatusValueId == null ) )
            );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by their Record Status <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <param name="recordStatusId">A <see cref="System.Int32"/> representing the Id of the Record Status <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByRecordStatusId( int? recordStatusId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.RecordStatusValueId == recordStatusId || ( recordStatusId == null && t.RecordStatusValueId == null ) )
            );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by the RecordStatusReason <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="recordStatusReasonId">A <see cref="System.Int32"/> representing the Id of the RecordStatusReason <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByRecordStatusReasonId( int? recordStatusReasonId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.RecordStatusReasonValueId == recordStatusReasonId || ( recordStatusReasonId == null && t.RecordStatusReasonValueId == null ) )
            );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by their RecordType <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="recordTypeId">A <see cref="System.Int32"/> representing the Id of the RecordType <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByRecordTypeId( int? recordTypeId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.RecordTypeValueId == recordTypeId || ( recordTypeId == null && t.RecordTypeValueId == null ) )
            );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Person"/> entities by their Suffix <see cref="Rock.Model.DefinedValue"/>
        /// </summary>
        /// <param name="suffixId">An <see cref="System.Int32"/> representing the Id of Suffix <see cref="Rock.Model.DefinedValue"/> to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetBySuffixId( int? suffixId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.SuffixValueId == suffixId || ( suffixId == null && t.SuffixValueId == null ) )
            );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Person"/> entities by their Title <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="titleId">A <see cref="System.Int32"/> representing the Id of the Title <see cref="Rock.Model.DefinedValue"/>.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.
        /// </returns>
        public IEnumerable<Person> GetByTitleId( int? titleId, bool includeDeceased = false )
        {
            return Repository.Find( t => 
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.TitleValueId == titleId || ( titleId == null && t.TitleValueId == null ) ) 
            );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities by the person's full name.
        /// </summary>
        /// <param name="fullName">A <see cref="System.String"/> representing the full name to search by.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.</returns>
        public IQueryable<Person> GetByFullName( string fullName, bool includeDeceased = false )
        {
            var names = fullName.SplitDelimitedValues();

            string firstName = string.Empty;
            string lastName = string.Empty;

            if ( fullName.Contains( ',' ) )
            {
                lastName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                firstName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else if ( fullName.Contains( ' ' ) )
            {
                firstName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                lastName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else
            {
                lastName = fullName.Trim();
            }

            var qry = Queryable( includeDeceased );
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( p => p.LastName.StartsWith( lastName ) );
            }
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( p => p.FirstName.StartsWith( firstName ) );
            }

            return qry;
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Person"/> entities where their phone number partially matches the provided value.
        /// </summary>
        /// <param name="partialPhoneNumber">A <see cref="System.String"/> containing a partial phone number to match.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Person"/> entities that match the search criteria.</returns>
        public IEnumerable<Person> GetByPhonePartial( string partialPhoneNumber, bool includeDeceased = false )
        {
            string numericPhone = partialPhoneNumber.AsNumeric();

            return Repository.Find( p =>
                ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) &&
                p.PhoneNumbers.Any( n => n.Number.Contains( numericPhone ) )
            );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Person"/> entities containing the family members of the provided person.
        /// </summary>
        /// <param name="person">A <see cref="Rock.Model.Person"/> representing the person to get the family members for.</param>
        /// <param name="includeDeceased">A <see cref="System.Boolean"/> flag indicating if deceased individuals should be included in search results, if <c>true</c> then they will be 
        /// included, otherwise <c>false</c>.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Person"/> entities containing the family members of the provided person.</returns>
        public IQueryable<GroupMember> GetFamilyMembers( Person person, bool includeSelf = false )
        {
            Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            return new GroupMemberService().Queryable()
                .Where( m => m.PersonId == person.Id && m.Group.GroupType.Guid == familyGuid)
                .SelectMany( m => m.Group.Members)
                .Where( fm => includeSelf || fm.PersonId != person.Id)
                .Distinct();
        }

        #endregion

        #region Get Person



        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> by their PersonId
        /// </summary>
        /// <param name="id">The <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search for.</param>
        /// <param name="followMerges">A <see cref="System.Boolean"/> flag indicating that the provided PersonId should be checked against the <see cref="Rock.Model.PersonMerged"/> list.
        /// When <c>true</c> the <see cref="Rock.Model.PersonsMerged"/> log will be checked for the PersonId, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> associated with the provided Id, otherwise null.</returns>
        public Person Get( int id, bool followMerges )
        {
            if ( followMerges )
            {
                id = new PersonMergedService().Current( id );
            }
            return Get( id );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> by their Guid.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the <see cref="Rock.Model.Person">Person's</see> Guid identifier.</param>
        /// <param name="followMerges">A <see cref="System.Boolean"/> flag indicating that the provided Guid should be checked against the <see cref="Rock.Model.PersonMerged"/> list.
        /// When <c>true</c> the <see cref="Rock.Model.PersonsMerged"/> log will be checked for the Guid, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> associated with the provided Guid, otherwise null.</returns>
        public Person Get( Guid guid, bool followMerges )
        {
            if ( followMerges )
            {
                guid = new PersonMergedService().Current( guid );
            }
            return Get( guid );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> by their encrypted key value.
        /// </summary>
        /// <param name="encryptedKey">A <see cref="System.String"/> containing an encrypted key value.</param>
        /// <param name="followMergeTrail">A <see cref="System.Boolean"/> flag indicating that the provided Guid should be checked against the <see cref="Rock.Model.PersonMerged"/> list.
        /// When <c>true</c> the <see cref="Rock.Model.PersonsMerged"/> log will be checked for the Guid, otherwise <c>false</c>.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person"/> associated with the provided Key, otherwise null.
        /// </returns>
        public Person GetByEncryptedKey( string encryptedKey, bool followMergeTrail )
        {
            if ( followMergeTrail )
                encryptedKey = new PersonMergedService().Current( encryptedKey );

            return GetByEncryptedKey( encryptedKey );
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> entity of the provided Person's spouse.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> entity of the Person to retrieve the spouse of.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.</returns>
        public Person GetSpouse( Person person )
        {
            // Spouse is determined if all these conditions are met
            // 1) Adult in the same family as Person (GroupType = Family, GroupRole = Adult, and in same Group)
            // 2) Opposite Gender as Person
            // 3) Both Persons are Married
            
            Guid adultGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
            Guid marriedGuid = new Guid(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED);
            int marriedDefinedValueId = new DefinedValueService().Queryable().First(a => a.Guid == marriedGuid).Id;

            if ( person.MaritalStatusValueId != marriedDefinedValueId )
            {
                return null;
            }

            return GetFamilyMembers(person)
                .Where( m => m.GroupRole.Guid == adultGuid)
                .Where( m => m.Person.Gender != person.Gender )
                .Where( m => m.Person.MaritalStatusValueId == marriedDefinedValueId)
                .Select( m => m.Person )
                .FirstOrDefault();
        }

        #endregion

        #region User Preferences

        /// <summary>
        /// Saves a <see cref="Rock.Model.Person">Person's</see> user preference setting by key.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> who the preference value belongs to.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key (name) of the preference setting. </param>
        /// <param name="values">A list of <see cref="System.String"/> values representing the value of the preference setting.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> saving the setting.</param>
        public void SaveUserPreference(Person person, string key, List<string> values, int? personId)
        {
            int? PersonEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( Person.USER_VALUE_ENTITY ).Id;

            var attributeService = new Model.AttributeService();
            var attribute = attributeService.Get( PersonEntityTypeId, string.Empty, string.Empty, key );

            if ( attribute == null )
            {
                var fieldTypeService = new Model.FieldTypeService();
                var fieldType = fieldTypeService.GetByGuid( new Guid( Rock.SystemGuid.FieldType.TEXT ) );

                attribute = new Model.Attribute();
                attribute.IsSystem = false;
                attribute.EntityTypeId = PersonEntityTypeId;
                attribute.EntityTypeQualifierColumn = string.Empty;
                attribute.EntityTypeQualifierValue = string.Empty;
                attribute.Key = key;
                attribute.Name = key;
                attribute.DefaultValue = string.Empty;
                attribute.IsMultiValue = false;
                attribute.IsRequired = false;
                attribute.Description = string.Empty;
                attribute.FieldTypeId = fieldType.Id;
                attribute.Order = 0;

                attributeService.Add( attribute, personId );
                attributeService.Save( attribute, personId );
            }

            var attributeValueService = new Model.AttributeValueService();

            // Delete existing values
            var attributeValues = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, person.Id ).ToList();
            foreach ( var attributeValue in attributeValues )
            {
                attributeValueService.Delete( attributeValue, personId );
                attributeValueService.Save( attributeValue, personId );
            }

            // Save new values
            foreach ( var value in values.Where( v => !string.IsNullOrWhiteSpace( v ) ) )
            {
                var attributeValue = new Model.AttributeValue();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.EntityId = person.Id;
                attributeValue.Value = value;
                attributeValueService.Add( attributeValue, personId );
                attributeValueService.Save( attributeValue, personId );
            }
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Person"/> user preference value by preference setting's key.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the preference value for.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key name of the preference setting.</param>
        /// <returns>A list of <see cref="System.String"/> containing the values associated with the user's preference setting.</returns>
        public List<string> GetUserPreference( Person person, string key )
        {
            int? PersonEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( Person.USER_VALUE_ENTITY ).Id;

            var attributeService = new Model.AttributeService();
            var attribute = attributeService.Get( PersonEntityTypeId, string.Empty, string.Empty, key );

            if (attribute != null)
            {
                var attributeValueService = new Model.AttributeValueService();
                var attributeValues = attributeValueService.GetByAttributeIdAndEntityId(attribute.Id, person.Id);
                if (attributeValues != null && attributeValues.Count() > 0)
                    return attributeValues.Select( v => v.Value).ToList();
            }

            return null;
        }

        /// <summary>
        /// Returns all of the user preference settings for a <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve the user preference settings for.</param>
        /// <returns>A dictionary containing all of the <see cref="Rock.Model.Person">Person's</see> user preference settings.</returns>
        public Dictionary<string, List<string>> GetUserPreferences( Person person )
        {
            int? PersonEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( Person.USER_VALUE_ENTITY ).Id;

            var values = new Dictionary<string, List<string>>();

            foreach ( var attributeValue in new Model.AttributeValueService().Queryable()
                .Where( v =>
                    v.Attribute.EntityTypeId == PersonEntityTypeId &&
                    v.Attribute.EntityTypeQualifierColumn == string.Empty &&
                    v.Attribute.EntityTypeQualifierValue == string.Empty &&
                    v.EntityId == person.Id ) )
            {
                if (!values.Keys.Contains(attributeValue.Attribute.Key))
                    values.Add(attributeValue.Attribute.Key, new List<string>());
                values[attributeValue.Attribute.Key].Add(attributeValue.Value);
            }

            return values;
        }

        #endregion
    }
}
