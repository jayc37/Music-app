using Levitate.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Levitate.Core
{
    public class Utils
    {
        /// <summary>
        /// Convert a Byte[] to a BitmapImage
        /// </summary>
        /// <param name="picture">Image:Byte[]</param>
        /// <returns></returns>
        public static BitmapImage ByteArray2BitmapImage(byte[] picture)
        {
            var coverArt = new BitmapImage();
            if (picture != null)
            {
                using (var ms = new MemoryStream(picture))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    coverArt.BeginInit();
                    coverArt.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    coverArt.CacheOption = BitmapCacheOption.OnLoad;
                    coverArt.StreamSource = ms;
                    coverArt.EndInit();
                }
            }

            return coverArt;
        }

        /// <summary>
        /// Convert a BitmapImage (from uri) to ByteArray
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static byte[] ConvertBitmapSourceToByteArray(Uri _uri)
        {
            var info = Application.GetResourceStream(_uri);
            using (var memoryStream = new MemoryStream())
            {
                info.Stream.CopyTo(memoryStream); info = null;
                byte[] _data = memoryStream.ToArray();
                return _data;
            }
        }

        /// <summary>
        /// Track processing with dialog
        /// </summary>
        /// <param name="filespaths">string array</param>
        public static async Task TrackProcess(MetroWindow window, string[] filespaths)
        {
            // Progress Dialog
            var processDialog = await window.ShowProgressAsync(
                window.FindResource("pleaseWait").ToString(), window.FindResource("trackProcessing").ToString());
            processDialog.SetIndeterminate();

            await Task.Run(() =>
            {
                // Parallel for loop, with maximun of degree is the number of processors
                Parallel.For(0, filespaths.Length, (int i) =>
                {
                    using (var dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
                    {
                        // Thread sleep
                        //Thread.Sleep(new Random().Next(50, 500));

                        // Only handle file with extension is M4A or MP3
                        string ext = System.IO.Path.GetExtension(filespaths[i]).ToUpperInvariant();
                        if (ext == ".M4A" || ext == ".MP3")
                        {
                            Writer.ImportTrack2Database(dc, Reader.GetBasicTrack(filespaths[i]));
                        }
                    }
                });

                // One way processing

                //for (int i = 0; i < filespaths.Length; i++)
                //{
                //    using (var dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
                //    {
                //        string ext = System.IO.Path.GetExtension(filespaths[i]).ToUpperInvariant();
                //        if (ext == ".M4A" || ext == ".MP3")
                //        {
                //            //Track _track = Reader.GetBasicTrack(filespaths[i]);
                //            Writer.ImportTrack2Database(dc, Reader.GetBasicTrack(filespaths[i]));
                //        }
                //    }
                //}
            });

            await processDialog.CloseAsync();
        }

        /// <summary>
        /// Track processing with dialog
        /// </summary>
        /// <param name="filespaths">List of string</param>
        public static async Task TrackProcess(MetroWindow window, List<string> filespaths)
        {
            // Call ProgressDialog
            var processDialog = await window.ShowProgressAsync(
                window.FindResource("pleaseWait").ToString(), window.FindResource("trackProcessing").ToString());
            processDialog.SetIndeterminate();

            await Task.Run(() =>
            {
                // Parallel for loop, with maximun of degree is the number of processors
                Parallel.For(0, filespaths.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                {
                    using (var dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
                    {
                        // Thread sleep
                        //Thread.Sleep(new Random().Next(50, 500));

                        // Only handle file with extension is M4A or MP3
                        string ext = System.IO.Path.GetExtension(filespaths[i]).ToUpperInvariant();
                        if (ext == ".M4A" || ext == ".MP3")
                        {
                            Writer.ImportTrack2Database(dc, Reader.GetBasicTrack(filespaths[i]));
                        }
                    }
                });

                // One way processing

                //for (int i = 0; i < filespaths.Count; i++)
                //{
                //    using (var dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
                //    {
                //        string ext = System.IO.Path.GetExtension(filespaths[i]).ToUpperInvariant();
                //        if (ext == ".M4A" || ext == ".MP3")
                //        {
                //            //Track _track = Reader.GetBasicTrack(filespaths[i]);
                //            Writer.ImportTrack2Database(dc, Reader.GetBasicTrack(filespaths[i]));
                //        }
                //    }
                //}
            });

            await processDialog.CloseAsync();
        }

        /// <summary>
        /// Scan files in folder and it's subfolder use recursion
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> ScanFolder(string path)
        {
            List<string> _files = new List<string>();

            // Collect files from a path
            _files.AddRange(Directory.GetFiles(path));

            // Get subDirectories
            string[] subdirectoryEntries = Directory.GetDirectories(path);
            // Recurse to get files/subDirectories
            foreach (string subdirectory in subdirectoryEntries)
                _files.AddRange(ScanFolder(subdirectory));

            return _files;
        }

        /// <summary>
        /// Auto generating ID
        /// </summary>
        /// <param name="kind">Group kind</param>
        /// <returns></returns>
        public static string GenerateID(string kind)
        {
            StringBuilder sBuilder = new StringBuilder();
            switch (kind)
            {
                case "TUA":
                    sBuilder.Append("tit-");
                    sBuilder.Append(Guid.NewGuid());
                    break;
                case "CASI":
                    sBuilder.Append("art-");
                    sBuilder.Append(Guid.NewGuid());
                    break;
                case "CASIALBUM":
                    sBuilder.Append("aab-");
                    sBuilder.Append(Guid.NewGuid());
                    break;
                case "ALBUM":
                    sBuilder.Append("abm-");
                    sBuilder.Append(Guid.NewGuid());
                    break;
                case "THELOAI":
                    sBuilder.Append("ger-");
                    sBuilder.Append(Guid.NewGuid());
                    break;
                case "BAIHAT":
                    sBuilder.Append("sog-");
                    sBuilder.Append(Guid.NewGuid());
                    break;
                default:
                    break;
            }
            return sBuilder.ToString();
        }


        /// <summary>
        /// Delete unexist track
        /// </summary>
        /// <param name="_window"></param>
        /// <param name="_track"></param>
        public static async void DeleteUnTrack(MetroWindow _window, Track _track)
        {
            // Show comfirm message dialog
            MessageDialogResult result = await _window.ShowMessageAsync
                    (_window.FindResource("attention").ToString(), _window.FindResource("trackDoesNotExist.Deletet?").ToString(), MessageDialogStyle.AffirmativeAndNegative, null);

            // If return affirmative then clean db
            if (result == MessageDialogResult.Affirmative)
            {
                Updater.DeleteOneTrack(_track);
            }

        }
    }
}
