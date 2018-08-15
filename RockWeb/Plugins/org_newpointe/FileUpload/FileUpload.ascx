<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FileUpload.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.FileUpload.FileUpload" %>
 
<Rock:FileUploader ID="fuFile" runat="server" Label="Upload File" Required="true" OnFileUploaded="fuFile_FileUploaded" />
<br/>
<Rock:NotificationBox ID="nbSuccess" runat="server" visible="false" />
