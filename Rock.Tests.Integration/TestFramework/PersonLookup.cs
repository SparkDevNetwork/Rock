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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Integration.TestFramework
{
    /// <summary>
    /// A lookup service for person records.
    /// </summary>
    /// <param name="dataContext"></param>
    /// <returns></returns>
    public class PersonLookup
    {
        #region Well-Known Person Guids

        //public static Guid TedDeckerPersonGuid = "{8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4}";
        public static Guid BillMarblePersonGuid = new Guid( "1EA811BB-3118-42D1-B020-32A82BC8081A" );
        public static Guid TedDeckerPersonGuid = new Guid( "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4" );
        public static Guid AlishaMarblePersonGuid = new Guid( "69DC0FDC-B451-4303-BD91-EF17C0015D23" );
        public static Guid SarahSimmonsPersonGuid = new Guid( "FC6B9819-EF2E-44C9-93DB-05571B39E58F" );
        public static Guid BenJonesPersonGuid = new Guid( "3C402382-3BD2-4337-A996-9E62F1BAB09D" );
        public static Guid BrianJonesPersonGuid = new Guid( "3D7F6605-3666-4AB5-9F4E-D7FEBF93278E" );

        public static Guid HelenEvansPersonGuid = new Guid( "89b175ad-6bf1-4590-b936-1cf42014a337" );
        public static Guid HelenaEvansPersonGuid = new Guid( "0F5FA8C3-F9FE-42B7-B6E0-DBFF6C3FF973" );

        #endregion

        private Dictionary<Guid, int> _PersonGuidToAliasIdMap = null;
        private RockContext _DataContext;

        public PersonLookup( RockContext dataContext, int maxPersonCount = 0 )
        {
            _DataContext = dataContext;

            this.LoadMap( dataContext, maxPersonCount );
        }

        public List<Guid> PersonGuids
        {
            get
            {
                return _PersonGuidToAliasIdMap.Keys.ToList();
            }
        }

        private void LoadMap( RockContext dataContext, int maxPersonCount = 0 )
        {
            var aliasService = new PersonAliasService( dataContext );

            var personAliasList = aliasService.Queryable()
                .AsNoTracking()
                .Where( x => !x.Person.IsSystem );

            if ( maxPersonCount > 0 )
            {
                personAliasList = personAliasList.Take( maxPersonCount );
            }

            _PersonGuidToAliasIdMap = personAliasList.GroupBy( x => x.Person.Guid )
                 .ToDictionary( k => k.Key, v => v.First().Id );

            // Add entries for well-known Person records if they do not already exist.
            var personAliasService = new PersonAliasService( dataContext );

            var personService = new PersonService( dataContext );

            var personKnownGuids = new List<Guid>();

            personKnownGuids.Add( AlishaMarblePersonGuid );
            personKnownGuids.Add( BenJonesPersonGuid );
            personKnownGuids.Add( BillMarblePersonGuid );
            personKnownGuids.Add( BrianJonesPersonGuid );
            personKnownGuids.Add( SarahSimmonsPersonGuid );
            personKnownGuids.Add( TedDeckerPersonGuid );

            var knownPeople = personService.Queryable().Where( x => personKnownGuids.Contains( x.Guid ) );

            foreach ( var knownPerson in knownPeople )
            {
                if ( _PersonGuidToAliasIdMap.ContainsKey( knownPerson.Guid ) )
                {
                    continue;
                }

                _PersonGuidToAliasIdMap.Add( knownPerson.Guid, knownPerson.PrimaryAliasId ?? 0 );
            }

        }

        public int GetPersonAliasId( Guid personGuid, string personDescription = null )
        {
            if ( _PersonGuidToAliasIdMap == null )
            {
                throw new Exception( "Map must be initialized before use." );
            }

            if ( !_PersonGuidToAliasIdMap.ContainsKey( personGuid ) )
            {
                throw new Exception( string.Format( "Person Guid Lookup Failed. A matching person record could not be found for the specified Guid [Guid={0}, Description=\"{1}\"]", personGuid, personDescription ) );
            }

            return _PersonGuidToAliasIdMap[personGuid];
        }

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        public Person GetAdminPersonOrThrow()
        {
            // Return the record for Alisha Marble, who is the well-known default Administrator.
            var personService = new PersonService( _DataContext );

            var adminPerson = personService.Queryable().FirstOrDefault( x => x.Guid == AlishaMarblePersonGuid );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }
    }
}
