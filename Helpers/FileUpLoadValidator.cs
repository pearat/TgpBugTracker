using System;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Web.Mvc;

namespace TgpBugTracker.Helpers
{

    [RequireHttps]
    public class FileUpLoadValidator
    {

        public string SaveUpLoadFile(HttpPostedFileBase upLoadFile, string fullPathName)
        {
            if (upLoadFile != null)
            {
                var fileName = Path.GetFileName(upLoadFile.FileName);
                var OkFileName = Regex.IsMatch(fileName, "[a-zA-Z0-9]{1,200}\\.[a-zA-Z0-9]{1,10}", RegexOptions.IgnoreCase);
                if (OkFileName)
                {
                    var extension = Path.GetExtension(upLoadFile.FileName);
                    var docFileType = (extension == ".pdf" || extension == ".doc" || extension == ".docx" ||
                                        extension == ".rtf" || extension == ".txt");
                    var validImage = FileUpLoadValidator.IsWebFriendlyImage(upLoadFile);

                    if (validImage || docFileType)
                    {
                        // code for saving the image file to a physical location.
                        // var fullPathName = Path.Combine(Server.MapPath("/Uploads"), fileName);
                        try
                        {
                            upLoadFile.SaveAs(fullPathName);
                        }
                        catch (Exception e)
                        {
                            fileName = "Tried to save[" + fullPathName + ", but this error occurred :" + e;
                            Debug.WriteLine(fileName);
                            return fileName;

                        }

                        if (docFileType)
                        {
                            // test whether extension and mimeType are consistent
                            var mimeType = FileUpLoadValidator.getMimeFromFile(fullPathName);
                            Debug.WriteLine("MimeType: " + mimeType);
                        }
                        // prepare a relative path to be stored in the database and used to display later on.
                        // ticket.Attachment = 


                        return "/Uploads/" + fileName;
                    }
                }
                else
                {
                    return "Problem with file name [[" + fileName + "]]";
                }
            }
            return null;
        }






        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private extern static System.UInt32 FindMimeFromData(
            System.UInt32 pBC,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            System.UInt32 cbSize,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
            System.UInt32 dwMimeFlags,
            out System.UInt32 ppwzMimeOut,
            System.UInt32 dwReserverd
        );

        // public static string getMimeFromFile(string filename)
        public static string getMimeFromFile(string upLoadFile)
        {
            if (!File.Exists(upLoadFile))
                throw new FileNotFoundException(upLoadFile + " not found");

            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(upLoadFile, FileMode.Open))
            {
                if (fs.Length >= 256)
                    fs.Read(buffer, 0, 256);
                else
                    fs.Read(buffer, 0, (int)fs.Length);
            }
            try
            {
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mime;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in getMimeFromFile(): " + e + " :");
                return "unknown/unknown";
            }
        }


        //IIS 6.0 Security Best Practices
        //http://technet.microsoft.com/en-us/library/cc782762(WS.10).aspx
        //Securing Sites with Web Site Permissions
        //http://technet.microsoft.com/en-us/library/cc756133(WS.10).aspx
        //IIS 6.0 Operations Guide
        //http://technet.microsoft.com/en-us/library/cc785089(WS.10).aspx
        //Improving Web Application Security: Threats and Countermeasures
        //http://msdn.microsoft.com/en-us/library/ms994921.aspx
        //Understanding the Built-In User and Group Accounts in IIS 7.0
        //http://www.iis.net/learn/get-started/planning-for-security/understanding-built-in-user-and-group-accounts-in-iis


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
                    return ImageFormat.Bmp.Equals(img.RawFormat) ||
                            ImageFormat.Gif.Equals(img.RawFormat) ||
                            ImageFormat.Icon.Equals(img.RawFormat) ||
                            ImageFormat.Jpeg.Equals(img.RawFormat) ||
                            ImageFormat.Png.Equals(img.RawFormat) ||
                            ImageFormat.Tiff.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}