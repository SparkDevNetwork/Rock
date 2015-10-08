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
namespace Rock.Apps.CheckScannerUtility
{
    #region added types

    /// <summary>
    /// 
    /// </summary>
    public enum FeederType
    {
        SingleItem,
        MultipleItems
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FeedSource
    {
        public const int FeedSourceMainHopper = 0;
        public const int FeedSourceManualDrop = 2;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FeedItemCount
    {
        public const int FeedContinuously = 0;
        public const int FeedOne = 1;
    }

    #endregion

    // port of RangerConstants.h
    #region port of RangerConstants.h

    public enum RangerTransportStates
    {
        TransportUnknownState = -1,
        TransportShutDown = 0,
        TransportStartingUp = 1,
        TransportChangeOptions = 2,
        TransportEnablingOptions = 3,
        TransportReadyToFeed = 4,
        TransportFeeding = 5,
        TransportExceptionInProgress = 6,
        TransportShuttingDown = 7
    };

    public enum RangerOpStatus
    {
        OpStatusComplete = 0,
        OpStatusIncomplete = 1,
        OpStatusUnknown = 2
    };

    public enum RangerItemStates
    {
        RangerItemStateUnknown = 0,
        RangerItemInProcess = 1,
        RangerItemInPocket = 2,
        RangerItemRejected = 3,
        RangerItemDeleted = 4,
        RangerItemHandPocketed = 5,
    };

    public enum RangerFeedSources
    {
        FeedSourceUnknown = -1,
        FeedSourceMainHopper = 0,
        FeedSourceMergeHopper = 1,
        FeedSourceManualDrop = 2,
        FeedSourceCardScanner = 3,
        FeedSourceAlternateScanner = 4
    };

    public enum RangerFeedingStoppedReasons
    {
        UnknownFeedingStoppedReason = -1,
        FeedRequestFinished = 0,
        MainHopperEmpty = 1,
        MergeHopperEmpty = 2,
        ManualDropEmpty = 3,
        FeedStopRequested = 4,
        ClearTrackRequested = 5,
        BlackBandItemDetected = 6,
        EndOfLogicalMicrofilmRoll = 7,
        ExceptionDetected = 8
    };

    public enum RangerCommonSymbols
    {
        RangerRejectSymbol = '!'
    };

    public enum RangerE13BMicrSymbols
    {
        E13B_AmountSymbol = 'b',
        E13B_OnUsSymbol = 'c',
        E13B_TransitSymbol = 'd',
        E13B_DashSymbol = '-',
        E13B_RejectSymbol = RangerCommonSymbols.RangerRejectSymbol
    };

    public enum RangerSides
    {
        TransportFront = 0,
        TransportRear = 1
    };

    public enum RangerLocations
    {
        LocationUnknown = -1,
        LocationUpstream = 0,
        LocationDownstream = 1
    };

    public enum RangerBlipTypes
    {
        TransportNoBlip = 0,
        TransportItemBlip = 1,
        TransportBatchBlip = 2
    };

    public enum RangerFeedModes
    {
        TransportFeedModeNormal = 0,
        TransportFeedModeReadAndKey = 1
    };

    public enum RangerImageColorTypes
    {
        ImageColorTypeUnknown = -1,
        ImageColorTypeBitonal = 0,
        ImageColorTypeGrayscale = 1,
        ImageColorTypeColor = 2,
        ImageColorTypeUltraviolet = 3,    //ImageColorUltraViolet will return the "Type" specified in TransportInfo or the first UltraVioletImage specified in TransportInfo (if multiple)
        ImageColorTypeBitonalUV = 4,
        ImageColorTypeGrayscaleUV = 5,
    };

    public enum RangerExceptionTypes
    {
        TransportExceptionUnknown = -1,
        TransportExceptionNone = 0,
        TransportExceptionJam = 1,
        TransportExceptionLatePocket = 2,
        TransportExceptionInternalSoftwareError = 3,
        TransportExceptionPocketFull = 4,
        TransportExceptionTransportOffLine = 5,
        TransportExceptionImageSubsystemDead = 6,
        TransportExceptionInitializationError = 7,
        TransportExceptionItemsInTrack = 8,
        TransportExceptionDeviceError = 9,
        TransportExceptionFeedError = 10,
        TransportExceptionPrinterError = 11,
        TransportExceptionFeedButtonPressed = 12,
        TransportExceptionWaitingForFeedButton = 13,
        TransportExceptionEndOfMicrofilm = 14,
        TransportExceptionMisfeed = 15,
        TransportExceptionDogEar = 16,
        TransportExceptionMediaEmpty = 17,
        TransportExceptionPrinterInkCartridgeMissing = 18,
        TransportExceptionPrinterInkOut = 19,
        TransportExceptionPrinterInkLow = 20,
        TransportExceptionEndorserInkCartridgeMissing = 21,
        TransportExceptionEndorserInkOut = 22,
        TransportExceptionEndorserInkLow = 23,
        TransportExceptionCoverOpen = 24,
        TransportExceptionEndorserError = 25,
        TransportExceptionDoubleFeed = 26,
        TransportExceptionEndorsementTooLong = 27,
        TransportExceptionItemTooLong = 28,
        TransportExceptionItemLengthError = 29
    };

    public enum RangerExceptionDevices
    {
        TransportExceptionDeviceUnknown = -1,
        TransportExceptionDeviceNone = 0,
        TransportExceptionDeviceTransport = 1,  //error not related to a specific device
        TransportExceptionDeviceMainHopper = 2,
        TransportExceptionDeviceMergeHopper = 3,
        TransportExceptionDeviceManualDrop = 4,
        TransportExceptionDeviceMicrReader = 5,
        TransportExceptionDeviceOcrReader = 6,
        TransportExceptionDeviceIcrReader = 7,
        TransportExceptionDeviceViewWindow = 8,
        TransportExceptionDeviceMicrEncoder = 9,
        TransportExceptionDeviceMicrVerifier = 10,
        TransportExceptionDeviceFrontEndorser = 11,
        TransportExceptionDeviceRearEndorser = 12,
        TransportExceptionDeviceFrontStamp = 13,
        TransportExceptionDeviceRearStamp = 14,
        TransportExceptionDeviceMicrofilmer = 15,
        TransportExceptionDeviceFrontImageCamera = 16,
        TransportExceptionDeviceRearImageCamera = 17,
        TransportExceptionDeviceJournalPrinter = 18,
        TransportExceptionDevicePocketPrinter = 19,
        TransportExceptionDeviceImageFileSet = 20,
        TransportExceptionDeviceImageCameraController = 21,
        TransportExceptionDevicePocket = 22,
    };

    public enum RangerButtonTypes
    {
        ButtonType_Unknown = 0,
        ButtonType_Start = 1,
        ButtonType_Stop = 2,
        ButtonType_StartStop = 3,
    };

    public enum RangerEndorseMode
    {
        EndorseModeOEM_Batch = 1,
        EndorseModeCurrentItem = 2,
        EndorseModeNextItem = 3,
    };

    #endregion
}
