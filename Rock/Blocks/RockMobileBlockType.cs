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
using Rock.Attribute;
using Rock.Mobile;
using Rock.Web;

namespace Rock.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />
    public abstract class RockMobileBlockType : RockBlockType, IRockMobileBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the required mobile application binary interface version.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version.
        /// </value>
        public abstract int RequiredMobileAbiVersion { get; }

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public abstract string MobileBlockType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the property values that will be sent to the block.
        /// </summary>
        /// <returns>A collection of string/object pairs.</returns>
        public override object GetBlockInitialization( RockClientType clientType )
        {
            if ( clientType == RockClientType.Mobile )
            {
                return GetMobileConfigurationValues();
            }

            return null;
        }

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual object GetMobileConfigurationValues()
        {
            return null;
        }


        /// <summary>
        /// Gets the additional settings defined for this block instance.
        /// </summary>
        /// <returns>An AdditionalBlockSettings object.</returns>
        public AdditionalBlockSettings GetAdditionalSettings()
        {
            return BlockCache?.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();
        }

        #endregion

        #region Custom Settings

        /// <summary>
        /// Defines the control that will provide the Basic Settings tab content
        /// for all RockMobileBockType blocks.
        /// </summary>
        /// <seealso cref="Rock.Web.RockCustomSettingsUserControlProvider" />
        [TargetType( typeof( RockMobileBlockType ) )]
        public class RockMobileBlockTypeCustomSettingsProvider : RockCustomSettingsUserControlProvider
        {
            /// <summary>
            /// Gets the path to the user control file.
            /// </summary>
            /// <value>
            /// The path to the user control file.
            /// </value>
            protected override string UserControlPath => "~/Blocks/Mobile/MobileCustomSettings.ascx";

            /// <summary>
            /// Gets the custom settings title. Used when displaying tabs or links to these settings.
            /// </summary>
            /// <value>
            /// The custom settings title.
            /// </value>
            public override string CustomSettingsTitle => "Mobile Settings";
        }

        #endregion
    }
}
