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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A logical representation of a physical html layout (page).  The physical layout controls the zones that 
    /// are availble for one or more <see cref="Page">Pages</see> to use.  The logical layout is used to configure
    /// which blocks are present in each zone
    /// </summary>
    [Table( "Layout" )]
    [DataContract]
    public partial class Layout : Model<Layout>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Layout was created by and is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Block is part of the Rock core system/framework, otherwise is <c>false</c>.
        /// </value>
        /// <example>
        /// True
        /// </example>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Site"/> that this layout is associated with. 
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Site"/> that this layout is associated with.
        /// </value>
        [DataMember]
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the file name portion of the associated .Net ASCX UserControl that provides the HTML Markup and code for this Layout. 
        /// Value should not include the extension.  And the path is relative to the theme folder.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the relative path to the supporting UserControl for the Layout.
        /// </value>
        /// <example>
        /// ~/Blocks/Security/Login.ascx
        /// </example>
        [Required]
        [MaxLength( 260 )]
        [DataMember( IsRequired = true )]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the Layout.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the logical Name of the Layout. This property is required.
        /// </value>
        /// <example>
        /// Login
        /// </example>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the Layout. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Description of the Layout
        /// </value>
        /// <example>
        /// Provides ability to login to site.
        /// </example>
        [DataMember]
        public string Description { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Site" /> that this Layout Block is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Site" /> that this Layout Block is associated with.
        /// </value>
        public virtual Site Site { get; set; }

        /// <summary>
        /// Gets or sets a collection of  <see cref="Rock.Model.Page">Pages</see> that are using this Layout.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.Page">Pages</see> that use this Layout.
        /// </value>
        public virtual ICollection<Page> Pages
        {
            get { return _pages ?? ( _pages = new Collection<Page>() ); }
            set { _pages = value; }
        }
        private ICollection<Page> _pages;

        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.Block">Blocks</see> that are used on the layout.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.Block"/> entities that are used on the layout.
        /// </value>
        [DataMember]
        public virtual ICollection<Block> Blocks
        {
            get { return _blocks ?? ( _blocks = new Collection<Block>() ); }
            set { _blocks = value; }
        }
        private ICollection<Block> _blocks;

        /// <summary>
        /// Gets the parent authority for the layout. Layout security is automatically inherited from the site.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Site != null ? this.Site : base.ParentAuthority;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance. Returns the name of the Layout
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Block Type Configuration class.
    /// </summary>
    public partial class LayoutConfiguration : EntityTypeConfiguration<Layout>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutConfiguration"/> class.
        /// </summary>
        public LayoutConfiguration()
        {
            this.HasRequired( p => p.Site ).WithMany( p => p.Layouts ).HasForeignKey( p => p.SiteId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
