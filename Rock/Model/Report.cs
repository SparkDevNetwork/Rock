//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Campus POCO Entity.
    /// </summary>
    [Table( "Report" )]
    [DataContract( IsReference = true )]
    public partial class Report : Model<Report>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System indicates whether or not the campus is part of the core framework/system.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Given Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Notes about the job..
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the root filter id.
        /// </summary>
        /// <value>
        /// The root filter id.
        /// </value>
        [DataMember]
        public int? ReportFilterId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the report filter.
        /// </summary>
        /// <value>
        /// The report filter.
        /// </value>
        [DataMember]
        public virtual ReportFilter ReportFilter { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public Expression GetExpression( ParameterExpression parameter )
        {
            if ( ReportFilter != null )
            {
                return ReportFilter.GetExpression( parameter );
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class ReportConfiguration : EntityTypeConfiguration<Report>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        public ReportConfiguration()
        {
            this.HasOptional( r => r.ReportFilter).WithMany().HasForeignKey( r => r.ReportFilterId).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
