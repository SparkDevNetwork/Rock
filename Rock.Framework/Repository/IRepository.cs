using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rock.Repository
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> AsQueryable();
        IEnumerable<T> GetAll();

        IEnumerable<T> Find( Expression<Func<T, bool>> where );
        T Single( Expression<Func<T, bool>> where );
        T First( Expression<Func<T, bool>> where );
        T FirstOrDefault( Expression<Func<T, bool>> where );

        void Delete( T entity );
        void Add( T entity );
        void Attach( T entity );
        List<Rock.Models.Core.EntityChange> Save( T entity, int? personId );
    }
}
