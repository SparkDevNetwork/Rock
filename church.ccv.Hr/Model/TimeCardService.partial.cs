using System;
using System.Collections.Generic;
using System.Linq;
using church.ccv.Hr.Data;
using Rock;
using Rock.Communication;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace church.ccv.Hr.Model
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
            timeCard.ApprovedDateTime = RockDateTime.Now;
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
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "TimeCardPayPeriod", timeCard.TimeCardPayPeriod.ToString() );
                mergeFields.Add( "TimeCard", timeCard );
                mergeFields.Add( "Person", timeCard.PersonAlias.Person );
                mergeFields.Add( "ApprovedByPerson", rockPage.CurrentPerson );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( timeCard.PersonAlias.Person.Email, mergeFields ) );
                Email.Send( approvedEmailTemplateGuid.Value, recipients, rockPage.ResolveRockUrl( "~/" ), rockPage.ResolveRockUrl( "~~/" ) );
            }

            return true;
        }

        /// <summary>
        /// Un-approves time card.
        /// </summary>
        /// <param name="timeCardId">The time card identifier.</param>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="unapprovedEmailTemplateGuid">The unapproved email template unique identifier.</param>
        /// <returns></returns>
        public bool UnApproveTimeCard( int timeCardId, RockPage rockPage, Guid? unapprovedEmailTemplateGuid )
        {
            var timeCardService = this;
            var timeCard = timeCardService.Get( timeCardId );
            if ( timeCard == null )
            {
                return false;
            }

            timeCard.TimeCardStatus = TimeCardStatus.InProgress;
            timeCard.ApprovedByPersonAliasId = null;
            timeCard.ApprovedDateTime = null;
            var hrContext = this.Context as HrContext;

            var timeCardHistoryService = new TimeCardHistoryService( hrContext );
            var timeCardHistory = new TimeCardHistory();
            timeCardHistory.TimeCardId = timeCard.Id;
            timeCardHistory.TimeCardStatus = timeCard.TimeCardStatus;
            timeCardHistory.StatusPersonAliasId = rockPage.CurrentPersonAliasId;
            timeCardHistory.HistoryDateTime = RockDateTime.Now;

            timeCardHistory.Notes = string.Format( "Un-approved by {0}", rockPage.CurrentPersonAlias );

            timeCardHistoryService.Add( timeCardHistory );

            hrContext.SaveChanges();

            if ( unapprovedEmailTemplateGuid.HasValue )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "TimeCardPayPeriod", timeCard.TimeCardPayPeriod.ToString() );
                mergeFields.Add( "TimeCard", timeCard );
                mergeFields.Add( "Person", timeCard.PersonAlias.Person );
                mergeFields.Add( "UnApprovedByPerson", rockPage.CurrentPerson );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( timeCard.PersonAlias.Person.Email, mergeFields ) );
                Email.Send( unapprovedEmailTemplateGuid.Value, recipients, rockPage.ResolveRockUrl( "~/" ), rockPage.ResolveRockUrl( "~~/" ) );
            }

            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class TimeCardExtensions
    {
        /// <summary>
        /// Gets the time cards that non-zero hours ( Worked, Holiday, Vacation, Sick and 'Earned-Holiday' ).
        /// Only time cards where something was entered will be returned
        /// </summary>
        /// <returns></returns>
        public static IQueryable<TimeCard> WhereTimeCardsHaveHours( this IQueryable<TimeCard> qry )
        {
            return qry.Where( a =>
                a.TimeCardDays.Any( b =>
                    b.TotalWorkedDuration > 0
                    || b.PaidHolidayHours > 0
                    || b.EarnedHolidayHours > 0
                    || b.PaidSickHours > 0
                    || b.PaidVacationHours > 0
                    ) );
        }
    }
}
