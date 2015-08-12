using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using com.centralaz.Accountability.Data;

using Rock.Data;
using Rock.Model;

namespace com.centralaz.Accountability.Model
{
    /// <summary>
    /// A Response to an Accountability Group Question
    /// </summary>
    [Table("_com_centralaz_Accountability_Response")]
    [DataContract]
    public class Response : NamedModel<Response>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the response yes/no option.
        /// </summary>
        /// <value>
        /// The response yes/no option.
        /// </value>
        [DataMember]
        public bool IsResponseYes { get; set; }

        /// <summary>
        /// Gets or sets the comment of the response.
        /// </summary>
        /// <value>
        /// The comment of the response.
        /// </value>
        [MaxLength(300)]
        [DataMember]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the question identifier.
        /// </summary>
        /// <value>
        /// The question identifier.
        /// </value>
        [DataMember]
        public int QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the response set identifier.
        /// </summary>
        /// <value>
        /// The response set identifier.
        /// </value>
        [DataMember]
        public int ResponseSetId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group type.
        /// </summary>
        /// <value>
        /// The group type.
        /// </value>
        public virtual Question Question { get; set; }

        /// <summary>
        /// Gets or sets the group type.
        /// </summary>
        /// <value>
        /// The group type.
        /// </value>
        public virtual ResponseSet ResponseSet { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ResponseConfiguration : EntityTypeConfiguration<Response>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseConfiguration"/> class.
        /// </summary>
        public ResponseConfiguration()
        {
            this.HasRequired(r => r.Question).WithMany().HasForeignKey(r => r.QuestionId).WillCascadeOnDelete(false);
            this.HasRequired(r => r.ResponseSet).WithMany().HasForeignKey(r => r.ResponseSetId).WillCascadeOnDelete(false);
        }
    }

    #endregion

}
