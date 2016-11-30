

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;

namespace church.ccv.Promotions.Model
{
    [Table( "_church_ccv_Promotions_PromotionRequest" )]
    [DataContract]
    public class PromotionRequest : Model<PromotionRequest>, IRockEntity
    {
        [DataMember]
        public int EventItemOccurrenceId { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime? EventLastModifiedTime { get; set; }
        
        [DataMember]
        public int ContentChannelId { get; set; }

        public PromotionRequest()
        {

        }

        #region Virtual Properties
        public virtual ContentChannel ContentChannel { get; set; }

        public virtual EventItemOccurrence EventItemOccurrence { get; set; }
        #endregion
    }
}