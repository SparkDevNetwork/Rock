<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampDecisionTracker.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.NextGen.CampDecisionTracker" %>

<style>
    #checkbox-wrapper {
            font-size: 1.25em;
        }

    @media screen and (min-width: 340px) {
        #checkbox-wrapper {
            font-size: 1.5em;
        }
    }

    input[type='checkbox'] {
        visibility: hidden;
    }

    input[type='checkbox']:after {
        width: 9px;
        height: 9px;
        border-radius: 9px;
        top: -2px;
        left: -1px;
        position: relative;
        font-family: FontAwesome;
        content: "\f096";
        display: inline-block;
        visibility: visible;
        cursor: pointer;
    }

    input[type='checkbox']:checked:after {
        font-family: FontAwesome;
        content: "\f046";
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <h1>Your Students</h1>
        <asp:Panel ID="pStudentsPanel" runat="server" class="panel panel-block">
            <asp:Repeater ID="rptStudents" runat="server" OnItemDataBound="rptStudents_ItemDataBound">
                <ItemTemplate>
                    <div style="display: flex; flex-direction: row; flex-wrap: nowrap; justify-content: space-around; align-items: center;">
                        <div style="padding-top: 10px; padding-bottom: 25px; text-align: center;">
                            <asp:Image runat="server" ID="studentImg" style="width: 76px; border-radius: 38px;"/>
                            <div style="padding: 0px; font-weight: 400; font-size: 1.25em; width: 150px;">
                                <asp:Literal runat="server" ID="studentName"></asp:Literal>
                            </div>
                        </div>
                        <div style="padding-top: 10px; padding-bottom: 25px; text-align: center;">
                            <asp:HiddenField runat="server" ID="studentGroupMemberId"></asp:HiddenField>
                            <asp:HiddenField runat="server" ID="studentAttribValueId"></asp:HiddenField>
                            <div ID="checkbox-wrapper" style="text-align: left;">
                                <asp:CheckBoxList ID="studentDecisions" runat="server"/>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
        
        <div ID="success-panel" class="alert alert-success" style="visibility: hidden; position: fixed; z-index:9999; left: 50%; top: 50%; transform: translate( -50%, -50%);">
            <p><i class="fa fa-check"></i> Decisions Updated!</p>
        </div>

        <div class="well" style="margin-top: 25px;">
            <ul style="padding-left: 10px;">
                <li><strong>No Decision:</strong> Decided not to make any commitments during camp</li>
                <li><strong>Follow Jesus + Baptism:</strong> 1st time decision to follow Jesus paired with getting baptism</li>
                <li><strong>Baptism:</strong> Already follows Jesus – wants to get baptized</li>
                <li><strong>Return:</strong> Renew commitment to Jesus</li>
                <li><strong>Next Step:</strong> Wants to serve at CCV</li>
                <li><strong>Vocational Ministry:</strong> Called to full-time ministry</li>
            </ul>
        </div>
        
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    // adds a hook so we get a function call on postback
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest(function () {

        $("#success-panel").css("visibility", "visible");
        setTimeout(hideAlert, 5000);
    });

    function hideAlert() {
        $("#success-panel").css("visibility", "hidden" );
    }
</script>