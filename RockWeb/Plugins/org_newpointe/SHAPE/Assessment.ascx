<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Assessment.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.SHAPE.Assessment" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>
        
        <asp:Panel runat="server" ID="Panel1">
            <section class="shape-person-details">
                <Rock:DataTextBox runat="server" id="dtbFirstName" SourceTypeName="Rock.Model.Person" PropertyName="FirstName" Required="true" />
                <Rock:DataTextBox runat="server" id="dtbLastName" SourceTypeName="Rock.Model.Person" PropertyName="LastName" Required="true" />
                <Rock:EmailBox runat="server" id="ebEmailAddress" Label="Email" Required="true" />
            </section>
        </asp:Panel>
        
        <asp:Panel runat="server" ID="pShapeGifts">
            <section class="shape-spiritual-gifts">

                <h2>Spiritual Gifts</h2>
                <h5 class="margin-b-lg">Please rate how each statement applies to you, from Never to Always.</h5>

                <asp:Repeater runat="server" ID="rSpiritualGiftsQuestions">
                    <ItemTemplate>

                        <div class="clearfix margin-b-md">
                            <asp:HiddenField ID="hfSpiritualGiftsQuestionId" runat="server" Value='<%# Eval("Id") %>' />
                            <Rock:RockRadioButtonList ID="rrblSpiritualGiftsQuestion" runat="server" Label='<%# Eval("Value") %>' RepeatDirection="Horizontal" CssClass="margin-t-sm" RequiredErrorMessage="_" Required="true">
                                <asp:ListItem Value="1">Never</asp:ListItem>
                                <asp:ListItem Value="2">Almost Never</asp:ListItem>
                                <asp:ListItem Value="3">Sometimes</asp:ListItem>
                                <asp:ListItem Value="4">Almost Always</asp:ListItem>
                                <asp:ListItem Value="5">Always</asp:ListItem>
                            </Rock:RockRadioButtonList>
                        </div>

                    </ItemTemplate>
                </asp:Repeater>
            
            </section>
        </asp:Panel>

        <asp:Panel runat="server" ID="pShapeAbilities">
            <section class="shape-abilities">

                <h2>Abilities</h2>
                <h5 class="margin-b-lg">Please rate how each statement applies to you, from Never to Always.</h5>

                <asp:Repeater runat="server" ID="rAbilitiesQuestions">
                    <ItemTemplate>
                        
                        <div class="clearfix margin-b-md">
                            <asp:HiddenField ID="hfAbilitiesQuestionId" runat="server" Value='<%# Eval("Id") %>' />
                            <Rock:RockRadioButtonList ID="rrblAbilitiesQuestion" runat="server" Label='<%# Eval("Value") %>' RepeatDirection="Horizontal" CssClass="margin-t-sm" RequiredErrorMessage="_" Required="true">
                                <asp:ListItem Value="1">Never</asp:ListItem>
                                <asp:ListItem Value="2">Almost Never</asp:ListItem>
                                <asp:ListItem Value="3">Sometimes</asp:ListItem>
                                <asp:ListItem Value="4">Almost Always</asp:ListItem>
                                <asp:ListItem Value="5">Always</asp:ListItem>
                            </Rock:RockRadioButtonList>
                        </div>

                    </ItemTemplate>
                </asp:Repeater>
        
            </section>
        </asp:Panel>

        <asp:Panel runat="server" ID="pShapeHeart">
            <section class="shape-heart">
            
                <h2>Heart</h2>
                <h5 class="margin-b-lg">Please share a little about your Heart, which refers to our God-given passions and interests.</h5>

                <Rock:RockCheckBoxList runat="server" ID="rcblHeartOptions" Label="I enjoy working with the following: (select all that apply)" CssClass="margin-b-lg" Required="true"></Rock:RockCheckBoxList>
                
                <Rock:RockTextBox runat="server" ID="rtbHeartPast" TextMode="MultiLine" Label="What causes or non-profit organizations have you supported or volunteered with in the past?" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox runat="server" ID="rtbHeartFuture" TextMode="MultiLine" Label="What causes or issues do you want to work on/with, even if you don't get a paycheck?" Required="true"></Rock:RockTextBox>
                
            </section>
        </asp:Panel>

        <asp:Panel runat="server" ID="pShapeExperiences">
            <section class="shape-experiences">
            
                <h2>Experiences</h2>
                <h5 class="margin-b-lg">Please share a little more about who you are and how you are shaped by listing and describing the top 3 critical people, places, and events in your life.</h5>
                
                <Rock:RockTextBox runat="server" ID="rtbExperiences_People" TextMode="MultiLine" Label="3 People who had an impact on your life (positive or negative)" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox runat="server" ID="rtbExperiences_Places" TextMode="MultiLine" Label="3 Places that had an impact on your life (positive or negative)" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox runat="server" ID="rtbExperiences_Events" TextMode="MultiLine" Label="3 Events that had an impact on your life (positive or negative)" Required="true"></Rock:RockTextBox>
                
            </section>
        </asp:Panel>

        <div class="actions">

            <Rock:BootstrapButton runat="server" ID="bbSubmit" CssClass="btn btn-primary" Text="Submit" OnClick="bbSubmit_Click"></Rock:BootstrapButton>

        </div>



    </ContentTemplate>
</asp:UpdatePanel>
