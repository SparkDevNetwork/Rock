//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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

        public void Delete( EntityType entity )
        {
            db.ToList().Remove( entity );
        }

        public bool Save( int? PersonId, out List<EntityChange> changes, out List<Audit> audits, out List<string> errorMessages )
        {
            changes = new List<EntityChange>();
            audits = new List<Audit>();
            errorMessages = new List<string>();

            return true;
        }

        public void SetConfigurationValue( string key, string value )
        {
        }

    }
}
