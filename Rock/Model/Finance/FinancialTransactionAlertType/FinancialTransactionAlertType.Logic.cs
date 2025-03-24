// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

namespace Rock.Model
{
    /// <summary>
    /// FinancialTransactionAlertType Logic
    /// </summary>
    public partial class FinancialTransactionAlertType
    {
        #region Public Methods

        /// <summary>
        /// Gets a description of what <see cref="AmountSensitivityScale"/> means depending on the AlertType.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        /// <returns>string.</returns>
        public static string GetAmountSensitivityDescription( AlertType alertType )
        {
            /* 11-19-2021 MDP

           AlertType drives the logic of how sensitivity values are used. (See notes and logic here https://github.com/SparkDevNetwork/Rock/blob/6dacabe84dcaf041450c3bc075164c7580151390/Rock/Jobs/GivingAutomation.cs#L1602)

           Follow-Up uses sensitivity to look for 'worse than usual':
               - Gifts with amounts that are significantly smaller than usual. For example, a $50 gift for somebody that usually gives $300 a week.

           Gratitude uses sensitivity to look for 'better than usual':
               - Gifts with amounts that are significantly larger than usual. For example, a $1200 gift for somebody that usually gives $300 a week.

           In both cases, a positive value for sensitivity should be used.
           A negative sensitivity could be entered if they really wanted to, but it'll do weird things such as express gratitude for a gift over $180 for a person that normally gives $200.

            */

            /* Amount Sensitivity */
            if ( alertType == AlertType.Gratitude )
            {
                // Amount Sensitivity Help for Gratitude.
                // Gratitude. Larger amount than usual.
                return @"
 <p>
    For gratitude alerts, the amount sensitivity scale determines when a 'larger-than-usual' gift triggers the alert. Leave blank to ignore the sensitivity criteria.
</p>
<p>
    Typical values are shown below.
    <ul>
        <li>2 (Aggressive) - This would alert when a gift was more than 2 times the interquartile range (IQR) from their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $530 was received.</li>
        <li>3 (Normal) - This would alert when a gift was more than 3 times the interquartile range (IQR) from their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $595 was received.</li>
    </ul>
</p>
<p>
    In cases where the giver is very consistent — every gift is the exact same amount — a fallback value is used. The fallback sensitivity is calculated as 15% of the median gift amount.
</p>";
            }
            else
            {
                // Amount Sensitivity Help for Follow-Up
                // Follow-Up. The logic is flipped so the sensitivity is used to determine smaller than usual amounts."
                return @"
 <p>
    For follow-up alerts, the amount sensitivity scale determines when a 'smaller-than-usual' gift triggers the alert.
</p>
<p>
    Typical values are shown below.
    <ul>
        <li>2 (Aggressive) - This would alert when a gift is 2 times or more of the interquartile range (IQR) smaller than their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $270 was received.</li>
        <li>3 (Normal) - This would alert when a gift is 3 times or more of the interquartile range (IQR) smaller than their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $205 was received.</li>
    </ul>
</p>
<p>
    In cases where the giver is very consistent — every gift is the exact same amount — a fallback value is used. The fallback sensitivity is calculated as 15% of the median gift amount.
</p>";
            }
        }

        /// <summary>
        /// Gets a description of what <see cref="FrequencySensitivityScale"/> means depending on the AlertType.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        /// <returns>string.</returns>
        public static string GetFrequencySensitivityDescription( AlertType alertType )
        {
            /* 11-19-2021 MDP

            AlertType drives the logic of how sensitivity values are used. (See notes and logic here https://github.com/SparkDevNetwork/Rock/blob/6dacabe84dcaf041450c3bc075164c7580151390/Rock/Jobs/GivingAutomation.cs#L1602)

            Follow-Up uses sensitivity to look for 'worse than usual':
                - Gifts that are significantly late. For example: a bi-weekly giver than hasn't given for several weeks

            Gratitude uses sensitivity to look for 'better than usual':
                - Gifts that are significantly early. For example: a monthly giver that gave 20 days earlier than usual.

            In both cases, a positive value for sensitivity should be used.
            A negative sensitivity could be entered if they really wanted to, but it'll do weird things such as create a follow up (a late gift alert)
            for a consistent weekly giver if it has been 4 or more days since their last gift.

            */

            /* Frequency Sensitivity */
            if ( alertType == AlertType.FollowUp )
            {
                // Follow-Up because gift is late or later than usual.
                // Aggressive is 'a little later than usual'
                // Normal is 'significantly later than usual'
                return @"
<p>
    For a follow-up alert, the frequency sensitivity scale determines when to generate an alert if a gift hasn't been received for a while, or was received later than usual.
</p>
<p>
    Typical values are shown below.
    <ul>
                             
        <li>2 (Aggressive) - This would alert when the frequency of a gift is later than 2 times of the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, this alert would be generated if no gift was received within 22 days since their last gift.</li>
        <li>3 (Normal) - This would alert when the frequency of a gift is later than 3 times of the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, this alert would be generated if no gift was received within 26 days since their last gift.</li>
    </ul>
</p>
<p>
    In cases where the giver is very consistent — every gift is the same number of days apart — a fallback value is used. The fallback frequency sensitivity is calculated as 15% of the average days between gifts. If that value is less than 3 days, then we again fallback to 3 days.
</p>
";
            }
            else
            {
                // Gratitude for Earlier than Usual
                // Aggressive is 'a little bit earlier than usual'
                // Normal is 'significantly earlier than usual'
                return @"
<p>
    For a gratitude alert, the frequency sensitivity scale determines when to generate an alert if a gift is earlier than usual. Leave blank to ignore the sensitivity criteria.
</p>
<p>
    Typical values are shown below.
    <ul>
        <li>2 (Aggressive) - This would alert when the frequency of a gift is earlier than 2 times the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, an alert would be generated for a gift if it has been fewer than 10 days since their last gift. (4 days early).</li>
        <li>3 (Normal) -     This would alert when the frequency of a gift is earlier than 3 times the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, an alert would be generated for a gift if it has been fewer than 7 days since their last gift. (7 days early).</li>
    </ul>
</p>
<p>
    In cases where the giver is very consistent — every gift is the same number of days apart — a fallback value is used. The fallback frequency sensitivity is calculated as 15% of the average days between gifts. If that value is less than 3 days, then we again fallback to 3 days.
</p>
";
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name.IsNullOrWhiteSpace() ? base.ToString() : Name;
        }

        #endregion Public Methods
    }
}