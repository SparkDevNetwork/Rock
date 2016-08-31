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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Fakes
{
    public class FakeEntityTypeRepository : IRepository<EntityType>
    {
        private static IQueryable<EntityType> db;

        public FakeEntityTypeRepository()
        {
            Reset();
        }

        public static void Reset()
        {
            var assembly = Assembly.GetAssembly( typeof ( EntityType ) );
            var entities = new List<EntityType>();
            var i = 0;

            foreach ( var type in assembly.GetTypes().Where( type => type.Namespace == "Rock.Model" ) )
            {
                i++;
                entities.Add( new EntityType()
                    {
                        Name = type.Name,
                        AssemblyName = type.Assembly.FullName,
                        FriendlyName = type.Name,
                        Guid = Guid.NewGuid(),
                        Id = i
                    } );
            }

            db = entities.AsQueryable();
        }

        public IQueryable<EntityType> AsQueryable()
        {
            return db.AsQueryable();
        }

        public IQueryable<EntityType> AsQueryable( string includes )
        {
            return db.AsQueryable();
        }

        public IEnumerable<EntityType> GetAll()
        {
            return db;
        }

        public IEnumerable<EntityType> Find( Expression<Func<EntityType, bool>> where )
        {
            return db.Where( where );
        }

        public EntityType Single( Expression<Func<EntityType, bool>> where )
        {
            return db.Single( where );
        }

        public EntityType First( Expression<Func<EntityType, bool>> where )
        {
            return db.First( where );
        }

        public EntityType FirstOrDefault( Expression<Func<EntityType, bool>> where )
        {
            return db.FirstOrDefault( where );
        }

        public DateTime? DateCreated( EntityType entity )
        {
            return DateCreated( entity.TypeId, entity.Id );
        }

        public DateTime? DateCreated( int entityTypeId, int entityId )
        {
            return null;
        }

        public DateTime? DateLastModified( EntityType entity )
        {
            return DateLastModified( entity.TypeId, entity.Id );
        }

        public DateTime? DateLastModified( int entityTypeId, int entityId )
        {
            return null;
        }

        public int? CreatedByPersonId( EntityType entity )
        {
            return CreatedByPersonId( entity.TypeId, entity.Id );
        }

        public int? CreatedByPersonId( int entityTypeId, int entityId )
        {
            return null;
        }

        public int? LastModifiedByPersonId( EntityType entity )
        {
            return LastModifiedByPersonId( entity.TypeId, entity.Id );
        }

        public int? LastModifiedByPersonId( int entityTypeId, int entityId )
        {
            return null;
        }

        public IQueryable<Audit> Audits( EntityType entity )
        {
            return Audits( entity.TypeId, entity.Id );
        }

        public IQueryable<Audit> Audits( int entityTypeId, int entityId )
        {
            return null;
        }

        public void Add( EntityType entity )
        {
            db.ToList().Add( entity );
        }

        public void Attach( EntityType entity )
        {
        }

        public virtual void SetValues( EntityType sourceItem, EntityType targetTtem )
        {
            throw new NotImplementedException();
        }

        public void Delete( EntityType entity )
        {
            db.ToList().Remove( entity );
        }

        public bool Save( PersonAlias personAlias, out List<Audit> audits, out List<string> errorMessages )
        {
            audits = new List<Audit>();
            errorMessages = new List<string>();

            return true;
        }

        public IEnumerable<EntityType> ExecuteQuery( string query, params object[] parameters )
        {
            throw new NotImplementedException();
        }

        public void SetConfigurationValue( string key, string value )
        {
        }

    }
}
