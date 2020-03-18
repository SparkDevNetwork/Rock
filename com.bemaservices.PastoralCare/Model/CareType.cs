using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Web.Cache;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Security;
using Rock.Transactions;
using Rock;

namespace com.bemaservices.PastoralCare.Model
{
    [RockDomain( "BEMA Services > Care" )]
    [Table( "_com_bemaservices_PastoralCare_CareType" )]
    [DataContract]
    public partial class CareType : Rock.Data.Model<CareType>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Name of the CareType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the CareType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the CareType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the CareType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual ICollection<CareTypeItem> CareTypeItems
        {
            get { return _careTypeItems ?? ( _careTypeItems = new Collection<CareTypeItem>() ); }
            set { _careTypeItems = value; }
        }
        private ICollection<CareTypeItem> _careTypeItems;

        [LavaInclude]
        public virtual int CareItemCount
        {
            get
            {
                return CareItemQuery.Count();
            }
        }

        /// <summary>Gets the care item query.</summary>
        /// <value>The care item query.</value>
        public virtual IQueryable<CareItem> CareItemQuery
        {
            get
            {
                var careItemService = new CareItemService( new RockContext() );
                var qry = careItemService.Queryable().Where( a => a.CareTypeItems.Any(cit=> cit.CareTypeId == this.Id) );
                return qry;
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                }
                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        #endregion

        #region Public Methods      

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the CareType that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the name of the CareType that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        public List<AttributeCache> GetInheritedAttributesForQualifier( Rock.Data.RockContext rockContext, int entityTypeId, string entityTypeQualifierColumn )
        {
            var careTypeId = this.Id;

            var inheritedAttributes = new List<AttributeCache>();

            var entityAttributesList = new List<EntityAttributes>();

            var allEntityAttributes = EntityAttributesCache.Get();
            if ( allEntityAttributes != null )
            {
                List<EntityAttributes> result;
                if ( entityTypeId != null )
                {
                    result = allEntityAttributes.EntityAttributesByEntityTypeId.GetValueOrNull( entityTypeId ) ?? new List<EntityAttributes>();
                }
                else
                {
                    result = allEntityAttributes.EntityAttributes.Where( a => !a.EntityTypeId.HasValue ).ToList();
                }

                entityAttributesList = result;
            }

            foreach ( var entityAttributes in entityAttributesList )
            {
                // group type ids exist and qualifier is for a group type id
                if ( string.Compare( entityAttributes.EntityTypeQualifierColumn, entityTypeQualifierColumn, true ) == 0 )
                {
                    int careTypeIdValue = int.MinValue;
                    if ( int.TryParse( entityAttributes.EntityTypeQualifierValue, out careTypeIdValue ) && careTypeIdValue == careTypeId )
                    {
                        foreach ( int attributeId in entityAttributes.AttributeIds )
                        {
                            inheritedAttributes.Add( AttributeCache.Get( attributeId ) );
                        }
                    }
                }
            }

            return inheritedAttributes.OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            return GetInheritedAttributesForQualifier( rockContext, TypeId, "Id" );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Group Type Configuration class.
    /// </summary>
    public partial class CareTypeConfiguration : EntityTypeConfiguration<CareType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareTypeConfiguration"/> class.
        /// </summary>
        public CareTypeConfiguration()
        {
            // IMPORTANT!!
            this.HasEntitySetName( "CareType" );
        }
    }

    #endregion
}
