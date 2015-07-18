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
using System.Windows.Controls;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for RangerScannerHostPage.xaml
    /// </summary>
    public partial class RangerScannerHostPage : Page
    {
        public RangerScannerHostPage()
        {
            InitializeComponent();
        }

        #region Ranger (Canon CR50/80) Scanner Events (DEBUG Only)

        public void rangerScanner_TransportItemSuspended( object sender, AxRANGERLib._DRangerEvents_TransportItemSuspendedEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportItemSuspended", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportPassthroughEvent( object sender, AxRANGERLib._DRangerEvents_TransportPassthroughEventEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportPassthroughEvent", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportItemInPocket( object sender, AxRANGERLib._DRangerEvents_TransportItemInPocketEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportItemInPocket", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportTrackIsClear( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportTrackIsClear", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportStartingUpState( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportStartingUpState", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportShuttingDownState( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportShuttingDownState", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportReadyToSetEndorsement( object sender, AxRANGERLib._DRangerEvents_TransportReadyToSetEndorsementEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportReadyToSetEndorsement", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportReadyToFeedState( object sender, AxRANGERLib._DRangerEvents_TransportReadyToFeedStateEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportReadyToFeedState", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportOverrideOptions( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportOverrideOptions", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportIsDead( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportIsDead", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportInExceptionState( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportInExceptionState", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportExceptionComplete( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportExceptionComplete", DateTime.Now.ToString( "o" ) ) );
        }

        public void rangerScanner_TransportEnablingOptionsState( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportEnablingOptionsState", DateTime.Now.ToString( "o" ) ) );
        }

        #endregion
    }
}
