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
        /// <param name="itemFriendlyName">Name of the item friendly.</param>
        /// <returns></returns>
        public static string DuplicateFoundMessage( string nameFieldname, string itemFriendlyName )
        {
            return string.Format( "This {0} is already being used by another {1}.", nameFieldname.ToLower(), itemFriendlyName.ToLower() );
        }

        /// <summary>
        /// Nots the authorized to edit.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string NotAuthorizedToEdit( string itemFieldName )
        {
            return string.Format( "You are not authorized to edit {0}.", itemFieldName.Pluralize().ToLower() );
        }

        /// <summary>
        /// Dates the time format invalid.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string DateTimeFormatInvalid( string itemFieldName )
        {
            return string.Format( "Invalid format for {0}.", itemFieldName.SplitCase().ToLower() );
        }

        /// <summary>
        /// Dates the range end date before start date.
        /// </summary>
        /// <returns></returns>
        public static string DateRangeEndDateBeforeStartDate()
        {
            return string.Format( "End Date cannot be earlier than Start Date" );
        }

        /// <summary>
        /// Cannots the be blank.
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string CannotBeBlank( string itemFieldName )
        {
            return string.Format( "Value required for {0}.", itemFieldName.SplitCase().ToLower() );
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

        /// <summary>
        /// 
        /// </summary>
        public const string TextHtml = "&lt;none&gt;";

        /// <summary>
        /// Gets the list item.
        /// </summary>
        /// <value>
        /// The list item.
        /// </value>
        public static System.Web.UI.WebControls.ListItem ListItem
        {
            get
            {
                return new System.Web.UI.WebControls.ListItem( None.Text, None.IdValue );
            }
        }
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
        public const string Text = "[All]";

        /// <summary>
        /// Gets the list item.
        /// </summary>
        /// <value>
        /// The list item.
        /// </value>
        public static System.Web.UI.WebControls.ListItem ListItem
        {
            get
            {
                return new System.Web.UI.WebControls.ListItem( All.Text, All.IdValue );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ActionTitle
    {
        /// <summary>
        /// Adds the specified item field name.
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string Add( string itemFriendlyName )
        {
            return string.Format( "Add {0}", itemFriendlyName );
        }

        /// <summary>
        /// Edits the specified item field name.
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string Edit( string itemFriendlyName )
        {
            return string.Format( "Edit {0}", itemFriendlyName );
        }

        /// <summary>
        /// Views the specified item field name.
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string View( string itemFriendlyName )
        {
            return string.Format( "View {0}", itemFriendlyName );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class EditModeMessage
    {
        /// <summary>
        /// Reads the only system.
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item friendly.</param>
        /// <returns></returns>
        public static string ReadOnlySystem( string itemFriendlyName )
        {
            return string.Format( "INFO: This is a read-only system {0}.", itemFriendlyName.ToLower() );
        }

        /// <summary>
        /// Reads the only edit action not allowed.
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string ReadOnlyEditActionNotAllowed( string itemFriendlyName )
        {
            return string.Format( "INFO: You do not have access to edit this {0}.", itemFriendlyName.ToLower() );
        }
    }
}