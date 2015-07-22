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
using System.Linq;
using System.Reflection;

namespace Rock
{
    /// <summary>
    /// Object and Stream Extensions
    /// </summary>
    public static class ObjectExtensions
    {
        #region Object Extensions

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="rootObj">The root obj.</param>
        /// <param name="propertyPathName">Name of the property path.</param>
        /// <returns></returns>
        public static object GetPropertyValue( this object rootObj, string propertyPathName )
        {
            var propPath = propertyPathName.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            object obj = rootObj;
            Type objType = rootObj.GetType();

            while ( propPath.Any() && obj != null )
            {
                PropertyInfo property = objType.GetProperty( propPath.First() );
                if ( property != null )
                {
                    obj = property.GetValue( obj );
                    objType = property.PropertyType;
                    propPath = propPath.Skip( 1 ).ToList();
                }
                else
                {
                    obj = null;
                }
            }

            return obj;
        }

        /// <summary>
        /// Safely ToString() this item, even if it's null.
        /// </summary>
        /// <param name="obj">an object</param>
        /// <returns>The ToString or the empty string if the item is null.</returns>
        public static string ToStringSafe( this object obj )
        {
            if ( obj != null )
            {
                return obj.ToString();
            }
            return String.Empty;
        }

        #endregion

        #region Stream extension methods

        /// <summary>
        /// Reads entire stream and converts to byte array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static byte[] ReadBytesToEnd( this System.IO.Stream stream )
        {
            long originalPosition = 0;

            if ( stream.CanSeek )
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ( ( bytesRead = stream.Read( readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead ) ) > 0 )
                {
                    totalBytesRead += bytesRead;

                    if ( totalBytesRead == readBuffer.Length )
                    {
                        int nextByte = stream.ReadByte();
                        if ( nextByte != -1 )
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy( readBuffer, 0, temp, 0, readBuffer.Length );
                            Buffer.SetByte( temp, totalBytesRead, (byte)nextByte );
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if ( readBuffer.Length != totalBytesRead )
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy( readBuffer, 0, buffer, 0, totalBytesRead );
                }
                return buffer;
            }
            finally
            {
                if ( stream.CanSeek )
                {
                    stream.Position = originalPosition;
                }
            }
        }

        #endregion Stream extension methods
    }
}
