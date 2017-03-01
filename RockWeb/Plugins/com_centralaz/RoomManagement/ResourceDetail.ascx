<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResourceDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ResourceDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfResourceId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" EntityTypeName="com.centralaz.RoomManagement.Model.Resource" Required="true" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbQuantity" runat="server" Label="Quantity" NumberType="Integer" Required="true" MinimumValue="1" Text="1" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus"/>
                        </div>
                    </div>
                    <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnSaveThenAdd" runat="server" AccessKey="d" Text="Save Then Add" CssClass="btn btn-link" OnClick="btnSaveThenAdd_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
