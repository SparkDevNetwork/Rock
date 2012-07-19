

            /* make a test word doc */
            /*
            DocX doc = DocX.Load( "D:\\Development\\Rock-ChMS\\RockWeb\\Assets\\Word Merge Docs\\test.docx" );
            
            doc.AddCustomProperty( new CustomProperty( "nick_name", "Mike" ) );
            doc.AddCustomProperty( new CustomProperty( "last_name", "Sever" ) );

            doc.SaveAs( "d:\\out.docx" );
             * */

            /* test person viewed transaction */
/*
            PersonViewTransaction transaction = new PersonViewTransaction();
            transaction.DateViewed = DateTime.Now;
            transaction.Source = "Site: " + PageInstance.Site.Id.ToString() + "Page: " + PageInstance.Id.ToString();
            if ( CurrentPersonId != null )
                transaction.ViewerPersonId = ( int )CurrentPersonId;
            transaction.IPAddress = Request.UserHostAddress;

            RockQueue.TransactionQueue.Enqueue( transaction );

            Rock.Communication.SendGridEmailProvider sgp = new Rock.Communication.SendGridEmailProvider();
            List<Rock.Communication.BouncedEmail> bouncedMail = sgp.BouncedEmails(false);
            bool result = sgp.DeleteBouncedEmail( "jon@jonedmiston.com" );
*/

            btnShowDialog.Click += new EventHandler( btnShowDialog_Click );
//            mdTest.SaveClick += new EventHandler( mdTest_SaveClick );

            TextBox tb = new TextBox();
            tb.ID = "tbTest";
            mdTest.Content.Controls.Add( tb );
        }

        void btnShowDialog_Click( object sender, EventArgs e )
        {
//            mdTest.Show();
        }

        void mdTest_SaveClick( object sender, EventArgs e )
        {
            string testText = tbContent.Text;

            TextBox tb = mdTest.FindControl( "tbTest" ) as TextBox;
            if ( tb != null )
            {
                if ( tb.Text.Trim() == string.Empty )
                {
                    //                    mdTest.Show();
                }
            }
        }

        void MyBlock_AttributesUpdated( object sender, EventArgs e )
        {
            ShowAttributeValue();
            pnlAttributeValues.Update();
        }

        private void ShowAttributeValue()
        {
            pnlAttribute.Visible = Boolean.Parse( AttributeValue( "ShowColor" ) ?? "False" );
            string color = AttributeValue( "Color" );
            if (color != string.Empty)
                pnlAttribute.BackColor = System.Drawing.Color.FromName( color );

            lMovie.Text = AttributeValue( "Movie" );

            pnlAttribute.BorderWidth = Convert.ToInt32( AttributeValue( "BorderWidth" ) );

            switch ( AttributeValue( "Border" ) )
            {
                //case "solid": pnlAttribute.BorderStyle = BorderStyle.Solid; break;
                //case "dashed": pnlAttribute.BorderStyle = BorderStyle.Dashed; break;
                //default: pnlAttribute.BorderStyle = BorderStyle.None; break;
            }

        }

        private void DisplayItem()
        {
            string cachedData = GetCacheItem() as string;
            if ( cachedData != null )
                lItemCache.Text = cachedData + " (From Cache)";
            else
            {
                string text = DateTime.Now.ToString();
                AddCacheItem( text );
                lItemCache.Text = text + " (Saved to Cache)";
            }
        }

        protected void bFlushItemCache_Click( object sender, EventArgs e )
        {
            FlushCacheItem();
            DisplayItem();
        }

        //protected void Button1_Click( object sender, EventArgs e )
        //{
        //    ObjectCache cache = MemoryCache.Default;

        //    string fileContents = cache["filecontents"] as string;

        //    if ( fileContents == null )
        //    {
        //        CacheItemPolicy policy = new CacheItemPolicy();
        //        policy.AbsoluteExpiration =
        //        DateTimeOffset.Now.AddSeconds( 10.0 );

        //        List<string> filePaths = new List<string>();
        //        string cachedFilePath = Server.MapPath( "~" ) + "\\cachedText.txt";
        //        filePaths.Add( cachedFilePath );

        //        policy.ChangeMonitors.Add( new
        //        HostFileChangeMonitor( filePaths ) );

        //        // Fetch the file contents.
        //        fileContents =
        //        File.ReadAllText( cachedFilePath ) + "\n" +
        //            " Using built-in cache " + "\n" + DateTime.Now.ToString();

        //        cache.Set( "filecontents", fileContents, policy );

        //        Label1.Text = fileContents;
        //    }

        //}
    }
}