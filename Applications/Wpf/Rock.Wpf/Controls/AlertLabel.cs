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
using System.Windows.Controls;

namespace Rock.Wpf.Controls
{
    /// <summary>
    /// A Bootstrap style Alert
    /// </summary>
    /// <seealso cref="System.Windows.Controls.TextBlock" />
    public class AlertLabel : Label
    {
        private AlertMessageType alertMessageType;
        private AccessText accessText = new AccessText() { TextWrapping = TextWrapping.Wrap };

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
        /// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
        protected override void OnContentChanged( object oldContent, object newContent )
        {
            base.OnContentChanged( oldContent, newContent );

            if ( this.Content is string )
            {
                accessText.Text = this.Content as string;
                this.Content = accessText;
            }
        }

        /// <summary>
        /// Gets or sets the type of the alert.
        /// </summary>
        /// <value>
        /// The type of the alert.
        /// </value>
        public AlertMessageType AlertType
        {
            get => alertMessageType;
            set
            {
                alertMessageType = value;

                switch ( alertMessageType )
                {
                    case AlertMessageType.Danger:
                        this.Style = Application.Current.Resources["labelStyleAlertDanger"] as Style;
                        break;
                    case AlertMessageType.Info:
                        this.Style = Application.Current.Resources["labelStyleAlertInfo"] as Style;
                        break;
                    case AlertMessageType.Warning:
                        this.Style = Application.Current.Resources["labelStyleAlertWarning"] as Style;
                        break;
                    case AlertMessageType.Success:
                        this.Style = Application.Current.Resources["labelStyleAlertSuccess"] as Style;
                        break;
                    default:
                        this.Style = Application.Current.Resources["labelStyleAlertBase"] as Style;
                        return;
                }
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get => (this.Content as AccessText)?.Text ?? this.Content as string;
            set => Content = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AlertMessageType
    {
        Success,
        Info,
        Warning,
        Danger
    }
}
