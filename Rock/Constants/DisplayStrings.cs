//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Constants
{
    /// <summary>
    /// 
    /// </summary>
    public static class WarningMessage
    {
        /// <summary>
        /// Duplicates the found message.
        /// </summary>
        /// <param name="nameFieldname">The name fieldname.</param>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string DuplicateFoundMessage( string nameFieldname, string itemFieldName )
        {
            return string.Format( "This {0} is already being used by another {1}.", nameFieldname.ToLower(), itemFieldName.ToLower() );
        }

        /// <summary>
        /// Nots the authorized to edit.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string NotAuthorizedToEdit( string itemFieldName)
        {
            return string.Format( "You are not authorized to edit {0}.", itemFieldName.Pluralize().ToLower() );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class None
    {
        /// <summary>
        /// 
        /// </summary>
        public const int Id = 0;

        /// <summary>
        /// 
        /// </summary>
        public const string IdValue = "0";

        /// <summary>
        /// 
        /// </summary>
        public const string Text = "<none>";
    }

    /// <summary>
    /// 
    /// </summary>
    public static class All
    {
        /// <summary>
        /// 
        /// </summary>
        public const int Id = -1;

        /// <summary>
        /// 
        /// </summary>
        public const string IdValue = "-1";

        /// <summary>
        /// 
        /// </summary>
        public const string Text = "<All>";
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ActionTitle
    {
        /// <summary>
        /// Adds the specified item field name.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string Add( string itemFieldName )
        {
            return string.Format( "Add {0}", itemFieldName );
        }

        /// <summary>
        /// Edits the specified item field name.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string Edit( string itemFieldName )
        {
            return string.Format( "Edit {0}", itemFieldName );
        }

        /// <summary>
        /// Views the specified item field name.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string View( string itemFieldName )
        {
            return string.Format( "View {0}", itemFieldName );
        }
    }
}