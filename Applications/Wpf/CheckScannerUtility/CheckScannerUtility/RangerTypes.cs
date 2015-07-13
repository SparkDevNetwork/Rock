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
    /// <summary>
    /// 
    /// </summary>
    public enum XportStates
    {
        TransportShutDown = 0,
        TransportStartingUp = 1,
        TransportChangeOptions = 2,
        TransportEnablingOptions = 3,
        TransportReadyToFeed = 4,
        TransportFeeding = 5,
        TransportExceptionInProgress = 6,
        TransportShuttingDown = 7
    };

    /// <summary>
    /// 
    /// </summary>
    public enum FeedingStoppedReasons
    {
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

    /// <summary>
    /// 
    /// </summary>
    public enum IQATestIDs
    {
        IQATest_UndersizeImage = 1,
        IQATest_OversizeImage = 2,
        IQATest_BelowMinCompressedSize = 3,
        IQATest_AboveMaxCompressedSize = 4,
        IQATest_FrontRearDimensionMismatch = 5,
        IQATest_HorizontalStreaks = 6,
        IQATest_ImageTooLight = 7,
        IQATest_ImageTooDark = 8,
        IQATest_CarbonStrip = 9,
        IQATest_FramingError = 10,
        IQATest_ExcessiveSkew = 11,
        IQATest_TornEdges = 12,
        IQATest_TornCorners = 13,
        IQATest_SpotNoise = 14
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQATestStatus
    {
        IQATestStatus_NotTested = 0,
        IQATestStatus_DefectPresent = 1,
        IQATestStatus_DefectNotPresent = 2
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults
    {
        IQAResult_TestResult = 1
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_UndersizeImage : int
    {
        UndersizeImage_ImageWidth = 2,
        UndersizeImage_ImageHeight = 3
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_OversizeImage
    {
        OversizeImage_ImageWidth = 2,
        OversizeImage_ImageHeight = 3
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_BelowMinCompressedSize
    {
        BelowMinCompressedSize_CompressedImageSize = 2,
        BelowMinCompressedSize_ImageResolution = 3
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_AboveMaxCompressedSize
    {
        AboveMaxCompressedSize_CompressedImageSize = 2,
        AboveMaxCompressedSize_ImageResolution = 3
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_FrontRearDimensionMismatch
    {
        FrontRearDimensionMismatch_WidthDifference = 2,
        FrontRearDimensionMismatch_HeightDifference = 3
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_HorizontalStreaks
    {
        HorizontalStreaks_StreakCount = 2,
        HorizontalStreaks_ThickestStreak = 3,
        HorizontalStreaks_ThickestStreakLocation = 4
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_ImageTooLight
    {
        ImageTooLight_BitonalBlackPixelPercent = 2
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_ImageTooDark
    {
        ImageTooDark_BitonalBlackPixelPercent = 2
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_FramingError
    {
        FramingError_TopEdge = 2,
        FramingError_LeftEdge = 3,
        FramingError_BottomEdge = 4,
        FramingError_RightEdge = 5
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_ExcessiveSkew
    {
        ExcessiveSkew_Angle = 2
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_TornEdges
    {
        TornEdges_LeftTearWidth = 2,
        TornEdges_LeftTearHeight = 3,
        TornEdges_BottomTearWidth = 4,
        TornEdges_BottomTearHeight = 5,
        TornEdges_RightTearWidth = 6,
        TornEdges_RightTearHeight = 7,
        TornEdges_TopTearWidth = 8,
        TornEdges_TopTearHeight = 9
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_TornCorners
    {
        TornCorners_TopLeftTearWidth = 2,
        TornCorners_TopLeftTearHeight = 3,
        TornCorners_BottomLeftTearWidth = 4,
        TornCorners_BottomLeftTearHeight = 5,
        TornCorners_TopRightTearWidth = 6,
        TornCorners_TopRightTearHeight = 7,
        TornCorners_BottomRightTearWidth = 8,
        TornCorners_BottomRightTearHeight = 9
    };

    /// <summary>
    /// 
    /// </summary>
    public enum IQAResults_SpotNoise
    {
        SpotNoise_AverageSpotNoiseCount = 2
    };

    /// <summary>
    /// 
    /// </summary>
    public enum Sides
    {
        TransportFront = 0,
        TransportRear = 1,
    };

    /// <summary>
    /// 
    /// </summary>
    public enum ImageColorType
    {
        ImageColorTypeUnknown = -1,
        ImageColorTypeBitonal = 0,
        ImageColorTypeGrayscale = 1,
        ImageColorTypeColor = 2
    };

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

}
