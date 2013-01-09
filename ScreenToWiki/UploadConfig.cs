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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ScreenToWiki {
    public class UploadConfig : ViewModelBase
    {
        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; OnPropertyChanged("Url"); }
        }

        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; OnPropertyChanged("UserName"); }
        }

        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; OnPropertyChanged("Password"); }
        }

        private string _uploaderTypeName;

        public string UploaderTypeName
        {
            get { return _uploaderTypeName; }
            set { _uploaderTypeName = value; OnPropertyChanged("UploaderTypeName"); }
        }

        private ObservableCollection<string> _uploaderTypeNameList = new ObservableCollection<string>();
        public ObservableCollection<string> UploaderTypeNameList
        {
            get { return _uploaderTypeNameList; }
            set { _uploaderTypeNameList = value; OnPropertyChanged("UploaderTypeNameList"); }
        }

        private bool savePassword;

        public bool SavePassword
        {
            get { return savePassword; }
            set { savePassword = value; OnPropertyChanged("SavePassword"); }
        }

        private string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; OnPropertyChanged("Filename"); }
        }

    }
}
