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
using System.Windows.Input;

namespace Rock.Wpf
{ 
    public static class ControlDoubleClickBehavior
    {
        public static ICommand GetExecuteCommand( DependencyObject obj )
        {
            return ( ICommand ) obj.GetValue( ExecuteCommand );
        }

        public static void SetExecuteCommand( DependencyObject obj, ICommand command )
        {
            obj.SetValue( ExecuteCommand, command );
        }

        public static readonly DependencyProperty ExecuteCommand = DependencyProperty.RegisterAttached( "ExecuteCommand",
            typeof( ICommand ), typeof( ControlDoubleClickBehavior ),
            new UIPropertyMetadata( null, OnExecuteCommandChanged ) );

        public static Window GetExecuteCommandParameter( DependencyObject obj )
        {
            return ( Window ) obj.GetValue( ExecuteCommandParameter );
        }

        public static void SetExecuteCommandParameter( DependencyObject obj, ICommand command )
        {
            obj.SetValue( ExecuteCommandParameter, command );
        }

        public static readonly DependencyProperty ExecuteCommandParameter = DependencyProperty.RegisterAttached( "ExecuteCommandParameter",
            typeof( Window ), typeof( ControlDoubleClickBehavior ) );

        private static void OnExecuteCommandChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            var control = sender as Control;

            if ( control != null )
            {
                control.MouseDoubleClick += control_MouseDoubleClick;
            }
        }

        static void control_MouseDoubleClick( object sender, MouseButtonEventArgs e )
        {
            var control = sender as Control;

            if ( control != null )
            {
                var command = control.GetValue( ExecuteCommand ) as ICommand;
                var commandParameter = control.GetValue( ExecuteCommandParameter );

                if ( command.CanExecute( e ) )
                {
                    command.Execute( commandParameter );
                }
            }
        }
    }
}
