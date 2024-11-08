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
using System.Windows;

namespace Rock.CloudPrint.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
#if RELEASE
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal( identity );
            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            if ( assembly != null && !principal.IsInRole( System.Security.Principal.WindowsBuiltInRole.Administrator ) )
            {
                var objProcessInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = System.IO.Path.Join( AppContext.BaseDirectory, "Rock.CloudPrint.Desktop.exe" ),
                    Verb = "runas"
                };

                try
                {
                    var proc = System.Diagnostics.Process.Start( objProcessInfo );
                }
                catch
                {
                }
                finally
                {
                    Current.Shutdown();
                }
            }
#endif
        }
    }
}
