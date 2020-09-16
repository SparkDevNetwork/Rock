<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResourceDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ResourceDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    
    <ContentTemplate>
        <script type="text/javascript">
        function pageLoad() {
            if ($('div.alert.alert-success').length > 0) {
    	            window.setTimeout("fadeAndClear()", 5000);
            }
        }

        function fadeAndClear() {
            $('div.alert.alert-success').animate({ opacity: 0 }, 2000 );
        }
        </script>

        <asp:HiddenField ID="hfResourceId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
                <Rock:NotificationBox ID="nbSaveMessage" runat="server" NotificationBoxType="Success" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" Label="Name" Required="true" SourceTypeName="com.centralaz.RoomManagement.Model.Resource, com.centralaz.RoomManagement" PropertyName="Name" />
                        </div>
                        <div class="col-md-3">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" EntityTypeName="com.centralaz.RoomManagement.Model.Resource" Required="true" />
                        </div>
                        <div class="col-md-3">
                            <Rock:GroupPicker ID="gpApprovalGroup" runat="server" Label="Approval Group" Help="If this resource requires special approval, select the group in charge of approving it here."/>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbQuantity" runat="server" Label="Quantity" NumberType="Integer" Required="true" MinimumValue="1" Text="1" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus"/>
                        </div>
                        <div class="col-md-3">
                            <Rock:LocationPicker ID="lpLocationPicker" runat="server" Label="Attached Location" AllowedPickerModes="Named" CurrentPickerMode="Named" Help="Specify a location only if this resource is constrained or locked to a location."/>
                        </div>
                    </div>
                    <Rock:DataTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" SourceTypeName="com.centralaz.RoomManagement.Model.Resource, com.centralaz.RoomManagement" PropertyName="Note" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnSaveThenAdd" runat="server" AccessKey="d" Text="Save Then Add" CssClass="btn btn-link" OnClick="btnSaveThenAdd_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Visible="false" CssClass="btn btn-link btn-danger margin-l-md pull-right" OnClick="btnDelete_Click" CausesValidation="false"><i class="fa fa-trash"></i> Delete</asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
