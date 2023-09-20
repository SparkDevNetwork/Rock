// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;

namespace org.secc.RecurringCommunications.Model
{
    [Table( "_org_secc_RecurringCommunications_RecurringCommunication" )]
    [DataContract]
    public class RecurringCommunication : Model<RecurringCommunication>, ISecured, IRockEntity
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int DataViewId { get; set; }
        [LavaVisible]
        public virtual DataView DataView { get; set; }
        [DataMember]
        public int ScheduleId { get; set; }
        [LavaVisible]
        public virtual Schedule Schedule { get; set; }
        [DataMember]
        public DateTime? LastRunDateTime { get; set; }
        [DataMember]
        public CommunicationType CommunicationType { get; set; }
        [DataMember]
        public string FromName { get; set; }
        [DataMember]
        public string FromEmail { get; set; }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public string EmailBody { get; set; }
        [DataMember]
        public string SMSBody { get; set; }
        [DataMember]
        public int? PhoneNumberValueId { get; set; }
        [LavaVisible]
        public virtual DefinedValue PhoneNumberValue { get; set; }
        [DataMember]
        public string PushMessage { get; set; }
        [DataMember]
        public string PushTitle { get; set; }
        [DataMember]
        public string PushSound { get; set; }
        [DataMember]
        public string ScheduleDescription { get; set; }
        [DataMember]
        public int? TransformationEntityTypeId { get; set; }
        [LavaVisible]
        public virtual EntityType TransformationEntityType { get; set; }
    }

    public partial class RecurringCommunicationConfiguration : EntityTypeConfiguration<RecurringCommunication>
    {
        public RecurringCommunicationConfiguration()
        {
            this.HasRequired( rc => rc.DataView ).WithMany().HasForeignKey( dv => dv.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( rc => rc.PhoneNumberValue ).WithMany().HasForeignKey( dv => dv.PhoneNumberValueId ).WillCascadeOnDelete( false );
            this.HasOptional(rc => rc.TransformationEntityType).WithMany().HasForeignKey(et => et.TransformationEntityTypeId).WillCascadeOnDelete(false);
            this.HasEntitySetName( "RecurringCommunications" );
        }
    }

}
