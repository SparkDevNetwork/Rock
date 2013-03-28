//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
