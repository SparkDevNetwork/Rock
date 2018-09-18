using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    public partial class AssetStorageSystemService
    {
        public IQueryable<AssetStorageSystem> GetActiveNoTracking()
        {
            return Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive == true );
        }

        public IQueryable<AssetStorageSystem> GetAllNoTracking()
        {
            return Queryable().AsNoTracking();
        }


    }
}
