using System;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    public partial class DataView
    {
        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        public IQueryable<IEntity> GetQuery()
        {
            throw new NotImplementedException();
        }
    }
}
