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

namespace Rock.Financial
{
    /// <summary>
    /// Describes a gateway component that is setup to handle recurring payments by accepting the number of payments.
    /// </summary>
    /// <remarks>
    /// A <see cref="GatewayComponent"/> implementing this interface should have a <see cref="GatewayComponent.AddScheduledPayment(Model.FinancialGateway, PaymentSchedule, PaymentInfo, out string)"/>
    /// implementation that can add a scheduled transaction using the <see cref="PaymentSchedule.StartDate"/>, <see cref="PaymentSchedule.NumberOfPayments"/>, and <see cref="PaymentSchedule.TransactionFrequencyValue"/>.
    /// </remarks>
    public interface IScheduledNumberOfPaymentsGateway
    {
    }
}
