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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Site Model Entity. A Site in Rock is a collection of <see cref="Page">pages</see> and usually 
    /// associated with one or more <see cref="SiteDomain">SiteDomains </see>.
    /// </summary>
    [Table( "Site" )]
    [DataContract]
    public partial class Site : Model<Site>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Site was created by and is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Site is part of the Rock core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the Site. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the Site.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description/summary  of the Site.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the description of the Site. If no description is provided
        ///  this value will be an null string.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the Theme that is used on the Site.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the name of the theme that is being used on the Site.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Theme { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the Site's default <see cref="Rock.Model.Page"/>. 
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when the user browses to the root of one of the Site's <see cref="Rock.Model.SiteDomain"/>
        /// (i.e. http://www.mychurchdomain.org/) without providing a page or route. 
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's default <see cref="Rock.Model.Page"/>. If the site doesn't have a default <see cref="Rock.Model.Page"/>
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? DefaultPageId { get; set; }

        /// <summary>
        /// Gets or sets the default page route unique identifier.
        /// If this has a value (and the PageRoute can be found) use this instead of the DefaultPageId
        /// </summary>
        /// <value>
        /// The default page route unique identifier.
        /// </value>
        [DataMember]
        public int? DefaultPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Site's login <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when the user is not logged in and they browse to a page or block that requires authentication.
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's login <see cref="Rock.Model.Page"/>. If the site doesn't have a login <see cref="Rock.Model.Page"/>
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? LoginPageId { get; set; }

        /// <summary>
        /// Gets or sets the login page route unique identifier.
        /// If this has a value (and the PageRoute can be found) use this instead of the LoginPageId
        /// </summary>
        /// <value>
        /// The login page route unique identifier.
        /// </value>
        [DataMember]
        public int? LoginPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Site's registration <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when the user requests to register for a <see cref="Rock.Model.Group"/>.
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's registration <see cref="Rock.Model.Page"/>. If the site doesn't have a registration <see cref="Rock.Model.Page"/>
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? RegistrationPageId { get; set; }

        /// <summary>
        /// Gets or sets the registration page route unique identifier.
        /// If this has a value (and the PageRoute can be found) use this instead of the RegistrationPageId
        /// </summary>
        /// <value>
        /// The registration page route unique identifier.
        /// </value>
        [DataMember]
        public int? RegistrationPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the 404 <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <remarks>
        /// This is the <see cref="Rock.Model.Page"/> that is loaded when a page is not found.
        /// </remarks>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the Site's 404 <see cref="Rock.Model.Page"/>. 
        /// </value>
        [DataMember]
        public int? PageNotFoundPageId { get; set; }

        /// <summary>
        /// Gets or sets the 404 page route unique identifier.
        /// </summary>
        /// <value>
        /// The 404 page route unique identifier.
        /// </value>
        [DataMember]
        public int? PageNotFoundPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the communication page identifier.
        /// </summary>
        /// <value>
        /// The communication page identifier.
        /// </value>
        [DataMember]
        public int? CommunicationPageId { get; set; }

        /// <summary>
        /// Gets or sets the communication page route identifier.
        /// </summary>
        /// <value>
        /// The communication page route identifier.
        /// </value>
        [DataMember]
        public int? CommunicationPageRouteId { get; set; }

        /// <summary>
        /// Gets or sets the path to the error page.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the path to the error page.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string ErrorPage { get; set; }

        /// <summary>
        /// Gets or sets the google analytics code.
        /// </summary>
        /// <value>
        /// The google analytics code.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string GoogleAnalyticsCode { get; set; }

        /// <summary>
        /// Gets or sets the Site's Facebook AppId for utilizing the Facebook SDK.
        /// </summary>
        /// <remarks>
        /// Each site that utilizes the Facebook SDK requires a different AppId. More info is available at http://developer.facebook.com
        /// </remarks>
        /// <value>
        /// Facebook App Id.
        /// </value>
        [MaxLength( 25 )]
        [DataMember]
        public string FacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets the site's App Secret for the Facebook API
        /// </summary>
        /// <value>
        /// Facebook App Secret.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string FacebookAppSecret { get; set; }
        
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.Layout"/> entities that are a part of the Site.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Layout"/> entities that are a part of the site.
        /// </value>
        [DataMember]
        public virtual ICollection<Layout> Layouts { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.SiteDomain"/> entities that reference the Site.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.SiteDomain"/> entities that reference the Site.
        /// </value>
        [DataMember]
        public virtual ICollection<SiteDomain> SiteDomains { get; set; }
        
        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The default <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page DefaultPage { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the DefaultPage will be used
        /// </summary>
        /// <value>
        /// The default page route.
        /// </value>
        [DataMember]
        public virtual PageRoute DefaultPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the login <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The login <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page LoginPage { get; set; }

        /// <summary>
        /// Gets or sets the login <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the LoginPage will be used
        /// </summary>
        /// <value>
        /// The login page route.
        /// </value>
        [DataMember]
        public virtual PageRoute LoginPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the registration <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The registration <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page RegistrationPage { get; set; }

        /// <summary>
        /// Gets or sets the registration <see cref="Rock.Model.PageRoute"/> page route for this site. If this value is null, the RegistrationPage will be used
        /// </summary>
        /// <value>
        /// The registration page route.
        /// </value>
        [DataMember]
        public virtual PageRoute RegistrationPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the 404 <see cref="Rock.Model.Page"/> page for the site.
        /// </summary>
        /// <value>
        /// The 404 <see cref="Rock.Model.Page"/> for the site. 
        /// </value>
        [DataMember]
        public virtual Page PageNotFoundPage { get; set; }

        /// <summary>
        /// Gets or sets the 404 <see cref="Rock.Model.PageRoute"/> page route for this site. 
        /// </summary>
        /// <value>
        /// The registration page route.
        /// </value>
        [DataMember]
        public virtual PageRoute PageNotFoundPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the communication page.
        /// </summary>
        /// <value>
        /// The communication page.
        /// </value>
        [DataMember]
        public virtual Page CommunicationPage { get; set; }

        /// <summary>
        /// Gets or sets the communication page route.
        /// </summary>
        /// <value>
        /// The communication page route.
        /// </value>
        [DataMember]
        public virtual PageRoute CommunicationPageRoute { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the site that that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the site that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var result = base.SupportedActions;;
                result.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve. Used as a base for blocks that use the approve action." );
                return result;
            }
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Site Configuration class.
    /// </summary>
    public partial class SiteConfiguration : EntityTypeConfiguration<Site>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteConfiguration"/> class.
        /// </summary>
        public SiteConfiguration()
        {
            this.HasOptional( p => p.DefaultPage ).WithMany().HasForeignKey( p => p.DefaultPageId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.DefaultPageRoute ).WithMany().HasForeignKey( p => p.DefaultPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.LoginPage ).WithMany().HasForeignKey( p => p.LoginPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.LoginPageRoute ).WithMany().HasForeignKey( p => p.LoginPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RegistrationPage ).WithMany().HasForeignKey( p => p.RegistrationPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RegistrationPageRoute ).WithMany().HasForeignKey( p => p.RegistrationPageRouteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PageNotFoundPage).WithMany().HasForeignKey(p => p.PageNotFoundPageId).WillCascadeOnDelete(false);
            this.HasOptional( p => p.PageNotFoundPageRoute).WithMany().HasForeignKey(p => p.PageNotFoundPageRouteId).WillCascadeOnDelete(false);
            this.HasOptional( p => p.CommunicationPage ).WithMany().HasForeignKey( p => p.CommunicationPageId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CommunicationPageRoute ).WithMany().HasForeignKey( p => p.CommunicationPageRouteId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
