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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "MetricCategory" )]
    [DataContract]
    public class MetricCategory : Entity<MetricCategory>, IOrdered, ICategorized
    {
        /// <summary>
        /// Gets or sets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        [DataMember]
        [Index( "IX_MetricCategory", 0 )]
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        [Index( "IX_MetricCategory", 1 )]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        [DataMember]
        public virtual Metric Metric { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public virtual string IconCssClass 
        { 
            get
            {
                return this.Metric.IconCssClass;
            }
        }

        #region ICategorized
        
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name
        {
            get 
            { 
                return Metric.Title; 
            }
        }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        int? ICategorized.CategoryId
        {
            get 
            { 
                return this.CategoryId; 
            }
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public virtual Security.ISecured ParentAuthority
        {
            get 
            { 
                return this.Metric.ParentAuthority; 
            }
        }

        /// <summary>
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        public virtual Security.ISecured ParentAuthorityPre
        {
            get { return null; }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public virtual System.Collections.Generic.Dictionary<string, string> SupportedActions
        {
            get 
            { 
                return this.Metric.SupportedActions; 
            }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Person person )
        {
            if ( this.Metric != null )
            {
                return this.Metric.IsAuthorized( action, person );
            }
            else
            {
                return action == Rock.Security.Authorization.VIEW;
            }
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool IsAllowedByDefault( string action )
        {
            return this.Metric.IsAllowedByDefault( action );
        }

        /// <summary>
        /// Determines whether the specified action is private (Only the current user has access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPrivate( string action, Person person )
        {
            return this.Metric.IsPrivate( action, person );
        }

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            this.Metric.MakePrivate( action, person, rockContext );
        }

        /// <summary>
        /// If action on the current entity is private, removes security that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            this.Metric.MakeUnPrivate( action, person, rockContext );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class MetricCategoryConfiguration : EntityTypeConfiguration<MetricCategory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricCategoryConfiguration"/> class.
        /// </summary>
        public MetricCategoryConfiguration()
        {
            this.HasRequired( a => a.Metric ).WithMany( a => a.MetricCategories );
            this.HasRequired( a => a.Category ).WithMany();
        }
    }

    #endregion
}