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
using System.Windows;
using Rock.Net;
using Rock.Wpf;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the App control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        public void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e )
        {
            LogException( e.Exception );
            ErrorMessageWindow errorMessageWindow = new ErrorMessageWindow(e.Exception);
            errorMessageWindow.ShowDialog();
            e.Handled = true;
        }

        /// <summary>
        /// Silently tries to log the exception to the server's exception log service
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void LogException( Exception ex)
        {
            try
            {
                RockConfig config = RockConfig.Load();
                RockRestClient client = new RockRestClient( config.RockBaseUrl );
                client.Login( config.Username, config.Password );
                client.PostData<Exception>( "api/ExceptionLogs/LogException", ex );
            }
            catch
            {
                // intentionally ignore if we can't log the exception to the server
            }
        }
    }
}
