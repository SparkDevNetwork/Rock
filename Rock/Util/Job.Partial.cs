//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Util
{
    /// <summary>
    /// Job notification status
    /// </summary>
    public enum JobNotificationStatus
    {
        /// <summary>
        /// Notify on all status
        /// </summary>
        All = 1,

        /// <summary>
        /// Notify when successful
        /// </summary>
        /// 
        Success = 2,

        /// <summary>
        /// Notify when an error occurs
        /// </summary>
        Error = 3,

        /// <summary>
        /// Notify when a warning occurs
        /// </summary>
        None = 4
    }
}
