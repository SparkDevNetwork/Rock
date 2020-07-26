<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusDetail" %>

<asp:UpdatePanel ID="upCampusDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            
            <asp:HiddenField ID="hfCampusId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlEditDetails" runat="server">

                    <fieldset>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbCampusName" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                            </div>
                            
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DefinedValuePicker ID="dvpCampusStatus" runat="server" Label="Status" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="CampusStatusValueId" />
                                <Rock:DataTextBox ID="tbCampusCode" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="ShortCode" Label="Code" />
                                <Rock:RockDropDownList ID="ddlTimeZone" runat="server" CausesValidation="false" CssClass="input-width-xxl" Label="Time Zone" Help="The time zone you want certain time calculations of the Campus to operate in. Leave this blank to use the default Rock TimeZone." ></Rock:RockDropDownList>
                                <Rock:PersonPicker ID="ppCampusLeader" runat="server" Label="Campus Leader" />
                                <Rock:KeyValueList ID="kvlServiceTimes" runat="server" label="Service Times" KeyPrompt="Day" ValuePrompt="Time" Help="A list of days and times that this campus has services." />
                            </div>
                            <div class="col-md-6">
                                <Rock:DefinedValuePicker ID="dvpCampusType" runat="server" Label="Type" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="CampusTypeValueId" />
                                <Rock:UrlLinkBox ID="urlCampus" runat="server" Label="Url" />
                                <Rock:PhoneNumberBox ID="pnbPhoneNumber" runat="server" Label="Phone Number" />
                                <Rock:LocationPicker ID="lpLocation" runat="server" AllowedPickerModes="Named" Required="true" Label="Location" Help="Select a Campus Location" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="attributes">
                                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                                </div>
                            </div>
                        </div>

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <p class="description">
                        <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lMainDetailsLeft" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lMainDetailsRight" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
