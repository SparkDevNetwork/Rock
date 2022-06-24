using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Migrations.Migrations
{
    internal class RockSQLMigration : Rock.Data.IMigration
    {
        public void Sql( string sql )
        {
            throw new NotImplementedException();
        }

        public object SqlScalar( string sql )
        {
            throw new NotImplementedException();
        }
    }
}
