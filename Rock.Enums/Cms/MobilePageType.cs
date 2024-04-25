namespace Rock.Enums.Cms
{
    /// <summary>
    /// Represents the type of a mobile page.
    /// </summary>
    public enum MobilePageType
    {
        /// <summary>
        /// A native page, such as a page that is part of the Rock mobile app.
        /// </summary>
        NativePage,

        /// <summary>
        /// An internal web page, such as a web page that is part of the Rock mobile app.
        /// </summary>
        InternalWebPage,

        /// <summary>
        /// An external web page, such as a web page that is not part of the Rock mobile app.
        /// </summary>
        ExternalWebPage
    }
}
