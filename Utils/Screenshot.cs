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
 * 01/03/2012      RC          1.11       Initial coding.
 * 05/08/2012      RC          2.11       Created another method that does not take a project as a parameter.
 *       
 * 
 */

using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RTI
{
    /// <summary>
    /// Take a screenshot of a UIElement. Store the 
    /// screenshot as a byte array of a JPG image.
    /// Return the  JPG byte array or store it to a file.
    /// Code found at: http://www.grumpydev.com/2009/01/03/taking-wpf-screenshots/
    /// </summary>
    public static class Screenshot
    {
        /// <summary>
        /// Gets a JPG "screenshot" of the current UIElement
        /// </summary>
        /// <param name="source">UIElement to screenshot</param>
        /// <param name="scale">Scale to render the screenshot</param>
        /// <param name="quality">JPG Quality</param>
        /// <returns>Byte array of JPG data</returns>
        public static byte[] GetJpgImage(this UIElement source, double scale, int quality)
        {
            double actualHeight = source.RenderSize.Height;
            double actualWidth = source.RenderSize.Width;

            double renderHeight = actualHeight * scale;
            double renderWidth = actualWidth * scale;

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            VisualBrush sourceBrush = new VisualBrush(source);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = quality;
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            Byte[] _imageArray;

            using (MemoryStream outputStream = new MemoryStream())
            {
                jpgEncoder.Save(outputStream);
                _imageArray = outputStream.ToArray();
            }

            return _imageArray;
        }

        /// <summary>
        /// Take a screenshot of a UIElement.  Then
        /// save the screenshot to the project folder.
        /// The filename will have the same name as the
        /// project with the extension .jpg.
        /// </summary>
        /// <param name="prj">Project to save screenshot to.</param>
        /// <param name="source">UIElement to take a screenshot of.</param>
        /// <param name="scale">Scale of the screenshot.</param>
        /// <param name="quality">Quality of the screenshot.</param>
        public static void SaveJpgImage(this UIElement source, double scale, int quality, Project prj)
        {
            // Take screenshot of the UIElement
            byte[] screenshot = GetJpgImage(source, scale, quality);

            // Create file name
            string fileName = prj.GetProjectImagePath();

            // Save to file
            try
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    using(BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        binaryWriter.Write(screenshot);
                        binaryWriter.Close();
                        binaryWriter.Dispose();
                    }
                }

                GC.Collect();
            }
            catch (Exception)
            {
                // Do nothing
            }
        
        }

        /// <summary>
        /// Take a screenshot of a UIElement.  Then
        /// save the screenshot to the project folder.
        /// The filename will have the same name as the
        /// project with the extension .jpg.
        /// </summary>
        /// <param name="source">UIElement to take a screenshot of.</param>
        /// <param name="scale">Scale of the screenshot.</param>
        /// <param name="quality">Quality of the screenshot.</param>
        /// <param name="fileName">File path for the image.</param>
        public static void SaveJpgImage(this UIElement source, double scale, int quality, string fileName)
        {
            // Take screenshot of the UIElement
            byte[] screenshot = GetJpgImage(source, scale, quality);

            // Save to file
            try
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        binaryWriter.Write(screenshot);
                        binaryWriter.Close();
                        binaryWriter.Dispose();
                    }
                }

                GC.Collect();
            }
            catch (Exception)
            {
                // Do nothing
            }

        }
    }
}
