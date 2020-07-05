using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Levitate.Model
{
    [Serializable, XmlType(TypeName ="Playlists")]
    public class PlaylistsSetting : IDisposable
    {
        protected const string Filename = "Levitate_Playlists.xml";
        [XmlElement("List")]
        public ObservableCollection<Playlist> Playlists { get; set; }

        public PlaylistsSetting()
        {
            Playlists = new ObservableCollection<Playlist>();
        }

        /// <summary>
        /// Save all playlists to xml type file
        /// </summary>
        /// <param name="_playlists"></param>
        /// <param name="rootPath"></param>
        public void SavePlaylist()
        {
            var playlistsFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Filename));

            // If existed an old playlistsFile before => delete
            if (playlistsFile.Exists)
                playlistsFile.Delete();

            // Create a new one
            using (var tempFile = new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Filename), FileMode.Create, FileAccess.Write))
            {
                // Write all playlists content to temporary file as xml type file
                var serializer = new XmlSerializer(typeof(ObservableCollection<Playlist>));
                serializer.Serialize(tempFile, Playlists);
            }
        }


        /// <summary>
        /// Load playlists
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static ObservableCollection<Playlist> LoadPlaylists()
        {
            ObservableCollection<Playlist> playlists = new ObservableCollection<Playlist>();

            // create a file info
            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Filename));

            // check that file info is exist or not, has content or empty
            if (!file.Exists || string.IsNullOrWhiteSpace(File.ReadAllText(file.FullName)))
            {
                return playlists;
            }
            // read that file info, deserialize it and return as PlaylistSettings
            using (FileStream fs = new FileStream(file.FullName, FileMode.Open))
            {
                var deserializer = new XmlSerializer(typeof(ObservableCollection<Playlist>));
                playlists = (ObservableCollection<Playlist>)deserializer.Deserialize(fs);
            }

            for (var i = 0; i < playlists.Count; i++)
            {
                playlists[i] = Playlist.LoadPlaylist(playlists[i]);
            }

            return playlists;
        }

        public static void DeletePlaylistsFile()
        {
            var playlistsFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Filename));

            // If existed an old playlistsFile before => delete
            if (playlistsFile.Exists)
                playlistsFile.Delete();
        }

        #region Disposable Implement
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    Playlists = null;
                }
                _disposed = true;
            }
        }

        ~PlaylistsSetting()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion
    }
}
