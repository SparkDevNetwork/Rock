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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// PageView Model Entity. A PageView in Rock is a log of each time a page was visited
    /// </summary>
    [Table( "PageView" )]
    [DataContract]
    [NotAudited]
    public partial class PageView : Entity<PageView>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the page view session identifier.
        /// </summary>
        /// <value>
        /// The page view session identifier.
        /// </value>
        [DataMember]
        public int PageViewSessionId { get; set; }

        /// <summary>
        /// Gets or sets the page id of the page viewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> the id of the page viewed.
        /// </value>
        /// 
        [DataMember]
        [IgnoreCanDelete]
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the site id of the page viewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> the id of the site viewed.
        /// </value>
        [DataMember]
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the person alias id of the person who viewed the page.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> the id of the person alias who viewed the page.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date and time the page was viewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> the date and time the page was viewed.
        /// </value>
        [Required]
        [DataMember]
        [Index( "IX_DateTimeViewed", IsUnique = false )]
        public DateTime? DateTimeViewed { get; set; }

        /// <summary>
        /// Gets or sets the query string of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the query string of the request.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the page title of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the page title of the request.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string PageTitle { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the page view session.
        /// </summary>
        /// <value>
        /// The page view session.
        /// </value>
        public virtual PageViewSession PageViewSession { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Page"/> page that was viewed.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Page"/> that was viewed. 
        /// </value>
        public virtual Page Page { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Site"/> that the page is a part of
        /// </summary>
        /// <value>
        /// The site the page is a part of
        /// </value>
        public virtual Site Site { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// PageView Configuration class.
    /// </summary>
    public partial class PageViewConfiguration : EntityTypeConfiguration<PageView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteConfiguration"/> class.
        /// </summary>
        public PageViewConfiguration()
        {
            this.HasRequired( p => p.PageViewSession ).WithMany().HasForeignKey( p => p.PageViewSessionId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Page ).WithMany().HasForeignKey( p => p.PageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Site ).WithMany().HasForeignKey( p => p.SiteId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
