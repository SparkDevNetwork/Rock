<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <section class="promo-secondary container">
      <ul>
        
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
    </section>
  </xsl:template>
</xsl:stylesheet>


	  		
			    
			  