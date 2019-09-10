using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageProcessing
{
    public static class Compressor
    {

        #region Remove Exif
        private static Stream PatchAwayExif(Stream inStream, Stream outStream)
        {
            byte[] jpegHeader = new byte[2];
            jpegHeader[0] = (byte)inStream.ReadByte();
            jpegHeader[1] = (byte)inStream.ReadByte();
            if (jpegHeader[0] == 0xff && jpegHeader[1] == 0xd8) //check if it's a jpeg file
            {
                SkipAppHeaderSection(inStream);
            }
            outStream.WriteByte(0xff);
            outStream.WriteByte(0xd8);

            int readCount;
            byte[] readBuffer = new byte[4096];
            while ((readCount = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                outStream.Write(readBuffer, 0, readCount);

            return outStream;
        }

        private static void SkipAppHeaderSection(Stream inStream)
        {
            byte[] header = new byte[2];
            header[0] = (byte)inStream.ReadByte();
            header[1] = (byte)inStream.ReadByte();

            while (header[0] == 0xff && (header[1] >= 0xe0 && header[1] <= 0xef))
            {
                int exifLength = inStream.ReadByte();
                exifLength = exifLength << 8;
                exifLength |= inStream.ReadByte();

                for (int i = 0; i < exifLength - 2; i++)
                {
                    inStream.ReadByte();
                }
                header[0] = (byte)inStream.ReadByte();
                header[1] = (byte)inStream.ReadByte();
            }
            inStream.Position -= 2; //skip back two bytes
        }

        public static Image RemoveExif(string file)
        {
            Image bmp = Image.FromFile(file);
            if (bmp.RawFormat.Equals(ImageFormat.Jpeg) && bmp.RawFormat.Equals(ImageFormat.Png))
                return bmp;

            // Convert image to memory stream
            var ms = new MemoryStream();
            bmp.Save(ms, bmp.RawFormat);
            ms.Position = 0;

            var os = new MemoryStream();
            PatchAwayExif(ms, os);

            return Image.FromStream(os);
        }

        #endregion

        /// <summary>
        /// Resize an image by a given max resolution. Scales image based on longest side = max resolution.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="maxLength">The resized image's new longest side.</param>
        /// <returns>Resized image.</returns>
        public static Bitmap ResizeImage(Image image, int maxLength)
        {
            int longestSide = image.Width > image.Height ? image.Width : image.Height;
            int shortestSide = image.Width < image.Height ? image.Width : image.Height;

            if (longestSide < maxLength) return (Bitmap)image;

            double ratio = shortestSide / (double)longestSide;
            double scale = maxLength / (double)longestSide;
            int newWidth = (int)(scale * longestSide);
            int newHeight = (int)(newWidth * ratio);
            return ResizeImage(image, newWidth, newHeight);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Compresses the image based on a certain factor.
        /// </summary>
        /// <param name="image">Image being compressed.</param>
        /// <param name="factor">Factor that will determine level of compression.</param>
        /// <param name="type">Type of image being compressed.</param>
        public static void CompressImage(string image, byte factor, ImageFormat type)
        {
            CompressImage(image, image, factor, type);
        }

        /// <summary>
        /// Compresses the image based on a certain factor.
        /// </summary>
        /// <param name="source">Source image location.</param>
        /// <param name="dest">Destination where new image is saved.</param>
        /// <param name="factor">Factor that will determine level of compression.</param>
        /// <param name="type">Type of image being compressed.</param>
        public static void CompressImage(string source, string dest, byte factor, ImageFormat type)
        {
            // Get a bitmap. The using statement ensures objects  
            // are automatically disposed from memory after use.  
            using (Bitmap bmp1 = new Bitmap(source))
            {
                ImageCodecInfo imageCodec = GetEncoder(type);

                // Create an Encoder object based on the GUID  
                // for the Quality parameter category.  
                System.Drawing.Imaging.Encoder encoder =
                    System.Drawing.Imaging.Encoder.Quality;

                // Create an EncoderParameters object.  
                // An EncoderParameters object has an array of EncoderParameter  
                // objects. In this case, there is only one  
                // EncoderParameter object in the array.  
                EncoderParameters encoderParams = new EncoderParameters(1);

                EncoderParameter encoderParam = new EncoderParameter(encoder, (long)factor);
                encoderParams.Param[0] = encoderParam;
                bmp1.Save(dest, imageCodec, encoderParams);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
