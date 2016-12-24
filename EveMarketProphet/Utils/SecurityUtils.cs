using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace EveMarketProphet.Utils
{
    public static class SecurityUtils
    {
        // https://web.archive.org/web/20120219150840/http://blog.evepanel.net/eve-online/igb/colors-of-the-security-status.html

        public static Dictionary<double, Color> SecurityColors => new Dictionary<double, Color>()
        {
            { 1.0, Color.FromRgb(47, 239, 239) },
            { 0.9, Color.FromRgb(72, 240, 192) },
            { 0.8, Color.FromRgb(0, 239, 71) },
            { 0.7, Color.FromRgb(0, 240, 0) },
            { 0.6, Color.FromRgb(143, 239, 47) },
            { 0.5, Color.FromRgb(239, 239, 0) },
            { 0.4, Color.FromRgb(215, 119, 0) },
            { 0.3, Color.FromRgb(240, 96, 0) },
            { 0.2, Color.FromRgb(240, 72, 0) },
            { 0.1, Color.FromRgb(215, 48, 0) },
            { 0.0, Color.FromRgb(240, 0, 0) }
        };

        public static double RoundSecurity(double trueSecurity)
        {
            // http://wiki.eveuniversity.org/System_Security#True_Security

            if (trueSecurity <= 0.0) trueSecurity = 0.0;
            else if (trueSecurity > 0.0 && trueSecurity < 0.05) trueSecurity = 0.05;
            
            return Math.Round(trueSecurity, 1, MidpointRounding.ToEven);
        }
    }
}
