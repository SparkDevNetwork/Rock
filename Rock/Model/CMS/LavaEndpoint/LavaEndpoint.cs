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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Cms;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a lava endpoint
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "LavaEndpoint" )]
    [DataContract]
    [CodeGenerateRest]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Exclude v1 API controller.
    [Rock.SystemGuid.EntityTypeGuid( "F1BBF7D4-CAFD-450D-A89A-B3312C9738A2" )]
    [DebuggerDisplay( "Endpoint {Id}: {Name} [{HttpMethod}] ({LavaApplication.Slug}/{Slug})" )]
    public partial class LavaEndpoint : Model<LavaEndpoint>, ICacheable, IHasAdditionalSettings
    {
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
        /// Gets or sets the Id of the <see cref="Model.LavaApplication"/> lava application that is associated with this end point.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Model.LavaApplication"/> lava application that is associated with this end point.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int LavaApplicationId { get; set; }

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
        /// Gets or sets the security mode.
        /// </summary>
        /// <value>
        /// The security mode.
        /// </value>
        [DataMember]
        public LavaEndpointSecurityMode SecurityMode { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this endpoint is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this endpoint is part of the Rock core system/framework; otherwise <c>false</c>.
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
        /// Gets or sets the additional settings json.
        /// </summary>
        /// <value>
        /// The additional settings json.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the http method.
        /// </summary>
        /// <value>
        /// The http method.
        /// </value>
        [DataMember]
        public LavaEndpointHttpMethod HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the code template.
        /// </summary>
        /// <value>
        /// The code template.
        /// </value>
        [DataMember]
        public string CodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets a comma-delimited list of enabled LavaCommands
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets a cache control settings.
        /// </summary>
        /// <value>
        /// The cache control settings.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string CacheControlHeaderSettings { get; set; }

        /// <summary>
        /// Gets or sets the rate limit period in seconds.
        /// </summary>
        /// <value>
        /// The rate limit period in seconds.
        /// </value>
        [DataMember]
        public int? RateLimitPeriodDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the rate limit requests per period.
        /// </summary>
        /// <value>
        /// The rate limit requests per period.
        /// </value>
        [DataMember]
        public int? RateLimitRequestPerPeriod { get; set; }

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
                        { Authorization.EXECUTE, "The roles and/or users that have access to execute the endpoint when the application is set to custom authentication." }
                    };
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        #endregion

        #region Properties
        private RockCacheability _cacheControlHeader;
        /// <summary>
        /// Gets the cache control header.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        [NotMapped]
        public RockCacheability CacheControlHeader
        {
            get
            {
                if ( _cacheControlHeader == null && CacheControlHeaderSettings.IsNotNullOrWhiteSpace() )
                {
                    _cacheControlHeader = Newtonsoft.Json.JsonConvert.DeserializeObject<RockCacheability>( CacheControlHeaderSettings );
                }
                return _cacheControlHeader;
            }
        }
        #endregion

        #region Static Members

        /// <summary>
        /// Converts a .Net Request Method to a Helix HttpMethod
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static LavaEndpointHttpMethod GetHttpMethodFromRequestMethod( System.Net.Http.HttpMethod method )
        {
            if ( method == System.Net.Http.HttpMethod.Put )
            {
                return LavaEndpointHttpMethod.Put;
            }
            else if ( method == System.Net.Http.HttpMethod.Delete )
            {
                return LavaEndpointHttpMethod.Delete;
            }
            else if ( method == System.Net.Http.HttpMethod.Post )
            {
                return LavaEndpointHttpMethod.Post;
            }
            else
            {
                return LavaEndpointHttpMethod.Get;
            }
        }

        /// <summary>
        /// Converts a string to a Helix HttpMethod
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static LavaEndpointHttpMethod GetHttpMethodFromString( string method )
        {
            switch( method.ToLower() )
            {
                case "put":
                    {
                        return LavaEndpointHttpMethod.Put;
                    }
                case "delete":
                    {
                        return LavaEndpointHttpMethod.Delete;
                    }
                case "post":
                    {
                        return LavaEndpointHttpMethod.Post;
                    }
                default:
                    {
                        return LavaEndpointHttpMethod.Get;
                    }
            }
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Model.LavaApplication" />.
        /// </summary>
        /// <value>
        /// The lava application
        /// </value>
        [DataMember]
        public virtual LavaApplication LavaApplication { get; set; }

        #endregion Navigation

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Endpoint's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Endpoint's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// LavaEndpoint Configuration class
    /// </summary>
    public partial class LavaEndpointConfiguration : EntityTypeConfiguration<LavaEndpoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavaEndpointConfiguration"/> class.
        /// </summary>
        public LavaEndpointConfiguration()
        {
#if REVIEW_WEBFORMS
            this.HasEntitySetName( "LavaEndpoints" );
#endif
            this.HasRequired( a => a.LavaApplication ).WithMany( a => a.LavaEndpoints ).HasForeignKey( a => a.LavaApplicationId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}