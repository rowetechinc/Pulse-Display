/*
 * Copyright © 2011 
 * Rowe Technology Inc.
 * All rights reserved.
 * http://www.rowetechinc.com
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification is NOT permitted.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 * 
 * HISTORY
 * -----------------------------------------------------------------
 * Date            Initials    Version    Comments
 * -----------------------------------------------------------------
 * 06/13/2012      RC          2.11       Initial coding.
 * 07/20/2012      RC          2.12       Changed default colors.
 * 01/16/2014      RC          3.2.3      Changed the default colors to a string.
 */

namespace RTI
{

    using OxyPlot;
    using System.Collections.Generic;

    /// <summary>
    /// Methods for colors for the oxy line plots.
    /// </summary>
    public class BeamColor
    {
        /// <summary>
        /// Default Beam 0 Color.
        /// </summary>
        public static readonly string DEFAULT_COLOR_BEAM_0 = OxyColors.Chartreuse.ToString();        // OxyColors.LightGreen;

        /// <summary>
        /// Default Beam 1 Color.
        /// </summary>
        public static readonly string DEFAULT_COLOR_BEAM_1 = OxyColors.Orange.ToString();

        /// <summary>
        /// Default Beam 2 Color.
        /// </summary>
        public static readonly string DEFAULT_COLOR_BEAM_2 = OxyColors.DeepSkyBlue.ToString();

        /// <summary>
        /// Default Beam 3 Color.
        /// </summary>
        public static readonly string DEFAULT_COLOR_BEAM_3 = OxyColors.Crimson.ToString();


        /// <summary>
        /// Default Vertical Beam Color.
        /// </summary>
        public static readonly string DEFAULT_COLOR_BEAM_VERT = OxyColors.Khaki.ToString();

        /// <summary>
        /// Color as an Int.
        /// The string will be #AARRGGBB
        /// Need to remove the # 
        /// Then convert to an int.
        /// Convert back: OxyColor.FromUInt32(UInt32.Parse(beam, System.Globalization.NumberStyles.HexNumber));
        /// </summary>
        /// <returns>Int of the color.</returns>
        public static string ColorValue(OxyColor color)
        {
            string beam = color.ToString();

            // Remove #
            return beam.Substring(1, beam.Length - 1);
        }

        /// <summary>
        /// Create a list of all the possible colors used
        /// in a plot.
        /// </summary>
        /// <returns>List of all the colors.</returns>
        public static List<OxyColor> GetBeamColorList()
        {
            List<OxyColor>  BeamColorList = new List<OxyColor>();
            BeamColorList.Add(OxyColors.AliceBlue);
            BeamColorList.Add(OxyColors.AntiqueWhite);
            BeamColorList.Add(OxyColors.Aqua);
            BeamColorList.Add(OxyColors.Aquamarine);
            BeamColorList.Add(OxyColors.Azure);
            BeamColorList.Add(OxyColors.Beige);
            BeamColorList.Add(OxyColors.Bisque);
            BeamColorList.Add(OxyColors.Black);
            BeamColorList.Add(OxyColors.BlanchedAlmond);
            BeamColorList.Add(OxyColors.Blue);
            BeamColorList.Add(OxyColors.BlueViolet);
            BeamColorList.Add(OxyColors.Brown);
            BeamColorList.Add(OxyColors.BurlyWood);
            BeamColorList.Add(OxyColors.CadetBlue);
            BeamColorList.Add(OxyColors.Chartreuse);
            BeamColorList.Add(OxyColors.Chocolate);
            BeamColorList.Add(OxyColors.Coral);
            BeamColorList.Add(OxyColors.CornflowerBlue);
            BeamColorList.Add(OxyColors.Cornsilk);
            BeamColorList.Add(OxyColors.Crimson);
            BeamColorList.Add(OxyColors.Cyan);
            BeamColorList.Add(OxyColors.DarkBlue);
            BeamColorList.Add(OxyColors.DarkCyan);
            BeamColorList.Add(OxyColors.DarkGoldenrod);
            BeamColorList.Add(OxyColors.DarkGray);
            BeamColorList.Add(OxyColors.DarkGreen);
            BeamColorList.Add(OxyColors.DarkKhaki);
            BeamColorList.Add(OxyColors.DarkMagenta);
            BeamColorList.Add(OxyColors.DarkOliveGreen);
            BeamColorList.Add(OxyColors.DarkOrange);
            BeamColorList.Add(OxyColors.DarkOrchid);
            BeamColorList.Add(OxyColors.DarkRed);
            BeamColorList.Add(OxyColors.DarkSalmon);
            BeamColorList.Add(OxyColors.DarkSeaGreen);
            BeamColorList.Add(OxyColors.DarkSlateBlue);
            BeamColorList.Add(OxyColors.DarkSlateGray);
            BeamColorList.Add(OxyColors.DarkTurquoise);
            BeamColorList.Add(OxyColors.DarkViolet);
            BeamColorList.Add(OxyColors.DeepPink);
            BeamColorList.Add(OxyColors.DeepSkyBlue);
            BeamColorList.Add(OxyColors.DimGray);
            BeamColorList.Add(OxyColors.DodgerBlue);
            BeamColorList.Add(OxyColors.Firebrick);
            BeamColorList.Add(OxyColors.FloralWhite);
            BeamColorList.Add(OxyColors.ForestGreen);
            BeamColorList.Add(OxyColors.Fuchsia);
            BeamColorList.Add(OxyColors.Gainsboro);
            BeamColorList.Add(OxyColors.GhostWhite);
            BeamColorList.Add(OxyColors.Gold);
            BeamColorList.Add(OxyColors.Goldenrod);
            BeamColorList.Add(OxyColors.Gray);
            BeamColorList.Add(OxyColors.Green);
            BeamColorList.Add(OxyColors.GreenYellow);
            BeamColorList.Add(OxyColors.Honeydew);
            BeamColorList.Add(OxyColors.HotPink);
            BeamColorList.Add(OxyColors.IndianRed);
            BeamColorList.Add(OxyColors.Indigo);
            BeamColorList.Add(OxyColors.Ivory);
            BeamColorList.Add(OxyColors.Khaki);
            BeamColorList.Add(OxyColors.Lavender);
            BeamColorList.Add(OxyColors.LavenderBlush);
            BeamColorList.Add(OxyColors.LawnGreen);
            BeamColorList.Add(OxyColors.LemonChiffon);
            BeamColorList.Add(OxyColors.LightBlue);
            BeamColorList.Add(OxyColors.LightCoral);
            BeamColorList.Add(OxyColors.LightCyan);
            BeamColorList.Add(OxyColors.LightGoldenrodYellow);
            BeamColorList.Add(OxyColors.LightGray);
            BeamColorList.Add(OxyColors.LightGreen);
            BeamColorList.Add(OxyColors.LightPink);
            BeamColorList.Add(OxyColors.LightSalmon);
            BeamColorList.Add(OxyColors.LightSeaGreen);
            BeamColorList.Add(OxyColors.LightSkyBlue);
            BeamColorList.Add(OxyColors.LightSlateGray);
            BeamColorList.Add(OxyColors.LightSteelBlue);
            BeamColorList.Add(OxyColors.LightYellow);
            BeamColorList.Add(OxyColors.Lime);
            BeamColorList.Add(OxyColors.LimeGreen);
            BeamColorList.Add(OxyColors.Linen);
            BeamColorList.Add(OxyColors.Magenta);
            BeamColorList.Add(OxyColors.Maroon);
            BeamColorList.Add(OxyColors.MediumAquamarine);
            BeamColorList.Add(OxyColors.MediumBlue);
            BeamColorList.Add(OxyColors.MediumOrchid);
            BeamColorList.Add(OxyColors.MediumPurple);
            BeamColorList.Add(OxyColors.MediumSeaGreen);
            BeamColorList.Add(OxyColors.MediumSlateBlue);
            BeamColorList.Add(OxyColors.MediumSpringGreen);
            BeamColorList.Add(OxyColors.MediumTurquoise);
            BeamColorList.Add(OxyColors.MediumVioletRed);
            BeamColorList.Add(OxyColors.MidnightBlue);
            BeamColorList.Add(OxyColors.MintCream);
            BeamColorList.Add(OxyColors.MistyRose);
            BeamColorList.Add(OxyColors.Moccasin);
            BeamColorList.Add(OxyColors.NavajoWhite);
            BeamColorList.Add(OxyColors.Navy);
            BeamColorList.Add(OxyColors.OldLace);
            BeamColorList.Add(OxyColors.Olive);
            BeamColorList.Add(OxyColors.OliveDrab);
            BeamColorList.Add(OxyColors.Orange);
            BeamColorList.Add(OxyColors.OrangeRed);
            BeamColorList.Add(OxyColors.Orchid);
            BeamColorList.Add(OxyColors.PaleGoldenrod);
            BeamColorList.Add(OxyColors.PaleGreen);
            BeamColorList.Add(OxyColors.PaleTurquoise);
            BeamColorList.Add(OxyColors.PaleVioletRed);
            BeamColorList.Add(OxyColors.PapayaWhip);
            BeamColorList.Add(OxyColors.PeachPuff);
            BeamColorList.Add(OxyColors.Peru);
            BeamColorList.Add(OxyColors.Pink);
            BeamColorList.Add(OxyColors.Plum);
            BeamColorList.Add(OxyColors.PowderBlue);
            BeamColorList.Add(OxyColors.Purple);
            BeamColorList.Add(OxyColors.Red);
            BeamColorList.Add(OxyColors.RosyBrown);
            BeamColorList.Add(OxyColors.RoyalBlue);
            BeamColorList.Add(OxyColors.SaddleBrown);
            BeamColorList.Add(OxyColors.Salmon);
            BeamColorList.Add(OxyColors.SandyBrown);
            BeamColorList.Add(OxyColors.SeaGreen);
            BeamColorList.Add(OxyColors.SeaShell);
            BeamColorList.Add(OxyColors.Sienna);
            BeamColorList.Add(OxyColors.Silver);
            BeamColorList.Add(OxyColors.SkyBlue);
            BeamColorList.Add(OxyColors.SlateBlue);
            BeamColorList.Add(OxyColors.SlateGray);
            BeamColorList.Add(OxyColors.Snow);
            BeamColorList.Add(OxyColors.SpringGreen);
            BeamColorList.Add(OxyColors.SteelBlue);
            BeamColorList.Add(OxyColors.Tan);
            BeamColorList.Add(OxyColors.Teal);
            BeamColorList.Add(OxyColors.Thistle);
            BeamColorList.Add(OxyColors.Tomato);
            BeamColorList.Add(OxyColors.Transparent);
            BeamColorList.Add(OxyColors.Turquoise);
            BeamColorList.Add(OxyColors.Violet);
            BeamColorList.Add(OxyColors.Wheat);
            BeamColorList.Add(OxyColors.White);
            BeamColorList.Add(OxyColors.WhiteSmoke);
            BeamColorList.Add(OxyColors.Yellow);
            BeamColorList.Add(OxyColors.YellowGreen);

            return BeamColorList;
        }
    }
}
