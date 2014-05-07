using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Reporting.Dashboard
{
    public class ChartTheme
    {
        public ChartTheme()
        {
            this.SeriesColors = new Color[] { 
                ColorTranslator.FromHtml("#8498ab"),
                ColorTranslator.FromHtml("#a4b4c4"),
                ColorTranslator.FromHtml("#b9c7d5"),
                ColorTranslator.FromHtml("#c6d2df"),
                ColorTranslator.FromHtml("#d8e1ea")
            };
        }

        public Color[] SeriesColors { get; set; }
    }
}
