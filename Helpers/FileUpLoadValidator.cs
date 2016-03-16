using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TgpBugTracker.Helpers
{
    public class FileUpLoadValidator
    {
        public static bool IsWebFriendlyImage(HttpPostedFileBase file)
        {
            // check for actual object
            if (file == null)
                return false;

            // Filename regular expression: [a - zA - Z0 - 9]{ 1,200}\.[a-zA-Z0-9]{1,10})
            
            // check whether size is less than 1KB and greater than 2MB
            if (file.ContentLength < 1024 || file.ContentLength > 2 * 1024 * 1024)
                return false;

            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                           ImageFormat.Png.Equals(img.RawFormat) ||
                           ImageFormat.Gif.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}