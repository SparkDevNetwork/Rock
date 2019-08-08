// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.UI;
using Rock.Attribute;
using Rock.Data;

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
        /// <param name="control">The control returned by the GetCustomSettingsControl() method.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <exception cref="InvalidOperationException">Custom settings user control does not implement {nameof( IRockCustomSettingsUserControl )}</exception>
        /// <remarks>
        /// Do not save the entity, it will be automatically saved later. This call will be made inside
        /// a SQL transaction for the passed rockContext. If you need to make changes to the database
        /// do so on this context so they can be rolled back if something fails during the final save.
        /// </remarks>
        public override void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control, RockContext rockContext )
        {
            if ( !( control is IRockCustomSettingsUserControl customSettingsControl ) )
            {
                throw new InvalidOperationException( $"Custom settings user control does not implement {nameof( IRockCustomSettingsUserControl )}" );
            }

            customSettingsControl.WriteSettingsToEntity( attributeEntity, rockContext );
        }

        #endregion
    }
}
