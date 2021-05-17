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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "RestAction" )]
    [DataContract]
    public partial class RestAction : Model<RestAction>, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the controller identifier.
        /// </summary>
        /// <value>
        /// The controller identifier.
        /// </value>
        [NotAudited]
        [Required]
        [DataMember( IsRequired = true )]
        public int ControllerId { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the api identifier.
        /// </summary>
        /// <value>
        /// The rest identifier.
        /// </value>
        [MaxLength( 2000 )]
        [DataMember]
        public string ApiId { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [MaxLength( 2000 )]
        [DataMember]
        public string Path { get; set; }

        private string _cacheControlHeaderSettings;
        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        /// <value>
        /// The cache control header settings.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string CacheControlHeaderSettings
        {
            get => _cacheControlHeaderSettings;
            set
            {
                if ( _cacheControlHeaderSettings != value )
                {
                    _cacheControlHeader = null;
                }
                _cacheControlHeaderSettings = value;
            }
        }

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

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        [LavaVisible]
        public virtual RestController Controller { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get 
            {
                return this.Controller != null ? this.Controller : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this RestAction.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this RestAction.
        /// </returns>
        public override string ToString()
        {
            return this.Method;
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RestActionCache.Get( this.ApiId );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RestActionCache.UpdateCachedEntity( this.ApiId, entityState );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Defined Type Configuration class.
    /// </summary>
    public partial class RestActionConfiguration : EntityTypeConfiguration<RestAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestActionConfiguration"/> class.
        /// </summary>
        public RestActionConfiguration()
        {
            this.HasRequired( a => a.Controller).WithMany( c => c.Actions).HasForeignKey( a => a.ControllerId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
