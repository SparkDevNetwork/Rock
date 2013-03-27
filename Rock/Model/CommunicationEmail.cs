//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Communication POCO Entity.
    /// </summary>
    [Table( "CommunicationEmail" )]
    [DataContract]
    public partial class CommunicationEmail : Communication
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the sender email.
        /// </summary>
        /// <value>
        /// The sender email.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string SenderEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>
        /// The reply to email.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        /// <value>
        /// The cc.
        /// </value>
        [DataMember]
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the BCC.
        /// </summary>
        /// <value>
        /// The BCC.
        /// </value>
        [DataMember]
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember]
        public string HtmlContent { get; set; }

        /// <summary>
        /// Gets or sets the content of the text.
        /// </summary>
        /// <value>
        /// The content of the text.
        /// </value>
        [DataMember]
        public string TextContent { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the type id.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public override int TypeId
        {
            get
            {
                return Rock.Web.Cache.EntityTypeCache.Read( typeof( CommunicationEmail ) ).Id;
            }
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        public override string TypeName
        {
            get
            {
                return typeof( CommunicationEmail ).FullName;
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        /// <summary>
        /// Froms the json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static new CommunicationEmail FromJson(string json)
        {
            return JsonConvert.DeserializeObject<CommunicationEmail>(json);
        }

        #endregion

    }

    #region Entity Configuration

    #endregion

}

