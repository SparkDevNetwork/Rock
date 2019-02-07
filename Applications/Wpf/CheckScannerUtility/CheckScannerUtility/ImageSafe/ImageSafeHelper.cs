
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern unsafe int CreateFile(string lpFileName,
                 UInt32 dwDesiredAccess, UInt32 dwShareMode,
                 UInt32 lpSecurityAttributes, UInt32 dwCreationDisposition,
                 UInt32 dwFlagsAndAttributes, int hTemplateFile);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern unsafe bool WriteFile(int hFile,
              byte[] pBuf, int nBytesToWrite, ref int nBytesWritten, IntPtr pOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern unsafe bool CloseHandle(int hHandle);
        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        #endregion
        #region MTXMLMCR.DLL

        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRGetImage(string strDeviceName, string strImageID, byte[] imageBuf, ref int nBufLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRGetDevice(int dwDeviceContext, StringBuilder strDeviceName);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRQueryInfo(string strDeviceName, string strQueryParm, StringBuilder strResponse, ref int nResponseLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRSendCommand(string strDeviceName, string strSendParm, StringBuilder strResponse, ref int nResponseLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRSetValue(StringBuilder strOptions, string strSection, string strKey, string strValue, ref int nActualLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRGetValue(string strDocInfo, string strSection, string strKey, StringBuilder strResponse, ref int nResponseLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRSetIndexValue(StringBuilder strOptions, string strSection, string strKey, int nIndex, string strValue, ref int nActualLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRGetIndexValue(string strDocInfo, string strSection, string strKey, int nIndex, StringBuilder strResponse, ref int nResponseLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRProcessCheck(string strDeviceName, string strOptions, StringBuilder strResponse, ref int nResponseLength);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRSetLogFileHandle(Int32 hLogHandle);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRSetLogEnable(int nEnable);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICROpenDevice(string strDeviceName);
        [DllImport("mtxmlmcr.dll", SetLastError = true)]
        public static extern unsafe int MTMICRCloseDevice(string strDeviceName);

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

        public const short MICR_ST_OK = 0;
        public const short MICR_ST_UNKNOWN_ERROR = 1;
        public const short MICR_ST_BAD_PARAMETER = 2;
        public const short MICR_ST_PENDING = 3;
        public const short MICR_ST_PROCESS_CHECK_FAILED = 4;
        public const short MICR_ST_OVERFLOW = 5;
        public static short MICR_ST_DEVICE_NOT_FOUND = 6;
        private static int m_nTotalDevice;
        public const short MICR_ST_ACCESS_DENIED = 7;
        public const short MICR_ST_DEVICE_NOT_RESPONDING = 8;
        public const short MICR_ST_DEVICE_NOT_READY = 9;
        public const short MICR_ST_HOPPER_EMPTY = 10;
        public const short MICR_ST_FILE_COPY_ERROR = 11;
        public const short MICR_ST_DEVICE_PROCESS_ERROR = 12;
        public const short MICR_ST_IMAGE_NOT_SPECIFIED = 13;
        public const short MICR_ST_IMAGE_NOT_FOUND = 14;
        public const short MICR_ST_NO_CHECKS_PRESENT = 15;
        public const short MICR_ST_NOT_ENOUGH_MEMORY = 16;
        public const short MICR_ST_KEY_NOT_FOUND = 17;
        public const short MICR_ST_SECTION_NOT_FOUND = 18;
        public const short MICR_ST_INVALID_SECTION = 19;
        public const short MICR_ST_INVALID_DATA = 20;
        public const short MICR_ST_FUNCTION_NOT_SUPPORTED = 21;
        public const short MICR_ST_MEMORY_ALLOCATION_PROBLEM = 22;
        public const short MICR_ST_REQUEST_TIMEDOUT = 23;
        public const short MICR_ST_QUERY_DATA_LENGTH_ERROR = 24;
        public const short MICR_ST_DEVICE_CONNECTION_ERROR = 25;
        public const short MICR_ST_DEVICE_NOT_OPEN = 26;
        public const short MICR_ST_ERR_GET_DOM_POINTER = 27;
        public const short MICR_ST_ERR_LOAD_XML = 28;
        public const short MICR_ST_KEY_NUMBER_NOT_FOUND = 29;
        public const short MICR_ST_ERR_INTERNET_CONNECT = 30;
        public const short MICR_ST_ERR_HTTP_OPEN_REQUEST = 31;
        public const short MICR_ST_ERR_HTTP_SEND_REQUEST = 32;
        public const short MICR_ST_ERR_CREATE_EVENT = 33;
        public const short MICR_ST_ERR_DOM_CREATE_NODE = 34;
        public const short MICR_ST_ERR_DOM_QUERY_INTERFACE = 35;
        public const short MICR_ST_ERR_DOM_ADD_KEY = 36;
        public const short MICR_ST_ERR_DOM_APPEND_CHILD = 37;
        public const short MICR_ST_ERR_DOM_GET_DOCUMENT_ELEMENT = 38;
        public const short MICR_ST_ERR_DOM_GET_XML = 39;
        public const short MICR_ST_ERR_DOM_GET_ITEM = 40;
        public const short MICR_ST_ERR_DOM_GET_CHILD_NODES = 41;
        public const short MICR_ST_ERR_DOM_GET_BASE_NAME = 42;
        public const short MICR_ST_ERR_DOM_GET_LENGTH = 43;
        public const short MICR_ST_ERR_DOM_GET_ELEMENT_BY_TAG_NAME = 44;
        public const short MICR_ST_ERR_DOM_GET_TEXT = 45;
        public const short MICR_ST_ERR_DOM_PUT_TEXT = 46;
        public const short MICR_ST_ERR_HTTP_QUERY_INFO = 47;
        public const short MICR_ST_INSUFFICIENT_DATA = 48;
        public const short MICR_ST_BAD_HTTP_CONNECTION = 49;
        public const short MICR_ST_CONTENT_ZERO_LENGTH = 50;
        public const short MICR_ST_BAD_DEVICE_NAME = 51;
        public const short MICR_ST_BAD_DATA = 52;
        public const short MICR_ST_BAD_SECTION_NAME = 53;
        public const short MICR_ST_BAD_KEY_NAME = 54;
        public const short MICR_ST_BAD_VALUE_BUFFER = 55;
        public const short MICR_ST_BAD_BUFFER_LENGTH = 56;
        public const short MICR_ST_BAD_QUERY_PARM = 57;
        public const short MICR_ST_BAD_IMAGE_NAME = 58;
        public const short MICR_ST_BAD_BUFFER = 59;
        public const short MICR_ST_BAD_BUFFER_SIZE = 60;
        public const short MICR_ST_CONNECT_REQUEST_TIMEDOUT = 61;
        public const short MICR_ST_INSUFFICIENT_DISKSPACE = 62;
        public const short MICR_ST_MSXML_FAILED = 63;
        public const short MICR_ST_QUERY_CONTENT_ERROR = 64;
        public const short MICR_ST_ERR_INTERNET_CONNECTION = 65;
        public const short MICR_ST_BAD_DEVICE_IP_OR_DOMAIN_NAME = 66;

        public const short MICR_ST_USB_GET_DATA_FAILED = 67;

        public const short MICR_ST_INET_GET_DATA_FAILED = 68;
        public const short MICR_ST_HTTP_HEADER_NOT_FOUND = 69;


        public static string DocInfo { get; set; }
        public static string CurrentDeviceName { get; set; }
        public static string Options { get; private set; }
        public static StringBuilder ErrorLog = new StringBuilder();
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

            ImageSafeHelper.CurrentDeviceName = "ImageSafe.ImageSafe001";

            var status = ImageSafeHelper.MTMICROpenDevice(ImageSafeHelper.CurrentDeviceName);
            if (status == 0)
            {
                return true;
            }
            return false;
        }

        public static void CloseDevice()
        {
            var status = MTMICRCloseDevice("ImageSafe.ImageSafe001");

        }
        private static void GetDeviceList()
        {
            byte nTotalDev = 1;
            List<string> deviceNames = new List<string>();

            try
            {
                string strLogTxt = "Enumerating Configured Devices...";

                string strDeviceName;
                StringBuilder str1 = new StringBuilder();
                str1.Capacity = 256;
                int nRetCode = (int)MICR_ST_DEVICE_NOT_FOUND;
                nRetCode = MTMICRGetDevice(nTotalDev++, str1);
                if (nRetCode != (int)MICR_ST_DEVICE_NOT_FOUND)

                    while (nRetCode != (int)MICR_ST_DEVICE_NOT_FOUND)
                    {
                        strDeviceName = str1.ToString();
                        if (strDeviceName.Length > 1)
                        {
                            deviceNames.Add(strDeviceName);
                            //comboDeviceName.Items.Add( strDeviceName );
                            m_nTotalDevice++;
                        }
                        nRetCode = (int)MICR_ST_DEVICE_NOT_FOUND;
                        nRetCode = MTMICRGetDevice(nTotalDev++, str1);
                    }
            }
            catch (NullReferenceException e)
            {

            }
        }
        public static void ProcessDocument(Action<CheckData> callback)
        {
            var checkData = new CheckData();
            SelectedDocType = DocType.CHECK;

            //Process Document
            if (OpenDevice())
            {
                int nRet;
                string strLog;
                string strTmp;

                nRet = SetupOptions();

                if (nRet != MICR_ST_OK)
                {
                    strLog = "Setup Options FAILED...";
                    //PrintStatus(strLog);
                    ErrorLog.AppendLine(strLog);
                    checkData.Errors = ErrorLog;
                }

                strLog = "Begin Process Options Info...";

                ErrorLog.AppendLine(strLog);


                strLog = "End Process Options Info...";
                ErrorLog.AppendLine(strLog);

                StringBuilder strResponse = new StringBuilder();
                strResponse.Capacity = 4096;
                int nResponseLength = 4096;
                DocInfo = "";
                nRet = MTMICRProcessCheck(CurrentDeviceName, Options, strResponse, ref nResponseLength); ;
                if (nRet == (int)MICR_ST_OK)
                {
                    DocInfo = strResponse.ToString();


                    nRet = MTMICRGetValue(DocInfo, "CommandStatus", "ReturnCode", strResponse, ref nResponseLength);
                    strTmp = strResponse.ToString();

                    int nReturnCode = Convert.ToInt32(strTmp);
                    strLog = "Process Check return code " + nReturnCode;



                    if (nReturnCode == 0)
                    {
                        if (SelectedDocType == DocType.CHECK)
                        {
                            SetImageData(ref checkData);
                            SetCheckData(ref checkData);
                            callback.DynamicInvoke(checkData);
                            return;
                        }
                        else
                        {
                            checkData.HasError = true;
                            ErrorLog.AppendLine(strLog);
                            checkData.Errors = ErrorLog;
                            callback.DynamicInvoke(checkData);
                            //this.HandleError(handler, "Process Check Feeder not Set to Check.", checkData);
                            return;
                        }

                    }
                    else if (nReturnCode == 250)
                    {
                        ErrorLog.AppendLine(strLog);
                        ErrorLog.AppendLine("Check Waiting Timeout!");
                        checkData.HasError = true;
                        checkData.Errors = ErrorLog;
                        callback.DynamicInvoke(checkData);
                                         
                        return;
                    }
                    else
                    {
                        checkData.HasError = true;
                        checkData.Errors = ErrorLog;
                        ErrorLog.AppendLine("Process Check FAILED!");
                        callback.DynamicInvoke(checkData);
                        //this.HandleError(handler, strLog, checkData);
                        return;
                    }
                }
                else
                {
                    strLog = "MTMICRProcessCheck return code " + nRet;

                    //this.HandleError(handler, strLog, checkData);
                    return;
                }

            }
            checkData.Errors = ErrorLog;
            callback.DynamicInvoke(checkData);

            //HandleError(handler, "No Current Device Name found.", checkData);
        }

        private static void SetImageData(ref CheckData checkData)
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            int nImageSize;
            string strTmp;
            string strImageID;


            nRet = MTMICRGetIndexValue(DocInfo, "ImageInfo", "ImageSize", 1, strResponse, ref nResponseLength);
            strTmp = strResponse.ToString();
            nImageSize = Convert.ToInt32(strTmp);

            if (nImageSize > 0)
            {

                nRet = MTMICRGetIndexValue(DocInfo, "ImageInfo", "ImageURL", 1, strResponse, ref nResponseLength);
                strImageID = strResponse.ToString();
                string strLog = "Image size =" + nImageSize + "ImageID = " + strImageID;
                Trace.WriteLine(strLog);

                byte[] imageBuf = new byte[nImageSize];

                nRet = MTMICRGetImage(CurrentDeviceName, strImageID, imageBuf, ref nImageSize);
                if (nRet == (int)MICR_ST_OK)
                {
                    checkData.ImageData = imageBuf;
                    int nActualSize = nImageSize;
                    strLog = "NumOfBytes to write =" + nActualSize;

                    Trace.WriteLine(strLog);

                    IntPtr pOverlapped = IntPtr.Zero;
                }
                else
                {
                    Trace.WriteLine("GetImage FAILED");
                }

            }
        }

        private static void SetCheckData(ref CheckData checkData)
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            string strTmp;
            if (SelectedDocType == DocType.MSR)
            {
                checkData.OtherData = string.Empty;
                checkData.RoutingNumber = string.Empty;
                checkData.AccountNumber = string.Empty;
                checkData.CheckNumber = string.Empty;

            }
            else
            {
                nRet = MTMICRGetValue(DocInfo, "DocInfo", "MICRRaw", strResponse, ref nResponseLength);
                strTmp = strResponse.ToString();
                checkData.ScannedCheckMicrData = strTmp;

                nRet = MTMICRGetValue(DocInfo, "DocInfo", "MICRTransit", strResponse, ref nResponseLength);
                strTmp = strResponse.ToString();
                checkData.RoutingNumber = strTmp;

                nRet = MTMICRGetValue(DocInfo, "DocInfo", "MICRAcct", strResponse, ref nResponseLength);
                strTmp = strResponse.ToString();
                checkData.AccountNumber = strTmp;

                nRet = MTMICRGetValue(DocInfo, "DocInfo", "MICRSerNum", strResponse, ref nResponseLength);
                strTmp = strResponse.ToString();
                checkData.CheckNumber = strTmp;
            }
        }
        public static string QueryDevice(string queryInfo)
        {

            StringBuilder strResults = new StringBuilder();

            int nRet;
            int nLength = 4096;
            strResults.Capacity = 4096;

            switch (queryInfo)
            {
                case "DeviceCapabilities":
                    nRet = MTMICRQueryInfo(CurrentDeviceName, "DeviceCapabilities", strResults, ref nLength);
                    break;
                case "DeviceStatus":
                    nRet = MTMICRQueryInfo(CurrentDeviceName, "DeviceStatus", strResults, ref nLength);
                    break;
                case "DeviceUsage":
                    nRet = MTMICRQueryInfo(CurrentDeviceName, "DeviceUsage", strResults, ref nLength);
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

            nRet = MTMICRSetValue(strOptions, "Application", "Transfer", "HTTP", ref nActualLength);

            if (nRet != (int)MICR_ST_OK)
                return -1;

            nRet = MTMICRSetValue(strOptions, "Application", "DocUnits", "ENGLISH", ref nActualLength);

            if (nRet != (int)MICR_ST_OK)
                return -1;

            nRet = MTMICRSetValue(strOptions, "ProcessOptions", "DocFeedTimeout", "10000", ref nActualLength);

            if (nRet != (int)MICR_ST_OK)
                return -1;

            if (SelectedDocType == DocType.MSR)
            {
                nRet = MTMICRSetValue(strOptions, "ProcessOptions", "DocFeed", "MSR", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;
                Options = strOptions.ToString();
                return nRet;
            }
            else
            {
                nRet = MTMICRSetValue(strOptions, "ProcessOptions", "DocFeed", "MANUAL", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;

                nRet = MTMICRSetValue(strOptions, "ImageOptions", "Number", "1", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;
                nRet = MTMICRSetIndexValue(strOptions, "ImageOptions", "ImageSide", 1, "FRONT", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;

                nRet = MTMICRSetIndexValue(strOptions, "ImageOptions", "ImageColor", 1, "GRAY8", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;


                nRet = MTMICRSetIndexValue(strOptions, "ImageOptions", "Resolution", 1, "200x200", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;

                nRet = MTMICRSetIndexValue(strOptions, "ImageOptions", "Compression", 1, "JPEG", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;

                nRet = MTMICRSetIndexValue(strOptions, "ImageOptions", "FileType", 1, "JPG", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;

                nRet = MTMICRSetValue(strOptions, "ProcessOptions", "ReadMICR", "E13B", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;

                nRet = MTMICRSetValue(strOptions, "ProcessOptions", "MICRFmtCode", "6200", ref nActualLength);
                if (nRet != (int)MICR_ST_OK)
                    return -1;
                Options = strOptions.ToString();
                return nRet;

            }

        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        public static string GetOtherDataFromMicrData(CheckData e)
        {
            var  otherData = e.ScannedCheckMicrData.ToString().Replace(e.AccountNumber, "").Replace(e.CheckNumber, "").Replace(e.RoutingNumber, "").Replace("TTU", "");
            return otherData;
        }
        #endregion


    }
}
