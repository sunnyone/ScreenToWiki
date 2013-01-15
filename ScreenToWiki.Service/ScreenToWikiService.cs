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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Timers;
using ScreenToWiki.ImageUploader;
using System.Reflection;
using System.ComponentModel.Composition.Hosting;

namespace ScreenToWiki.Service
{
    public partial class ScreenToWikiService : ServiceBase
    {
        public string UploaderName { get; set; }
        public string FolderPath { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ScreenToWikiService()
        {
            InitializeComponent();
        }

        const string EventLogSourceName = "ScreenToWikiService";
        private void writeEventLog(System.Diagnostics.EventLogEntryType entryType, string message)
        {
            if (!System.Diagnostics.EventLog.SourceExists(EventLogSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(EventLogSourceName, "Application");
            }

            System.Diagnostics.EventLog.WriteEntry(EventLogSourceName, message);
        }


        private const int FileSizeTimeoutSec = 30;

        private FileSystemWatcher folderWatcher = new FileSystemWatcher();
        private IImageUploader uploader = null;

        private void startWatching()
        {
            var imageUploaders = ScreenToWikiUtil.GetImageUploaders();
            var q = imageUploaders.Where(x => x.GetType().Name == UploaderName);

            if (!q.Any())
            {
                throw new ArgumentException("Invalid uploader name: " + UploaderName);
            }
            uploader = q.First();
            
            folderWatcher.Path = FolderPath;
            folderWatcher.Filter = "*";
            folderWatcher.IncludeSubdirectories = false;
            folderWatcher.NotifyFilter = NotifyFilters.FileName;
            folderWatcher.Created += new FileSystemEventHandler((o1, e1) =>
            {
                fileDetected(e1.FullPath);
            });

            folderWatcher.Renamed += new RenamedEventHandler((o1, e1) =>
            {
                fileDetected(e1.FullPath);
            });

            folderWatcher.EnableRaisingEvents = true;

            writeEventLog(EventLogEntryType.Information, "Start watching: " + FolderPath);
        }
        
        // TODO: too complex elapsed handler, needs refactoring.
        private void fileDetected(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLower();
            if (extension != ".jpg" && extension != ".png")
            {
                return;
            }

            int elapsedTime = -1;
            long prevSize = 0;

            Timer fileSizeCheckTimer = new Timer(1000);
            fileSizeCheckTimer.Elapsed += new ElapsedEventHandler((o1, e1) =>
            {
                elapsedTime++;


                bool disposeTimer = false;

                System.IO.FileInfo fileInfo = null;
                try
                {
                    fileInfo = new System.IO.FileInfo(path);
                }
                catch (Exception ex)
                {
                    fileSizeCheckTimer.Stop();
                    disposeTimer = true;

                    writeEventLog(EventLogEntryType.Error, string.Format("Failed to get the file info ({0}): {1}", path, ex.ToString()));
                }

                if (fileInfo != null)
                {
                    if (prevSize != 0 || fileInfo.Length == prevSize || elapsedTime > FileSizeTimeoutSec)
                    {
                        fileSizeCheckTimer.Stop();
                        disposeTimer = true;

                        try
                        {
                            startUpload(path);
                        }
                        catch (Exception ex)
                        {
                            writeEventLog(EventLogEntryType.Error, string.Format("Failed to upload an image({0}): {1}", path, ex.ToString()));
                        }
                    }

                    prevSize = fileInfo.Length;
                }


                if (disposeTimer)
                {
                    fileSizeCheckTimer.Dispose();
                }
            });

            fileSizeCheckTimer.Start();
        }

        private void startUpload(string path)
        {
            UploadConfig config = new UploadConfig();
            config.Url = this.Url;
            config.UserName = this.Username;
            config.Password = this.Password;
            config.Filename = System.IO.Path.GetFileName(path);

            uploader.UploadImage(path, config);
            writeEventLog(EventLogEntryType.Information, "Succeeded to upload an image: " + path);

            // FIXME: when error...?
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        private void stopWatching()
        {
            folderWatcher.EnableRaisingEvents = false;
            folderWatcher.Dispose();
        }

        protected override void OnStart(string[] args)
        {
            startWatching();
        }

        protected override void OnStop()
        {
            stopWatching();
        }
    }
}
