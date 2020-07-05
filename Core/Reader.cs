using Levitate.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Levitate.Core
{
    public class Reader : IDisposable
    {
        #region Fields/AutoImplemented Declaration
        private static MusicDBDataContext dc { get; set; }
        private const string UNKNOW = "Unknow";
        private static TagLib.File f { get; set; }
        #endregion


        #region Get a track with basic Metadata
        /// <summary>
        /// Get a track with basic information using location to read directly on file
        /// </summary>
        /// <param name="_location"></param>
        /// <returns></returns>
        public static Track GetBasicTrack(string _location)
        {
            if (string.IsNullOrWhiteSpace(_location) || !System.IO.File.Exists(_location))
                return null;

            try
            {
                Track _track = new Track();
                _track.Location = _location;

                using (f = TagLib.File.Create(_track.Location))
                {
                    _track.Title = string.IsNullOrWhiteSpace(f.Tag.Title) ? Path.GetFileNameWithoutExtension(_track.Location) : f.Tag.Title.Replace("\0", "");
                    _track.Artist = string.IsNullOrWhiteSpace(f.Tag.FirstPerformer) ? UNKNOW : f.Tag.FirstPerformer.Replace("\0", "");
                    _track.Album = string.IsNullOrWhiteSpace(f.Tag.Album) ? UNKNOW : f.Tag.Album.Replace("\0", "");
                    _track.AlbumArtist = string.IsNullOrWhiteSpace(f.Tag.FirstAlbumArtist) ? UNKNOW : f.Tag.FirstAlbumArtist.Replace("\0", "");
                    _track.Genre = string.IsNullOrWhiteSpace(f.Tag.FirstGenre) ? UNKNOW : f.Tag.FirstGenre.Replace("\0", "");
                    _track.Duration = f.Properties.Duration;
                    _track.IsFavourite = false;
                    _track.HasCoverArt = (f.Tag.Pictures != null && f.Tag.Pictures.Any()) ? true : false;
                    _track.Year = string.IsNullOrWhiteSpace(f.Tag.Year.ToString()) ? null : (uint?)f.Tag.Year;
                    _track.PlayCount = 0;
                    _track.DateAdded = DateTime.Now;
                    //_track.LastPlayed   = null;
                }

                return _track;
            }
            catch (Exception)
            {
                return null;
            }


        }
        /// <summary>
        /// Get a track with basic information using ID to looking in database
        /// </summary>
        /// <param name="_ID"></param>
        /// <returns></returns>
        public static Track GetBasicTrackFromID(string _ID)
        {
            Track _track = new Track();
            _track.ID = _ID;

            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                // Looking in database
                var query = (from bh in dc.BAIHATs
                             join tua in dc.TUAs on bh.Ma_Tua equals tua.Ma_Tua
                             join casi in dc.CASIs on bh.Ma_CaSi equals casi.Ma_CaSi
                             join album in dc.ALBUMs on bh.Ma_Album equals album.Ma_Album
                             join csAlbum in dc.CASIs on bh.Ma_CaSiAlbum equals csAlbum.Ma_CaSi
                             join theloai in dc.THELOAIs on bh.Ma_TheLoai equals theloai.Ma_TheLoai
                             join chitiet in dc.CHITIETBAIHATs on bh.Ma_BaiHat equals chitiet.Ma_BaiHat
                             where bh.Ma_BaiHat == _ID
                             select new
                             {
                                 DuongDan = bh.DuongDan,
                                 Tua = tua.Ten_Tua,
                                 CaSi = casi.Ten_CaSi,
                                 Album = album.Ten_Album,
                                 CasiAlbum = csAlbum.Ten_CaSi,
                                 TheLoai = theloai.Ten_TheLoai,
                                 ThLuong = chitiet.ThoiLuong,
                                 Rating = chitiet.Rating,
                                 CoAnhBia = chitiet.CoAnhBia,
                                 Year = chitiet.NamPhatHanh,
                                 PlayCount = chitiet.SoLanNghe,
                                 NgayThem = chitiet.NgayThemVaoCSDL,
                                 LanNgheCuoi = chitiet.LanNgheCuoi
                             }).First();

                // Getting information
                _track.Location = query.DuongDan;
                _track.Title = query.Tua;
                _track.Artist = query.CaSi;
                _track.Album = query.Album;
                _track.AlbumArtist = query.CasiAlbum;
                _track.Genre = query.TheLoai;
                _track.Duration = query.ThLuong;
                _track.IsFavourite = query.Rating;
                _track.HasCoverArt = query.CoAnhBia;
                if (!string.IsNullOrWhiteSpace(query.Year))
                    _track.Year = uint.Parse(query.Year);
                _track.PlayCount = query.PlayCount;
                _track.DateAdded = query.NgayThem;
                _track.LastPlayed = query.LanNgheCuoi;
            }

            return _track;
        }
        #endregion


        #region Fill full metadata to Track
        /// <summary>
        /// Get other details of a track by reading directly on file
        /// </summary>
        /// <param name="_location"></param>
        /// <returns></returns>
        public static void GetDetailsOfTrack(ref Track _track)
        {
            if (!_track.IsExist)
                return;

            using (f = TagLib.File.Create(_track.Location))
            {

                // Album artist
                _track.AlbumArtist = string.IsNullOrWhiteSpace(f.Tag.FirstAlbumArtist) ? UNKNOW : f.Tag.FirstAlbumArtist;

                // Composer
                _track.Composer = string.IsNullOrWhiteSpace(f.Tag.FirstComposer) ? UNKNOW : f.Tag.FirstComposer;

                // Cover Art
                if (_track.HasCoverArt)
                {
                    _track.CoverArt = f.Tag.Pictures[0].Data.Data;
                }
                else
                {
                    _track.CoverArt = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/emptyImage.png"));
                }

                // File kind
                _track.Kind = GetFormatKind(Path.GetExtension(_track.Location).ToUpperInvariant());

                // Track info
                var trackNo = f.Tag.Track;
                var trackCount = f.Tag.TrackCount;
                // Using C# 6.0 here!
                _track.TrackInfo = (trackCount > 0) ? string.Format($"{trackNo}/{trackCount}") : string.Empty;

                // Lyrics
                _track.Lyrics = (f.Tag.Lyrics != null) ? f.Tag.Lyrics : null;

                // Is Variable Bit Rate
                if (_track.Kind == "MPEG-3")
                {
                    var codec = f.Properties.Codecs.FirstOrDefault(c => c is TagLib.Mpeg.AudioHeader);
                    _track.IsVBR = codec != null && (((TagLib.Mpeg.AudioHeader)codec).VBRIHeader.Present || ((TagLib.Mpeg.AudioHeader)codec).XingHeader.Present);
                }
                else
                {
                    _track.IsVBR = false;
                }

                // Bit rate
                if (_track.IsVBR)
                {
                    var fileSizeinBit = new FileInfo(_track.Location).Length * 8;
                    var seconds = f.Properties.Duration.TotalSeconds;

                    _track.BitRate = ((int)(fileSizeinBit / seconds)).ToString().Remove(3) + " kbps (VBR)";
                }
                else
                {
                    _track.BitRate = f.Properties.AudioBitrate.ToString() + " kbps";
                }

                // Sample Rate
                _track.SampleRate = f.Properties.AudioSampleRate.ToString() + " Hz";

                // Channels
                _track.Channels = GetChannels(f.Properties.AudioChannels);

                // Copy Right
                _track.CopyRight = f.Tag.Copyright;
            }

            using(var dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                var temp = _track;
                var track = dc.CHITIETBAIHATs.FirstOrDefault(item => item.Ma_BaiHat == temp.ID);

                _track.PlayCount = track.SoLanNghe;
                _track.LastPlayed = track.LanNgheCuoi;

                temp = null; track = null;
            }
        }
        #endregion


        #region Need for reading metadata of a file
        /// <summary>
        /// Get format name of file
        /// </summary>
        /// <param name="upperExtension"></param>
        /// <returns></returns>
        private static string GetFormatKind(string upperedExtension)
        {
            switch (upperedExtension)
            {
                case ".MP3":
                    return "MPEG Layer-3 Audio (MP3)";
                case ".M4A":
                    return "MPEG-4 Audio with Advanced Audio Coding (AAC)";
                default:
                    return UNKNOW;
            }
        }
        /// <summary>
        /// Get channel-mode in string
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        private static string GetChannels(int channels)
        {
            switch (channels)
            {
                case 1:
                    return "Mono";
                case 2:
                    return "Stereo";
                default:
                    return "Unknow";
            }
        }
        /// <summary>
        /// Get only Cover Art
        /// </summary>
        /// <param name="_track"></param>
        /// <returns></returns>
        public static byte[] GetCoverArt(Track _track)
        {
            if (!_track.HasCoverArt)
                return Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/emptyImage.png"));

            using (var f = TagLib.File.Create(_track.Location))
            {
                byte[] _image = null;

                _image = f.Tag.Pictures[0].Data.Data;

                return _image;
            }
        }
        #endregion


        #region Getting an application playlist
        /// <summary>
        /// Get all songs in database
        /// </summary>
        /// <returns></returns>
        public static Playlist GetSongs()   
        {
            var _songs = new Playlist();
            _songs.Name = Application.Current.Resources["songs"].ToString();
            _songs.Prio = Playlist.Priority.Application;
            _songs.Icon = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/listbox_songs.png"));
            _songs.CanEdit = true;

            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                var tmp = new ObservableCollection<Track>();

                // Get all IDs in table BaiHat, foreach ID get basic track, and collect all of them
                (from baihat in dc.BAIHATs
                 select baihat.Ma_BaiHat).ToList().ForEach(maBaiHat => tmp.Add(GetBasicTrackFromID(maBaiHat)));

                // Sorting collection
                _songs.ListTrack = new ObservableCollection<Track>(tmp.OrderBy(track => track.Title));
                tmp = null;
            }

            return _songs;
        }


        /// <summary>
        /// Get 50 recently added tracks from given _playlist
        /// </summary>
        /// <returns></returns>
        public static Playlist GetRecentlyAdded(Playlist _playlist)
        {
            Playlist _recentlyAdded = new Playlist();
            _recentlyAdded.Name = Application.Current.Resources["recentlyAdded"].ToString();
            _recentlyAdded.Prio = Playlist.Priority.Application;
            _recentlyAdded.Icon = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/listbox_recentlyAdded.png"));
            _recentlyAdded.CanEdit = false;

            // Use LINQ to get 50 most recent tracks from a given _playlist, and collect them
            (from track in _playlist.ListTrack
             orderby track.DateAdded descending
             select track).Take(50).ToList().ForEach(track => _recentlyAdded.ListTrack.Add(track));

            return _recentlyAdded;
        }


        /// <summary>
        /// Get 50 recently added tracks from library
        /// </summary>
        /// <param name="_playlist"></param>
        /// <returns></returns>
        public static Playlist GetRecentlyAdded()
        {
            Playlist _recentlyAdded = new Playlist();
            _recentlyAdded.Name = Application.Current.Resources["recentlyAdded"].ToString();
            _recentlyAdded.Prio = Playlist.Priority.Application;
            _recentlyAdded.Icon = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/listbox_recentlyAdded.png"));
            _recentlyAdded.CanEdit = false;

            // Use LINQ to get 50 most recent tracks from library, and collect them
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                var query = (from chitiet in dc.CHITIETBAIHATs
                             orderby chitiet.NgayThemVaoCSDL descending
                             select chitiet.Ma_BaiHat).Take(50);
                query.ToList().ForEach(maBaiHat => _recentlyAdded.ListTrack.Add(GetBasicTrack(maBaiHat)));
            }
            return _recentlyAdded;
        }


        /// <summary>
        /// Get favourite tracks from given _playlist
        /// </summary>
        /// <returns></returns>
        public static Playlist GetFavourites(Playlist _playlist)
        {
            Playlist _favourites = new Playlist();
            _favourites.Name = Application.Current.Resources["favourite"].ToString();
            _favourites.Prio = Playlist.Priority.Application;
            _favourites.Icon = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/listbox_favourite.png"));
            _favourites.CanEdit = false;

            // Use LINQ to get favourite tracks from a given _playlist, and collect them
            (from track in _playlist.ListTrack
             where track.IsFavourite == true
             select track).ToList().ForEach(track => _favourites.ListTrack.Add(track));

            return _favourites;
        }


        /// <summary>
        /// Get favourite tracks from given _playlist
        /// </summary>
        /// <returns></returns>
        public static Playlist GetFavourites()
        {
            Playlist _favourites = new Playlist();
            _favourites.Name = Application.Current.Resources["favourite"].ToString();
            _favourites.Prio = Playlist.Priority.Application;
            _favourites.Icon = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/listbox_favourite.png"));
            _favourites.CanEdit = false;

            // Use LINQ to get favourite tracks from a given _playlist, and collect them
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                var query = (from chitiet in dc.CHITIETBAIHATs
                             where chitiet.Rating == true
                             select chitiet.Ma_BaiHat);
                query.ToList().ForEach(maBaiHat => _favourites.ListTrack.Add(GetBasicTrack(maBaiHat)));
            }

            return _favourites;
        }
        #endregion


        


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
                    dc.Dispose();
                    f.Dispose();
                }
                _disposed = true;
            }
        }

        ~Reader()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion
    }
}
