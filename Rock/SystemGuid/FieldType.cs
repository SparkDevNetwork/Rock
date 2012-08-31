//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
{
	/// <summary>
	/// System Blocks.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
	/// </summary>
	public static class FieldType
	{
		/// <summary>
		/// Gets the Plugin Manager guid
		/// </summary>
		public static Guid TEXT { get { return new Guid( "9C204CD0-1233-41C5-818A-C5DA439445AA" ); } }
	}
}