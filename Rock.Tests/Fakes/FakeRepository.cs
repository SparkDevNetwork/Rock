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
using Rock.Model;
using Rock.Data;


namespace Rock.Tests.Fakes
{
    public class FakeRepository<T> : IRepository<T> 
        where T : Entity<T>, new()
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

        public virtual DateTime? DateCreated( T entity )
        {
            return DateCreated( entity.TypeId, entity.Id );
        }

        public virtual DateTime? DateCreated( int entityTypeId, int entityId )
        {
            return null; 
        }

        public virtual DateTime? DateLastModified( T entity )
        {
            return DateLastModified( entity.TypeId, entity.Id );
        }

        public virtual DateTime? DateLastModified( int entityTypeId, int entityId )
        {
            return null;
        }

        public virtual int? CreatedByPersonId( T entity )
        {
            return CreatedByPersonId( entity.TypeId, entity.Id );
        }

        public virtual int? CreatedByPersonId( int entityTypeId, int entityId )
        {
            return null;
        }

        public virtual int? LastModifiedByPersonId( T entity )
        {
            return LastModifiedByPersonId( entity.TypeId, entity.Id );
        }

        public virtual int? LastModifiedByPersonId( int entityTypeId, int entityId )
        {
            return null;
        }

        public virtual IQueryable<Audit> Audits( T entity )
        {
            return Audits( entity.TypeId, entity.Id );
        }

        public virtual IQueryable<Audit> Audits( int entityTypeId, int entityId )
        {
            return null;
        }

        public virtual void Add( T entity )
        {
            db.ToList().Add( entity );
        }

        public virtual void Attach( T entity )
        {
            //
        }

        public virtual void SetValues( T sourceItem, T targetTtem )
        {
            //
        }

        public virtual void Delete( T entity )
        {
            db.ToList().Remove( entity );
        }

        public bool Save( PersonAlias personAlias, out List<Audit> audits, out List<string> errorMessages )
        {
            audits = new List<Audit>();
            errorMessages = new List<string>();

            return true;
        }

        public IEnumerable<T> ExecuteQuery( string query, params object[] parameters )
        {
            throw new NotImplementedException();
        }

        public void SetConfigurationValue( string key, string value )
        {
        }

    }
}
