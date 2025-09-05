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

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Core.Automation;
using Rock.Data;
using Rock.Lava;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// A single event that will be executed in response to an <see cref="Model.AutomationTrigger"/>.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AutomationEvent" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Disable v1 controller.
    [Rock.SystemGuid.EntityTypeGuid( "905de2d9-1ea8-4e59-b0cf-e2bac8383927" )]
    public partial class AutomationEvent : Model<AutomationEvent>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the <see cref="Model.AutomationTrigger"/> that will
        /// cause this event to to execute.
        /// </summary>
        [DataMember]
        [IgnoreCanDelete]
        public int AutomationTriggerId { get; set; }

        /// <summary>
        /// Indicates if this event is active. If this is set to <c>false</c>
        /// then the event will not be executed when the trigger fires.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// The order in which this event will be executed for the trigger.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// The <see cref="EntityType"/> identifier of the <see cref="AutomationEventComponent"/>
        /// that will handle the logic for this event.
        /// </summary>
        [DataMember]
        public int ComponentEntityTypeId { get; set; }

        /// <summary>
        /// The configuration data for the <see cref="AutomationEventComponent"/>.
        /// This is stored as a dictionary of string key/value pairs.
        /// </summary>
        [DataMember]
        public string ComponentConfigurationJson { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }
        #endregion

        #region Navigation Properties

        /// <summary>
        /// The <see cref="AutomationTrigger"/> that will cause this event to
        /// execute.
        /// </summary>
        [LavaVisible]
        public virtual AutomationTrigger AutomationTrigger { get; set; }

        /// <summary>
        /// The <see cref="EntityType"/> that represents the <see cref="AutomationEventComponent"/>
        /// that will handle the logic for this event.
        /// </summary>
        [LavaVisible]
        public virtual EntityType ComponentEntityType { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Automation Event Configuration class.
    /// </summary>
    public partial class AutomationEventConfiguration : EntityTypeConfiguration<AutomationEvent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationEventConfiguration"/> class.
        /// </summary>
        public AutomationEventConfiguration()
        {
            this.HasRequired( ae => ae.AutomationTrigger ).WithMany( at => at.AutomationEvents ).HasForeignKey( ae => ae.AutomationTriggerId ).WillCascadeOnDelete( true );
            this.HasRequired( ae => ae.ComponentEntityType ).WithMany().HasForeignKey( ae => ae.ComponentEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
