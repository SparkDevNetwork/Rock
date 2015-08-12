using com.centralaz.Accountability.Data;
using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

namespace com.centralaz.Accountability.Model
{
    /// <summary>
    /// A Response Set for an Accountability Group Report
    /// </summary>
    [Table("_com_centralaz_Accountability_ResponseSet")]
    [DataContract]
    public class ResponseSet : NamedModel<ResponseSet>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the response set comment.
        /// </summary>
        /// <value>
        /// The response set comment.
        /// </value>
        [MaxLength(4000)]
        [DataMember]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the "submitted for" date.
        /// </summary>
        /// <value>
        /// The date the response set was submitted for.
        /// </value>
        [DataMember]
        public DateTime SubmitForDate { get; set; }

        /// <summary>
        /// Gets or sets the response set score.
        /// </summary>
        /// <value>
        /// The response set score.
        /// </value>
        [DataMember]
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [DataMember]
        public int PersonId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the responseset's responses
        /// </summary>
        [DataMember]
        public virtual ICollection<Response> Responses
        {
            get { return _responses ?? ( _responses = new Collection<Response>() ); }
            set { _responses = value; }
        }
        private ICollection<Response> _responses;

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ResponseSetConfiguration : EntityTypeConfiguration<ResponseSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseSetConfiguration"/> class.
        /// </summary>
        public ResponseSetConfiguration()
        {
            this.HasRequired(r => r.Group).WithMany().HasForeignKey(r => r.GroupId).WillCascadeOnDelete(false);
            this.HasRequired(r => r.Person).WithMany().HasForeignKey(r => r.PersonId).WillCascadeOnDelete(false);
            this.HasMany( r => r.Responses ).WithRequired( r => r.ResponseSet ).HasForeignKey( r => r.ResponseSetId );
        }
    }

    #endregion

}
