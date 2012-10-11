//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Crm
{
    /// <summary>
    /// Person POCO Service class
    /// </summary>
    public partial class PersonService : Service<Person, PersonDto>
    {
        #region Get People

        /// <summary>
        /// Gets People by Email
        /// </summary>
        /// <param name="email">Email.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByEmail( string email )
        {
            return Repository.Find( t => ( t.Email == email || ( email == null && t.Email == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Marital Status Id
        /// </summary>
        /// <param name="maritalStatusId">Marital Status Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByMaritalStatusId( int? maritalStatusId )
        {
            return Repository.Find( t => ( t.MaritalStatusId == maritalStatusId || ( maritalStatusId == null && t.MaritalStatusId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Person Status Id
        /// </summary>
        /// <param name="personStatusId">Person Status Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByPersonStatusId( int? personStatusId )
        {
            return Repository.Find( t => ( t.PersonStatusId == personStatusId || ( personStatusId == null && t.PersonStatusId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Record Status Id
        /// </summary>
        /// <param name="recordStatusId">Record Status Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByRecordStatusId( int? recordStatusId )
        {
            return Repository.Find( t => ( t.RecordStatusId == recordStatusId || ( recordStatusId == null && t.RecordStatusId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Record Status Reason Id
        /// </summary>
        /// <param name="recordStatusReasonId">Record Status Reason Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByRecordStatusReasonId( int? recordStatusReasonId )
        {
            return Repository.Find( t => ( t.RecordStatusReasonId == recordStatusReasonId || ( recordStatusReasonId == null && t.RecordStatusReasonId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Record Type Id
        /// </summary>
        /// <param name="recordTypeId">Record Type Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByRecordTypeId( int? recordTypeId )
        {
            return Repository.Find( t => ( t.RecordTypeId == recordTypeId || ( recordTypeId == null && t.RecordTypeId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Suffix Id
        /// </summary>
        /// <param name="suffixId">Suffix Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetBySuffixId( int? suffixId )
        {
            return Repository.Find( t => ( t.SuffixId == suffixId || ( suffixId == null && t.SuffixId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Title Id
        /// </summary>
        /// <param name="titleId">Title Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByTitleId( int? titleId )
        {
            return Repository.Find( t => ( t.TitleId == titleId || ( titleId == null && t.TitleId == null ) ) );
        }

        /// <summary>
        /// Gets a list of people with a matching full name
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public IQueryable<Person> GetByFullName( string fullName )
        {
            string firstName = string.Empty;
            string lastName = string.Empty;

            if ( fullName.Contains( ' ' ) )
            {
                firstName = fullName.Substring( 0, fullName.LastIndexOf( ' ' ) );
                lastName = fullName.Substring( fullName.LastIndexOf( ' ' ) + 1 );
            }
            else
                lastName = fullName;

            return Queryable().
                    Where( p => p.LastName.ToLower().StartsWith( lastName.ToLower() ) &&
                        ( p.NickName.ToLower().StartsWith( firstName.ToLower() ) ||
                        p.GivenName.StartsWith( firstName.ToLower() ) ) );
        }

        #endregion

        #region Get Person

        /// <summary>
        /// Gets the by encrypted ID.
        /// </summary>
        /// <param name="encryptedID">The encrypted ID.</param>
        /// <returns></returns>
        public Person GetByEncryptedID( string encryptedID )
        {
            string identifier = Rock.Security.Encryption.DecryptString( encryptedID );

            string[] idParts = identifier.Split( '|' );
            if ( idParts.Length == 2 )
            {
                Guid personGuid = new Guid( idParts[0] );
                int personId = Int32.Parse( idParts[1] );

                Person person = Queryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();

                if ( person != null )
                    return person;

                // Check to see if the record was merged
                PersonMergedService personMergedService = new PersonMergedService();
                PersonMerged personMerged = personMergedService.Queryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();

                if ( personMerged != null )
                    return Get( personMerged.Id, true );
            }

            return null;
        }

        /// <summary>
        /// Gets Person by Id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="followTrail">if set to <c>true</c> follow the merge trail.</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Person Get( int id, bool followTrail )
        {
            if ( followTrail )
                id = new PersonMergedService().Current( id );

            return Get( id );
        }

        /// <summary>
        /// Gets Person by Guid
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <param name="followTrail">if set to <c>true</c> follow the merge trail</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Person GetByEncryptedKey( string encryptedKey, bool followTrail )
        {
            if ( followTrail )
                encryptedKey = new PersonMergedService().Current( encryptedKey );

            return GetByEncryptedKey( encryptedKey );
        }

        #endregion

        #region User Values

        /// <summary>
        /// Saves values for a given person, and key
        /// </summary>
        /// <param name="person"></param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="personId"></param>
        public void SaveUserValue(Person person, string key, List<string> values, int? personId)
        {
            var attributeService = new Core.AttributeService();
            var attribute = attributeService.GetByEntityAndEntityQualifierColumnAndEntityQualifierValueAndKey(
                Person.USER_VALUE_ENTITY, string.Empty, string.Empty, key );

            if ( attribute == null )
            {
                var fieldTypeService = new Core.FieldTypeService();
                var fieldType = fieldTypeService.GetByGuid(Rock.SystemGuid.FieldType.TEXT);

                attribute = new Core.Attribute();
                attribute.IsSystem = false;
                attribute.Entity = Person.USER_VALUE_ENTITY;
                attribute.EntityQualifierColumn = string.Empty;
                attribute.EntityQualifierValue = string.Empty;
                attribute.Key = key;
                attribute.Name = key;
                attribute.Category = string.Empty;
                attribute.DefaultValue = string.Empty;
                attribute.IsMultiValue = false;
                attribute.IsRequired = false;
                attribute.Description = string.Empty;
                attribute.FieldTypeId = fieldType.Id;
                attribute.Order = 0;

                attributeService.Add( attribute, personId );
                attributeService.Save( attribute, personId );
            }

            var attributeValueService = new Core.AttributeValueService();
            var attributeValues = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, person.Id );
            foreach ( var attributeValue in attributeValues )
                attributeValueService.Delete( attributeValue, personId );

            foreach ( var value in values )
            {
                var attributeValue = new Core.AttributeValue();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.EntityId = person.Id;
                attributeValue.Value = value;
                attributeValueService.Add( attributeValue, personId );
                attributeValueService.Save( attributeValue, personId );
            }
        }

        /// <summary>
        /// Gets values for a given person, and key
        /// </summary>
        /// <param name="person"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> GetUserValue( Person person, string key )
        {
            var attributeService = new Core.AttributeService();
            var attribute = attributeService.GetByEntityAndEntityQualifierColumnAndEntityQualifierValueAndKey(
                Person.USER_VALUE_ENTITY, string.Empty, string.Empty, key);

            if (attribute != null)
            {
                var attributeValueService = new Core.AttributeValueService();
                var attributeValues = attributeValueService.GetByAttributeIdAndEntityId(attribute.Id, person.Id);
                if (attributeValues != null && attributeValues.Count() > 0)
                    return attributeValues.Select( v => v.Value).ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets all values for a given person
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetUserValues( Person person )
        {
            var values = new Dictionary<string, List<string>>();

            foreach ( var attributeValue in new Core.AttributeValueService().Queryable()
                .Where( v =>
                    v.Attribute.Entity == Person.USER_VALUE_ENTITY &&
                    v.Attribute.EntityQualifierColumn == string.Empty &&
                    v.Attribute.EntityQualifierValue == string.Empty &&
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
