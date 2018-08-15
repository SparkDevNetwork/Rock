<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShapeResults.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Shape.ShapeResults" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" NotificationBoxType="Danger" Text="You must be logged in or access this page from a vaild link." Visible="False" ID="nbNoPerson" Title="No Valid Person"></Rock:NotificationBox>

        <Rock:NotificationBox runat="server" NotificationBoxType="Success" Text="It looks like you don't have a MyNewPointe account yet.  Create one at the bottom of this page after you finish reading your SHAPE Profile!  " Visible="False" ID="nbTip" Title="Tip!"></Rock:NotificationBox>


        <h1 class="text-center">My SHAPE Profile</h1>
        <h2 class="text-center" style="margin-top: -10px;">
            <asp:Label runat="server" ID="lbPersonName"></asp:Label><br />
            <small>Assessment Taken:
                <asp:Label runat="server" ID="lbAssessmentDate"></asp:Label></small></h2>


        <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapseSpiritualGifts" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-share-square-o"></i>Spiritual Gifts
                    </a>
                </h4>
            </div>

            <div class="panel-body collapse in" id="collapseSpiritualGifts">
                <div class="row">
                    <div class="col-md-6">
                        <h5 class="text-center">Spiritual Gift 1: </h5>
                        <h3 class="text-center" style="margin-top: -10px;">
                            <asp:Label runat="server" ID="lbGift1Title"></asp:Label></h3>
                        <asp:Label runat="server" ID="lbGift1BodyHTML"></asp:Label>
                        <hr />
                    </div>

                    <div class="col-md-6">
                        <h5 class="text-center">Spiritual Gift 2: </h5>
                        <h3 class="text-center" style="margin-top: -10px;">
                            <asp:Label runat="server" ID="lbGift2Title"></asp:Label></h3>
                        <asp:Label runat="server" ID="lbGift2BodyHTML"></asp:Label>
                        <hr />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <h5 class="text-center">Spiritual Gift 3: </h5>
                        <h3 class="text-center" style="margin-top: -10px;">
                            <asp:Label runat="server" ID="lbGift3Title"></asp:Label></h3>
                        <asp:Label runat="server" ID="lbGift3BodyHTML"></asp:Label>
                    </div>

                    <div class="col-md-6">
                        <h5 class="text-center">Spiritual Gift 4: </h5>
                        <h3 class="text-center" style="margin-top: -10px;">
                            <asp:Label runat="server" ID="lbGift4Title"></asp:Label></h3>
                        <asp:Label runat="server" ID="lbGift4BodyHTML"></asp:Label>
                    </div>

                    <div class="col-md-12 text-center">
                        <a href="/SpiritualGiftDescriptions" class="btn btn-primary np-button">View Spiritual Gifts Library</a>
                        <br />
                    </div>

                </div>

            </div>

        </div>

        <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapseHeart" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-share-square-o"></i>Heart
                    </a>
                </h4>
            </div>
            <div class="panel-body collapse in" id="collapseHeart">
                <div class="col-md-4">
                    <h3 class="text-center">My Interests: </h3>
                    <p>
                        <asp:Label runat="server" ID="lbHeartCategories"></asp:Label></p>
                </div>

                <div class="col-md-4">
                    <h3 class="text-center">My Causes: </h3>
                    <p>
                        <asp:Label runat="server" ID="lbHeartCauses"></asp:Label></p>
                </div>

                <div class="col-md-4">
                    <h3 class="text-center">My Passions: </h3>
                    <p>
                        <asp:Label runat="server" ID="lbHeartPassion"></asp:Label></p>
                </div>


                <div class="col-md-12">
                    <br />
                </div>

            </div>
        </div>








        <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapseAbility" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-share-square-o"></i>Abilities
                    </a>
                </h4>
            </div>
            <div class="panel-body collapse in" id="collapseAbility">
                <div class="col-md-6">
                    <h5 class="text-center">Ability 1: </h5>
                    <h3 class="text-center" style="margin-top: -10px;">
                        <asp:Label runat="server" ID="lbAbility1Title"></asp:Label></h3>
                    <asp:Label runat="server" ID="lbAbility1BodyHTML"></asp:Label>
                </div>

                <div class="col-md-6">
                    <h5 class="text-center">Ability 2: </h5>
                    <h3 class="text-center" style="margin-top: -10px;">
                        <asp:Label runat="server" ID="lbAbility2Title"></asp:Label></h3>
                    <asp:Label runat="server" ID="lbAbility2BodyHTML"></asp:Label>
                </div>


                <div class="col-md-12">
                    <br />
                </div>

            </div>

        </div>



        <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapsePersonality" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-share-square-o"></i>Personality
                    </a>
                </h4>
            </div>
            <div class="panel-body collapse in" id="collapsePersonality">
                <div class="col-md-12">
                </div>


                <asp:Panel runat="server" ID="DISCResults" Visible="False">

                    <div class="col-md-12">

                        <ul class="discchart">
                            <li class="discchart-midpoint"></li>
                            <li style="height: 100%; width: 0px;"></li>
                            <li id="discNaturalScore_D" runat="server" class="discbar discbar-d">
                                <div class="discbar-label">D</div>
                            </li>
                            <li id="discNaturalScore_I" runat="server" class="discbar discbar-i">
                                <div class="discbar-label">I</div>
                            </li>
                            <li id="discNaturalScore_S" runat="server" class="discbar discbar-s">
                                <div class="discbar-label">S</div>
                            </li>
                            <li id="discNaturalScore_C" runat="server" class="discbar discbar-c">
                                <div class="discbar-label">C</div>
                            </li>
                        </ul>



                        <h3>Description</h3>
                        <asp:Literal ID="lDescription" runat="server"></asp:Literal>

                        <h3>Strengths</h3>
                        <asp:Literal ID="lStrengths" runat="server"></asp:Literal>

                        <h3>Challenges</h3>
                        <asp:Literal ID="lChallenges" runat="server"></asp:Literal>

                        <h3>Team Contribution</h3>
                        <asp:Literal ID="lTeamContribution" runat="server"></asp:Literal>

                        <h3>Leadership Style</h3>
                        <asp:Literal ID="lLeadershipStyle" runat="server"></asp:Literal>


                        <br />
                    </div>

                </asp:Panel>

                <asp:Panel runat="server" ID="NoDISCResults" Visible="True">

                    <div class="col-md-12 text-center">

                        <p>
                            The DISC Assessment is used for the Personality portion of your SHAPE Profile.
                    It looks like you haven't taken the DISC Assessment yet.
                        </p>
                        <a href="https://newpointe.org/DISC?rckipid=<%= PersonEncodedKey %>" target="_blank" class="btn btn-primary np-button">TAKE THE DISC ASSESSMENT NOW</a>


                        <br />
                    </div>

                </asp:Panel>


            </div>
        </div>


        <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapsePersonality" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-share-square-o"></i>Experiences 
                    </a>
                </h4>
            </div>
            <div class="panel-body collapse in" id="collapsePersonality">
                <div class="col-md-12">
                    <h4 class="text-center">People </h4>
                    <p>
                        <asp:Label runat="server" ID="lbPeople"></asp:Label><br />
                    </p>

                    <h4 class="text-center">Places </h4>
                    <p>
                        <asp:Label runat="server" ID="lbPlaces"></asp:Label><br />
                    </p>

                    <h4 class="text-center">Events </h4>
                    <p>
                        <asp:Label runat="server" ID="lbEvents"></asp:Label><br />
                    </p>
                </div>

                <div class="col-md-12">
                    <br />
                </div>

            </div>

        </div>




        <div class="panel panel-green">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" data-target="#collapseVolunteer" aria-expanded="true" aria-controls="collapseExample">
                        <i class="fa fa-share-square-o"></i>Volunteer Opportunities
                    </a>
                </h4>
            </div>
            <div class="panel-body collapse in" id="collapseVolunteer">

                <div class="col-md-12">
                    <h2 class="text-center">Volunteer Opportunities</h2>
                    <p>Based on your profile, here is a list of strategic serving opportunities that match your SHAPE. Pick one or two opportunities that you are most interested in.</p>

                    <asp:Repeater runat="server" ID="rpVolunteerOpportunities">
                        <ItemTemplate>

                            <div class="panel panel-default margin-t-md">
                                <div class="panel-heading clearfix">
                                    <h1 class="panel-title pull-left">
                                        <i class='<%# Eval("IconCssClass") %>'></i><%# Eval("Name") %>
                                    </h1>
                                </div>
                                <div class="panel-body">
                                    <div class="col-md-12">
                                        <p><%# Eval("Summary") %></p>
                                        <a class="btn btn-default" href="https://newpointe.org/VolunteerOpportunities/<%# Eval("Id") %>" role="button">More Info / I'm Interested!</a>
                                    </div>
                                </div>
                            </div>

                        </ItemTemplate>
                    </asp:Repeater>

                    <p>
                        <br />
                        Want to see more opportunities to use your gifts?  Click below!</p>
                    <a class="btn btn-newpointe" href="https://newpointe.org/VolunteerOpportunities/" role="button">See More Volunteer Opportunities</a>


                </div>

            </div>

            <div class="col-md-12">
                <br />
            </div>



        </div>

        <div class="col-md-12">
            <br />
        </div>

        <asp:Panel runat="server" Visible="False" ID="pnlAccount">
            <div class="panel panel-green">
                <div class="panel-heading">
                    <h4 class="panel-title">
                        <a data-toggle="collapse" data-target="#collapseAccount" aria-expanded="true" aria-controls="collapseExample">
                            <i class="fa fa-share-square-o"></i>MyNewPointe Account
                        </a>
                    </h4>
                </div>
                <div class="panel-body collapse in" id="collapseAccount">
                    <div class="col-md-12">
                    </div>

                    <asp:Panel ID="pnlSaveAccount" runat="server" Visible="true">
                        <div class="well">
                            <legend>Create a MyNewPointe Account</legend>
                            <fieldset>

                                <asp:PlaceHolder ID="phCreateLogin" runat="server" Visible="true">

                                    <Rock:RockTextBox ID="txtUserName" runat="server" Label="Username" CssClass="input-medium" />
                                    <Rock:RockTextBox ID="txtPassword" runat="server" Label="Password" CssClass="input-medium" TextMode="Password" />
                                    <Rock:RockTextBox ID="txtPasswordConfirm" runat="server" Label="Confirm Password" CssClass="input-medium" TextMode="Password" />

                                </asp:PlaceHolder>

                                <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                                <div id="divSaveActions" runat="server" class="actions">
                                    <asp:LinkButton ID="lbSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" OnClick="lbSaveAccount_Click" />
                                </div>
                            </fieldset>
                        </div>
                    </asp:Panel>

                </div>

                <div class="col-md-12">
                    <br />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
