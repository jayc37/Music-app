using Levitate.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Levitate.Core
{
    /// <summary>
    /// Contains some methods to update Information to DB and File
    /// </summary>
    public class Updater : IDisposable
    {
        // Auto_implemented Property Declaration
        public static MusicDBDataContext dc { get; set; }


        // UPDATER


        /// <summary>
        /// Update Details to DB
        /// </summary>
        /// <param name="_track"></param>
        public static void UpdateDetails2DB(Track _track)
        {
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                // Update Title
                UpdateTitle(dc, _track);
                // Update Artist
                UpdateArtist(dc, _track);
                // Update Album
                UpdateAlbum(dc, _track);
                // Update AlbumArtist
                UpdateAlbumArtist(dc, _track);
                // Update Genre
                UpdateGenre(dc, _track);
                // Update Year
                UpdateYear(dc, _track);
                // Update Favourite
                UpdateFavourite(dc, _track);
            }
        }

        #region Need to updating details to DB
        /// <summary>
        /// Update Title from Track (view) to DB
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="_track"></param>
        public static void UpdateTitle(MusicDBDataContext dc, Track _track)
        {
            if (dc.TUAs.Any(tua => tua.Ten_Tua == _track.Title))
            {
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_Tua = dc.TUAs.Single(tua => tua.Ten_Tua == _track.Title).Ma_Tua;
            }
            else
            {
                Writer.Write2_TUA(_track, dc);
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_Tua = dc.TUAs.Single(tua => tua.Ten_Tua == _track.Title).Ma_Tua;
            }
            dc.SubmitChanges();
        }
        /// <summary>
        /// Update Artist from Track (view) to DB
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="_track"></param>
        public static void UpdateArtist(MusicDBDataContext dc, Track _track)
        {
            if (dc.CASIs.Any(casi => casi.Ten_CaSi == _track.Artist))
            {
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_CaSi = dc.CASIs.Single(casi => casi.Ten_CaSi == _track.Artist).Ma_CaSi;
            }
            else
            {
                Writer.Write2_CASI1(_track, dc);
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_CaSi = dc.CASIs.Single(casi => casi.Ten_CaSi == _track.Artist).Ma_CaSi;
            }
            dc.SubmitChanges();
        }
        /// <summary>
        /// Update Album from Track (view) to DB
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="_track"></param>
        public static void UpdateAlbum(MusicDBDataContext dc, Track _track)
        {
            if (dc.ALBUMs.Any(album => album.Ten_Album == _track.Album))
            {
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_Album = dc.ALBUMs.Single(album => album.Ten_Album == _track.Album).Ma_Album;
            }
            else
            {
                Writer.Write2_ALBUM(_track, dc);
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_Album = dc.ALBUMs.Single(album => album.Ten_Album == _track.Album).Ma_Album;
            }
            dc.SubmitChanges();
        }
        /// <summary>
        /// Update AlbumArtist from Track (view) to DB
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="_track"></param>
        public static void UpdateAlbumArtist(MusicDBDataContext dc, Track _track)
        {
            if (dc.CASIs.Any(csAlbum => csAlbum.Ten_CaSi == _track.AlbumArtist))
            {
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_CaSiAlbum = dc.CASIs.Single(csalbum => csalbum.Ten_CaSi == _track.AlbumArtist).Ma_CaSi;
            }
            else
            {
                Writer.Write2_CASI2(_track, dc);
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_CaSiAlbum = dc.CASIs.Single(csalbum => csalbum.Ten_CaSi == _track.AlbumArtist).Ma_CaSi;
            }
            dc.SubmitChanges();
        }
        /// <summary>
        /// Update Genre from Track (view) to DB
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="_track"></param>
        public static void UpdateGenre(MusicDBDataContext dc, Track _track)
        {
            if (dc.THELOAIs.Any(tl => tl.Ten_TheLoai == _track.Genre))
            {
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_TheLoai = dc.THELOAIs.Single(tl => tl.Ten_TheLoai == _track.Genre).Ma_TheLoai;
            }
            else
            {
                Writer.Write2_THELOAI(_track, dc);
                dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID).Ma_TheLoai = dc.THELOAIs.Single(tl => tl.Ten_TheLoai == _track.Genre).Ma_TheLoai;
            }
            dc.SubmitChanges();
        }
        /// <summary>
        /// Update Year from track (view) to DB
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="_track"></param>
        public static void UpdateYear(MusicDBDataContext dc, Track _track)
        {
            dc.CHITIETBAIHATs.Single(ctiet => ctiet.Ma_BaiHat == _track.ID).NamPhatHanh = _track.Year.Value.ToString();
            dc.SubmitChanges();
        }
        /// <summary>
        /// Update Favourite
        /// </summary>
        /// <param name="_ID">Song_ID:int</param>
        /// <param name="_isFavourite">Favourite:Boolean</param>
        public static void UpdateFavourite(MusicDBDataContext dc, Track _track)
        {
            dc.CHITIETBAIHATs.Single(ctiet => ctiet.Ma_BaiHat == _track.ID).Rating = _track.IsFavourite;
            dc.SubmitChanges();
        }
        #endregion



        /// <summary>
        /// Update Details to File
        /// </summary>
        /// <param name="_track"></param>
        public static void UpdateDetails2File(Track _track)
        {
            using (TagLib.File f = TagLib.File.Create(_track.Location))
            {
                // Title
                f.Tag.Title = _track.Title;

                // Artist: In case there are many Artists, split into many substrings using separator is ', '
                string[] _artists = _track.Artist.Split(new string[] { ", " }, StringSplitOptions.None);
                f.Tag.Performers = _artists;

                // Album
                f.Tag.Album = _track.Album;

                // AlbumArtist: In case there are many AlbumArtists, split into many substrings using separator is ', '
                string[] _albumArtists = _track.AlbumArtist.Split(new string[] { ", " }, StringSplitOptions.None);
                f.Tag.AlbumArtists = _albumArtists;

                // Genre: In case there are many Genres, split into many substrings using separator is ', '
                string[] _genres = _track.Genre.Split(new string[] { ", " }, StringSplitOptions.None);
                f.Tag.Genres = _genres;

                // Year
                f.Tag.Year = _track.Year.Value;

                // Lyris
                f.Tag.Lyrics = _track.Lyrics;

                // Save to file
                f.Save();
            }
        }

        /// <summary>
        /// Update Favourite Only
        /// </summary>
        /// <param name="_ID">Song_ID:int</param>
        /// <param name="_isFavourite">Favourite:Boolean</param>
        public static void UpdateOnlyFavourite2DB(string _ID, bool _isFavourite)
        {
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                CHITIETBAIHAT _ctiet = dc.CHITIETBAIHATs.Single(ctiet => ctiet.Ma_BaiHat == _ID);

                if (_ctiet != null)
                    _ctiet.Rating = _isFavourite;

                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Update playcount after finish playing a track
        /// </summary>
        /// <param name="track"></param>
        public static void UpdatePlayCount(Track track)
        {
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                CHITIETBAIHAT _ctiet = dc.CHITIETBAIHATs.Single(ctiet => ctiet.Ma_BaiHat == track.ID);

                if(_ctiet!=null)
                    _ctiet.SoLanNghe++;

                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Update last time play that 'track'
        /// </summary>
        /// <param name="track"></param>
        public static void UpdateLastTimePlay(Track track)
        {
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                CHITIETBAIHAT _ctiet = dc.CHITIETBAIHATs.Single(ctiet => ctiet.Ma_BaiHat == track.ID);

                if (_ctiet != null)
                    _ctiet.LanNgheCuoi = DateTime.Now;

                dc.SubmitChanges();
            }
        }

        // DELETE


        /// <summary>
        /// Delete a track in DB
        /// </summary>
        /// <param name="_track"></param>
        public static void DeleteOneTrack(Track _track)
        {
            using (dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                // Deleting in ChiTiet first
                dc.CHITIETBAIHATs.DeleteOnSubmit(dc.CHITIETBAIHATs.Single(ctiet => ctiet.Ma_BaiHat == _track.ID));
                dc.BAIHATs.DeleteOnSubmit(dc.BAIHATs.Single(bh => bh.Ma_BaiHat == _track.ID));

                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Clean up and refresh database
        /// </summary>
        public static async void CleanDatabase()
        {
            using (var dc = new MusicDBDataContext(Properties.Settings.Default.MusicConnectionString))
            {
                dc.ExecuteCommand("DELETE FROM CHITIETBAIHAT");
                dc.ExecuteCommand("DELETE FROM BAIHAT");
                await Task.Run(() => { dc.ExecuteCommand("DELETE FROM TUA"); });
                await Task.Run(() => { dc.ExecuteCommand("DELETE FROM CASI"); });
                await Task.Run(() => { dc.ExecuteCommand("DELETE FROM ALBUMS"); });
                await Task.Run(() => { dc.ExecuteCommand("DELETE FROM THELOAI"); });
                await Task.Run(() => { PlaylistsSetting.DeletePlaylistsFile(); });
                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Delete all user's playlists
        /// </summary>
        /// <param name="playlists"></param>
        public static void CleanAllPlaylists(ObservableCollection<Playlist> playlists)
        {
            foreach (var pl in playlists)
                if (pl.Prio == Playlist.Priority.User)
                    playlists.Remove(pl);
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
                    dc.Dispose();
                }
                _disposed = true;
            }
        }

        ~Updater()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion
    }
}
