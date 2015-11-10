<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProtectMyMinistrySettings.ascx.cs" Inherits="RockWeb.Blocks.Security.BackgroundCheck.ProtectMyMinistrySettings" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i> Protect My Ministry</h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlMode" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlActive" runat="server" />
                </div>

            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbNotification" runat="server" Title="Please Correct the Following" NotificationBoxType="Danger" Visible="false" />

                <div id="pnlNew" runat="server" class="row">
                    <div class="col-md-6">
                        <asp:Image ID="imgPromotion" runat="server" CssClass="img-responsive" />
                        <div class="actions">
                            <asp:HyperLink ID="hlGetStarted" runat="server" Text="Get Started Now" CssClass="btn btn-primary btn-block margin-b-lg" />
                        </div>
                    </div>
                    <div class="col-md-1">
                    </div>
                    <div class="col-md-5">
                        <Rock:RockTextBox ID="tbUserNameNew" runat="server" Label="Username" />
                        <Rock:RockTextBox ID="tbPasswordNew" runat="server" Label="Password" TextMode="Password" />
                        <div class="actions">
                            <asp:LinkButton ID="lbSaveNew" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveNew_Click" />
                        </div>
                    </div>
                </div>

                <div id="pnlViewDetails" runat="server" visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lUserName" runat="server" Label="Username" />
                            <Rock:RockLiteral ID="lPassword" runat="server" Label="Password" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lPackages" runat="server" Label="Enabled Packages" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server" visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" />
                            <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" TextMode="Password" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox id="cbActive" runat="server" Label="Active" />
                            <Rock:RockCheckBox ID="cbTestMode" runat="server" Label="Test Mode" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

            </div>

        </div>

        <asp:Panel ID="pnlPackages" CssClass="panel panel-block" runat="server" Visible="true">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-archive"></i> Packages</h1>
            </div>
            <div class="panel-body">

                <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gDefinedValues" runat="server" AllowPaging="true" DisplayType="Full" RowItemText="Package" OnRowSelected="gDefinedValues_RowSelected" AllowSorting="False" >
                        <Columns>
                            <Rock:ReorderField/>
                            <Rock:RockTemplateField HeaderText="Package" SortExpression="Name">
                                <ItemTemplate>
                                    <%# Eval("Value") %><br /><small><%# Eval("Description") %></small>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="PackageName" HeaderText="PMM Package Name"/>
                            <Rock:RockBoundField DataField="DefaultCounty" HeaderText="Default County"/>
                            <Rock:BoolField DataField="SendAddressCounty" HeaderText="Send County from Home Address" />
                            <Rock:RockBoundField DataField="DefaultState" HeaderText="Default State"/>
                            <Rock:BoolField DataField="SendAddressState" HeaderText="Send State from Home Address" />
                            <Rock:BoolField DataField="IncludeMVRInfo" HeaderText="Include MVR Info" />
                            <Rock:DeleteField OnClick="gDefinedValues_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
                                        
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgPackage" runat="server" Title="Package" ValidationGroup="Package" 
            OnSaveClick="dlgPackage_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>

                <asp:HiddenField ID="hfDefinedValueId" runat="server" />
                <asp:ValidationSummary ID="valSummaryPackage" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Package" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" Required="true" ValidationGroup="Package" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" ValidationGroup="Package" TextMode="MultiLine" Rows="2" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbPackageName" runat="server" Label="PMM Package Name" Required="true" ValidationGroup="Package" Help="The exact package name to use when sending the background check request to Protect My Ministry" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIncludeMVR" runat="server" Label="Include MVR Information" Text="Yes" ValidationGroup="Package" Help="Flag indicating if this package requires that the Motor Vehicle Registration (MVR) fields be included with the request." />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbDefaultCounty" runat="server" Label="Default County" ValidationGroup="Package" Help="The county to include with the background check request. " />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbSendCounty" runat="server" Label="Send County from Home Address" Text="Yes" ValidationGroup="Package" Help="If person's home address includes a valid county, send that instead of the Default County." />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbDefaultState" runat="server" Label="Default State" ValidationGroup="Package" Help="The state abbreviation to include with the background check request. " />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbSendState" runat="server" Label="Send State from Home Address" Text="Yes" ValidationGroup="Package" Help="If person's home address includes a valid state, send that instead of the Default State." />
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>