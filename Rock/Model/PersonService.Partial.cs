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
    /// Person POCO Service class
    /// </summary>
    public partial class PersonService 
    {
        /// <summary>
        /// Gets a queryable list of people
        /// </summary>
        /// <returns></returns>
        public override IQueryable<Person> Queryable()
        {
            return Queryable( false );
        }

        /// <summary>
        /// Gets a queryable list of people
        /// </summary>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<Person> Queryable( bool includeDeceased )
        {
            return base.Repository.AsQueryable().Where( p => includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value );
        }

        /// <summary>
        /// Gets a list of all people with eager loading of properties specfied in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        public override IQueryable<Person> Queryable( string includes )
        {
            return Queryable( includes, false );
        }

        /// <summary>
        /// Gets a list of all people with eager loading of properties specfied in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<Person> Queryable( string includes, bool includeDeceased )
        {
            return base.Repository.AsQueryable( includes ).Where( p => includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value );
        }

        #region Get People

        /// <summary>
        /// Gets People by Email
        /// </summary>
        /// <param name="email">Email.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByEmail( string email, bool includeDeceased = false )
        {
            return Repository.Find( t => 
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value) &&
                ( t.Email == email || ( email == null && t.Email == null ) )
            );
        }

        /// <summary>
        /// Gets People by Marital Status Id
        /// </summary>
        /// <param name="maritalStatusId">Marital Status Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByMaritalStatusId( int? maritalStatusId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.MaritalStatusValueId == maritalStatusId || ( maritalStatusId == null && t.MaritalStatusValueId == null ) )
            );
        }

        /// <summary>
        /// Gets People by Person Status Id
        /// </summary>
        /// <param name="personStatusId">Person Status Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByPersonStatusId( int? personStatusId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.PersonStatusValueId == personStatusId || ( personStatusId == null && t.PersonStatusValueId == null ) )
            );
        }

        /// <summary>
        /// Gets People by Record Status Id
        /// </summary>
        /// <param name="recordStatusId">Record Status Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByRecordStatusId( int? recordStatusId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.RecordStatusValueId == recordStatusId || ( recordStatusId == null && t.RecordStatusValueId == null ) )
            );
        }

        /// <summary>
        /// Gets People by Record Status Reason Id
        /// </summary>
        /// <param name="recordStatusReasonId">Record Status Reason Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByRecordStatusReasonId( int? recordStatusReasonId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.RecordStatusReasonValueId == recordStatusReasonId || ( recordStatusReasonId == null && t.RecordStatusReasonValueId == null ) )
            );
        }

        /// <summary>
        /// Gets People by Record Type Id
        /// </summary>
        /// <param name="recordTypeId">Record Type Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByRecordTypeId( int? recordTypeId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.RecordTypeValueId == recordTypeId || ( recordTypeId == null && t.RecordTypeValueId == null ) )
            );
        }

        /// <summary>
        /// Gets People by Suffix Id
        /// </summary>
        /// <param name="suffixId">Suffix Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetBySuffixId( int? suffixId, bool includeDeceased = false )
        {
            return Repository.Find( t =>
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.SuffixValueId == suffixId || ( suffixId == null && t.SuffixValueId == null ) )
            );
        }

        /// <summary>
        /// Gets People by Title Id
        /// </summary>
        /// <param name="titleId">Title Id.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns>
        /// An enumerable list of Person objects.
        /// </returns>
        public IEnumerable<Person> GetByTitleId( int? titleId, bool includeDeceased = false )
        {
            return Repository.Find( t => 
                ( includeDeceased || !t.IsDeceased.HasValue || !t.IsDeceased.Value ) &&
                ( t.TitleValueId == titleId || ( titleId == null && t.TitleValueId == null ) ) 
            );
        }

        /// <summary>
        /// Gets a list of people with a matching full name
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
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
        /// Gets a list of people with a phone number that contains the specified partial number.
        /// </summary>
        /// <param name="partialPhoneNumber">The partial phone number.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IQueryable<Person> GetByPhonePartial( string partialPhoneNumber, bool includeDeceased = false )
        {
            string numericPhone = partialPhoneNumber.AsNumeric();

            return Repository.AsQueryable().Where( p =>
                ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) &&
                p.PhoneNumbers.Any( n => n.Number.Contains( numericPhone ) )
            );
        }

        /// <summary>
        /// Gets a list of people with a name that contains the specified partial name.
        /// </summary>
        /// <param name="partialName">The partial name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IEnumerable<Person> GetByNamePartial( string partialName, bool includeDeceased = false )
        {
            return Repository.Find( p =>
                ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) &&
                p.FullName.ToLower().Contains( partialName )
            );
        }

        /// <summary>
        /// Gets a list of people with a matching first name.
        /// </summary>
        /// <param name="firstName">The last name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IEnumerable<Person> GetByFirstName( string firstName, bool includeDeceased = false )
        {
            return Repository.Find( p =>
                ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) &&
                p.FirstName.ToLower().StartsWith( firstName )
            );
        }

        /// <summary>
        /// Gets a list of people with a matching last name.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        public IEnumerable<Person> GetByLastName( string lastName, bool includeDeceased = false )
        {
            return Repository.Find( p =>
                ( includeDeceased || !p.IsDeceased.HasValue || !p.IsDeceased.Value ) &&
                p.LastName.ToLower().StartsWith( lastName )
            );
        }

        /// <summary>
        /// Gets the family members.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
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
        /// Gets Person by Id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="followMerges">if set to <c>true</c> follow the merge trail.</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Person Get( int id, bool followMerges )
        {
            if ( followMerges )
            {
                id = new PersonMergedService().Current( id );
            }
            return Get( id );
        }

        /// <summary>
        /// Gets the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="followMerges">if set to <c>true</c> follow the merge trail.</param>
        /// <returns></returns>
        public Person Get( Guid guid, bool followMerges )
        {
            if ( followMerges )
            {
                guid = new PersonMergedService().Current( guid );
            }
            return Get( guid );
        }

        /// <summary>
        /// Gets Person by Guid
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <param name="followMergeTrail">if set to <c>true</c> follow the merge trail</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Person GetByEncryptedKey( string encryptedKey, bool followMergeTrail )
        {
            if ( followMergeTrail )
                encryptedKey = new PersonMergedService().Current( encryptedKey );

            return GetByEncryptedKey( encryptedKey );
        }

        /// <summary>
        /// Gets the spouse.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public Person GetSpouse( Person person )
        {
            Guid adultGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
            return GetFamilyMembers(person)
                .Where( m => m.GroupRole.Guid == adultGuid && m.Person.Gender != person.Gender )
                .Select( m => m.Person )
                .FirstOrDefault();
        }

        #endregion

        #region User Preferences

        /// <summary>
        /// Saves preference for a given person, and key
        /// </summary>
        /// <param name="person"></param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="personId"></param>
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
        /// Gets preference for a given person, and key
        /// </summary>
        /// <param name="person"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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
        /// Gets all preferences for a given person
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
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
