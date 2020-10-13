using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Jobs
{
    internal class JobMigration : IMigration
    {
        private readonly int _commandTimeout;

        public JobMigration(int commandTimeout )
        {
            _commandTimeout = commandTimeout;
        }

        public void Sql( string sql )
        {
            DbService.ExecuteCommand( sql, commandTimeout: _commandTimeout );
        }

        public object SqlScalar( string sql )
        {
            return DbService.ExecuteScaler( sql );
        }
    }
}
