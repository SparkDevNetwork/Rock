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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
    /// A trigger point for the automation system. This contains the configuration
    /// data for a component to know when to trigger. When that happens all the
    /// <see cref="AutomationEvent"/> instances that are associated with this
    /// trigger will be executed.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AutomationTrigger" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Disable v1 controller.
    [Rock.SystemGuid.EntityTypeGuid( "89abfa37-68e5-41b7-b43c-a0cf823dea61" )]
    public partial class AutomationTrigger : Model<AutomationTrigger>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The name of the trigger. This is used to identify the trigger in the
        /// user interface and logs. It should be short, but descriptive.
        /// </summary>
        [Required]
        [Index( IsUnique = true )]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The description of the trigger. This is used to provide additional
        /// details about when the trigger will execute the events and describe
        /// the purpose the trigger serves.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if this trigger is active. If this is set to <c>false</c>
        /// then the trigger will not be initialized and no events will execute.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The <see cref="EntityType"/> identifier of the <see cref="AutomationTriggerComponent"/>
        /// that will handle the logic for this trigger.
        /// </summary>
        [DataMember]
        public int ComponentEntityTypeId { get; set; }

        /// <summary>
        /// The configuration data for the <see cref="AutomationTriggerComponent"/>.
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
        /// The <see cref="EntityType"/> that represents the <see cref="AutomationTriggerComponent"/>
        /// that will handle the logic for this trigger.
        /// </summary>
        [LavaVisible]
        public virtual EntityType ComponentEntityType { get; set; }

        /// <summary>
        /// A collection containing the <see cref="AutomationEvent"/> items that
        /// will be executed when this trigger fires.
        /// </summary>
        [LavaVisible]
        public virtual ICollection<AutomationEvent> AutomationEvents
        {
            get { return _automationEvents ?? ( _automationEvents = new Collection<AutomationEvent>() ); }
            set { _automationEvents = value; }
        }

        private ICollection<AutomationEvent> _automationEvents;

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
    /// Automation Trigger Configuration class.
    /// </summary>
    public partial class AutomationTriggerConfiguration : EntityTypeConfiguration<AutomationTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationTriggerConfiguration"/> class.
        /// </summary>
        public AutomationTriggerConfiguration()
        {
            this.HasRequired( ae => ae.ComponentEntityType ).WithMany().HasForeignKey( ae => ae.ComponentEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
