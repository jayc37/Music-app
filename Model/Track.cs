using Levitate.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Levitate.Model
{
    [Serializable, XmlType(TypeName = "Track")]
    public class Track : PropertyChangedBase, IEquatable<Track>, IDisposable
    {
        #region Fields, Properties, auto-Props
        // Basic Information
        private string _ID;
        public string ID
        {
            get { return _ID; }
            set
            {
                if (value != _ID)
                {
                    SetProperty(value, ref _ID);
                    OnPropertyChanged(ID);
                }
            }
        }
        public string Location { get; set; }
        private string _title;
        private string _artist;
        private string _album;
        private string _albumArtist;
        private string _genre;

        [XmlIgnore]
        public bool HasCoverArt { get; set; }
        private byte[] _playingState;
        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (value != _duration)
                {
                    SetProperty(value, ref _duration);
                    OnPropertyChanged("Duration");
                }
            }
        }
        private bool _isFavourite;
        private uint? _year;

        [XmlIgnore]
        public int PlayCount { get; set; }
        private DateTime _dateAdded;
        public DateTime DateAdded
        {
            get { return _dateAdded; }
            set
            {
                if (value != _dateAdded)
                {
                    SetProperty(value, ref _dateAdded);
                    OnPropertyChanged("DateAdded");
                }
            }
        }
        private DateTime? _lastPlayed;

        // Extended Information
        private string _composer;
        [XmlIgnore]
        public string Lyrics { get; set; }
        [XmlIgnore]
        public string TrackInfo { get; set; }
        [XmlIgnore]
        public byte[] CoverArt { get; set; }
        [XmlIgnore]
        public string Kind { get; set; }
        [XmlIgnore]
        public string BitRate { get; set; }
        [XmlIgnore]
        public string SampleRate { get; set; }
        [XmlIgnore]
        public string Channels { get; set; }
        [XmlIgnore]
        public string CopyRight { get; set; }
        [XmlIgnore]
        public bool IsVBR { get; set; }

        // Props
        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    SetProperty(value, ref _title);
                    OnPropertyChanged("Title");
                }
            }
        }
        public string Artist
        {
            get { return _artist; }
            set
            {
                if (value != _artist)
                {
                    SetProperty(value, ref _artist);
                    OnPropertyChanged("Artist");
                }
            }
        }
        public string Album
        {
            get { return _album; }
            set
            {
                if (value != _album)
                {
                    SetProperty(value, ref _album);
                    OnPropertyChanged("Album");
                }
            }
        }
        public string AlbumArtist
        {
            get { return _albumArtist; }
            set
            {
                if(value!=_albumArtist)
                {
                    SetProperty(value, ref _albumArtist);
                    OnPropertyChanged("AlbumArtist");
                }
            }
        }
        public string Genre
        {
            get { return _genre; }
            set
            {
                if (value != _genre)
                {
                    SetProperty(value, ref _genre);
                    OnPropertyChanged("Genre");
                }
            }
        }
        public bool IsFavourite
        {
            get { return _isFavourite; }
            set
            {
                if (value != _isFavourite)
                {
                    SetProperty(value, ref _isFavourite);
                    OnPropertyChanged("IsFavourite");
                }
            }
        }
        public uint? Year
        {
            get { return _year; }
            set
            {
                if (value != _year)
                {
                    SetProperty(value, ref _year);
                    OnPropertyChanged("Year");
                }
            }
        }
        public DateTime? LastPlayed
        {
            get { return _lastPlayed; }
            set
            {
                if (value != _lastPlayed)
                {
                    SetProperty(value, ref _lastPlayed);
                    OnPropertyChanged("LastPlayed");
                }
            }
        }
        public bool IsExist
        {
            get { return File.Exists(this.Location); }
        }
        public string Composer
        {
            get { return _composer; }
            set
            {
                if (value != _composer)
                {
                    SetProperty(value, ref _composer);
                    OnPropertyChanged("Composer");
                }
            }
        }

        [XmlIgnore]
        public byte[] PlayingState
        {
            get { return _playingState; }
            set
            {
                if(value!=_playingState)
                {
                    SetProperty(value, ref _playingState);
                    OnPropertyChanged("PlayingState");
                }
            }
        }
        [XmlIgnore]
        public string TrueDuration { get { return Duration.ToString(Duration.Hours == 0 ? @"mm\:ss" : @"hh\:mm\:ss"); } }
        #endregion

        //Ctor
        public Track()
        {
            //
        }

        /// <summary>
        /// Open file location
        /// </summary>
        public void OpenTrackLocation()
        {
            if (this.IsExist)
                Process.Start("explorer.exe", "/select, \"" + Location + "\"");
        }

        /// <summary>
        /// IEquatable implements, Equals method
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Track other)
        {
            if (other == null) return false;
            if (!other.IsExist || !this.IsExist) return false;
            if (GetType() != other.GetType()) return false;
            if (other.Location == this.Location) return true;
            return false;
        }

        public override string ToString()
        {
            string s = "";
            s += Artist + " " + Title + " " + Album + " " + Genre;
            return s.ToUpper();
        }

        #region Disposable Implement
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void DisposeDetails()
        {
            DisposeDetails(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose a whole instance
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    ID = null;
                    Location = null;
                    Title = null;
                    Artist = null;
                    Album = null;
                    AlbumArtist = null;
                    Genre = null;
                    HasCoverArt = false;
                    Duration = TimeSpan.Zero;
                    IsFavourite = false;
                    CoverArt = null;
                    PlayingState = null;
                    Year = null;
                    PlayCount = 0;
                    DateAdded = DateTime.MinValue;
                    LastPlayed = null;
                    Composer = null;
                    Lyrics = null;
                    TrackInfo = null;
                    Kind = null;
                    BitRate = null;
                    SampleRate = null;
                    Channels = null;
                    CopyRight = null;
                    IsVBR = false;
                }
                _disposed = true;
            }
        }
        /// <summary>
        /// Dispost details of instance
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void DisposeDetails(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    CoverArt = null;
                    //PlayingState = null;
                    Composer = null;
                    PlayCount = 0;
                    DateAdded = DateTime.MinValue;
                    Lyrics = null;
                    TrackInfo = null;
                    Kind = null;
                    BitRate = null;
                    SampleRate = null;
                    Channels = null;
                    CopyRight = null;
                    IsVBR = false;
                }
                _disposed = true;
            }
        }

        // Destructor
        ~Track()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion
    }
}
