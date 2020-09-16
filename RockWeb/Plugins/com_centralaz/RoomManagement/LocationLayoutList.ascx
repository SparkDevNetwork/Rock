<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationLayoutList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.LocationLayoutList" %>
<%@ Register Namespace="com.centralaz.RoomManagement.Web.UI.Controls" TagPrefix="CentralAZ" Assembly="com.centralaz.RoomManagement" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Location Layouts</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:Grid ID="rGrid" runat="server" RowItemText="Location Layout" OnRowSelected="rGrid_Edit">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:RockBoundField DataField="LayoutPhoto" HeaderText="Layout Photo" HtmlEncode="false" />
                                <Rock:RockBoundField DataField="IsActive" HeaderText="Is Active" />
                                <Rock:RockBoundField DataField="IsDefault" HeaderText="Is Default" />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="modalDetails" runat="server" Title="Add/Edit Layout" ValidationGroup="LocationLayouts" OnSaveClick="modalDetails_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" ValidationGroup="LocationLayouts" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Is Active" ValidationGroup="LocationLayouts" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbIsDefault" runat="server" Label="Is Default" ValidationGroup="LocationLayouts" />
                    </div>
                </div>
                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" ValidationGroup="LocationLayouts" />
                <Rock:ImageUploader ID="iuPhoto" runat="server" Label="Layout Photo" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
