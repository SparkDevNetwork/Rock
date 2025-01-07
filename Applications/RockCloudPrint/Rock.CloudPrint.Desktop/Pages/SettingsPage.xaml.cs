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
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Controls;

namespace Rock.CloudPrint.Desktop.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private const string SettingsFile = "Service\\appsettings.json";

        public SettingsPage()
        {
            InitializeComponent();

            ProxyName.Text = Environment.MachineName;
            ReadSettings();

            AppVersionTextBlock.Text = $"Version {GetType().Assembly.GetName().Version}";
        }

        private void ReadSettings()
        {
            if ( !File.Exists( SettingsFile ) )
            {
                return;
            }

            try
            {
                using var filestream = File.OpenRead( SettingsFile );
                var json = JsonSerializer.Deserialize<JsonElement>( filestream );

                if ( json.TryGetProperty( "Url", out var url ) )
                {
                    ServerUrl.Text = url.GetString() ?? string.Empty;
                }

                if ( json.TryGetProperty( "Name", out var name ) )
                {
                    ProxyName.Text = name.GetString() ?? string.Empty;
                }

                if ( json.TryGetProperty( "Id", out var id ) )
                {
                    ProxyId.Text = id.GetString() ?? string.Empty;
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
            }
        }

        private void WriteSettings()
        {
            JsonNode json;

            try
            {
                if ( !File.Exists( SettingsFile ) )
                {
                    json = new JsonObject();
                }
                else
                {
                    using var filestream = File.OpenRead( SettingsFile );
                    json = JsonNode.Parse( filestream ) ?? new JsonObject();
                }

                json["Url"] = ServerUrl.Text;
                json["Name"] = ProxyName.Text;
                json["Id"] = ProxyId.Text;

                var text = JsonSerializer.Serialize( json, new JsonSerializerOptions
                {
                    WriteIndented = true
                } );

                File.WriteAllText( SettingsFile, text );

                SavedMessage.Title = "Saved";
                SavedMessage.Message = "Settings have been saved.";
                SavedMessage.Severity = Wpf.Ui.Controls.InfoBarSeverity.Success;
                SavedMessage.IsOpen = true;
            }
            catch ( Exception ex )
            {
                SavedMessage.Title = "Error";
                SavedMessage.Message = ex.Message;
                SavedMessage.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
                SavedMessage.IsOpen = true;
            }
        }

        private void Save_Click( object sender, System.Windows.RoutedEventArgs e )
        {
            WriteSettings();
        }
    }
}
