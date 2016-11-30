

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;

namespace church.ccv.Promotions.Model
{
    [Table( "_church_ccv_Promotions_PromotionOccurrence" )]
    [DataContract]
    public class PromotionOccurrence : Model<PromotionOccurrence>, IRockEntity
    {
        [DataMember]
        public int? PromotionRequestId { get; set; }

        [DataMember]
        public int ContentChannelItemId { get; set; }

        public PromotionOccurrence()
        {

        }

        #region Virtual Properties
        public virtual PromotionRequest PromotionRequest { get; set; }

        public virtual ContentChannelItem ContentChannelItem { get; set; }
        #endregion
    }
}