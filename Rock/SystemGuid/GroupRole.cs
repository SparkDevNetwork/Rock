using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.SystemGuid
{
	/// <summary>
	/// Group Role System Guids
	/// </summary>
    public static class GroupRole
    {
		/// <summary>
		/// Gets the Known Relationships owner role.
		/// </summary>
		/// <value>
		/// The role Guid
		/// </value>
		public static Guid GROUPROLE_KNOWN_RELATIONSHIPS_OWNER { get { return new Guid( "7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42" ); } }

		/// <summary>
		/// Gets the Implied Relationships owner role.
		/// </summary>
		/// <value>
		/// The role Guid.
		/// </value>
		public static Guid GROUPROLE_IMPLIED_RELATIONSHIPS_OWNER { get { return new Guid( "CB9A0E14-6FCF-4C07-A49A-D7873F45E196" ); } }

    }
}