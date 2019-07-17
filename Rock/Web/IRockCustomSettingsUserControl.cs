using Rock.Attribute;

namespace Rock.Web
{
    /// <summary>
    /// Defines the methods that an ASCX based user control which provides custom
    /// UI for settings must implement.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockCustomSettingsUserControlProvider"/>
    public interface IRockCustomSettingsUserControl
    {
        /// <summary>
        /// Update the custom UI to reflect the current settings found in the entity.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        void ReadSettingsFromEntity( IHasAttributes attributeEntity );

        /// <summary>
        /// Update the entity with values from the custom UI.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        void WriteSettingsToEntity( IHasAttributes attributeEntity );
    }
}
