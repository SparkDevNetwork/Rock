![](https://user-images.githubusercontent.com/81330042/113305126-5610fb00-92c8-11eb-9be0-48644f6f17bb.png)


# Vimeo Sync
_Tested/Supported in Rock Version:  8.0-13.0_  
_Released:  1/13/2019_     
_Updated:  2/2/2022_ 

## Summary

**This plugin requires a Vimeo Pro or higher account**

This plugin gives you 2 ways to sync video items from Vimeo to your Rock install. The first is to sync all of the videos in a Vimeo account to a content channel. This would be used primarily for an initial sync to a new content channel. The second allows you to sync the video information to a specific content channel item. This would be used weekly after the video is uploaded to Vimeo to add the video and information to your website.



Quick Links:

- [What's New](#whats-new)
- [Vimeo Setup](#vimeo-setup)
- [Configuration](#configuration)
- [Syncing with Vimeo](#syncing-with-vimeo)



## What's New

- The following new goodness will be added to your Rock install with this plugin:
  - **New Block**: Content Channel Item Vimeo Sync 
  - **New Block**:  Content Channel Item Vimeo Sync All



## Vimeo Setup

You will need to setup a Vimeo developer account at https://developer.vimeo.com if you don't already have one. 

1. Create a new app.  
   ![](https://user-images.githubusercontent.com/81330042/113305883-1991cf00-92c9-11eb-9b9a-e488e5263c4b.png)
2. In your app configuration, under Authentication check the boxes for Public and Private then click the Generate button.
   ![](https://user-images.githubusercontent.com/81330042/113305955-2d3d3580-92c9-11eb-9a50-3f894d8bee14.png)
3. **Be sure to copy the token!** If you do not copy it now, it will be masked when you come back to it and you will have to create a new token.

   ![](https://user-images.githubusercontent.com/81330042/113306063-4b0a9a80-92c9-11eb-8eeb-90a60b187f66.png)
4. If you are planning to sync entire Content Channels, you will also need your UserID from your profile settings. Go to your profile > under the edit menu choose Profile Settings.
   
   ![](https://user-images.githubusercontent.com/81330042/113306139-62e21e80-92c9-11eb-8ce0-92a5f305a2c7.png)




## Configuration

#### You Need to Add

Before you can sync videos to Content Channel Items, you will need to add item attributes to your content channel. Attributes can be added to entire Content Channel Types or to individual Content Channels depending on your use case.

1. On Admin Tools > CMS Configuration > Content Channels or Content Channel Types, edit the Content Channel (Type) you want to sync

2. Under Item Attributes, add the following attributes

   |    Name    | Field Type |
   | :--------: | :--------: |
   |  Vimeo Id  |  Integer   |
   |  Duration  |  Integer   |
   |   Image    |  URL Link  |
   |  Video HD  |  URL Link  |
   | Video HTTP |  URL Link  |
   |  Video SD  |  URL Link  |

3. For syncing individual Content Channel Items, on the Tools > Content > Edit Content Channel Item page, add the Content Channel Item Vimeo Sync block to the page (generally above the existing blocks on the page)

4. For syncing the entire Content Channel, on the Admin Tools > CMS Configuration > Content Channels > Content Channel Details page, add the Content Channel Item Vimeo Sync All block



#### Content Channel Item Vimeo Sync Block

![](https://user-images.githubusercontent.com/81330042/113307495-cd478e80-92ca-11eb-8eac-f701b2e98f14.png)

> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;1&nbsp;&nbsp;</span>**Name** Block name
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;2&nbsp;&nbsp;</span>**Vimeo Id Key** The attribute key for the Video Id attribute
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;3&nbsp;&nbsp;</span>**Access Token** The authentication token for Vimeo
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;4&nbsp;&nbsp;</span>**Preview** Indicate if a preview of the Vimeo should be displayed
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;5&nbsp;&nbsp;</span>**Preview Width** The bootstrap column width to display the preview in
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;6&nbsp;&nbsp;</span>**Sync Name** Indicate if the video name should be stored
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;7&nbsp;&nbsp;</span>**Sync Description** Indicate if the video description should be stored
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;8&nbsp;&nbsp;</span>**Image Attribute Key** The attribute key for the Vimeo Image URL. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;9&nbsp;&nbsp;</span>**Image Width** The desired width of the image to link to in the image url
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;10&nbsp;&nbsp;</span>**Duration Attribute Key** The attribute key for the Vimeo Duration. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;11&nbsp;&nbsp;</span>**HD Video Attribute Key** The attribute key for the HD Video. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;12&nbsp;&nbsp;</span>**SD Video Attribute Key** The attribute key for the SD Video. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;13&nbsp;&nbsp;</span>**HLS Video Attribute Key** The attribute key for the HLS Video. Leave blank to never sync



#### Content Channel Item Vimeo Sync All Block

![](https://user-images.githubusercontent.com/81330042/113307628-f7994c00-92ca-11eb-9013-d2a13c1dc359.png)

> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;1&nbsp;&nbsp;</span>**Name** Block name
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;2&nbsp;&nbsp;</span>**Vimeo Id Key** The attribute key for the Video Id attribute
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;3&nbsp;&nbsp;</span>**Vimeo User Id** The User Id of the Vimeo account to sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;4&nbsp;&nbsp;</span>**Access Token** The authentication token for Vimeo
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;5&nbsp;&nbsp;</span>**Sync Name** Indicate if the video name should be stored
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;6&nbsp;&nbsp;</span>**Sync Description** Indicate if the video description should be stored
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;7&nbsp;&nbsp;</span>**Image Attribute Key** The attribute key for the Vimeo Image URL. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;8&nbsp;&nbsp;</span>**Image Width** The desired width of the image to link to in the image url
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;9&nbsp;&nbsp;</span>**Duration Attribute Key** The attribute key for the Vimeo Duration. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;10&nbsp;&nbsp;</span>**HD Video Attribute Key** The attribute key for the HD Video. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;11&nbsp;&nbsp;</span>**SD Video Attribute Key** The attribute key for the SD Video. Leave blank to never sync
>
> <span style="padding-left: 30px; margin-right: 10px; width: .8em;background: #d21919; border-radius: 100%; color: white; text-align: center; display: inline-block;">&nbsp;&nbsp;12&nbsp;&nbsp;</span>**HLS Video Attribute Key** The attribute key for the HLS Video. Leave blank to never sync



## Syncing with Vimeo

#### To Sync an individual Content Channel Item

1. Upload your video to your Vimeo account

2. Get the Vimeo video Id

3. On Tools > Content, create a new Content Channel Item using the plus button on the grid

4. In the Vimeo Sync block, enter the Vimeo Id and the Active date

5. Check the boxes for the attributes you would like to sync and click the Run Vimeo Sync Button
   ![](https://user-images.githubusercontent.com/81330042/113307729-139ced80-92cb-11eb-828a-9374903d67ff.png)

6. After the sync has run once, the image for the video will appear. If changes are made on Vimeo after the initial sync, you can resync using the button.
   ![](https://user-images.githubusercontent.com/81330042/113307795-231c3680-92cb-11eb-82d8-b0143d616301.png)



#### To Sync a Content Channel

1. Create all of the items for the videos in your Content Channel
2. Enter the Vimeo video Ids for all of the items
3. On the Admin Tools > CMS Configuration > Content Channels > Content Channel Details page, check the boxes for the attributes you would like to sync and click the Run Vimeo Sync button
   ![](https://user-images.githubusercontent.com/81330042/113307895-3cbd7e00-92cb-11eb-87d0-b31ed9b99a75.png)
