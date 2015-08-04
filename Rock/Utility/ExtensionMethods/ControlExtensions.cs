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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Rock
{
    /// <summary>
    /// Control Extensions
    /// </summary>
    public static class ControlExtensions
    {
        #region Control Extensions

        /// <summary>
        /// Loads a user control using the constructor with the parameters specified.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="userControlPath">The user control path.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <returns></returns>
        public static System.Web.UI.UserControl LoadControl( this System.Web.UI.Control control, string userControlPath, params object[] constructorParameters )
        {
            List<Type> constParamTypes = new List<Type>();
            foreach ( object constParam in constructorParameters )
                constParamTypes.Add( constParam.GetType() );

            System.Web.UI.UserControl ctl = control.Page.LoadControl( userControlPath ) as System.Web.UI.UserControl;

            ConstructorInfo constructor = ctl.GetType().BaseType.GetConstructor( constParamTypes.ToArray() );

            if ( constructor == null )
                throw new MemberAccessException( "The requested constructor was not found on " + ctl.GetType().BaseType.ToString() );
            else
                constructor.Invoke( ctl, constructorParameters );

            return ctl;
        }

        /// <summary>
        /// Gets the parent RockBlock.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static Rock.Web.UI.RockBlock RockBlock( this System.Web.UI.Control control )
        {
            System.Web.UI.Control parentControl = control.Parent;
            while ( parentControl != null )
            {
                if ( parentControl is Rock.Web.UI.RockBlock )
                {
                    return (Rock.Web.UI.RockBlock)parentControl;
                }
                parentControl = parentControl.Parent;
            }
            return null;
        }

        /// <summary>
        /// Returns the parent update panel for the given control (or null if none is found).
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static System.Web.UI.UpdatePanel ParentUpdatePanel( this System.Web.UI.Control control )
        {
            System.Web.UI.Control parentControl = control.Parent;
            while ( parentControl != null )
            {
                if ( parentControl is System.Web.UI.UpdatePanel )
                {
                    return (System.Web.UI.UpdatePanel)parentControl;
                }
                parentControl = parentControl.Parent;
            }
            return null;
        }

        /// <summary>
        /// Gets all controls of Type recursively.
        /// http://stackoverflow.com/questions/7362482/c-sharp-get-all-web-controls-on-page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controlCollection">The control collection.</param>
        /// <param name="resultCollection">The result collection.</param>
        private static void GetControlListRecursive<T>( this System.Web.UI.ControlCollection controlCollection, List<T> resultCollection ) where T : System.Web.UI.Control
        {
            foreach ( System.Web.UI.Control control in controlCollection )
            {
                if ( control is T )
                {
                    resultCollection.Add( (T)control );
                }

                if ( control.HasControls() )
                {
                    GetControlListRecursive( control.Controls, resultCollection );
                }
            }
        }

        /// <summary>
        /// Gets all controls of Type recursively.
        /// http://stackoverflow.com/questions/7362482/c-sharp-get-all-web-controls-on-page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static List<T> ControlsOfTypeRecursive<T>( this System.Web.UI.Control control ) where T : System.Web.UI.Control
        {
            List<T> result = new List<T>();
            GetControlListRecursive<T>( control.Controls, result );
            return result;
        }

        /// <summary>
        /// Goes up the parent tree of the control returning the first parent that is of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static T FirstParentControlOfType<T>( this System.Web.UI.Control control ) where T : System.Web.UI.Control
        {
            if ( control != null )
            {
                var parentControl = control.Parent;
                while ( parentControl != null )
                {
                    if ( parentControl is T )
                    {
                        return parentControl as T;
                    }

                    parentControl = parentControl.Parent;
                }
            }

            return null;
        }

        #endregion Control Extensions

        #region WebControl Extensions

        /// <summary>
        /// Adds a CSS class name to a web control.
        /// </summary>
        /// <param name="webControl">The web control.</param>
        /// <param name="className">Name of the class.</param>
        public static WebControl AddCssClass( this WebControl webControl, string className )
        {
            string match = @"(^|\s+)" + className + @"($|\s+)";
            string css = webControl.CssClass;

            if ( !Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
            {
                css += " " + className;
            }

            webControl.CssClass = css.Trim();

            return webControl;
        }

        /// <summary>
        /// Removes a CSS class name from a web control.
        /// </summary>
        /// <param name="webControl">The web control.</param>
        /// <param name="className">Name of the class.</param>
        public static WebControl RemoveCssClass( this WebControl webControl, string className )
        {
            string match = @"(^|\s+)" + className + @"($|\s+)";
            string css = webControl.CssClass;

            while ( Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
            {
                css = Regex.Replace( css, match, " ", RegexOptions.IgnoreCase );
            }

            webControl.CssClass = css.Trim();

            return webControl;
        }

        #endregion WebControl Extensions

        #region HtmlControl Extensions

        /// <summary>
        /// Adds a CSS class name to an html control.
        /// </summary>
        /// <param name="htmlControl">The html control.</param>
        /// <param name="className">Name of the class.</param>
        public static void AddCssClass( this System.Web.UI.HtmlControls.HtmlControl htmlControl, string className )
        {
            string match = @"\b" + className + "\b";
            string css = htmlControl.Attributes["class"] ?? string.Empty;

            if ( !Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                htmlControl.Attributes["class"] = Regex.Replace( css + " " + className, @"^\s+", "", RegexOptions.IgnoreCase );
        }

        /// <summary>
        /// Removes a CSS class name from an html control.
        /// </summary>
        /// <param name="htmlControl">The html control.</param>
        /// <param name="className">Name of the class.</param>
        public static void RemoveCssClass( this System.Web.UI.HtmlControls.HtmlControl htmlControl, string className )
        {
            string match = @"\s*\b" + className + @"\b";
            string css = htmlControl.Attributes["class"] ?? string.Empty;

            if ( Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                htmlControl.Attributes["class"] = Regex.Replace( css, match, "", RegexOptions.IgnoreCase );
        }

        #endregion HtmlControl Extensions

        #region CheckBoxList Extensions

        /// <summary>
        /// Sets the Selected property of each item to true for each given matching string values.
        /// </summary>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="values">The values.</param>
        public static void SetValues( this CheckBoxList checkBoxList, IEnumerable<string> values )
        {
            foreach ( ListItem item in checkBoxList.Items )
            {
                item.Selected = values.Contains( item.Value, StringComparer.OrdinalIgnoreCase );
            }
        }

        /// <summary>
        /// Sets the Selected property of each item to true for each given matching int values.
        /// </summary>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="values">The values.</param>
        public static void SetValues( this CheckBoxList checkBoxList, IEnumerable<int> values )
        {
            foreach ( ListItem item in checkBoxList.Items )
            {
                int numValue = int.MinValue;
                item.Selected = int.TryParse( item.Value, out numValue ) && values.Contains( numValue );
            }
        }

        #endregion CheckBoxList Extensions

        #region ListControl Extensions

        /// <summary>
        /// Tries to set the selected value, if the value does not exist, it will attempt to set the value to defaultValue (if specified), 
        /// otherwise it will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        public static void SetValue( this ListControl listControl, string value, string defaultValue = null )
        {
            try
            {
                var valueItem = listControl.Items.FindByValue( value );
                if ( valueItem == null && defaultValue != null )
                {
                    valueItem = listControl.Items.FindByValue( defaultValue );
                }

                if ( valueItem != null )
                {
                    listControl.SelectedValue = valueItem.Value;
                }
                else
                {
                    if ( listControl.Items.Count > 0 )
                    {
                        listControl.SelectedIndex = 0;
                    }
                }
            }
            catch
            {
                if ( listControl.Items.Count > 0 )
                {
                    listControl.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Sets the read only value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        public static void SetReadOnlyValue( this ListControl listControl, string value )
        {
            listControl.Items.Clear();
            listControl.Items.Add( value );
        }

        /// <summary>
        /// Tries to set the selected value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this ListControl listControl, int? value )
        {
            listControl.SetValue( value == null ? "0" : value.ToString() );
        }

        /// <summary>
        /// Tries to set the selected value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this ListControl listControl, Guid? value )
        {
            listControl.SetValue( value == null ? "" : value.ToString() );
        }

        /// <summary>
        /// Binds to enum using the enum's integer value as the listitem value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listControl">The list control.</param>
        /// <param name="insertBlankOption">if set to <c>true</c> [insert blank option].</param>
        /// <param name="ignoreTypes">any enums that should not be included in the list control</param>
        public static void BindToEnum<T>( this ListControl listControl, bool insertBlankOption = false, T[] ignoreTypes = null )
        {
            var enumType = typeof( T );
            var dictionary = new Dictionary<int, string>();
            foreach ( var value in Enum.GetValues( enumType ) )
            {
                if ( ignoreTypes != null && ignoreTypes.Contains( (T)value ) )
                {
                    continue;
                }
                else
                {
                    dictionary.Add( Convert.ToInt32( value ), Enum.GetName( enumType, value ).SplitCase() );
                }
            }

            listControl.DataSource = dictionary;
            listControl.DataTextField = "Value";
            listControl.DataValueField = "Key";
            listControl.DataBind();

            if ( insertBlankOption )
            {
                listControl.Items.Insert( 0, new ListItem() );
            }
        }

        /// <summary>
        /// Binds to the values of a definedType using the definedValue's Id as the listitem value
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="definedType">Type of the defined.</param>
        /// <param name="insertBlankOption">if set to <c>true</c> [insert blank option].</param>
        /// <param name="useDescriptionAsText">if set to <c>true</c> [use description as text].</param>
        public static void BindToDefinedType( this ListControl listControl, Rock.Web.Cache.DefinedTypeCache definedType, bool insertBlankOption = false, bool useDescriptionAsText = false )
        {
            var ds = definedType.DefinedValues
                .Select( v => new
                {
                    Name = v.Value,
                    v.Description,
                    v.Id
                } );

            listControl.SelectedIndex = -1;
            listControl.DataSource = ds;
            listControl.DataTextField = useDescriptionAsText ? "Description" : "Name";
            listControl.DataValueField = "Id";
            listControl.DataBind();

            if ( insertBlankOption )
            {
                listControl.Items.Insert( 0, new ListItem() );
            }
        }

        /// <summary>
        /// Returns the Value as Int or null if Value is <see cref="T:Rock.Constants.None"/>.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="NoneAsNull">if set to <c>true</c>, will return Null if SelectedValue = <see cref="T:Rock.Constants.None" /> </param>
        /// <returns></returns>
        public static int? SelectedValueAsInt( this ListControl listControl, bool NoneAsNull = true )
        {
            if ( NoneAsNull )
            {
                if ( listControl.SelectedValue.Equals( Rock.Constants.None.Id.ToString() ) )
                {
                    return null;
                }
            }

            if ( string.IsNullOrWhiteSpace( listControl.SelectedValue ) )
            {
                return null;
            }
            else
            {
                return int.Parse( listControl.SelectedValue );
            }
        }

        /// <summary>
        /// Returns the value of the currently selected item.
        /// It will return NULL if either <see cref="T:Rock.Constants.None"/> or <see cref="T:Rock.Constants.All"/> is selected.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public static int? SelectedValueAsId( this ListControl listControl )
        {
            int? result = SelectedValueAsInt( listControl );

            if ( result == Rock.Constants.All.Id )
            {
                return null;
            }

            if ( result == Rock.Constants.None.Id )
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Selecteds the value as enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T SelectedValueAsEnum<T>( this ListControl listControl, T? defaultValue = null ) where T : struct
        {
            return listControl.SelectedValue.ConvertToEnum<T>( defaultValue );
        }

        /// <summary>
        /// Selecteds the value as enum or null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listControl">The list control.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T? SelectedValueAsEnumOrNull<T>( this ListControl listControl, T? defaultValue = null ) where T : struct
        {
            return listControl.SelectedValue.ConvertToEnumOrNull<T>( defaultValue );
        }

        /// <summary>
        /// Selecteds the value as unique identifier.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public static Guid? SelectedValueAsGuid( this ListControl listControl )
        {
            if ( string.IsNullOrWhiteSpace( listControl.SelectedValue ) )
            {
                return null;
            }
            else
            {
                return listControl.SelectedValue.AsGuidOrNull();
            }
        }

        /// <summary>
        /// Returns a List of ListItems that contains the values/text in this string that are formatted as either 'value1,value2,value3,...' or 'value1^text1,value2^text2,value3^text3,...'.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static List<ListItem> GetListItems( this string str )
        {
            var result = new List<ListItem>();
            foreach ( string keyvalue in str.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( keyValueArray.Length > 0 )
                {
                    var listItem = new ListItem();
                    listItem.Value = keyValueArray[0].Trim();
                    listItem.Text = keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim();
                    result.Add( listItem );
                }
            }

            return result;
        }

        #endregion ListControl Extensions

        #region HiddenField Extensions

        /// <summary>
        /// Returns the values as an int or 0 if not found or not an int.
        /// </summary>
        /// <param name="hiddenField">The hidden field.</param>
        /// <returns></returns>
        public static int ValueAsInt( this HiddenField hiddenField )
        {
            int intValue = 0;
            if ( int.TryParse( hiddenField.Value, out intValue ) )
            {
                return intValue;
            }

            return 0;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="hiddenField">The hidden field.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this HiddenField hiddenField, int value )
        {
            hiddenField.Value = value.ToString();
        }

        /// <summary>
        /// Determines whether the specified hidden field is zero.
        /// </summary>
        /// <param name="hiddenField">The hidden field.</param>
        /// <returns>
        ///   <c>true</c> if the specified hidden field is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero( this HiddenField hiddenField )
        {
            return hiddenField.Value.Equals( "0" );
        }

        #endregion HiddenField Extensions
    }
}
