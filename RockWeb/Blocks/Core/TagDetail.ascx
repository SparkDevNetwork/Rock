<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.TagDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>


            <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
                    
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-tag"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                    
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlEntityType" runat="server" LabelType="Type" />
                    </div>
                </div>
                <div class="panel-body">
                    
                    <asp:HiddenField ID="hfId" runat="server" />

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <fieldset>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                                    </div>
                            </div>
                    
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />
                                </div>
                            </div>
                    
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockRadioButtonList ID="rblScope" runat="server" Label="Scope" RepeatDirection="Horizontal"
                                        AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                                        <asp:ListItem Value="Organization" Text="Organizational" Selected="True" />
                                        <asp:ListItem Value="Personal" Text="Personal" />
                                    </Rock:RockRadioButtonList>
                                    <Rock:PersonPicker ID="ppOwner" runat="server" Label="Owner" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList id="ddlEntityType" runat="server" Label="Entity Type" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                                </div>
                            </div>

                            <Rock:NotificationBox ID="nbEditError" runat="server" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>

                            <div class="actions">
                                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </fieldset>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">

                        <p class="description">
                            <asp:Literal ID="lDescription" runat="server"></asp:Literal></p>

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                        </div>

                    </fieldset>
                </div>
            </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
