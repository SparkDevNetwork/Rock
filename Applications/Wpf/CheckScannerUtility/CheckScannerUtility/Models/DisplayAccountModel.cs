
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

        public DisplayAccountModel(FinancialAccount financialAccount )
        {
            _financialAccount = financialAccount;
            Children = new ObservableCollection<DisplayAccountModel>( financialAccount.ChildAccounts.Select( a => new DisplayAccountModel( a ) ) );
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
