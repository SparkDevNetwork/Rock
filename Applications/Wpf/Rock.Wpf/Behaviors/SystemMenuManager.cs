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
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
namespace Rock.Wpf.Behaviors
{
    public static class SystemMenuManager
    {
        public static void ShowMenu( Window targetWindow, Point menuLocation )
        {
            if ( targetWindow == null )
                throw new ArgumentNullException( "TargetWindow is null." );

            int x, y;

            try
            {
                x = Convert.ToInt32( menuLocation.X );
                y = Convert.ToInt32( menuLocation.Y );
            }
            catch ( OverflowException )
            {
                x = 0;
                y = 0;
            }

            uint WM_SYSCOMMAND = 0x112, TPM_LEFTALIGN = 0x0000, TPM_RETURNCMD = 0x0100;

            IntPtr window = new WindowInteropHelper( targetWindow ).Handle;

            IntPtr wMenu = NativeMethods.GetSystemMenu( window, false );

            int command = NativeMethods.TrackPopupMenuEx( wMenu, TPM_LEFTALIGN | TPM_RETURNCMD, x, y, window, IntPtr.Zero );

            if ( command == 0 )
                return;

            NativeMethods.PostMessage( window, WM_SYSCOMMAND, new IntPtr( command ), IntPtr.Zero );
        }
    }

    internal static class NativeMethods
    {
        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        internal static extern IntPtr GetSystemMenu( IntPtr hWnd, bool bRevert );

        [DllImport( "user32.dll" )]
        internal static extern int TrackPopupMenuEx( IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm );

        [DllImport( "user32.dll" )]
        internal static extern IntPtr PostMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

        [DllImport( "user32.dll" )]
        internal static extern bool EnableMenuItem( IntPtr hMenu, uint uIDEnableItem, uint uEnable );
    }
}
