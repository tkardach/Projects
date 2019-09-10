using System;
using ImageProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace ImageProcessingTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCompress()
        {
            ImageProcessing.Compressor.CompressImage(@"Resources/TestPhoto.jpg", @"Resources/TestPhoto50.jpg", 50, ImageFormat.Jpeg);

            Assert.IsTrue(new FileInfo(@"Resources/TestPhoto.jpg").Length > new FileInfo(@"Resources/TestPhoto50.jpg").Length);
        }

        [TestMethod]
        public void TestRemoveExif()
        {
            var image = ImageProcessing.Compressor.RemoveExif(@"Resources/ExifTestPhoto.jpeg");
            image.Save(@"Resources/PostExifRemove.jpg", ImageFormat.Jpeg);

            Assert.IsTrue(new FileInfo(@"Resources/ExifTestPhoto.jpeg").Length > new FileInfo(@"Resources/PostExifRemove.jpg").Length);        }

        [TestMethod]
        public void TestResize()
        {
            using (Bitmap bmp = new Bitmap(@"Resources/TestPhoto.jpg"))
            {
                Bitmap resized = ImageProcessing.Compressor.ResizeImage(bmp, 512);

                int longest = resized.Width > resized.Height ? resized.Width : resized.Height;

                Assert.IsTrue(longest == 512);
            }
        }

        [TestMethod]
        public void TestThumbnailCreation()
        {
            var image = ImageProcessing.Compressor.RemoveExif(@"Resources/ExifTestPhoto90.jpeg");
            image.Save(@"Resources/PostExifRemove.jpg", ImageFormat.Jpeg);
            
            Bitmap bmp = new Bitmap(@"Resources/PostExifRemove.jpg");
            bmp = ImageProcessing.Compressor.ResizeImage(bmp, 512);
            bmp.Save(@"Resources/ThumbnailResized.jpg", bmp.RawFormat);
            
            ImageProcessing.Compressor.CompressImage(@"Resources/ThumbnailResized.jpg", @"Resources/ThumbnailResizedCompressed.jpg", 85, ImageFormat.Jpeg);
            Assert.IsTrue(new FileInfo(@"Resources/ThumbnailResized.jpg").Length > new FileInfo(@"Resources/ThumbnailResizedCompressed.jpg").Length);
        }
    }
}
