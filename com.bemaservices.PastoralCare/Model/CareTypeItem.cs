
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Security;
using com.bemaservices.PastoralCare.Model;

namespace com.bemaservices.PastoralCare.Model
{
    [RockDomain( "BEMA Services > Care" )]
    [Table( "_com_bemaservices_PastoralCare_CareTypeItem" )]
    [DataContract]
    public partial class CareTypeItem : Model<CareTypeItem>, ISecured
    {
        [Required]
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int CareTypeId { get; set; }

        [Required]
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int CareItemId { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventCalendar"/> that this EventCalendarItem is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventCalendar"/> that this EventCalendarItem is a member of.
        /// </value>
        [LavaInclude]
        public virtual CareType CareType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> that this EventCalendarItem is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventItem"/> that this EventCalendarItem is a member of.
        /// </value>
        [LavaInclude]
        public virtual CareItem CareItem { get; set; }

        #endregion

        #region Methods

        public override ISecured ParentAuthority
        {
            get
            {
                return this.CareType != null ? this.CareType : base.ParentAuthority;
            }
        }
        #endregion

    }

    #region Entity Configuration

    public partial class CareTypeItemConfiguration : EntityTypeConfiguration<CareTypeItem>
    {
        public CareTypeItemConfiguration()
        {
            this.HasRequired( p => p.CareType ).WithMany( p => p.CareTypeItems ).HasForeignKey( p => p.CareTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.CareItem ).WithMany( p => p.CareTypeItems ).HasForeignKey( p => p.CareItemId ).WillCascadeOnDelete( true );
            
            // IMPORTANT!!
            this.HasEntitySetName( "CareTypeItem" );
        }
    }

    #endregion
}