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

using Rock.Data;
using Rock.Web.Cache;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Segment Entity
    /// </summary>
    /// <seealso cref="Data.Model{TEntity}" />
    /// <seealso cref="ICacheable" />
    [RockDomain( "CMS" )]
    [Table( "RequestFilter" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "97FAC672-37A4-4185-B1D4-C68426C625B1")]
    public partial class RequestFilter : Model<RequestFilter>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the request filter key.
        /// </summary>
        /// <value>
        /// The request filter key.
        /// </value>
        [DataMember]
        public string RequestFilterKey { get; set; }

        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        [DataMember]
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the filter json.
        /// </summary>
        /// <value>
        /// The filter json.
        /// </value>
        [DataMember]
        public string FilterJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the site
        /// </summary>
        /// <value>
        /// The site reference
        /// </value>
        [DataMember]
        public virtual Site Site { get; set; }

        #endregion Navigation Properties

        #region Entity Configuration

        /// <summary>
        /// EntityCampusFilterConfiguration class
        /// </summary>
        /// <seealso cref="T:System.Data.Entity.ModelConfiguration.EntityTypeConfiguration{Rock.Model.RequestFilter}" />
        public class RequestFilterConfiguration : EntityTypeConfiguration<RequestFilter>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RequestFilterConfiguration"/> class.
            /// </summary>
            public RequestFilterConfiguration()
            {
                HasOptional( e => e.Site ).WithMany().HasForeignKey( e => e.SiteId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration
    }
}
