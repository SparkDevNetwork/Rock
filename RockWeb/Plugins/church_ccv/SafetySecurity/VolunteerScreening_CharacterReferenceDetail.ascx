<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerScreening_CharacterReferenceDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.VolunteerScreening_CharacterReferenceDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pCharReference" runat="server" class="panel panel-block">
            <div class="panel-heading clearfix">
                <h4>Information</h4>
            </div>
            <div class="panel-body">
                <div class="well">
                    <h4>Applicant</h4>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lApplicant" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>

                <div class="well">
                    <h4>Character Reference</h4>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lReference" runat="server"></asp:Literal>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lReferenceEmail" runat="server"></asp:Literal>
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lReferencePhoneNumber" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pNoResponse" runat="server" class="panel panel-block">
            <div class="panel-heading clearfix">
                <h4>Responses</h4>
            </div>

            <div class="panel-body">

                <%-- Question --%>
                <div class="well">
                    <strong>No responses received yet.</strong>
                </div>
        </asp:Panel>
        
        <asp:Panel ID="pCharReferenceFeedback" runat="server" class="panel panel-block">
            <div class="panel-heading clearfix">
                <h4>Responses</h4>
            </div>
            <div class="panel-body">

                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_ThreeToFiveTraits" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>

                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_MoralIntegrity" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                    <br>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_MoralIntegrityReason" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>


                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_AroundChildren" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                    <br>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_AroundChildrenDesc" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>

                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_Commitment" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                    <br>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_CommitmentAdditional" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>

                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_TrustworthyWithChildren" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                    <br>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_TrustworthyWithChildrenReason" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>

                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_OverallRating" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>

                <%-- Question --%>
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lFeedback_AdditionalComments" runat="server">Title</asp:Literal>
                        </div>
                    </div>
                </div>
                <%-- End --%>
            </div>
        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
