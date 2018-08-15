<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DatePicker.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Reporting.DatePicker" %>
 
    <fieldset> 
    <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Update Date Range" OnClick="btnSave_Click" CssClass="btn btn-primary" LowerValue="" UpperValue="" />
        </div>
    </fieldset>   
