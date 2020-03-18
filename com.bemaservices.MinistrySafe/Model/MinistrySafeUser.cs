using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;
namespace com.bemaservices.MinistrySafe.Model
{
    [Table( "_com_bemaservices_MinistrySafe_MinistrySafeUser" )]
    [DataContract]
    public class MinistrySafeUser : Rock.Data.Model<MinistrySafeUser>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public int PersonAliasId { get; set; }

        [DataMember]
        public int? Score { get; set; }

        [DataMember]
        public string UserType { get; set; }

        [DataMember]
        public string SurveyCode { get; set; }

        [DataMember]
        public string DirectLoginUrl { get; set; }

        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [DataMember]
        public int? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the response date.
        /// </summary>
        /// <value>
        /// The response date.
        /// </value>
        [DataMember]
        public DateTime? ResponseDate { get; set; }

        #endregion

        #region Virtual Properties
        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        [LavaInclude]
        public virtual Rock.Model.Workflow Workflow { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class MinistrySafeUserConfiguration : EntityTypeConfiguration<MinistrySafeUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinistrySafeUserConfiguration"/> class.
        /// </summary> 
        public MinistrySafeUserConfiguration()
        {
            this.HasRequired( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Workflow ).WithMany().HasForeignKey( p => p.WorkflowId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "MinistrySafeUser" );
        }
    }

    #endregion

}