﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupMemberId" runat="server" />


        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"> <asp:Literal ID="lGroupIconHtml" runat="server" /> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <div id="pnlEditDetails" runat="server">
                
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true"/>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList runat="server" ID="ddlGroupRole" DataTextField="Name" DataValueField="Id" Label="Role" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                            <asp:PlaceHolder ID="phAttributesReadOnly" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                    </div>

                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
