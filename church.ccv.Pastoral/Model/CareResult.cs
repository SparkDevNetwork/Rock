using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace church.ccv.Pastoral.Model
{
    /// <summary>
    /// Represents a result of a Care Request that a person has submitted.
    /// </summary>
    [Table( "_church_ccv_Pastoral_CareResult" )]
    [DataContract]
    public partial class CareResult : Model<CareResult>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the Care Request the result is a result of.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the Id of the Care Request the result is a result of.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int CareRequestId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value <see cref="church.ccv.Utility.DefinedValue"/> representing the type of Care Result.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the type of Care Result.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [DefinedValue( church.ccv.Utility.SystemGuids.DefinedType.CARE_RESULT_TYPE )]
        public int ResultTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the cost amount of care
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the cost amount of care.
        /// </value>
        [DataMember]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the text of the result details.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the details of the result.
        /// </value>
        [DataMember]
        public string ResultSummary { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Care Request.
        /// </summary>
        /// <value>
        /// The Care Request.
        /// </value>
        [DataMember]
        public virtual CareRequest CareRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the type of Care Result.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the type of Care Result.
        /// </value>
        [DataMember]
        public virtual DefinedValue ResultTypeValue { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// BenevolenceResult Configuration class.
    /// </summary>
    public partial class CareResultConfiguration : EntityTypeConfiguration<CareResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareResultConfiguration"/> class.
        /// </summary>
        public CareResultConfiguration()
        {
            this.HasRequired( p => p.CareRequest ).WithMany( p => p.CareResults ).HasForeignKey( p => p.CareRequestId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.ResultTypeValue ).WithMany().HasForeignKey( p => p.ResultTypeValueId ).WillCascadeOnDelete( false );

        }
    }

    #endregion
}