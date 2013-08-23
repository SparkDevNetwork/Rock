<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:param name="application_path" />
    
  <xsl:template match="/">
      <section class="promo-slider container">
          <div class="flexslider">
			        <ul class="slides">
                  <xsl:for-each select="Ads/Ad">

                    <li>
                        <a>
                          <xsl:attribute name="href">
                            <xsl:value-of select="@DetailPageUrl"/>
                          </xsl:attribute>

                          <xsl:for-each select="./Attributes/Attribute[@Key='PromotionImage']" >
                            <xsl:value-of select="@Value" disable-output-escaping="yes"/>
                          </xsl:for-each>
                        </a>

                    </li>
                  </xsl:for-each>
              </ul>
		      </div>
		      <img class="slider-shadow">
            <xsl:attribute name="src">
              <xsl:value-of select="$application_path"/>Themes/GrayFabric/Assets/Images/slider-shadow.png
            </xsl:attribute>
          </img>  

	    </section>
  </xsl:template>
</xsl:stylesheet>
      

	
	  		
	  		
			    
			  