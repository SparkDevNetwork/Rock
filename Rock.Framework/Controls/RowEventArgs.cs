using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Controls
{
    public delegate void RowEventHandler( object sender, RowEventArgs e );
    public class RowEventArgs : EventArgs
    {
        public int RowIndex { get; private set; }

        public RowEventArgs( int rowIndex )
        {
            RowIndex = rowIndex;
        }
    }
}