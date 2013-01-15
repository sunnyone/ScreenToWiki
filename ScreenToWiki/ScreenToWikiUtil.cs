/*
 * Copyright (c) 2013 Yoichi Imai, All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using ScreenToWiki.ImageUploader;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace ScreenToWiki {
    public class ScreenToWikiUtil {
        public static string GetFileName(string extension)
        {
            return "ScreenShot_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
        }

        public static void SavePngFile(BitmapSource bitmap, string path)
        {
            PngBitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream fs = new System.IO.FileStream(path, FileMode.Create))
            {
                enc.Save(fs);
            }
        }

        public static void UseTempDir(Action<string> actionWithDir)
        {
            string randomDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                                        System.IO.Path.GetRandomFileName());
            try
            {
                System.IO.Directory.CreateDirectory(randomDir);

                actionWithDir(randomDir);
            }
            finally
            {
                System.IO.Directory.Delete(randomDir, true);
            }
        }

        // FIXME: this is insecure!! it is just intended to be invisible for human...
        public static string EncodePasswordString(string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        public static string DecodePasswordString(string encodedPassword)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedPassword));
        }

        public static IEnumerable<IImageUploader> GetImageUploaders()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);

            var imageUploaders = container.GetExportedValues<IImageUploader>();

            return imageUploaders;
        }
    }
}
