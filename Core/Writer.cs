using Levitate.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Levitate.Core
{
    /// <summary>
    /// Contains many methods to push data to Database
    /// </summary>
    public class Writer : IDisposable
    {
        #region Fields/AutoImplemented Declaration
        private static MusicDBDataContext dc { get; set; }
        private const string UNKNOW = "Unknow";
        private static TagLib.File f { get; set; }
        #endregion

        /// <summary>
        /// Import basic information of a track to database
        /// </summary>
        /// <param name="_track"></param>
        public static void ImportTrack2Database(MusicDBDataContext dc, Track _track)
        {
            if (_track == null) return;

            BAIHAT bh = new BAIHAT();
            CHITIETBAIHAT ctbh = new CHITIETBAIHAT();

            // If the file path has exist in database so that file has added. Return!
            if (dc.BAIHATs.Any(s => s.DuongDan == _track.Location))
                return;

            // FILL TABLE BAIHAT, CASI, TUA, THELOAI, ALBUM:
            // 1: Checking and FILL information to Tables: CASI, TUA, THELOAI, ALBUM
            // 2: Fill table BAIHAT by getting the ID from those tables.

            bh.DuongDan = _track.Location;
            bh.Ma_BaiHat = Utils.GenerateID("BAIHAT");

            // Check exist and add a new Artist in table
            bh.Ma_CaSi = Write2_CASI1(_track, dc);

            // Check exist and add a new Title in table TUA
            bh.Ma_Tua = Write2_TUA(_track, dc);

            // Check exist and add a new Album in table ALBUM
            bh.Ma_Album = Write2_ALBUM(_track, dc);

            // Check exist and add a new Genre in table THELOAI
            bh.Ma_TheLoai = Write2_THELOAI(_track, dc);

            // Check exist and add a new AlbumArtist in table CASI
            bh.Ma_CaSiAlbum = Write2_CASI2(_track, dc);

            dc.BAIHATs.InsertOnSubmit(bh);
            dc.SubmitChanges();

            // FILL TABLE CHITIETBAIHAT
            Write2_CHITIETBAIHAT(_track, dc, bh);
            //}
        }


        #region Need for adding to database
        /// <summary>
        /// Push artist name to CASI
        /// </summary>
        /// <param name="_track"></param>
        /// <param name="dc"></param>
        public static string Write2_CASI1(Track _track, MusicDBDataContext dc)
        {
            if (dc.CASIs.Any(cs => cs.Ten_CaSi == _track.Artist))
            {
                return (from cs in dc.CASIs
                        where cs.Ten_CaSi == _track.Artist
                        select cs.Ma_CaSi).FirstOrDefault();
            }
            else
            {
                CASI casi = new CASI();

                casi.Ma_CaSi = Utils.GenerateID("CASI");
                casi.Ten_CaSi = _track.Artist;

                dc.CASIs.InsertOnSubmit(casi);
                dc.SubmitChanges();

                return casi.Ma_CaSi;
            }
        }

        /// <summary>
        /// Push albumArtist name to CASI
        /// </summary>
        /// <param name="_track"></param>
        /// <param name="dc"></param>
        public static string Write2_CASI2(Track _track, MusicDBDataContext dc)
        {
            if (dc.CASIs.Any(csa => csa.Ten_CaSi == _track.AlbumArtist))
            {
                return (from csa in dc.CASIs
                        where csa.Ten_CaSi == _track.AlbumArtist
                        select csa.Ma_CaSi).FirstOrDefault();
            }
            else
            {
                CASI casi = new CASI();

                casi.Ma_CaSi = Utils.GenerateID("CASIALBUM");
                casi.Ten_CaSi = _track.AlbumArtist;

                dc.CASIs.InsertOnSubmit(casi);
                dc.SubmitChanges();

                return casi.Ma_CaSi;
            }
        }

        /// <summary>
        /// Push album name to ALBUM
        /// </summary>
        /// <param name="_track"></param>
        /// <param name="dc"></param>
        public static string Write2_ALBUM(Track _track, MusicDBDataContext dc)
        {
            if (dc.ALBUMs.Any(alb => alb.Ten_Album == _track.Album))
            {
                return (from alb in dc.ALBUMs
                        where alb.Ten_Album == _track.Album
                        select alb.Ma_Album).FirstOrDefault();
            }
            else
            {
                ALBUM album = new ALBUM();

                album.Ma_Album = Utils.GenerateID("ALBUM");
                album.Ten_Album = _track.Album;

                dc.ALBUMs.InsertOnSubmit(album);
                dc.SubmitChanges();

                return album.Ma_Album;
            }
        }

        /// <summary>
        /// Push title to TUA
        /// </summary>
        /// <param name="_track"></param>
        /// <param name="dc"></param>
        public static string Write2_TUA(Track _track, MusicDBDataContext dc)
        {
            if (dc.TUAs.Any(tua => tua.Ten_Tua == _track.Title))
            {
                return (from tua in dc.TUAs
                        where tua.Ten_Tua == _track.Title
                        select tua.Ma_Tua).FirstOrDefault();
            }
            else
            {
                TUA tua = new TUA();

                tua.Ma_Tua = Utils.GenerateID("TUA");
                tua.Ten_Tua = _track.Title;

                dc.TUAs.InsertOnSubmit(tua);
                dc.SubmitChanges();

                return tua.Ma_Tua;
            }
        }

        /// <summary>
        /// Push genre to THELOAI
        /// </summary>
        /// <param name="_track"></param>
        /// <param name="dc"></param>
        public static string Write2_THELOAI(Track _track, MusicDBDataContext dc)
        {
            if (dc.THELOAIs.Any(tl => tl.Ten_TheLoai == _track.Genre))
            {
                return (from tl in dc.THELOAIs
                        where tl.Ten_TheLoai == _track.Genre
                        select tl.Ma_TheLoai).FirstOrDefault();
            }
            else
            {
                THELOAI theloai = new THELOAI();

                theloai.Ma_TheLoai = Utils.GenerateID("THELOAI");
                theloai.Ten_TheLoai = _track.Genre;

                dc.THELOAIs.InsertOnSubmit(theloai);
                dc.SubmitChanges();

                return theloai.Ma_TheLoai;
            }
        }

        /// <summary>
        /// Push other details to CHITIETBAIHAT
        /// </summary>
        /// <param name="_track"></param>
        /// <param name="bh"></param>
        /// <param name="dc"></param>
        public static void Write2_CHITIETBAIHAT(Track _track, MusicDBDataContext dc, BAIHAT bh)
        {
            CHITIETBAIHAT chitiet = new CHITIETBAIHAT();

            chitiet.Ma_BaiHat = bh.Ma_BaiHat;
            chitiet.ThoiLuong = _track.Duration;
            chitiet.CoAnhBia = _track.HasCoverArt;
            chitiet.NgayThemVaoCSDL = _track.DateAdded;
            chitiet.NamPhatHanh = _track.Year.ToString();

            dc.CHITIETBAIHATs.InsertOnSubmit(chitiet);
            dc.SubmitChanges();
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

        ~Writer()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion
    }
}
