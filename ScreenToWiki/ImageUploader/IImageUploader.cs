﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ScreenToWiki.ImageUploader
{
    public interface IImageUploader
    {
        Uri UploadImage(String imagePath, UploadConfig config);
    }
}
