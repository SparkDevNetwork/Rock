//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.CMS
{
    /// <summary>
    /// Page POCO Entity.
    /// </summary>
    [Table( "cmsPage" )]
    public partial class Page : ModelWithAttributes<Page>, IAuditable, IOrdered
    {
		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[TrackChanges]
		[DataMember]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		/// <value>
		/// Title.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Title { get; set; }
		
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		[Required]
		[DataMember]
		public bool IsSystem { get; set; }
		
		/// <summary>
		/// Gets or sets the Parent Page Id.
		/// </summary>
		/// <value>
		/// Parent Page Id.
		/// </value>
		[DataMember]
		public int? ParentPageId { get; set; }
		
		/// <summary>
		/// Gets or sets the Site Id.
		/// </summary>
		/// <value>
		/// Site Id.
		/// </value>
		[DataMember]
		public int? SiteId { get; set; }
		
		/// <summary>
		/// Gets or sets the Layout.
		/// </summary>
		/// <value>
		/// Layout.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Layout { get; set; }
		
		/// <summary>
		/// Gets or sets the Requires Encryption.
		/// </summary>
		/// <value>
		/// Requires Encryption.
		/// </value>
		[Required]
		[DataMember]
		public bool RequiresEncryption { get; set; }
		
		/// <summary>
		/// Gets or sets the Enable View State.
		/// </summary>
		/// <value>
		/// Enable View State.
		/// </value>
		[Required]
		[DataMember]
        public bool EnableViewState
        {
            get { return _enableViewState; }
            set { _enableViewState = value; }
        }
        private bool _enableViewState = true;
		
		/// <summary>
		/// Gets or sets the Menu Display Description.
		/// </summary>
		/// <value>
		/// Menu Display Description.
		/// </value>
		[Required]
		[DataMember]
		public bool MenuDisplayDescription { get; set; }
		
		/// <summary>
		/// Gets or sets the Menu Display Icon.
		/// </summary>
		/// <value>
		/// Menu Display Icon.
		/// </value>
		[Required]
		[DataMember]
		public bool MenuDisplayIcon { get; set; }
		
		/// <summary>
		/// Gets or sets the Menu Display Child Pages.
		/// </summary>
		/// <value>
		/// Menu Display Child Pages.
		/// </value>
		[Required]
		[DataMember]
		public bool MenuDisplayChildPages { get; set; }
		
		/// <summary>
		/// Gets or sets the Display In Nav When.
		/// </summary>
		/// <value>
		/// Determines when to display in a navigation 
		/// 0 = When Security Allows
		/// 1 = Always
		/// 3 = Never   
		/// 
		/// Enum[DisplayInNavWhen].
		/// </value>
		[Required]
		[DataMember]
        public DisplayInNavWhen DisplayInNavWhen { get; set; }

		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		[Required]
		[DataMember]
		public int Order { get; set; }
		
		/// <summary>
		/// Gets or sets the Output Cache Duration.
		/// </summary>
		/// <value>
		/// Output Cache Duration.
		/// </value>
		[Required]
		[DataMember]
		public int OutputCacheDuration { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the Include Admin Footer.
		/// </summary>
		/// <value>
		/// Include Admin Footer.
		/// </value>
		[Required]
		[DataMember]
        public bool IncludeAdminFooter
        {
            get { return _includeAdminFooter; }
            set { _includeAdminFooter = value; }
        }
        private bool _includeAdminFooter = true;
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Icon Url.
		/// </summary>
		/// <value>
		/// Icon Url.
		/// </value>
		[MaxLength( 150 )]
		[DataMember]
		public string IconUrl { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CMS.Page"; } }
        
		/// <summary>
        /// Gets or sets the Block Instances.
        /// </summary>
        /// <value>
        /// Collection of Block Instances.
        /// </value>
		public virtual ICollection<BlockInstance> BlockInstances { get; set; }
        
		/// <summary>
        /// Gets or sets the Pages.
        /// </summary>
        /// <value>
        /// Collection of Pages.
        /// </value>
		public virtual ICollection<Page> Pages { get; set; }
        
		/// <summary>
        /// Gets or sets the Page Routes.
        /// </summary>
        /// <value>
        /// Collection of Page Routes.
        /// </value>
		public virtual ICollection<PageRoute> PageRoutes { get; set; }

        /// <summary>
        /// Gets or sets the Page Contexts.
        /// </summary>
        /// <value>
        /// Collection of Page Contexts.
        /// </value>
        public virtual ICollection<PageContext> PageContexts { get; set; }

        /// <summary>
        /// Gets or sets the Sites.
        /// </summary>
        /// <value>
        /// Collection of Sites.
        /// </value>
		public virtual ICollection<Site> Sites { get; set; }
        
		/// <summary>
        /// Gets or sets the Parent Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
		public virtual Page ParentPage { get; set; }
        
		/// <summary>
        /// Gets or sets the Site.
        /// </summary>
        /// <value>
        /// A <see cref="Site"/> object.
        /// </value>
		public virtual Site Site { get; set; }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        public override List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Configure" }; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Page Configuration class.
    /// </summary>
    public partial class PageConfiguration : EntityTypeConfiguration<Page>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageConfiguration"/> class.
        /// </summary>
        public PageConfiguration()
        {
			this.HasOptional( p => p.ParentPage ).WithMany( p => p.Pages ).HasForeignKey( p => p.ParentPageId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.Site ).WithMany( p => p.Pages ).HasForeignKey( p => p.SiteId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// How should page be displayed in a page navigation block
    /// </summary>
    public enum DisplayInNavWhen
    {
        /// <summary>
        /// Display this page in navigation controls when allowed by security
        /// </summary>
        WhenAllowed = 0,

        /// <summary>
        /// Always display this page in navigation controls, regardless of security
        /// </summary>
        Always = 1,

        /// <summary>
        /// Never display this page in navigation controls
        /// </summary>
        Never = 2
    }


}
