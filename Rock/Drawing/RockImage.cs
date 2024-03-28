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
using Rock.Data;
using Rock.Model;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Drawing;
using SixLabors.ImageSharp;
using System.Threading;
using System;
using System.Web;
using System.IO;
using Rock.Web.Cache;
using System.Data.Entity;
using Microsoft.Ajax.Utilities;
using Rock.Security;

namespace Rock.Drawing
{
    internal static class RockImage
    {
        /// <summary>
        /// Creates a image from a mask using the fill color provided.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="fillColor"></param>
        /// <returns></returns>
        public static Image CreateMaskImage( Image mask, string fillColor )
        {
            // Logic for this comes from: https://stackoverflow.com/questions/52875516/how-to-compose-two-images-using-source-in-composition

            // Create the "background" which is the solid color that we want our mask  to cut out
            var maskColor = RgbaVector.FromHex( fillColor );
            var background = new Image<RgbaVector>( mask.Width, mask.Height, maskColor );

            var processorCreator = new DrawImageProcessor(
                mask,
                Point.Empty,
                PixelColorBlendingMode.Normal,
                PixelAlphaCompositionMode.DestIn, // The destination where the destination and source overlap.
                1f
            );

            var pxProcessor = processorCreator.CreatePixelSpecificProcessor(
                SixLabors.ImageSharp.Configuration.Default,
                background,
                mask.Bounds() );

            pxProcessor.Execute();

            return background;
        }

        public static Image CreateSolidImage( int width, int height, string color )
        {
            return new Image<Rgba32>( width, height, Color.ParseHex( color ) );
        }

        /// <summary>
        /// Returns a Image Sharp Person Image from a BinaryFile ID.
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns>null if the image could not be retrieved for any reason, or if the requested image file is not of type PERSON_IMAGE.</returns>
        public static Image GetPersonImageFromBinaryFileService( int photoId )
        {
            var binaryFile = new BinaryFileService( new RockContext() ).Get( photoId );

            // This will only return a file of type PERSON_IMAGE.
            if ( binaryFile == null )
            {
                return null;
            }

            if ( !IsAuthorized( binaryFile ) )
            {
                throw new Exception( "The person is not authorized to view it" );
            }

            try
            {
                // Load image from stream if the file exists and can be loaded from the stream
                if ( binaryFile.ContentStream != null )
                {
                    return Image.Load<Rgba32>( binaryFile.ContentStream );
                }
            }

            // if the retrival fails due to some exception, swallow the exception
            catch { }

            // There was a problem with the content in the binary file so return a blank image
            return null;
        }

        /// <summary>
        /// Determines whether the current user is authorized to view the Person Image.
        /// Returns true without security check if the Person Image BinaryFileType has RequiresViewSecurity set to false.
        /// The file type will need to be checked before fetching the file (see RockImage.GetPersonImageFromBinaryFileService())
        /// Validates security and the BinaryFileType if RequiresViewSecurity is true.
        /// </summary>
        /// <param name="binaryFile"></param>
        /// <returns>
        ///   <c>true</c> if the current user is authorized; otherwise, <c>false</c>.</returns>
        private static Boolean IsAuthorized( BinaryFile binaryFile )
        {
            if ( binaryFile.BinaryFileType.RequiresViewSecurity == false )
            {
                return true;
            }

            var currentUser = new UserLoginService( new RockContext() ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser?.Person;
            var parentEntityAllowsView = binaryFile.ParentEntityAllowsView( currentPerson );

            // If no parent entity is specified then check if there is security on the BinaryFileType
            if ( parentEntityAllowsView == null )
            {
                if ( !binaryFile.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    return false;
                }
            }

            // Check if there is parent security and use it if it exists, otherwise return true.
            return parentEntityAllowsView ?? true;
        }
    }
}
