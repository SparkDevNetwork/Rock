// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// PageViewSession Model Entity.
    /// </summary>
    [Table( "PageViewSession" )]
    [DataContract]
    [NotAudited]
    public partial class PageViewSession : Entity<PageViewSession>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the page view user agent identifier.
        /// </summary>
        /// <value>
        /// The page view user agent identifier.
        /// </value>
        public int PageViewUserAgentId { get; set; }

        /// <summary>
        /// Gets or sets the session identifier that ASP.NET assigned to this session
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        [DataMember]
        [Index( "IX_SessionId", IsUnique = false )]
        public Guid? SessionId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the IP address of the request.
        /// </value>
        [DataMember]
        [MaxLength( 45 )]
        public string IpAddress { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the page view user agent.
        /// </summary>
        /// <value>
        /// The page view user agent.
        /// </value>
        public virtual PageViewUserAgent PageViewUserAgent { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Site Configuration class.
    /// </summary>
    public partial class PageViewSessionConfiguration : EntityTypeConfiguration<PageViewSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteConfiguration"/> class.
        /// </summary>
        public PageViewSessionConfiguration()
        {
            this.HasRequired( p => p.PageViewUserAgent ).WithMany().HasForeignKey( p => p.PageViewUserAgentId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
