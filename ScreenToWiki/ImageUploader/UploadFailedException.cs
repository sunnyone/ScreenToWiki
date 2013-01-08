using System;

namespace ScreenToWiki.ImageUploader
{
    public class UploadFailedException : Exception
    {
        public UploadFailedException()
        {
            
        }

        public UploadFailedException(string message) : base(message)
        {
            
        }
    }
}