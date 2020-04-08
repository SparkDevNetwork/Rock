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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Rock.Client;

namespace Rock.Apps.CheckScannerUtility.Models
{
    [System.Diagnostics.DebuggerDisplay( "{Id}:{AccountDisplayName}|{IsAccountChecked}" )]
    public class DisplayAccountValueModel : INotifyPropertyChanged
    {
        private decimal? _amount;

        public DisplayAccountValueModel( FinancialAccount financialAccount, List<FinancialAccount> allFinancialAccounts )
        {
            Account = financialAccount;
            ChildAccounts = allFinancialAccounts
                .Where( a => a.ParentAccountId.HasValue && a.ParentAccountId == financialAccount.Id ).OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new DisplayAccountValueModel( a, allFinancialAccounts ) )
                .ToList();


            if ( financialAccount.ParentAccountId.HasValue )
            {
                ParentAccount = allFinancialAccounts.FirstOrDefault( a => a.Id == financialAccount.ParentAccountId.Value );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int DisplayIndex { get; set; }

        // the 'Next' key has a tab index of 300, and the 'Complete' key has a tab index of 400, so set the AccountDisplayTabIndex so they can tab from the last Account box to the 'Next' or 'Complete' button (depending on what is visible)
        public int DisplayTabIndex => DisplayIndex + 200;

        public string AccountDisplayName => Account.PublicName.IsNotNullOrWhiteSpace() == true ? Account.PublicName : Account.Name;

        public int AccountOrder => Account.Order;

        public decimal? Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Amount" ) );
            }
        }

        public string AmountFormatted => $"{Amount:C}";

        public Visibility DisplayVisibility => ( Amount.HasValue && Amount.Value != 0.00M ) ? Visibility.Visible : Visibility.Collapsed;

        public int? ParentAccountId => Account?.ParentAccountId;

        public FinancialAccount ParentAccount { get; private set; } = null;

        public FinancialAccount Account { get; }

        public List<DisplayAccountValueModel> ChildAccounts { get; }

        public int AccountId => Account.Id;
    }
}
