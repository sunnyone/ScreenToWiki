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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenToWiki {
    public partial class UploadInfoWindow : Window {
        public UploadInfoWindow() {
            InitializeComponent();

            ClosedWithUploadButton = false;
        }

        public bool ClosedWithUploadButton { get; private set; }

        private void uploadInfoWindow_Loaded(object sender, RoutedEventArgs e) {
            UploadConfig config = (UploadConfig)DataContext;

            if (string.IsNullOrEmpty(config.Url))
            {
                this.textBoxUrl.Focus();
            } else if (string.IsNullOrEmpty(config.Password))
            {
                this.passwordBoxPassword.Focus();
            }
            // other case, Upload button by default.

            this.passwordBoxPassword.Password = config.Password;
        }

        // FIXME: dirty way
        private void passwordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UploadConfig config = (UploadConfig) DataContext;
            config.Password = passwordBoxPassword.Password;
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            this.ClosedWithUploadButton = true;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
