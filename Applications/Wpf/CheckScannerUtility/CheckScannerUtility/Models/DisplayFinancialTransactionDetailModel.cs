

using System.ComponentModel;

namespace Rock.Apps.CheckScannerUtility.Models
{
    public class DisplayFinancialTransactionDetailModel: INotifyPropertyChanged
    {
        private string _accountDisplayName;
        private decimal _amount;

        public string AccountDisplayName
        {
            get { return _accountDisplayName; }
            set
            {
                _accountDisplayName = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "AccountDisplayName" ) );
            }
        }
        public decimal Amount
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
