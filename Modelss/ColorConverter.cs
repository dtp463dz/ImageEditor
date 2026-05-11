using System;
using System.Collections.Generic;
using System.Text;

namespace ImageEditor.Modelss
{
    public static class ColorConverter
    {
        // Chuyển RGB sang HSL
        public static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;
            double max = Math.Max(Math.Max(rNorm, gNorm), bNorm);
            double min = Math.Min(Math.Min(rNorm, gNorm), bNorm);
            double delta = max - min;
            l = (max + min) / 2.0;
            if (delta == 0)
            {
                s = 0;
                h = 0;
            }
            else
            {
                s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);
                if (max == rNorm)
                    h = (gNorm - bNorm) / delta + (gNorm < bNorm ? 6 : 0);
                else if (max == gNorm)
                    h = (bNorm - rNorm) / delta + 2;
                else
                    h = (rNorm - gNorm) / delta + 4;
                h /= 6.0;
            }
        }
        // chuyển HSL sang RGB
        public static void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
        {
            double rNorm, gNorm, bNorm;
            if (s == 0)
            {
                rNorm = gNorm = bNorm = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                rNorm = HueToRgb(p, q, h + 1.0 / 3.0);
                gNorm = HueToRgb(p, q, h);
                bNorm = HueToRgb(p, q, h - 1.0 / 3.0);
            }
            r = (byte)(rNorm * 255);
            g = (byte)(gNorm * 255);
            b = (byte)(bNorm * 255);
        }
        // hsl conversion
        public static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

    }

}
