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
        /// Returns a message in the format: "You are not authorized to edit {0}."
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string NotAuthorizedToEdit( string itemFieldName )
        {
            return string.Format( "You are not authorized to edit {0}.", itemFieldName.Pluralize().ToLower() );
        }

        /// <summary>
        /// Returns a message in the format: Invalid format for {0}."
        /// </summary>
        /// <param name="itemFieldName">Name of the item field.</param>
        /// <returns></returns>
        public static string DateTimeFormatInvalid( string itemFieldName )
        {
            return string.Format( "Invalid format for {0}.", itemFieldName.SplitCase().ToLower() );
        }

        /// <summary>
        /// Returns a message: "End Date cannot be earlier than Start Date"
        /// </summary>
        /// <returns></returns>
        public static string DateRangeEndDateBeforeStartDate()
        {
            return string.Format( "End Date cannot be earlier than Start Date" );
        }

        /// <summary>
        /// Returns a message in the format: "Value required for {0}."
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
        /// 0
        /// </summary>
        public const int Id = 0;

        /// <summary>
        /// 0
        /// </summary>
        public const string IdValue = "0";

        /// <summary>
        /// "<none>"
        /// </summary>
        public const string Text = "<none>";

        /// <summary>
        /// 
        /// </summary>
        public const string TextHtml = "&lt;none&gt;";

        /// <summary>
        /// Return a ListItem with Text: "None", Value: 0
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
        /// Returns -1
        /// </summary>
        public const int Id = -1;

        /// <summary>
        /// Returns "-1"
        /// </summary>
        public const string IdValue = "-1";

        /// <summary>
        /// returns "[All]"
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
        /// Returns a message in the format: "Add {0}"
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string Add( string itemFriendlyName )
        {
            return string.Format( "Add {0}", itemFriendlyName );
        }

        /// <summary>
        /// Returns a message in the format: "Edit {0}"
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string Edit( string itemFriendlyName )
        {
            return string.Format( "Edit {0}", itemFriendlyName );
        }

        /// <summary>
        /// Returns a message in the format: "View {0}"
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
        /// Returns a message in the format: "INFO: This is a read-only system {0}."
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item friendly.</param>
        /// <returns></returns>
        public static string ReadOnlySystem( string itemFriendlyName )
        {
            return string.Format( "This is a read-only system {0}.", itemFriendlyName.ToLower() );
        }

        /// <summary>
        /// Returns a message in the format: "INFO: You do not have access to edit this {0}."
        /// </summary>
        /// <param name="itemFriendlyName">Name of the item field.</param>
        /// <returns></returns>
        public static string ReadOnlyEditActionNotAllowed( string itemFriendlyName )
        {
            return string.Format( "You do not have access to edit this {0}.", itemFriendlyName.ToLower() );
        }
    }
}