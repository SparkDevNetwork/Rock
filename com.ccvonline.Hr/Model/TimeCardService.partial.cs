using System;
using System.Collections.Generic;
using com.ccvonline.Hr.Data;
using Rock;
using Rock.Communication;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace com.ccvonline.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TimeCardService
    {
        /// <summary>
        /// Approves the time card.
        /// </summary>
        /// <param name="timeCardId">The time card identifier.</param>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="approvedEmailTemplateGuid">The approved email template unique identifier.</param>
        public bool ApproveTimeCard( int timeCardId, RockPage rockPage, Guid? approvedEmailTemplateGuid )
        {
            var timeCardService = this;
            var timeCard = timeCardService.Get( timeCardId );
            if ( timeCard == null )
            {
                return false;
            }

            timeCard.TimeCardStatus = TimeCardStatus.Approved;
            timeCard.ApprovedByPersonAliasId = rockPage.CurrentPersonAliasId;
            var hrContext = this.Context as HrContext;

            var timeCardHistoryService = new TimeCardHistoryService( hrContext );
            var timeCardHistory = new TimeCardHistory();
            timeCardHistory.TimeCardId = timeCard.Id;
            timeCardHistory.TimeCardStatus = timeCard.TimeCardStatus;
            timeCardHistory.StatusPersonAliasId = rockPage.CurrentPersonAliasId;
            timeCardHistory.HistoryDateTime = RockDateTime.Now;

            // NOTE: if status was already Approved, still log it as history
            timeCardHistory.Notes = string.Format( "Approved by {0}", rockPage.CurrentPersonAlias );

            timeCardHistoryService.Add( timeCardHistory );

            hrContext.SaveChanges();

            if ( approvedEmailTemplateGuid.HasValue )
            {
                var mergeObjects = GlobalAttributesCache.GetMergeFields( null );
                mergeObjects.Add( "TimeCardPayPeriod", timeCard.TimeCardPayPeriod.ToString() );
                mergeObjects.Add( "TimeCard", timeCard );
                mergeObjects.Add( "Person", timeCard.PersonAlias.Person );
                mergeObjects.Add( "ApprovedByPerson", rockPage.CurrentPerson );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( timeCard.PersonAlias.Person.Email, mergeObjects ) );
                Email.Send( approvedEmailTemplateGuid.Value, recipients, rockPage.ResolveRockUrl( "~/" ), rockPage.ResolveRockUrl( "~~/" ) );
            }

            return true;
        }
    }
}
