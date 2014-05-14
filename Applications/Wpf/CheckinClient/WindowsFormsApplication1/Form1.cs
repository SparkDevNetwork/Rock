// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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


using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click( object sender, EventArgs e )
        {
            string s = @"CT~~CD,~CC^~CT~
^XA~TA000~JSN^LT0^MNW^MTD^PON^PMN^LH0,0^JMA^PR6,6~SD15^JUS^LRN^CI0^XZ
^XA
^MMT
^PW812
^LL0406
^LS0
^FT607,68^A0N,73,72^FB177,1,0,R^FH\^FDWWW^FS
^FT6,122^A0N,39,38^FH\^FD2^FS
^FT631,118^A0N,25,24^FH\^FD4^FS
^FO12,161^GB40,42,42^FS
^FT8,194^A0N,34,33^FB57,1,0,C^FR^FH\^FDAAA^FS
^FT427,118^A0N,25,24^FH\^FD3^FS
^FO12,264^GB40,42,42^FS
^FT12,297^A0N,34,33^FB48,1,0,C^FR^FH\^FDLLL^FS
^FB330,4,0,L^FT68,250^A0N,23,24^FH\^FD5^FS
^FB330,4,0,L^FT68,354^A0N,23,24^FH\^FD7^FS
^FT420,177^A0N,23,24^FH\^FDNotes:^FS
^FO403,154^GB0,237,1^FS
^FO422,386^GB361,0,1^FS
^FO423,345^GB361,0,1^FS
^FO421,304^GB361,0,1^FS
^FO421,263^GB361,0,1^FS
^FO421,227^GB361,0,1^FS
^LRY^FO0,0^GB812,0,81^FS^LRN
^PQ1,0,1,Y^XZ"; // device-dependent string, need a FormFeed?

            foreach ( string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters )
            {
                MessageBox.Show( printer );
            }

            // Allow the user to select a printer.
            PrintDialog pd = new PrintDialog();
            pd.PrinterSettings = new PrinterSettings();
            if ( DialogResult.OK == pd.ShowDialog( this ) )
            {
                // Send a printer-specific to the printer.
                //RawPrinterHelper.SendStringToPrinter( pd.PrinterSettings.PrinterName, s );
                RawPrinterHelper.SendStringToPrinter( "ZDesigner GX420d (Copy 1)", s );
            }
        }
    }
}
