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
using System.IO;
using System.Net;
using System.Windows;

using RestSharp;

using Rock.Wpf;

namespace Rock.Apps.StatementGenerator
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            string applicationFolder = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );

            // set the current directory to the same as the current exe so that we can find the layout and logo files
            Directory.SetCurrentDirectory( applicationFolder );

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the App control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e )
        {
            LogException( e.Exception );
            ErrorMessageWindow errorMessageWindow = new ErrorMessageWindow( e.Exception );
            errorMessageWindow.Owner = MainWindow;
            errorMessageWindow.ShowDialog();
            e.Handled = true;
        }

        /// <summary>
        /// Silently tries to log the exception to the server's exception log service
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void LogException( Exception ex, string message = null )
        {
            try
            {
                RockConfig rockConfig = RockConfig.Load();
                var restClient = new RestClient( rockConfig.RockBaseUrl );
                restClient.LoginToRock( rockConfig.Username, rockConfig.Password );
                var request = new RestRequest( "api/ExceptionLogs/LogException" );
                StatementGeneratorException statementGeneratorException;
                if ( ex is StatementGeneratorException )
                {
                    statementGeneratorException = ex as StatementGeneratorException;
                }
                else
                {
                    statementGeneratorException = new StatementGeneratorException( ex, message );
                }
                var exceptionJson = statementGeneratorException.ToJson();
                request.AddJsonBody( statementGeneratorException );
                var response = restClient.Post( request );
            }
            catch
            {
                // intentionally ignore if we can't log the exception to the server
            }
        }
    }
}
