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
    /// A Question asked for the Accountability report
    /// </summary>
    [Table("_com_centralaz_Accountability_Question")]
    [DataContract]
    public class Question : NamedModel<Question>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the short form of the question.
        /// </summary>
        /// <value>
        /// The short form of the question.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string ShortForm { get; set; }

        /// <summary>
        /// Gets or sets the long form of the question.
        /// </summary>
        /// <value>
        /// The long form of the question.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string LongForm { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int GroupTypeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group type.
        /// </summary>
        /// <value>
        /// The group type.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class QuestionConfiguration : EntityTypeConfiguration<Question>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionConfiguration"/> class.
        /// </summary>
        public QuestionConfiguration()
        {
            this.HasRequired(r => r.GroupType).WithMany().HasForeignKey(r => r.GroupTypeId).WillCascadeOnDelete(false);
        }
    }

    #endregion

}
