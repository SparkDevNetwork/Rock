using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuidToClipboard
{
    
    class Program
    {
        [STAThreadAttribute]
        static void Main( string[] args )
        {
            var newGuid = Guid.NewGuid().ToString().ToUpper();
            Clipboard.SetText( newGuid );
            Console.WriteLine( "New Guid copied to clipboard: " + newGuid );
        }
    }
}
