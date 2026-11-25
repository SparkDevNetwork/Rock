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

namespace Rock.Apps.CheckScannerUtility.Client.Enums
{
    /// <summary>
    /// </summary>
    public enum AgeClassification
    {
        Unknown = 0x0,
        Adult = 0x1,
        Child = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum BatchStatus
    {
        Pending = 0x0,
        Open = 0x1,
        Closed = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum ColorDepth
    {
        BlackWhite = 0x0,
        Grayscale8bit = 0x1,
        Grayscale24bit = 0x2,
        Color8bit = 0x3,
        Color24bit = 0x4,
        Undefined = -1,
    }

    /// <summary>
    /// </summary>
    public enum CommunicationType
    {
        RecipientPreference = 0x0,
        Email = 0x1,
        SMS = 0x2,
        PushNotification = 0x3,
    }

    /// <summary>
    /// </summary>
    public enum EmailPreference
    {
        EmailAllowed = 0x0,
        NoMassEmails = 0x1,
        DoNotEmail = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum Format
    {
        JPG = 0x0,
        GIF = 0x1,
        PNG = 0x2,
        PDF = 0x3,
        Word = 0x4,
        Excel = 0x5,
        Text = 0x6,
        HTML = 0x7,
        Undefined = -1,
    }

    /// <summary>
    /// </summary>
    public enum Gender
    {
        Unknown = 0x0,
        Male = 0x1,
        Female = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum MICRStatus
    {
        Success = 0x0,
        Fail = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum Resolution
    {
        DPI72 = 0x0,
        DPI150 = 0x1,
        DPI300 = 0x2,
        DPI600 = 0x3,
        Undefined = -1,
    }
}
