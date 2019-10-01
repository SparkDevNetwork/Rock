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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Rock.Client;

namespace Rock.Apps.CheckScannerUtility.Models
{
    [System.Diagnostics.DebuggerDisplay( "{Id}:{AccountDisplayName}|{IsAccountChecked}" )]
    public class DisplayAccountModel : INotifyPropertyChanged
    {
        private FinancialAccount _financialAccount;

        public DisplayAccountModel(FinancialAccount financialAccount, List<FinancialAccount> allFinancialAccounts )
        {
            _financialAccount = financialAccount;
            Children = new ObservableCollection<DisplayAccountModel>( allFinancialAccounts.Where(a => a.ParentAccountId.HasValue && a.ParentAccountId == financialAccount.Id ).Select(a => new DisplayAccountModel(a, allFinancialAccounts)).ToList());
        }

        private bool _accountIsChecked;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id => _financialAccount.Id;
        public int? CampusId => _financialAccount.CampusId;
        public string AccountDisplayName => _financialAccount.PublicName.IsNotNullOrWhiteSpace() ? _financialAccount.Name : _financialAccount.PublicName;

        public bool IsAccountChecked
        {
            get => _accountIsChecked;
            set
            {
                _accountIsChecked = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "IsAccountChecked" ) );
            }
        }

        public ObservableCollection<DisplayAccountModel> Children { get; private set; }



    }
}
