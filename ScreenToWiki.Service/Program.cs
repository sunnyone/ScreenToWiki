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
using System.ServiceProcess;
using System.Text;
using System.Reflection;

namespace ScreenToWiki.Service
{
    static class Program
    {
        // TODO: create self-installer
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2) // program only
            {
                string path = Assembly.GetExecutingAssembly().Location;
                string filename = System.IO.Path.GetFileName(path);
                System.Console.WriteLine("usage: ");
                System.Console.WriteLine(string.Format("  {0} print FolderPath UploaderName Url Username Password", filename));

                return;
            }

            string subcommand = args[1];
            string[] subargs = args.Skip(2).ToArray();

            switch (subcommand)
            {
                case "run":
                    run(subargs);
                    break;
                case "print":
                    print(subargs);
                    break;
                default:
                    System.Console.WriteLine("Unknown subcommand: " + subcommand);
                    break;
            }
        }

        private static void run(string[] subargs)
        {
            if (subargs.Length != 5)
            {
                System.Console.WriteLine("Invalid argument count: " + subargs.Length);
                return;
            }

            string folderPath = subargs[0];
            string uploaderName = subargs[1];
            string url = subargs[2];
            string username = subargs[3];
            string password = subargs[4];

            var service = new ScreenToWikiService();
            service.FolderPath = folderPath;
            service.UploaderName = uploaderName;
            service.Url = url;
            service.Username = username;
            service.Password = ScreenToWikiUtil.DecodePasswordString(password);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ service };
            ServiceBase.Run(ServicesToRun);
        }

        private static void print(string[] subargs)
        {
            if (subargs.Length != 5)
            {
                System.Console.WriteLine("Invalid argument count: " + subargs.Length);
                return;
            }

            string folderPath = subargs[0];
            string uploaderName = subargs[1];
            string url = subargs[2];
            string username = subargs[3];
            string password = subargs[4];

            password = ScreenToWikiUtil.EncodePasswordString(password);

            string location = Assembly.GetExecutingAssembly().Location;

            // FIXME: if arg includes '"', this doesn't work.
            string[] serviceArgs = new string[] { location, "run", folderPath, uploaderName, url, username, password };
            string serviceArgsQuoted = string.Join(" ", serviceArgs.Select(x => "\\\"" + x + "\\\""));

            string installCommand = string.Format("sc create ScreenToWiki displayname= \"ScreenToWiki\" binpath= \"{0}\" depend= Tcpip start= auto", serviceArgsQuoted);
            string uninstallCommand = string.Format("sc delete ScreenToWiki");
            System.Console.WriteLine("To install: \n" + installCommand);
            System.Console.WriteLine("To uninstall: \n" + uninstallCommand);
        }
    }
}
