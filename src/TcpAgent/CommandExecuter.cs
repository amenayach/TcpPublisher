namespace TcpAgent
{
    using Ionic.Zip;
    using Ionic.Zlib;
    using Microsoft.Web.Administration;
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    public class CommandExecuter
    {
        public string RestartApp(string appname)
        {
            StopApp(appname, appname);
            System.Threading.Thread.Sleep(2000);
            StartApp(appname, appname);
            return "done";
        }

        public string StopApp(string appname)
        {
            return StopApp(appname, appname);
        }

        public string StopApp(string appname, string appPoolname)
        {
            using (var server = new ServerManager())
            {
                var site = server.Sites.FirstOrDefault(s => string.Equals(s.Name, appname, StringComparison.InvariantCultureIgnoreCase));
                if (site != null)
                {
                    if (site.State != ObjectState.Stopped)
                    {
                        site.Stop();
                    }

                    var appPool = server.ApplicationPools.FirstOrDefault(s => string.Equals(s.Name, appPoolname, StringComparison.InvariantCultureIgnoreCase));

                    if (appPool != null)
                    {
                        if (appPool.State != ObjectState.Stopped)
                        {
                            appPool.Stop();
                        }

                        return "done";
                    }
                }
            }

            return string.Empty;
        }

        public string StartApp(string appname)
        {
            return StartApp(appname, appname);
        }

        public string StartApp(string appname, string appPoolname)
        {
            using (var server = new ServerManager())
            {
                var site = server.Sites.FirstOrDefault(s => s.Name == appname);
                if (site != null)
                {
                    if (site.State != ObjectState.Started)
                    {
                        site.Start();
                    }

                    var appPool = server.ApplicationPools.FirstOrDefault(s => s.Name == appPoolname);

                    if (appPool != null)
                    {
                        if (appPool.State != ObjectState.Started)
                        {
                            appPool.Start();
                        }

                        return "done";
                    }
                }
            }

            return string.Empty;
        }

        public string Copy(string source, string destination)
        {
            if (Directory.Exists(source))
            {
                CopyFolder(source, destination);

                return "Folder copied";
            }
            else if (File.Exists(source))
            {
                File.Copy(source, $@"{destination}\{new FileInfo(source).Name}");

                return "File copied";
            }

            return string.Empty;
        }

        public string Zip(string source, string destination)
        {
            using (ZipFile zip = new ZipFile
            {
                CompressionLevel = CompressionLevel.BestCompression
            })
            {
                var files = Directory.GetFiles(source, "*",
                    SearchOption.AllDirectories).
                    Where(f => Path.GetExtension(f).
                        ToLowerInvariant() != ".zip").ToArray();

                foreach (var f in files)
                {
                    zip.AddFile(f, GetCleanFolderName(source, f));
                }

                var destinationFilename = destination;

                if (Directory.Exists(destination) && !destination.EndsWith(".zip"))
                {
                    destinationFilename += $"\\{new DirectoryInfo(source).Name}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffffff}.zip";
                }

                zip.Save(destinationFilename);

                return "done";
            }
        }

        /// <summary>
        /// Exec powershell
        /// </summary>
        public string Ps(string source)
        {
            if (!File.Exists(source))
            {
                return string.Empty;
            }

            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();

            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);

            Pipeline pipeline = runspace.CreatePipeline();

            Command command = new Command(File.ReadAllText(source));

            pipeline.Commands.Add(command);

            var results = pipeline.Invoke();

            foreach (var result in results)
            {
                Console.WriteLine(result);
            }

            return string.Empty;
        }

        public string Start(string filepath)
        {
            try
            {
                //No check for file existance because some files can be run from system path like cmd, calc, notepad...
                System.Diagnostics.Process.Start(filepath);
            }
            catch
            {
                //Ignored
            }

            return string.Empty;
        }

        public string ClearFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var di = new DirectoryInfo(folderPath);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }

            return string.Empty;
        }

        public string DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            return string.Empty;
        }

        public string Ping()
        {
            return "Pong";
        }

        private string GetCleanFolderName(string source, string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                return string.Empty;
            }

            var result = filepath.Substring(source.Length);

            if (result.StartsWith("\\"))
            {
                result = result.Substring(1);
            }

            result = result.Substring(0, result.Length - new FileInfo(filepath).Name.Length);

            return result;
        }

        private void CopyFolder(string source, string destination)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }
    }
}
