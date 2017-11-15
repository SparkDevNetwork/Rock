<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QuestionList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.QuestionList" %>
<%@ register namespace="com.centralaz.RoomManagement.Web.UI.Controls" tagprefix="CentralAZ" assembly="com.centralaz.RoomManagement"%>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:NotificationBox ID="nbOrdering" runat="server" NotificationBoxType="Info" Text="Note: Select a specific entity type filter in order to reorder note types." Dismissable="true" Visible="false" />
                    
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"> Reservation Questions</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:Grid ID="rGrid" runat="server" RowItemText="Reservation Question" OnRowSelected="rGrid_Edit" >
                            <Columns>
                                <Rock:ReorderField Visible="true" />
                                <Rock:RockBoundField DataField="Question" HeaderText="Question" />
                                <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="modalDetails" runat="server" Title="Add/Edit Question" ValidationGroup="ReservationQuestions" OnSaveClick="modalDetails_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <CentralAZ:SimpleAttributeEditor ID="edtQuestion" runat="server" ShowActions="false" ValidationGroup="ReservationQuestions" 
                    IsIconCssClassVisible="false" IsAnalyticsVisible="false" IsShowInGridVisible="false"
                    IsCategoriesVisible="false" IsDescriptionVisible="false" IsAllowSearchVisible="false" AllowSearch="false"
                    IsKeyEditable="false" NameFieldLabel="Question" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdCopyQuestions" runat="server" Title="Copy Questions From..." OnSaveClick="mdCopyQuestions_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <div>
                    Copy from resource: <Rock:RockDropDownList ID="ddlResourceCopySource" runat="server" EnhanceForLongLists="true"></Rock:RockDropDownList>
                </div>
                <div class="margin-t-md">
                    Copy from location: <Rock:LocationPicker ID="locpLocations" runat="server" AllowedPickerModes="Named"></Rock:LocationPicker>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
