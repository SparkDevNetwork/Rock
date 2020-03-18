using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.PastoralCare.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class CareItemService : Service<CareItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareItemService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CareItemService( RockContext context ) : base( context ) { }

        public bool CanDelete( CareItem careItem, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
