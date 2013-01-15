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
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using ScreenToWiki.ImageUploader;
using ScreenToWiki.Properties;

namespace ScreenToWiki
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool selectionStarted = false;
        double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        private IEnumerable<IImageUploader> imageUploaders;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imageUploaders = ScreenToWikiUtil.GetImageUploaders();

            int width = 0, height = 0;
            foreach (var s in System.Windows.Forms.Screen.AllScreens)
            {
                width += s.Bounds.Width;
                height += s.Bounds.Height;
            }

            this.Left = 0;
            this.Top = 0;
            this.Width = width;
            this.Height = height;

            this.Cursor = Cursors.Cross;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

        private Rect getRectFromXY()
        {
            double left = (x1 < x2) ? x1 : x2;
            double top = (y1 < y2) ? y1 : y2;
            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);

            return new Rect(left, top, width, height);
        }

        private void updateRectSize()
        {
            Rect rect = getRectFromXY();

            Canvas.SetLeft(this.rectangle, rect.Left);
            this.rectangle.Width = rect.Width;

            Canvas.SetTop(this.rectangle, rect.Top);
            this.rectangle.Height = rect.Height;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);

            x1 = x2 = point.X;
            y1 = y2 = point.Y;

            updateRectSize();

            selectionStarted = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.selectionStarted)
                return;

            var point = e.GetPosition(this);

            x2 = point.X;
            y2 = point.Y;

            updateRectSize();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);

            x2 = point.X;
            y2 = point.Y;

            captureAndUploadScreenShot();

            Application.Current.Shutdown();
        }

        private void loadConfig(UploadConfig config)
        {
            config.UploaderTypeName = Settings.Default.UploaderTypeName;
            config.Url = Settings.Default.Url;
            config.UserName = Settings.Default.UserName;
            config.SavePassword = Settings.Default.SavePassword;

            // FIXME: insecure
            string passwordStr = Settings.Default.Password;
            if (!String.IsNullOrEmpty(passwordStr))
            {
                config.Password = ScreenToWikiUtil.DecodePasswordString(passwordStr);
            }
        }

        private void saveConfig(UploadConfig config)
        {
            Settings.Default.UploaderTypeName = config.UploaderTypeName;
            Settings.Default.Url = config.Url;
            Settings.Default.UserName = config.UserName;
            Settings.Default.SavePassword = config.SavePassword;

            // FIXME: insecure
            if (config.SavePassword)
            {
                Settings.Default.Password = ScreenToWikiUtil.EncodePasswordString(config.Password);
            }
            else
            {
                Settings.Default.Password = "";
            }

            Settings.Default.Save();
        }

        private void captureAndUploadScreenShot() {
            this.Topmost = false;
            this.Width = 1;
            this.Height = 1;

            var rect = getRectFromXY();
            var bitmap = ScreenCaptureUtil.CaptureScreenShot(rect);

            var uploadConfig = new UploadConfig();
            foreach (var name in imageUploaders.Select(x => x.GetType().Name))
            {
                uploadConfig.UploaderTypeNameList.Add(name);
            }
            
            loadConfig(uploadConfig);
            if (string.IsNullOrEmpty(uploadConfig.UploaderTypeName) ||
                !imageUploaders.Where(x => x.GetType().Name == uploadConfig.UploaderTypeName).Any())
            {
                uploadConfig.UploaderTypeName = typeof (MediaWikiUploader).Name;
            }

            var infoWindow = new UploadInfoWindow();
            infoWindow.DataContext = uploadConfig;
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
            
            if (!infoWindow.ClosedWithUploadButton)
            {
                Application.Current.Shutdown();
            }

            var uploader = imageUploaders.Where(x => x.GetType().Name == uploadConfig.UploaderTypeName).First();

            if (String.IsNullOrEmpty(uploadConfig.Filename))
            {
                uploadConfig.Filename = ScreenToWikiUtil.GetFileName(".png");
            }

            if (!uploadConfig.Filename.ToLower().EndsWith(".png"))
            {
                uploadConfig.Filename = uploadConfig.Filename + ".png";
            }

            Uri uri = null;
            try
            {
                ScreenToWikiUtil.UseTempDir((tempDir) =>
                {
                    string imagePath = System.IO.Path.Combine(tempDir, "temp.png");
                    ScreenToWikiUtil.SavePngFile(bitmap, imagePath);

                    uri = uploader.UploadImage(imagePath, uploadConfig);
                });
            }
            catch (UploadFailedException ex)
            {
                MessageBox.Show(this, "An error ocurred when uploading: " + ex.Message);
            } catch (Exception ex) {
                MessageBox.Show(this, "An error ocurred when uploading: " + ex.ToString());
            }
            saveConfig(uploadConfig);

            if (uri != null)
                System.Diagnostics.Process.Start(uri.ToString());
        }
    }
}
