<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LabelPrinting.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.LabelPrinting" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnMerge" />
    </Triggers>
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
        <asp:Panel ID="pnlEntry" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlNumberOfIndividuals" runat="server" LabelType="Info"/>
                </div>
            </div>

            <div class="panel-body">
                <asp:HiddenField ID="hfCampusId" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbCombineFamilyMembers" runat="server" Text="Combine Family Members" Help="Set this to combine family members into a single row. For example, Ted Decker and Cindy Decker would be combined into 'Ted & Cindy Decker'. 
                                NOTE: Design your template to use {{ Row.FullName }} instead of {{ Row.NickName }} {{ Row.LastName }} to help in situations where family members might have different last names." />
                        </div>
                        <div class="col-md-6">
                            <Rock:CampusPicker FormGroupCssClass="pull-right" CssClass="input-width-xl" AutoPostBack="true" ID="cpCampus" runat="server" Label="Campus" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockCheckBox ID="cbSelectAll" CssClass="js-select-all" runat="server" Text="Select All" OnCheckedChanged="cbSelectAll_CheckedChanged" AutoPostBack="true" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockCheckBoxList ID="cblPerson" runat="server" RepeatDirection="Horizontal"></Rock:RockCheckBoxList>
                        </div>
                    </div>
                    <Rock:NotificationBox ID="nbMergeError" runat="server" NotificationBoxType="Warning" Visible="false" CssClass="js-merge-error"/>
                    <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Visible="false" CssClass="js-merge-error"/>
                    <div class="actions">
                        <asp:LinkButton runat="server" ID="btnMerge" CssClass="btn btn-default" OnClick="btnMerge_Click">
                        </asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnUpdate" CssClass="btn btn-default" OnClick="btnUpdate_Click">
                        <i class="fa fa-check"></i> Update Individuals</asp:LinkButton>
                    </div>
                </fieldset>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
