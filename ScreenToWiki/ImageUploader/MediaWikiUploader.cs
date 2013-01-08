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
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using EasyHttp.Http;
using EasyHttp.Infrastructure;

namespace ScreenToWiki.ImageUploader
{
    [Export(typeof(IImageUploader))]
    public class MediaWikiUploader : IImageUploader
    {
        public Uri UploadImage(BitmapSource bitmapSource, UploadConfig config)
        {
            string url = config.Url;
            string userName = config.UserName;
            string password = config.Password;

            HttpClient http = new HttpClient();
            http.Request.Accept = HttpContentTypes.ApplicationJson;

            string apiUrl = url + "/api.php";

            // at first, request action=login (to get token)
            var paramsLogin1 = new Dictionary<string, string>()
                {
                    {"action", "login"},
                    {"format", "json"},
                    {"lgname", userName},
                    {"lgpassword", password}
                };

            var respLogin1 = http.Post(apiUrl, paramsLogin1, HttpContentTypes.ApplicationXWwwFormUrlEncoded);
            var treeLogin1 = respLogin1.DynamicBody;

            string resultLogin1 = treeLogin1.login.result;
            if (resultLogin1 != "NeedToken")
            {
                throw new UploadFailedException(
                    String.Format("Failed to login (first). You may be using older MediaWiki. (result: {0})", resultLogin1));
            }

            string token = treeLogin1.login.token;
            string cookieprefix = treeLogin1.login.cookieprefix;
            Cookie cookieSession = respLogin1.Cookies[cookieprefix + "_session"];

            // next, request login really.
            var paramsLogin2 = new Dictionary<string, string>()
                {
                    {"action", "login"},
                    {"format", "json"},
                    {"lgname", userName},
                    {"lgpassword", password},
                    {"lgtoken", token}
                };
            http.Request.Cookies = respLogin1.Cookies;
            var respLogin2 = http.Post(apiUrl, paramsLogin2, HttpContentTypes.ApplicationXWwwFormUrlEncoded);
            var treeLogin2 = respLogin2.DynamicBody;

            string resultLogin2 = treeLogin2.login.result;
            if (resultLogin2 != "Success")
            {
                throw new UploadFailedException(
                    String.Format("Failed to login. Check username and password. (result: {0})", resultLogin2));
            }

            string userId = treeLogin2.login.lguserid.ToString();
            string lgtoken = treeLogin2.login.lgtoken;

            // get edittoken
            http.Request.Cookies.Add(new Cookie(cookieprefix + "UserName", userName, "/", cookieSession.Value));
            http.Request.Cookies.Add(new Cookie(cookieprefix + "UserId", userId, "/", cookieSession.Value));
            http.Request.Cookies.Add(new Cookie(cookieprefix + "Token", lgtoken, "/", cookieSession.Value));
            var paramsQuery = new Dictionary<string, string>()
                {
                    {"action", "query"},
                    {"format", "json"},
                    {"prop", "info"},
                    {"intoken", "edit"},
                    {"titles", "NonExistPageToGetEditToken"}
                };

            var respQuery = http.Post(apiUrl, paramsQuery, HttpContentTypes.ApplicationXWwwFormUrlEncoded);
            var treeQuery = respQuery.DynamicBody;

            var pages = treeQuery.query.pages;
            // "pages" has "-1" key... since to call "-1" is difficult. this is dirty way.
            var page = (dynamic) ((IDictionary<string, object>)pages)["-1"];
            string edittoken = page.edittoken;

            if (edittoken.Length < 3) {
                throw new UploadFailedException(
                    String.Format("Failed to get edittoken (edittoken: {0})", edittoken));
            }

            string filename = ScreenToWikiUtil.GetFileName(".png");
            string descriptionUrl = null;
            ScreenToWikiUtil.UseTempDir((tempDir) =>
            {
                string path = System.IO.Path.Combine(tempDir, filename);

                ScreenToWikiUtil.SavePngFile(bitmapSource, path);

                var fileData = new FileData();
                fileData.ContentType = "image/png";
                fileData.ContentTransferEncoding = HttpContentEncoding.Binary;
                fileData.FieldName = "file";
                fileData.Filename = path;

                var paramsUpload = new Dictionary<string, object>()
                    {
                        {"action", "upload"},
                        {"format", "json"},
                        {"filename", filename},
                        {"token", edittoken}
                    };
                var respUpload = http.Post(apiUrl, paramsUpload, new List<FileData>() {fileData});
                var respTree = respUpload.DynamicBody;

                string result = respTree.upload.result;

                if (result != "Success")
                {
                    throw new UploadFailedException(String.Format("Uploading failed (result: {0})", result));
                }

                descriptionUrl = respTree.upload.imageinfo.descriptionurl;
            });

            return new Uri(descriptionUrl);
        }
    }
}
