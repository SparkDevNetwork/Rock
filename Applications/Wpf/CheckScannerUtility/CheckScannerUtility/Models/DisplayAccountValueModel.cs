using System.ComponentModel;
using System.Windows;
using Rock.Client;

namespace Rock.Apps.CheckScannerUtility.Models
{
    public class DisplayAccountValueModel : INotifyPropertyChanged
    {
        private FinancialAccount _financialAccount;
        private decimal? _amount;

        public DisplayAccountValueModel( FinancialAccount financialAccount)
        {
            _financialAccount = financialAccount;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Index { get; set; }
        
        public string AccountDisplayName => _financialAccount.PublicName.IsNotNullOrWhiteSpace() == true ? _financialAccount.PublicName : _financialAccount.Name;

        public int AccountOrder => _financialAccount.Order;

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

        public FinancialAccount Account => _financialAccount;

        public int AccountId => Account.Id;
    }
}
