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

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a lava application
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "LavaApplication" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "FFFE0DE1-B410-435E-9AA8-3A0B18AAF0F7" )]
    public partial class LavaApplication : Model<LavaApplication>, ICacheable
    {
        #region Security Verbs

        /// <summary>
        /// Application: Execute View
        /// </summary>
        public const string EXECUTE_VIEW = "ExecuteView";

        /// <summary>
        /// Application: Execute Edit
        /// </summary>
        public const string EXECUTE_EDIT = "ExecuteEdit";

        /// <summary>
        /// Application: Execute Administrate
        /// </summary>
        public const string EXECUTE_ADMINISTRATE = "ExecuteAdministrate";

        #endregion

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [Required]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this application is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this application is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>
        /// The slug.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the additional settings json.
        /// </summary>
        /// <value>
        /// The additional settings json.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the configuration rigging json.
        /// </summary>
        /// <value>
        /// The configuration rigging json.
        /// </value>
        [DataMember]
        public string ConfigurationRiggingJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Model.LavaEndpoint">LavaEndpoints</see> who are associated with the Lava Application.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Model.LavaEndpoint">LavaEndpoints</see> who are associated with the Lava Application.
        /// </value>
        [DataMember]
        public virtual ICollection<LavaEndpoint> LavaEndpoints
        {
            get { return _lavaEndpoints ?? ( _lavaEndpoints = new Collection<LavaEndpoint>() ); }
            set { _lavaEndpoints = value; }
        }

        private ICollection<LavaEndpoint> _lavaEndpoints;

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>
                    {
                        { Authorization.VIEW, "The roles and/or users that have access to view." },
                        { Authorization.EDIT, "The roles and/or users that have access to edit." },
                        { Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." },
                        { EXECUTE_VIEW, "The roles and/or users that have access to execute endpoints in the context of viewing data." },
                        { EXECUTE_EDIT, "The roles and/or users that have access to execute endpoints in the context of editing data." },
                        { EXECUTE_ADMINISTRATE, "The roles and/or users that have access to execute endpoints in the context of administrating data." }
                    };
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        #endregion Navigation

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Application's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Application's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods

    }

    #region Entity Configuration

    /// <summary>
    /// Lava Application Configuration class
    /// </summary>
    public partial class LavaApplicationConfiguration : EntityTypeConfiguration<LavaApplication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavaApplicationConfiguration"/> class.
        /// </summary>
        public LavaApplicationConfiguration()
        {
            this.HasEntitySetName( "LavaApplications" );
        }
    }

    #endregion Entity Configuration
}