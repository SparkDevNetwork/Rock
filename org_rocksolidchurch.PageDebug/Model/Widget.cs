using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;

namespace com_rocksolidchurchdemo.PageDebug.Model
{
    /// <summary>
    /// Example Model
    /// </summary>
    [Table( TableName )]
    [DataContract]
    public class Widget : Model<Widget>, IRockEntity
    {
        public const string TableName = "_com_rocksolidchurchdemo_PageDebug_Widget";

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value>
        /// The secret.
        /// </value>
        [MaxLength( 100 )]
        [Required]
        [DataMember( IsRequired = true )]
        public string ThisIsTheSecret { get; set; }

        /// <summary>
        /// Gets or sets the API threshold.
        /// </summary>
        /// <value>
        /// The API threshold.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string ThisIsTheString { get; set; }

        /// <summary>
        /// Gets or sets the API timeout.
        /// </summary>
        /// <value>
        /// The API timeout.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ThisIsTheInt { get; set; }
    }
}