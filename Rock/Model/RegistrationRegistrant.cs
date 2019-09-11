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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationRegistrant" )]
    [DataContract]
    public partial class RegistrationRegistrant : Model<RegistrationRegistrant>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        [DataMember]
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registrant is on a wait list.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on wait list]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool OnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the registration's discount code applies to this registrant.
        /// </summary>
        /// <value>
        /// The discount applies.
        /// </value>
        [DataMember]
        public bool DiscountApplies 
        {
            get { return _discountApplies; }
            set { _discountApplies = value; }
        }
        private bool _discountApplies = true;

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [LavaInclude]
        public virtual Registration Registration { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group member.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        [LavaInclude]
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [NotMapped]
        public virtual int? PersonId
        {
            get { return PersonAlias != null ? PersonAlias.PersonId : (int?)null; }
        }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationRegistrantFee> Fees
        {
            get { return _fees ?? ( _fees = new Collection<RegistrationRegistrantFee>() ); }
            set { _fees = value; }
        }
        private ICollection<RegistrationRegistrantFee> _fees;

        /// <summary>
        /// Gets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string NickName 
        {
            get 
            {
                if ( PersonAlias != null && PersonAlias.Person != null )
                {
                    return PersonAlias.Person.NickName;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string FirstName 
        {
            get 
            {
                if ( PersonAlias != null && PersonAlias.Person != null )
                {
                    return PersonAlias.Person.FirstName;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string LastName
        {
            get 
            {
                if ( PersonAlias != null && PersonAlias.Person != null )
                {
                    return PersonAlias.Person.LastName;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string Email
        {
            get
            {
                if ( PersonAlias != null && PersonAlias.Person != null )
                {
                    return PersonAlias.Person.Email;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the cost with fees.
        /// </summary>
        /// <value>
        /// The cost with fees.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalCost
        {
            get
            {
                if ( OnWaitList )
                {
                    return 0.0M;
                }

                var cost = Cost;
                if ( Fees != null )
                {
                    cost += Fees
                        .Sum( f => f.TotalCost );
                }
                return cost;
            }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [LavaInclude]
        public virtual Person Person
        {
            get
            {
                if ( PersonAlias != null )
                {
                    return PersonAlias.Person;
                }
                return null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <param name="discountAmount">The discount amount.</param>
        /// <returns></returns>
        public virtual decimal DiscountedCost( decimal discountPercent, decimal discountAmount )
        {
            if ( OnWaitList )
            {
                return 0.0M;
            }

            var discountedCost = Cost - ( DiscountApplies ? ( Cost * discountPercent ) : 0.0M );
            if ( Fees != null )
            {
                foreach( var fee in Fees )
                {
                    discountedCost += DiscountApplies ? fee.DiscountedCost( discountPercent ) : fee.TotalCost;
                }
            }
            discountedCost = discountedCost - ( DiscountApplies ? discountAmount : 0.0M );

            return discountedCost > 0.0m ? discountedCost : 0.0m;
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var entityTypeCache = EntityTypeCache.Get( TypeId );

            // Get the registration
            var registration = this.Registration;
            if ( registration == null && this.RegistrationId > 0 )
            {
                registration = new RegistrationService( rockContext )
                    .Queryable().AsNoTracking()
                    .FirstOrDefault( r => r.Id == this.RegistrationId );
            }
            if ( entityTypeCache == null || registration == null )
            {
                return null;
            }

            // Get the instance
            var registrationInstance = registration.RegistrationInstance;
            if ( registrationInstance == null && registration.RegistrationInstanceId > 0 )
            {
                registrationInstance = new RegistrationInstanceService( rockContext )
                    .Queryable().AsNoTracking()
                    .FirstOrDefault( r => r.Id == registration.RegistrationInstanceId );
            }
            if ( registrationInstance == null )
            {
                return null;
            }

            // Get all attributes there were defined for instance's template.
            var attributes = new List<AttributeCache>();
            foreach( var entityAttributes in AttributeCache.GetByEntity( entityTypeCache.Id )
                .Where( e => 
                    e.EntityTypeQualifierColumn == "RegistrationTemplateId" &&
                    e.EntityTypeQualifierValue.AsInteger() == registrationInstance.RegistrationTemplateId ) )
            {
                foreach ( int attributeId in entityAttributes.AttributeIds )
                {
                    attributes.Add( AttributeCache.Get( attributeId ) );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PersonAlias != null && PersonAlias.Person != null )
            {
                return PersonAlias.Person.ToString();
            }
            else
            {
                return "Registrant";
            }
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationRegistrantConfiguration : EntityTypeConfiguration<RegistrationRegistrant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationRegistrantConfiguration"/> class.
        /// </summary>
        public RegistrationRegistrantConfiguration()
        {
            this.HasRequired( r => r.Registration ).WithMany( t => t.Registrants ).HasForeignKey( r => r.RegistrationId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.GroupMember ).WithMany().HasForeignKey( r => r.GroupMemberId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
