using System;
using System.Web.UI;
using Rock.Attribute;

namespace Rock.Web
{
    /// <summary>
    /// Defines a UI provider that uses a User Control (ascx) to provide the UI.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockCustomSettingsProvider" />
    public abstract class RockCustomSettingsUserControlProvider : RockCustomSettingsProvider
    {
        #region Abstract Properties

        /// <summary>
        /// Gets the path to the user control file.
        /// </summary>
        /// <value>
        /// The path to the user control file.
        /// </value>
        protected abstract string UserControlPath { get; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Gets the custom settings control. The returned control will be added to the parent automatically.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="parent">The parent control that will eventually contain the returned control.</param>
        /// <returns>
        /// A control that contains all the custom UI.
        /// </returns>
        public override Control GetCustomSettingsControl( IHasAttributes attributeEntity, Control parent )
        {
            var control = parent.LoadControl( UserControlPath );
            control.ClientIDMode = ClientIDMode.AutoID;

            return control;
        }

        /// <summary>
        /// Update the custom UI to reflect the current settings found in the entity.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="control">The control returned by GetCustomSettingsControl().</param>
        /// <exception cref="InvalidOperationException">Custom settings user control does not implement {nameof( IRockCustomSettingsUserControl )}</exception>
        public override void ReadSettingsFromEntity( IHasAttributes attributeEntity, Control control )
        {
            if ( !( control is IRockCustomSettingsUserControl customSettingsControl ) )
            {
                throw new InvalidOperationException( $"Custom settings user control does not implement {nameof( IRockCustomSettingsUserControl )}" );
            }

            customSettingsControl.ReadSettingsFromEntity( attributeEntity );
        }

        /// <summary>
        /// Update the entity with values from the custom UI.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="control">The control.</param>
        /// <exception cref="InvalidOperationException">Custom settings user control does not implement {nameof( IRockCustomSettingsUserControl )}</exception>
        /// <remarks>
        /// Do not save the entity, it will be automatically saved later.
        /// </remarks>
        public override void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control )
        {
            if ( !( control is IRockCustomSettingsUserControl customSettingsControl ) )
            {
                throw new InvalidOperationException( $"Custom settings user control does not implement {nameof( IRockCustomSettingsUserControl )}" );
            }

            customSettingsControl.WriteSettingsToEntity( attributeEntity );
        }

        #endregion
    }
}
