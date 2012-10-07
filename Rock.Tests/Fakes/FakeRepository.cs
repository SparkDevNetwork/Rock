//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rock.Core;
using Rock.Data;


namespace Rock.Tests.Fakes
{
    public class FakeRepository<T> : IRepository<T> where T : Entity<T>
    {
        private static IQueryable<T> db; 

        public static void Reset()
        {
            db = new List<T>().AsQueryable();
        }

        public FakeRepository()
        {
            Reset();
        }

        public virtual IQueryable<T> AsQueryable()
        {
            return db.AsQueryable();
        }

		public virtual IQueryable<T> AsQueryable(string includes)
		{
			return db.AsQueryable();
		}
		
		public virtual IEnumerable<T> GetAll()
        {
            return db;
        }

        public virtual IEnumerable<T> Find( Expression<Func<T, bool>> where )
        {
            return db.Where( where );
        }

        public virtual T Single( Expression<Func<T, bool>> where )
        {
            return db.Single( where );
        }

        public virtual T First( Expression<Func<T, bool>> where )
        {
            return db.First( where );
        }

        public virtual T FirstOrDefault( Expression<Func<T, bool>> where )
        {
            return FirstOrDefault( where );
        }

		public virtual DateTimeOffset? DateCreated( T entity )
		{
			return DateCreated( entity.EntityTypeName, entity.Id );
		}

		public virtual DateTimeOffset? DateCreated( string entityTypeName, int entityId )
		{
			return null; 
		}

		public virtual DateTimeOffset? DateLastModified( T entity )
		{
			return DateLastModified( entity.EntityTypeName, entity.Id );
		}

		public virtual DateTimeOffset? DateLastModified( string entityTypeName, int entityId )
		{
			return null;
		}

		public virtual int? CreatedByPersonId( T entity )
		{
			return CreatedByPersonId( entity.EntityTypeName, entity.Id );
		}

		public virtual int? CreatedByPersonId( string entityTypeName, int entityId )
		{
			return null;
		}

		public virtual int? LastModifiedByPersonId( T entity )
		{
			return LastModifiedByPersonId( entity.EntityTypeName, entity.Id );
		}

		public virtual int? LastModifiedByPersonId( string entityTypeName, int entityId )
		{
			return null;
		}

		public virtual IQueryable<Audit> Audits( T entity )
		{
			return Audits( entity.EntityTypeName, entity.Id );
		}

		public virtual IQueryable<Audit> Audits( string entityTypeName, int entityId )
		{
			return null;
		}

		public virtual void Add( T entity )
        {
            db.ToList().Add( entity );
        }

        public virtual void Attach( T entity )
        {
            
        }

        public virtual void Delete( T entity )
        {
            db.ToList().Remove( entity );
        }

        public virtual List<EntityChange> Save( int? personId, List<AuditDto> audits )
        {
            return null;
        }
    }
}
