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
        /// Binary File field type
        /// </summary>
        public const string BINARY_FILE = "C403E219-A56B-439E-9D50-9302DFE760CF";

        /// <summary>
        /// Boolean field type
        /// </summary>
        public const string BOOLEAN = "1EDAFDED-DFE6-4334-B019-6EECBA89E05A";

        /// <summary>
        /// Date field type
        /// </summary>
        public const string DATE = "6B6AA175-4758-453F-8D83-FCD8044B5F36";

        /// <summary>
        /// Defined value field type
        /// </summary>
        public const string DEFINED_VALUE = "59D5A94C-94A0-4630-B80A-BB25697D74C7";

        /// <summary>
        /// Multi Select field type
        /// </summary>
        public const string MULTI_SELECT = "BD0D9B57-2A41-4490-89FF-F01DAB7D4904";

        /// <summary>
        /// Integer field type
        /// </summary>
        public const string INTEGER = "A75DFC58-7A1B-4799-BF31-451B2BBE38FF";

        /// <summary>
        /// Image field type
        /// </summary>
        public const string IMAGE = "97f8157d-a8c8-4ab3-96a2-9cb2a9049e6d";

        /// <summary>
        /// Single Select field type
        /// </summary>
        public const string SINGLE_SELECT = "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0";

        /// <summary>
        /// Text field type
        /// </summary>
        public const string TEXT = "9C204CD0-1233-41C5-818A-C5DA439445AA";
    }
}