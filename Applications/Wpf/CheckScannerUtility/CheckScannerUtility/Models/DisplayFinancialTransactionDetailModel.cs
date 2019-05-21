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
using System;
using System.ComponentModel;

namespace Rock.Apps.CheckScannerUtility.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DisplayFinancialTransactionDetailModel: INotifyPropertyChanged
    {
        private Rock.Client.FinancialTransactionDetail _financialTransactionDetail;
        private decimal? _amount;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayFinancialTransactionDetailModel"/> class.
        /// </summary>
        /// <param name="financialTransactionDetail">The financial transaction detail.</param>
        public DisplayFinancialTransactionDetailModel( Rock.Client.FinancialTransactionDetail financialTransactionDetail )
        {
            _financialTransactionDetail = financialTransactionDetail;
            if ( financialTransactionDetail.Amount == decimal.Zero && financialTransactionDetail.Id == 0 )
            {
                _amount = null;
            }
            else
            {
                _amount = financialTransactionDetail.Amount;
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id => _financialTransactionDetail.Id;

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid => _financialTransactionDetail.Guid;

        /// <summary>
        /// Gets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int AccountId => _financialTransactionDetail.AccountId;

        /// <summary>
        /// Gets the display name of the account.
        /// </summary>
        /// <value>
        /// The display name of the account.
        /// </value>
        public string AccountDisplayName => _financialTransactionDetail.Account.PublicName.IsNotNullOrWhiteSpace() == true ? _financialTransactionDetail.Account.PublicName : _financialTransactionDetail.Account.Name;

        /// <summary>
        /// Gets the account order.
        /// </summary>
        /// <value>
        /// The account order.
        /// </value>
        public int AccountOrder => _financialTransactionDetail.Account.Order;

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Amount" ) );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
