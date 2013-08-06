<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  
            <xsl:template match="/">
                <section class="promo-slider container">
                    <div class="flexslider">
			                  <ul class="slides">
                            <xsl:for-each select="Ads/Ad">

                              <li>
                                  <a>
                                    <xsl:attribute name="href">
                                      <xsl:value-of select="@Url"/>
                                    </xsl:attribute>
                                    <xsl:value-of select="@Title" disable-output-escaping="yes"/>
                                  </a>
                                
                                  <xsl:for-each select="./Attributes/Attribute[@name='Central Leadership']" >
                                    <div>
                                      <xsl:value-of select="@Value" disable-output-escaping="yes"/>
                                    </div>
                                  </xsl:for-each>
                                
                              </li>
                            </xsl:for-each>
                        </ul>
		                </div>
		                <img class="slider-shadow" src="./assets/img/slider-shadow.png" />

	              </section>
            </xsl:template>
          </xsl:stylesheet>
      

	
	  		
	  		
			    
			  