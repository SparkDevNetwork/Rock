using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CheckinClient
{
    [PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
    [System.Runtime.InteropServices.ComVisibleAttribute( true )]
    public class RockCheckinScriptManager
    {
        MainWindow mainWindow;

        public RockCheckinScriptManager( Window w )
        {
            this.mainWindow = (MainWindow)w;
        }

        public void PrintLabels( string labelData )
        {
            string something = "";
        }
    }
}