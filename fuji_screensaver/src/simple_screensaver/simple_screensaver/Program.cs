using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace simple_screensaver
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            InstallScreenSaver();

            var app = Path.GetFileName(Application.ExecutablePath);
            InternetFeatureControl("FEATURE_BROWSER_EMULATION", app, Properties.Settings.Default.ie_version);
            InternetFeatureControl("FEATURE_GPU_RENDERING", app, 1);
            InternetFeatureControl("FEATURE_SSLUX", app, 1);

            if (Properties.Resources.screensaver != null && Properties.Resources.screensaver.Length > 0)
            {
                var name = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                var workDir = Path.Combine(Path.GetTempPath(), name);
                if (Directory.Exists(workDir))
                    Directory.Delete(workDir, true);
                Directory.CreateDirectory(workDir);
                using (var stream = new MemoryStream(Properties.Resources.screensaver))
                {
                    var zip = new ZipFile(stream);
                    foreach (ZipEntry e in zip)
                    {
                        if (!e.IsFile)
                            continue;

                        String entryFileName = e.Name;
                        // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        byte[] buffer = new byte[4096];     // 4K is optimum
                        Stream zipStream = zip.GetInputStream(e);

                        // Manipulate the output filename here as desired.
                        String fullZipToPath = Path.Combine(workDir, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
                Settings.WorkDir = workDir;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        
        private static void InstallScreenSaver()
        {
            var path = @"Control Panel\Desktop";
            var key = Registry.CurrentUser.CreateSubKey(path);
            key.SetValue("ScreenSaveActive", "1");
            key.SetValue("ScreenSaveTimeOut", Properties.Settings.Default.timeout);
            key.SetValue("SCRNSAVE.EXE", Application.ExecutablePath);
        }

        private static void InternetFeatureControl(string feature, string app, object value)
        {
            var path = $@"Software\Microsoft\Internet Explorer\Main\FeatureControl\{feature}";
            var key = Registry.CurrentUser.CreateSubKey(path);
            var v = key.GetValue(app);
            if (v == null || !Equals(v, value))
            {
                key.SetValue(app, value);
            }
        }
    }
}
