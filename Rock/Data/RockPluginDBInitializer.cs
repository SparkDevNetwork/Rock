using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Rock.Data
{
    public class RockPluginDBInitializer<T> : IDatabaseInitializer<T> where T : DbContext
    {
        public void InitializeDatabase( T context )
        {
            context.
            throw new NotImplementedException();
        }
    }
}