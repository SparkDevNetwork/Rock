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
            return Repository.Find( t => ( t.MaritalStatusValueId == maritalStatusId || ( maritalStatusId == null && t.MaritalStatusValueId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Person Status Id
        /// </summary>
        /// <param name="personStatusId">Person Status Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByPersonStatusId( int? personStatusId )
        {
            return Repository.Find( t => ( t.PersonStatusValueId == personStatusId || ( personStatusId == null && t.PersonStatusValueId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Record Status Id
        /// </summary>
        /// <param name="recordStatusId">Record Status Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByRecordStatusId( int? recordStatusId )
        {
            return Repository.Find( t => ( t.RecordStatusValueId == recordStatusId || ( recordStatusId == null && t.RecordStatusValueId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Record Status Reason Id
        /// </summary>
        /// <param name="recordStatusReasonId">Record Status Reason Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByRecordStatusReasonId( int? recordStatusReasonId )
        {
            return Repository.Find( t => ( t.RecordStatusReasonValueId == recordStatusReasonId || ( recordStatusReasonId == null && t.RecordStatusReasonValueId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Record Type Id
        /// </summary>
        /// <param name="recordTypeId">Record Type Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByRecordTypeId( int? recordTypeId )
        {
            return Repository.Find( t => ( t.RecordTypeValueId == recordTypeId || ( recordTypeId == null && t.RecordTypeValueId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Suffix Id
        /// </summary>
        /// <param name="suffixId">Suffix Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetBySuffixId( int? suffixId )
        {
            return Repository.Find( t => ( t.SuffixValueId == suffixId || ( suffixId == null && t.SuffixValueId == null ) ) );
        }
        
        /// <summary>
        /// Gets People by Title Id
        /// </summary>
        /// <param name="titleId">Title Id.</param>
        /// <returns>An enumerable list of Person objects.</returns>
        public IEnumerable<Person> GetByTitleId( int? titleId )
        {
            return Repository.Find( t => ( t.TitleValueId == titleId || ( titleId == null && t.TitleValueId == null ) ) );
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
                var fieldType = fieldTypeService.GetByGuid(Rock.SystemGuid.FieldType.TEXT);

                attribute = new Model.Attribute();
                attribute.IsSystem = false;
                attribute.EntityTypeId = PersonEntityTypeId;
                attribute.EntityTypeQualifierColumn = string.Empty;
                attribute.EntityTypeQualifierValue = string.Empty;
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

            var attributeValueService = new Model.AttributeValueService();
            var attributeValues = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, person.Id );
            foreach ( var attributeValue in attributeValues )
                attributeValueService.Delete( attributeValue, personId );

            foreach ( var value in values )
            {
                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    var attributeValue = new Model.AttributeValue();
                    attributeValue.AttributeId = attribute.Id;
                    attributeValue.EntityId = person.Id;
                    attributeValue.Value = value;
                    attributeValueService.Add( attributeValue, personId );
                    attributeValueService.Save( attributeValue, personId );
                }
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
