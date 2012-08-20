using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rock.CMS;
using Rock.Core;
using Rock.Data;

namespace Rock.Tests.Fakes
{
    public class FakeRepository<T> : IRepository<T> where T : Model<T>
    {
        public FakeRepository()
        {

        }

        public IQueryable<T> AsQueryable()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Find( Expression<Func<T, bool>> @where )
        {
            throw new NotImplementedException();
        }

        public T Single( Expression<Func<T, bool>> @where )
        {
            throw new NotImplementedException();
        }

        public T First( Expression<Func<T, bool>> @where )
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault( Expression<Func<T, bool>> @where )
        {
            throw new NotImplementedException();
        }

        public void Add( T entity )
        {
            throw new NotImplementedException();
        }

        public void Attach( T entity )
        {
            throw new NotImplementedException();
        }

        public void Delete( T entity )
        {
            throw new NotImplementedException();
        }

        public List<EntityChange> Save( int? personId )
        {
            throw new NotImplementedException();
        }
    }
}
