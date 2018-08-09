using Rock.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace church.ccv.PersonalizationEngine.Data
{
    public class PersonalizationEngineService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        public PersonalizationEngineService( RockContext rockContext )
            : base( rockContext )
        {
        }
    }
}
