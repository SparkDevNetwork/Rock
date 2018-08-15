// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class LandingPageContent : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("C90FCC3A-1C80-4D70-8A52-2DA27C30EF7A",@"<div><h1><small>Join a Small Group</small>Become who God created you to be.</h1></div>","1BA0623F-4435-487C-9921-C6E4909F37CC");
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("B86F9EAB-1778-4947-8166-720C890C0391",@"<h3>Make Friends</h3>
<p>Ea doming saperet eleifend pro, facete mnesarchum qui ne, purto concludaturque ei mea. In his suscipit oporteat, meis assum tritani ea vix, quodsi eirmod id sea. Debitis adolescens scribentur eam in.</p>","702126D1-A93D-4B26-84F2-11D892A19344");
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("FE043AE3-40D1-4AE4-A9BD-9D170FC067D4",@"<h3>Have Fun</h3>
<p>Tation libris in his, sit eu nemore eleifend liberavisse. Ad ius dolor vulputate consetetur. Pri id phaedrum intellegam, sed id postea scriptorem. Eligendi dissentiunt usu ad, ei sale tractatos sit. Mea modo idque apeirian id, usu an essent efficiendi. Paulo labore mentitum in per, ea pri quas omittam.</p>","75D46236-52FB-4085-AF95-5178858ADD3A");
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("61EA9BD2-57E4-4897-A914-A158DE755641",@"<h3>Grow Together</h3>
<p>Virtute deseruisse ne mel, labore animal mediocritatem qui in, mei oratio causae eu. Has cu vidit viris, te vim ubique numquam noluisse. Cum quem sapientem voluptatibus no, at agam voluptua vis. Vim tollit accusam honestatis te, tota probatus.</p>","AC3B45DA-65DD-43F5-9AEF-3D36A9F10394");
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("42105CB7-E593-4102-A259-9676F1DC899C",@"<h3>Mission</h3>
<p>Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut. Nec tibique deleniti delectus te, zril quaestio conclusionemque vis no, posse appellantur mei ei. <span class=""highlight"">At vix corpora fastidii vulputate.</span> Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut, nec tibique deleniti delectus</p>
<img src=""https://source.unsplash.com/anV_zgNDZhc/2500x1000"">
<h3>Vision</h3>
<p>Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut. Nec tibique deleniti delectus te, zril quaestio conclusionemque vis no, posse appellantur mei ei. At vix corpora fastidii vulputate. Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut, nec tibique deleniti delectus</p>
<img src=""https://source.unsplash.com/45AJzT4mGOs/2500x1500"">
<h3>Teamwork</h3>
<p>Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut. Nec tibique deleniti delectus te, zril quaestio conclusionemque vis no, posse appellantur mei ei. At vix corpora fastidii vulputate. Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut, nec tibique deleniti delectus</p>
<div class=""split right"">
<img src=""https://source.unsplash.com/omeaHbEFlN4/1000x1000""> <h3>Vision</h3> <p>Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut. Nec tibique deleniti delectus te, zril quaestio conclusionemque vis no, posse appellantur mei ei. At vix corpora fastidii vulputate. Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut, nec tibique deleniti delectus</p>
</div>
<div class=""split left"">
<img src=""http://source.unsplash.com/E6HjQaB7UEA/1000x1000"">
<h3>Vision</h3>
<p>Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut. Nec tibique deleniti delectus te, zril quaestio conclusionemque vis no, posse appellantur mei ei. At vix corpora fastidii vulputate. Lorem ipsum dolor sit amet, cum nibh error sapientem at. Qui duis summo at, tale tibique conclusionemque pro ut, nec tibique deleniti delectus</p>
</div>  ","D4AE7C65-A8EF-45D1-8DB4-D663567D2770");
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("2A6C7BED-26E6-4A00-A7CF-4E27095ADAE4",@"<button type=""button"" class=""btn btn-primary"" data-toggle=""modal"" data-target=""#workflowModal""> Join a group </button>  <button type=""button"" class=""btn btn-ghost"" data-toggle=""modal"" data-target=""#workflowModal""> Learn more </button>","A8F43863-4D23-4CE6-ACA1-8A59F967A688");
            // Add/Update HtmlContent for Block: HTML Content
            RockMigrationHelper.UpdateHtmlContentBlock("D8472640-94A7-4763-B0CD-BF6691699D2B",@"<h1>Lead a {{ 'Global' | Attribute:""OrganizationName"" }} Group</h1> <a href=""#"" class=""btn btn-primary btn-lg"">Lead a group</a>","28099731-43E8-43A2-AAEB-0D76E51CFF76");   

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
