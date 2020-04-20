using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rock.Data;
using Rock.Web.Cache;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Security;
using Rock.Model;
using Rock;

namespace com.bemaservices.PastoralCare.Model
{
    [RockDomain( "BEMA Services > Care" )]
    [Table( "_com_bemaservices_PastoralCare_CareItem" )]
    [DataContract]
    public partial class CareItem : Rock.Data.Model<CareItem>, Rock.Data.IRockEntity
    {
        #region Entity Properties    

        [DataMember]
        public bool IsActive { get; set; }

        [Required]
        [DataMember]
        public int? PersonAliasId { get; set; }

        [Required]
        [DataMember]
        public int? ContactorPersonAliasId { get; set; }

        [Required]
        [DataMember]
        public DateTime ContactDateTime { get; set; }

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
        public virtual PersonAlias PersonAlias { get; set; }

        [LavaInclude]
        public virtual PersonAlias ContactorPersonAlias { get; set; }

        public virtual ICollection<CareContact> CareContacts
        {
            get { return _careContacts ?? ( _careContacts = new Collection<CareContact>() ); }
            set { _careContacts = value; }
        }

        private ICollection<CareContact> _careContacts;
        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var careTypeIds = this.CareTypeItems.Select( c => c.CareTypeId ).ToList();
            if ( !careTypeIds.Any() )
            {
                return null;
            }

            var inheritedAttributes = new Dictionary<int, List<AttributeCache>>();
            careTypeIds.ForEach( c => inheritedAttributes.Add( c, new List<AttributeCache>() ) );


            var careTypeItemEntityType = EntityTypeCache.Get( typeof( CareTypeItem ) );
            if ( careTypeItemEntityType != null )
            {
                foreach ( var careTypeItemEntityAttributes in GetByEntity( careTypeItemEntityType.Id )
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "CareTypeId" &&
                        careTypeIds.Contains( a.EntityTypeQualifierValue.AsInteger() ) ) )
                {
                    foreach ( var attributeId in careTypeItemEntityAttributes.AttributeIds )
                    {
                        inheritedAttributes[careTypeItemEntityAttributes.EntityTypeQualifierValue.AsInteger()].Add(
                            AttributeCache.Get( attributeId ) );
                    }
                }
            }

            var attributes = new List<AttributeCache>();
            foreach ( var attributeGroup in inheritedAttributes )
            {
                foreach ( var attribute in attributeGroup.Value.OrderBy( a => a.Order ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Get any alternate Ids that should be used when loading attribute value for this entity.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns>
        /// A list of any alternate entity Ids that should be used when loading attribute values.
        /// </returns>
        public override List<int> GetAlternateEntityIds( RockContext rockContext )
        {
            //
            // Find all the calendar Ids this event item is present on.
            //
            return this.CareTypeItems.Select( c => c.Id ).ToList();
        }

        private static List<EntityAttributes> GetByEntity( int? entityTypeId )
        {
            var allEntityAttributes = EntityAttributesCache.Get();
            if ( allEntityAttributes != null )
            {
                List<EntityAttributes> result;
                if ( entityTypeId.HasValue )
                {
                    result = allEntityAttributes.EntityAttributesByEntityTypeId.GetValueOrNull( entityTypeId.Value ) ?? new List<EntityAttributes>();
                }
                else
                {
                    result = allEntityAttributes.EntityAttributes.Where( a => !a.EntityTypeId.HasValue ).ToList();
                }

                return result;
            }

            return new List<EntityAttributes>();
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
        /// Returns a <see cref="System.String" /> containing the Name of the CareItem that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the CareItem that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return PersonAlias?.Person?.ToString();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// CareItem Configuration class.
    /// </summary>
    public partial class CareItemConfiguration : EntityTypeConfiguration<CareItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareItemConfiguration"/> class.
        /// </summary>
        public CareItemConfiguration()
        {
            // IMPORTANT!!
            this.HasEntitySetName( "CareItem" );
        }
    }

    #endregion
}
