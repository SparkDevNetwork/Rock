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
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using ImageResizer;

using Microsoft.Extensions.Logging;

using Rock.Data;

namespace Rock.Model
{
    public partial class BinaryFile
    {
        internal class SaveHook : EntitySaveHook<BinaryFile>
        {
            protected override void PreSave()
            {
                if ( Entry.State == EntityContextState.Deleted )
                {
                    if ( Entity.StorageProvider != null )
                    {
                        Entity.BinaryFileTypeId = Entry.OriginalValues[nameof( Entity.BinaryFileTypeId )].ToString().AsInteger();

                        try
                        {
                            Entity.StorageProvider.DeleteContent( Entity );
                        }
                        catch ( Exception ex )
                        {
                            // If an exception occurred while trying to delete provider's file, log the exception, but continue with the delete.
                            ExceptionLogService.LogException( ex );
                        }

                        Entity.BinaryFileTypeId = null;
                    }
                }
                else
                {
                    if ( Entity.BinaryFileType == null && Entity.BinaryFileTypeId.HasValue )
                    {
                        Entity.BinaryFileType = new BinaryFileTypeService( ( RockContext ) DbContext ).Get( Entity.BinaryFileTypeId.Value );
                    }

                    if ( Entity.MimeType.StartsWith( "image/" ) )
                    {
                        try
                        {
                            using ( Bitmap bm = new Bitmap( Entity.ContentStream ) )
                            {
                                if ( bm != null )
                                {
                                    Entity.Width = bm.Width;
                                    Entity.Height = bm.Height;
                                }
                            }
                            Entity.ContentStream.Seek( 0, SeekOrigin.Begin );

                            var binaryFileType = Entity.BinaryFileType;
                            var binaryFileTypeMaxHeight = binaryFileType.MaxHeight??0;
                            var binaryFileTypeMaxWidth = binaryFileType.MaxWidth??0;
                            var binaryFileTypeMaxHeightIsValid = binaryFileTypeMaxHeight > 0;
                            var binaryFileTypeMaxWidthIsValid = binaryFileTypeMaxWidth > 0;
                            var binaryFileTypeDimensionsAreValid = binaryFileTypeMaxHeightIsValid && binaryFileTypeMaxWidthIsValid;

                            ResizeSettings settings = new ResizeSettings();
                            MemoryStream resizedStream = new MemoryStream();
                            if ( ( binaryFileTypeMaxWidthIsValid && binaryFileTypeMaxWidth < Entity.Width )
                                || ( binaryFileTypeMaxHeightIsValid && binaryFileTypeMaxHeight < Entity.Height ) )
                            {
                                /* How to handle aspect-ratio conflicts between the image and width+height.
                                     'pad' adds whitespace,
                                     'crop' crops minimally,
                                     'carve' uses seam carving,
                                     'stretch' loses aspect-ratio, stretching the image.
                                     'max' behaves like maxwidth/maxheight
                                */

                                settings.Add( "mode", "max" );

                                // Height and width are both set.
                                if ( binaryFileTypeDimensionsAreValid )
                                {
                                    // A valid max height and width but the max height is greater or equal than the width.
                                    if ( binaryFileTypeMaxHeight >= binaryFileTypeMaxWidth )
                                    {
                                        settings.Add( "height", binaryFileTypeMaxHeight.ToString() );
                                    }

                                    // A valid max height and width but the max height is less or equal the width.
                                    else if ( binaryFileTypeMaxHeight <= binaryFileTypeMaxWidth )
                                    {
                                        settings.Add( "width", binaryFileTypeMaxWidth.ToString() );
                                    }
                                }
                                else
                                {
                                    // A valid max height but less than the binary file height.
                                    if ( binaryFileTypeMaxHeightIsValid && binaryFileTypeMaxHeight < Entity.Height )
                                    {
                                        settings.Add( "height", binaryFileTypeMaxHeight.ToString() );
                                    }
                                    else
                                    {
                                        // A Valid max width.
                                        settings.Add( "width", binaryFileTypeMaxWidth.ToString() );
                                    }
                                }

                                if ( settings.HasKeys() )
                                {
                                    ImageBuilder.Current.Build( Entity.ContentStream, resizedStream, settings );
                                    Entity.ContentStream = resizedStream;

                                    using ( Bitmap bm = new Bitmap( Entity.ContentStream ) )
                                    {
                                        if ( bm != null )
                                        {
                                            Entity.Width = bm.Width;
                                            Entity.Height = bm.Height;
                                        }
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            Logger.LogError( ex, "Error trying to resize the file {0}.", Entity?.FileName );
                        }
                    }

                    if ( Entry.State == EntityContextState.Added )
                    {
                        // when a file is saved (unless it is getting Deleted/Saved), it should use the StoredEntityType that is associated with the BinaryFileType
                        if ( Entity.BinaryFileType != null )
                        {
                            // Persist the storage type
                            Entity.StorageEntityTypeId = Entity.BinaryFileType.StorageEntityTypeId;

                            // Persist the storage type's settings specific to this binary file type
                            var settings = new Dictionary<string, string>();
                            if ( Entity.BinaryFileType.Attributes == null )
                            {
                                Entity.BinaryFileType.LoadAttributes();
                            }
                            foreach ( var attributeValue in Entity.BinaryFileType.AttributeValues )
                            {
                                settings.Add( attributeValue.Key, attributeValue.Value.Value );
                            }
                            Entity.StorageEntitySettings = settings.ToJson();

                            if ( Entity.StorageProvider != null )
                            {
                                // save the file to the provider's new storage medium, and if the medium returns a filesize, save that value.
                                long? outFileSize = null;
                                Entity.StorageProvider.SaveContent( Entity, out outFileSize );
                                if ( outFileSize.HasValue )
                                {
                                    Entity.FileSize = outFileSize;
                                }

                                Entity.Path = Entity.StorageProvider.GetPath( Entity );
                            }
                            else
                            {
                                throw new Rock.Web.FileUploadException( "A storage provider has not been registered for this file type or the current storage provider is inactive.", System.Net.HttpStatusCode.BadRequest );
                            }
                        }
                    }


                    else if ( Entry.State == EntityContextState.Modified )
                    {
                        // when a file is saved (unless it is getting Deleted/Added), 
                        // it should use the StorageEntityType that is associated with the BinaryFileType
                        if ( Entity.BinaryFileType != null )
                        {
                            // if the storage provider changed, or any of its settings specific 
                            // to the binary file type changed, delete the original provider's content
                            if ( Entity.StorageEntityTypeId.HasValue && Entity.BinaryFileType.StorageEntityTypeId.HasValue )
                            {
                                var settings = new Dictionary<string, string>();
                                if ( Entity.BinaryFileType.Attributes == null )
                                {
                                    Entity.BinaryFileType.LoadAttributes();
                                }
                                foreach ( var attributeValue in Entity.BinaryFileType.AttributeValues )
                                {
                                    settings.Add( attributeValue.Key, attributeValue.Value.Value );
                                }
                                string settingsJson = settings.ToJson();

                                if ( Entity.StorageProvider != null && (
                                    Entity.StorageEntityTypeId.Value != Entity.BinaryFileType.StorageEntityTypeId.Value ||
                                    Entity.StorageEntitySettings != settingsJson ) )
                                {

                                    var ms = new MemoryStream();
                                    Entity.ContentStream.Position = 0;
                                    Entity.ContentStream.CopyTo( ms );
                                    Entity.ContentStream.Dispose();

                                    // Delete the current provider's storage
                                    Entity.StorageProvider.DeleteContent( Entity );

                                    // Set the new storage provider with its settings
                                    Entity.StorageEntityTypeId = Entity.BinaryFileType.StorageEntityTypeId;
                                    Entity.StorageEntitySettings = settingsJson;

                                    Entity.ContentStream = new MemoryStream();
                                    ms.Position = 0;
                                    ms.CopyTo( Entity.ContentStream );
                                    Entity.ContentStream.Position = 0;
                                    Entity.FileSize = Entity.ContentStream.Length;
                                }
                            }
                        }

                        if ( Entity.ContentIsDirty && Entity.StorageProvider != null )
                        {
                            /*
                              SK - 12/11/2021
                              Path should always be reset in case when there is any change in Storage Provider from previous value. Otherwise new storage provider may still be refering the older path.
                            */
                            Entity.Path = null;
                            long? fileSize = null;
                            Entity.StorageProvider.SaveContent( Entity, out fileSize );

                            Entity.FileSize = fileSize;
                            Entity.Path = Entity.StorageProvider.GetPath( Entity );
                        }
                    }
                }

                base.PreSave();
            }
        }
    }
}
