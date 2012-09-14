//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Static Guids used by the Rock ChMS application
    /// </summary>
    public static class DefinedValue
    {
		/// <summary>
		/// Guid for the types of Person phone numbers (such as Primary, Secondary, etc.)
		/// </summary>
		public static Guid PERSON_PHONE_PRIMARY { get { return new Guid( "407E7E45-7B2E-4FCD-9605-ECB1339F2453" ); } }
	
    }
}