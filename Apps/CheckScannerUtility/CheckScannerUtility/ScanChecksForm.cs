using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rock.CheckScannerUtility
{
    public partial class ScanChecksForm : Form
    {
        public ScanChecksForm()
        {
            InitializeComponent();
        }

        public void ShowCheckAccountMicr( string value )
        {
            lblAccountNumber.Text = value;
            lblCheckNumber.Text = value;
            lblRoutingNumber.Text = value;
        }

        public void ShowCheckImages( Bitmap frontImage, Bitmap backImage )
        {
            pbxFront.Image = frontImage;
            pbxBack.Image = backImage;
        }
    }
}
