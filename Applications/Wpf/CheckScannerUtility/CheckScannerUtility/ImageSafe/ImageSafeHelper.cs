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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Rock;

namespace ImageSafeInterop
{
    /// <summary>
    /// With the Interop Process without having the Code directly in the same project
    /// When the app was closed the mtxmlmcr.dll would stay in memory
    /// This Static Helper Interfaces the MagTek image safe scanner
    /// </summary>
    public static class ImageSafeHelper
    {
        public enum DocType
        {
            CHECK,
            MSR,
            INVALID
        }

        public static DocType SelectedDocType { get; set; }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{

        //    if (g_hLogFile > 0)
        //        CloseHandle(g_hLogFile);
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}


        #region DLL Import

        #region Win32 DLL
        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern unsafe int CreateFile( string lpFileName,
                 UInt32 dwDesiredAccess, UInt32 dwShareMode,
                 UInt32 lpSecurityAttributes, UInt32 dwCreationDisposition,
                 UInt32 dwFlagsAndAttributes, int hTemplateFile );


        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern unsafe bool WriteFile( int hFile,
              byte[] pBuf, int nBytesToWrite, ref int nBytesWritten, IntPtr pOverlapped );
        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern unsafe bool CloseHandle( int hHandle );
        [DllImport( "shell32.dll", CharSet = CharSet.Ansi )]
        static extern bool ShellExecuteEx( ref SHELLEXECUTEINFO lpExecInfo );

        #endregion
        #region MTXMLMCR.DLL

        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRGetImage( string strDeviceName, string strImageID, byte[] imageBuf, ref int nBufLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRGetDevice( int dwDeviceContext, StringBuilder strDeviceName );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRQueryInfo( string strDeviceName, string strQueryParm, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRSendCommand( string strDeviceName, string strSendParm, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRSetValue( StringBuilder strOptions, string strSection, string strKey, string strValue, ref int nActualLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRGetValue( string strDocInfo, string strSection, string strKey, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRSetIndexValue( StringBuilder strOptions, string strSection, string strKey, int nIndex, string strValue, ref int nActualLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRGetIndexValue( string strDocInfo, string strSection, string strKey, int nIndex, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRProcessCheck( string strDeviceName, string strOptions, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRSetLogFileHandle( Int32 hLogHandle );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRSetLogEnable( int nEnable );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICROpenDevice( string strDeviceName );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        public static extern unsafe int MTMICRCloseDevice( string strDeviceName );

        #endregion
        #endregion

        #region Win32 constants declaration

        const UInt32 GENERIC_READ = 0x80000000;
        const UInt32 GENERIC_WRITE = 0x40000000;
        const short INVALID_HANDLE_VALUE = -1;
        const uint CREATE_NEW = 3;
        const uint CREATE_ALWAYS = 2;
        const uint OPEN_EXISTING = 3;
        const uint OPEN_ALWAYS = 4;
        const uint FILE_SHARE_READ = 0x1;
        const uint FILE_SHARE_WRITE = 0x2;
        const uint FILE_SHARE_DELETE = 0x4;
        const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;


        const int SW_HIDE = 0;
        const int SW_NORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_SHOWMINNOACTIVE = 7;
        const int SW_SHOWNA = 8;
        const int SW_RESTORE = 9;


        #endregion

        #region MICR constants

        private static int m_nTotalDevice;

        private const short MICR_ST_OK = ( short ) ImageSafeConst.MICR_ST_OK;
        private const short MICR_ST_DEVICE_NOT_FOUND = ( short ) ImageSafeConst.MICR_ST_DEVICE_NOT_FOUND;

        public enum ImageSafeConst
        {

            MICR_ST_OK = 0,
            MICR_ST_UNKNOWN_ERROR = 1,
            MICR_ST_BAD_PARAMETER = 2,
            MICR_ST_PENDING = 3,
            MICR_ST_PROCESS_CHECK_FAILED = 4,
            MICR_ST_OVERFLOW = 5,
            MICR_ST_DEVICE_NOT_FOUND = 6,
            MICR_ST_ACCESS_DENIED = 7,
            MICR_ST_DEVICE_NOT_RESPONDING = 8,
            MICR_ST_DEVICE_NOT_READY = 9,
            MICR_ST_HOPPER_EMPTY = 10,
            MICR_ST_FILE_COPY_ERROR = 11,
            MICR_ST_DEVICE_PROCESS_ERROR = 12,
            MICR_ST_IMAGE_NOT_SPECIFIED = 13,
            MICR_ST_IMAGE_NOT_FOUND = 14,
            MICR_ST_NO_CHECKS_PRESENT = 15,
            MICR_ST_NOT_ENOUGH_MEMORY = 16,
            MICR_ST_KEY_NOT_FOUND = 17,
            MICR_ST_SECTION_NOT_FOUND = 18,
            MICR_ST_INVALID_SECTION = 19,
            MICR_ST_INVALID_DATA = 20,
            MICR_ST_FUNCTION_NOT_SUPPORTED = 21,
            MICR_ST_MEMORY_ALLOCATION_PROBLEM = 22,
            MICR_ST_REQUEST_TIMEDOUT = 23,
            MICR_ST_QUERY_DATA_LENGTH_ERROR = 24,
            MICR_ST_DEVICE_CONNECTION_ERROR = 25,
            MICR_ST_DEVICE_NOT_OPEN = 26,
            MICR_ST_ERR_GET_DOM_POINTER = 27,
            MICR_ST_ERR_LOAD_XML = 28,
            MICR_ST_KEY_NUMBER_NOT_FOUND = 29,
            MICR_ST_ERR_INTERNET_CONNECT = 30,
            MICR_ST_ERR_HTTP_OPEN_REQUEST = 31,
            MICR_ST_ERR_HTTP_SEND_REQUEST = 32,
            MICR_ST_ERR_CREATE_EVENT = 33,
            MICR_ST_ERR_DOM_CREATE_NODE = 34,
            MICR_ST_ERR_DOM_QUERY_INTERFACE = 35,
            MICR_ST_ERR_DOM_ADD_KEY = 36,
            MICR_ST_ERR_DOM_APPEND_CHILD = 37,
            MICR_ST_ERR_DOM_GET_DOCUMENT_ELEMENT = 38,
            MICR_ST_ERR_DOM_GET_XML = 39,
            MICR_ST_ERR_DOM_GET_ITEM = 40,
            MICR_ST_ERR_DOM_GET_CHILD_NODES = 41,
            MICR_ST_ERR_DOM_GET_BASE_NAME = 42,
            MICR_ST_ERR_DOM_GET_LENGTH = 43,
            MICR_ST_ERR_DOM_GET_ELEMENT_BY_TAG_NAME = 44,
            MICR_ST_ERR_DOM_GET_TEXT = 45,
            MICR_ST_ERR_DOM_PUT_TEXT = 46,
            MICR_ST_ERR_HTTP_QUERY_INFO = 47,
            MICR_ST_INSUFFICIENT_DATA = 48,
            MICR_ST_BAD_HTTP_CONNECTION = 49,
            MICR_ST_CONTENT_ZERO_LENGTH = 50,
            MICR_ST_BAD_DEVICE_NAME = 51,
            MICR_ST_BAD_DATA = 52,
            MICR_ST_BAD_SECTION_NAME = 53,
            MICR_ST_BAD_KEY_NAME = 54,
            MICR_ST_BAD_VALUE_BUFFER = 55,
            MICR_ST_BAD_BUFFER_LENGTH = 56,
            MICR_ST_BAD_QUERY_PARM = 57,
            MICR_ST_BAD_IMAGE_NAME = 58,
            MICR_ST_BAD_BUFFER = 59,
            MICR_ST_BAD_BUFFER_SIZE = 60,
            MICR_ST_CONNECT_REQUEST_TIMEDOUT = 61,
            MICR_ST_INSUFFICIENT_DISKSPACE = 62,
            MICR_ST_MSXML_FAILED = 63,
            MICR_ST_QUERY_CONTENT_ERROR = 64,
            MICR_ST_ERR_INTERNET_CONNECTION = 65,
            MICR_ST_BAD_DEVICE_IP_OR_DOMAIN_NAME = 66,

            MICR_ST_USB_GET_DATA_FAILED = 67,

            MICR_ST_INET_GET_DATA_FAILED = 68,
            MICR_ST_HTTP_HEADER_NOT_FOUND = 69,
        }


        public static string DocInfo { get; set; }
        public static string CurrentDeviceName { get; set; }
        public static string Options { get; private set; }
        #endregion

        #region Private Members

        #endregion

        #region Public Members

        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            public String lpVerb;
            public String lpFile;
            public String lpParameters;
            public String lpDirectory;
            public int nShow;
            public int hInstApp;
            public int lpIDList;
            public String lpClass;
            public int hkeyClass;
            public uint dwHotKey;
            public int hIcon;
            public int hProcess;
        }
        public static bool OpenDevice()
        {
            try
            {
                ImageSafeHelper.CurrentDeviceName = "ImageSafe.ImageSafe001";

                var status = ImageSafeHelper.MTMICROpenDevice( ImageSafeHelper.CurrentDeviceName );
                if ( status == 0 )
                {
                    return true;
                }
                return false;
            }
            catch
            {
                // if we dont have a imagesafe installed dont throw an exception
                //just return false
                return false;
            }

        }

        public static void CloseDevice()
        {
            var status = MTMICRCloseDevice( "ImageSafe.ImageSafe001" );

        }
        private static void GetDeviceList()
        {
            byte nTotalDev = 1;
            List<string> deviceNames = new List<string>();

            try
            {
                string strDeviceName;
                StringBuilder str1 = new StringBuilder();
                str1.Capacity = 256;
                int nRetCode = ( int ) MICR_ST_DEVICE_NOT_FOUND;
                nRetCode = MTMICRGetDevice( nTotalDev++, str1 );
                if ( nRetCode != ( int ) MICR_ST_DEVICE_NOT_FOUND )

                    while ( nRetCode != ( int ) MICR_ST_DEVICE_NOT_FOUND )
                    {
                        strDeviceName = str1.ToString();
                        if ( strDeviceName.Length > 1 )
                        {
                            deviceNames.Add( strDeviceName );
                            //comboDeviceName.Items.Add( strDeviceName );
                            m_nTotalDevice++;
                        }
                        nRetCode = ( int ) MICR_ST_DEVICE_NOT_FOUND;
                        nRetCode = MTMICRGetDevice( nTotalDev++, str1 );
                    }
            }
            catch ( NullReferenceException ex )
            {
                Debug.WriteLine( $"GetDeviceList {ex}" );
            }
        }
        public static void ProcessDocument( Action<CheckData> callback )
        {
            var checkData = new CheckData();
            SelectedDocType = DocType.CHECK;

            //Process Document
            if ( OpenDevice() )
            {
                int nRet;
                string strLog;
                string strTmp;

                nRet = SetupOptions();

                if ( nRet != MICR_ST_OK )
                {
                    strLog = "Setup Options FAILED...";
                    checkData.HasError = true;
                    checkData.ErrorMessage = strLog;
                }

                StringBuilder strResponse = new StringBuilder();
                strResponse.Capacity = 4096;
                int nResponseLength = 4096;
                DocInfo = "";
                nRet = MTMICRProcessCheck( CurrentDeviceName, Options, strResponse, ref nResponseLength );

                if ( nRet == ( int ) MICR_ST_OK )
                {
                    DocInfo = strResponse.ToString();


                    nRet = MTMICRGetValue( DocInfo, "CommandStatus", "ReturnCode", strResponse, ref nResponseLength );
                    strTmp = strResponse.ToString();

                    int nReturnCode = Convert.ToInt32( strTmp );
                    strLog = "Process Check return code " + nReturnCode;



                    if ( nReturnCode == 0 )
                    {
                        if ( SelectedDocType == DocType.CHECK )
                        {
                            SetImageData( ref checkData );
                            SetCheckData( ref checkData );
                            callback.DynamicInvoke( checkData );
                            return;
                        }
                        else
                        {
                            checkData.HasError = true;
                            checkData.ErrorMessage = strLog;
                            callback.DynamicInvoke( checkData );
                            //this.HandleError(handler, "Process Check Feeder not Set to Check.", checkData);
                            return;
                        }

                    }
                    else if ( nReturnCode == 250 )
                    {
                        checkData.HasError = true;
                        checkData.ErrorMessage = "Check Waiting Timeout";
                        callback.DynamicInvoke( checkData );

                        return;
                    }
                    else
                    {
                        checkData.HasError = true;
                        checkData.ErrorMessage = "Process Check Failed";
                        callback.DynamicInvoke( checkData );
                        //this.HandleError(handler, strLog, checkData);
                        return;
                    }
                }
                else
                {
                    strLog = "MTMICRProcessCheck return code " + nRet;
                    ImageSafeConst status = ( ImageSafeConst ) nRet;
                    checkData.HasError = true;
                    checkData.ErrorMessage = $"Unable to connect to scanner ( {status.ConvertToString()})";
                    callback.DynamicInvoke( checkData );

                    //this.HandleError(handler, strLog, checkData);
                    return;
                }

            }
            callback.DynamicInvoke( checkData );
            //HandleError(handler, "No Current Device Name found.", checkData);
        }

        private static void SetImageData( ref CheckData checkData )
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            int nImageSize;
            string strTmp;
            string strImageID;


            nRet = MTMICRGetIndexValue( DocInfo, "ImageInfo", "ImageSize", 1, strResponse, ref nResponseLength );
            strTmp = strResponse.ToString();
            nImageSize = Convert.ToInt32( strTmp );

            if ( nImageSize > 0 )
            {

                nRet = MTMICRGetIndexValue( DocInfo, "ImageInfo", "ImageURL", 1, strResponse, ref nResponseLength );
                strImageID = strResponse.ToString();
                string strLog = "Image size =" + nImageSize + "ImageID = " + strImageID;
                Trace.WriteLine( strLog );

                byte[] imageBuf = new byte[nImageSize];

                nRet = MTMICRGetImage( CurrentDeviceName, strImageID, imageBuf, ref nImageSize );
                if ( nRet == ( int ) MICR_ST_OK )
                {
                    checkData.ImageData = imageBuf;
                    int nActualSize = nImageSize;
                    strLog = "NumOfBytes to write =" + nActualSize;

                    Trace.WriteLine( strLog );

                    IntPtr pOverlapped = IntPtr.Zero;
                }
                else
                {
                    Debug.WriteLine( $"GetImage FAILED {( ( ImageSafeConst ) nRet ).ConvertToString()}" );
                }

            }
        }

        private static void SetCheckData( ref CheckData checkData )
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            string strTmp;
            if ( SelectedDocType == DocType.MSR )
            {
                checkData.OtherData = string.Empty;
                checkData.RoutingNumber = string.Empty;
                checkData.AccountNumber = string.Empty;
                checkData.CheckNumber = string.Empty;

            }
            else
            {
                nRet = MTMICRGetValue( DocInfo, "DocInfo", "MICRRaw", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.ScannedCheckMicrData = strTmp;

                nRet = MTMICRGetValue( DocInfo, "DocInfo", "MICRTransit", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.RoutingNumber = strTmp;

                nRet = MTMICRGetValue( DocInfo, "DocInfo", "MICRAcct", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.AccountNumber = strTmp;

                nRet = MTMICRGetValue( DocInfo, "DocInfo", "MICRSerNum", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.CheckNumber = strTmp;
            }
        }
        public static string QueryDevice( string queryInfo )
        {

            StringBuilder strResults = new StringBuilder();

            int nRet;
            int nLength = 4096;
            strResults.Capacity = 4096;

            switch ( queryInfo )
            {
                case "DeviceCapabilities":
                    nRet = MTMICRQueryInfo( CurrentDeviceName, "DeviceCapabilities", strResults, ref nLength );
                    break;
                case "DeviceStatus":
                    nRet = MTMICRQueryInfo( CurrentDeviceName, "DeviceStatus", strResults, ref nLength );
                    break;
                case "DeviceUsage":
                    nRet = MTMICRQueryInfo( CurrentDeviceName, "DeviceUsage", strResults, ref nLength );
                    break;


            }

            return strResults.ToString();
        }

        private static int SetupOptions()
        {
            int nRet;

            StringBuilder strOptions = new StringBuilder();
            strOptions.Capacity = 4096;

            int nActualLength = 4096;

            nRet = MTMICRSetValue( strOptions, "Application", "Transfer", "HTTP", ref nActualLength );

            if ( nRet != ( int ) MICR_ST_OK )
                return -1;

            nRet = MTMICRSetValue( strOptions, "Application", "DocUnits", "ENGLISH", ref nActualLength );

            if ( nRet != ( int ) MICR_ST_OK )
                return -1;

            nRet = MTMICRSetValue( strOptions, "ProcessOptions", "DocFeedTimeout", "10000", ref nActualLength );

            if ( nRet != ( int ) MICR_ST_OK )
                return -1;

            if ( SelectedDocType == DocType.MSR )
            {
                nRet = MTMICRSetValue( strOptions, "ProcessOptions", "DocFeed", "MSR", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;
                Options = strOptions.ToString();
                return nRet;
            }
            else
            {
                nRet = MTMICRSetValue( strOptions, "ProcessOptions", "DocFeed", "MANUAL", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;

                nRet = MTMICRSetValue( strOptions, "ImageOptions", "Number", "1", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;
                nRet = MTMICRSetIndexValue( strOptions, "ImageOptions", "ImageSide", 1, "FRONT", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;

                nRet = MTMICRSetIndexValue( strOptions, "ImageOptions", "ImageColor", 1, "GRAY8", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;


                nRet = MTMICRSetIndexValue( strOptions, "ImageOptions", "Resolution", 1, "200x200", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;

                nRet = MTMICRSetIndexValue( strOptions, "ImageOptions", "Compression", 1, "JPEG", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;

                nRet = MTMICRSetIndexValue( strOptions, "ImageOptions", "FileType", 1, "JPG", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;

                nRet = MTMICRSetValue( strOptions, "ProcessOptions", "ReadMICR", "E13B", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;

                nRet = MTMICRSetValue( strOptions, "ProcessOptions", "MICRFmtCode", "6200", ref nActualLength );
                if ( nRet != ( int ) MICR_ST_OK )
                    return -1;
                Options = strOptions.ToString();
                return nRet;

            }

        }

        #endregion

        #region Private Methods
        #endregion
    }
}
