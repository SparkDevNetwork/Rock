using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using com.bemaservices.MailChimp.Model;
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.PastoralCare.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class MailChipMemberService : Service<MailChimpMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareContactService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MailChipMemberService( RockContext context ) : base( context ) { }

    }
}
