using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.Datamart.Model
{
    [Table( "_church_ccv_Datamart_EraLoss" )]
    [DataContract]
    public partial class DatamartEraLoss : Rock.Data.Entity<DatamartEraLoss>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>
        /// The family identifier.
        /// </value>
        [DataMember]
        public int FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the loss date.
        /// </summary>
        /// <value>
        /// The loss date.
        /// </value>
        [DataMember]
        public DateTime LossDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DatamartEraLoss"/> is processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Processed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send email].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send email]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DatamartEraLoss"/> is sent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sent; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Sent { get; set; }

        #endregion
    }
}
