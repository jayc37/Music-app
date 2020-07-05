using Levitate.Core;
using Levitate.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Levitate.Model
{
    [Serializable, XmlType(TypeName = "Playlist")]
    public class Playlist : PropertyChangedBase, IPlaylist, IDisposable
    {
        #region Auto-Implemented Props

        [XmlElement("Track List")]
        public ObservableCollection<Track> ListTrack { get; set; }

        [XmlElement("Id")]
        public string Id { get; set; }
        private string _name;
        [XmlElement("Name")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    SetProperty(value, ref _name);
                    OnPropertyChanged("Name");
                }
            }
        }
        [XmlElement("DateCreated")]
        public DateTime? DateCreated { get; set; }
        [XmlElement("Prio")]
        public Priority Prio { get; set; }
        [XmlIgnore]
        public string GetTotalDuration
        {
            get
            {
                if (ListTrack.Count != 0)
                {

                    TimeSpan _totalDuration = new TimeSpan();

                    foreach (var track in ListTrack)
                        _totalDuration += track.Duration;

                    string s = "";
                    if (_totalDuration.TotalDays > 1)
                    {
                        s += _totalDuration.TotalDays + " " + Application.Current.FindResource("days").ToString();
                    }
                    else if (_totalDuration.Days == 1)
                    {
                        s += _totalDuration.TotalDays + " " + Application.Current.FindResource("days").ToString() + " ";
                        switch (_totalDuration.Hours)
                        {
                            case 1:
                                s += _totalDuration.Hours + " " + Application.Current.FindResource("day").ToString();
                                break;
                            case 0:
                                break;
                            default:
                                s += _totalDuration.Hours + " " + Application.Current.FindResource("days").ToString();
                                break;
                        }
                    }
                    else if (_totalDuration.Hours > 1)
                    {
                        s += _totalDuration.Hours + " " + Application.Current.FindResource("hours").ToString();
                        switch (_totalDuration.Minutes)
                        {
                            case 1:
                                s += _totalDuration.Minutes + " " + Application.Current.FindResource("minute").ToString();
                                break;
                            case 0:
                                break;
                            default:
                                s += _totalDuration.Minutes + " " + Application.Current.FindResource("minutes").ToString();
                                break;
                        }
                    }
                    else if (_totalDuration.Hours == 1)
                    {
                        s += _totalDuration.Hours + " " + Application.Current.FindResource("hour").ToString();
                        switch (_totalDuration.Minutes)
                        {
                            case 1:
                                s += _totalDuration.Minutes + " " + Application.Current.FindResource("minute").ToString();
                                break;
                            case 0:
                                break;
                            default:
                                s += _totalDuration.Minutes + " " + Application.Current.FindResource("minutes").ToString();
                                break;
                        }
                    }
                    else
                    {
                        switch (_totalDuration.Minutes)
                        {
                            case 1:
                                s += _totalDuration.Minutes + " " + Application.Current.FindResource("minute").ToString();
                                break;
                            case 0:
                                break;
                            default:
                                s += _totalDuration.Minutes + " " + Application.Current.FindResource("minutes").ToString();
                                break;
                        }
                    }
                    return s;
                }
                return null;
            }
        }
        [XmlIgnore]
        public string Details
        {
            get { return this.ToString(); }
        }
        [XmlIgnore]
        public byte[] Icon { get; set; }
        #endregion

        // Ctor
        public Playlist()
        {
            Id = new Guid().ToString("D");
            ListTrack = new ObservableCollection<Track>();
            //Name = Application.Current.FindResource("newPlaylist").ToString();
            Prio = Priority.User;
            DateCreated = DateTime.Now;
            Icon = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/playlistWhite.png"));
        }


        #region Interaction Methods

        /// <summary>
        /// Add a track
        /// </summary>
        /// <param name="_track"></param>
        public void AddTrack(Track _track)
        {
            ListTrack.Add(_track); ;
        }


        /// <summary>
        /// Remove a given track
        /// </summary>
        /// <param name="_track"></param>
        public void RemoveTrack(Track _track)
        {
            ListTrack.Remove(_track);
        }


        /// <summary>
        /// Remove a track at a given position
        /// </summary>
        /// <param name="index"></param>
        public void RemoveTrack(int index)
        {
            ListTrack.RemoveAt(index);
        }


        /// <summary>
        /// Clean up playlist
        /// </summary>
        public void Clear()
        {
            ListTrack.Clear();
        }


        /// <summary>
        /// Next track
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_playlist"></param>
        /// <param name="currentItem"></param>
        /// <returns></returns>
        public static Track NextTrack(Playlist playlist, Track currentTrack)
        {
            if (playlist.ListTrack.Count == 0) return null;

            var currentIndex = playlist.ListTrack.IndexOf(currentTrack);
            if (currentIndex < playlist.ListTrack.Count - 1)
            {
                return playlist.ListTrack[currentIndex + 1];
            }
            else
            {
                return playlist.ListTrack.First();
            }
        }

        /// <summary>
        /// Get previous track
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="currentTrack"></param>
        /// <returns></returns>
        public static Track PreviousTrack(Playlist playlist, Track currentTrack)
        {
            if (playlist.ListTrack.Count == 0) return null;

            var currentIndex = playlist.ListTrack.IndexOf(currentTrack);
            if (currentIndex > 0)
            {
                return playlist.ListTrack[currentIndex - 1];
            }
            else
            {
                return playlist.ListTrack.Last();
            }
        }

        /// <summary>
        /// Get a random track from given playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static Track RandomTrack(Playlist playlist)
        {
            Random rd = new Random();
            int randomIndex = rd.Next(0, playlist.ListTrack.Count);
            return playlist.ListTrack[randomIndex];
        }
        #endregion

        /// <summary>
        /// Create a shuffle playlist from a give playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static Playlist GetShuffle(Playlist playlist)
        {
            Random rd = new Random();

            Playlist shuffle = new Playlist();
            shuffle = playlist;

            for (int i = playlist.ListTrack.Count - 1; i > 0; i--)
            {
                int k = rd.Next(0, playlist.ListTrack.Count + 1);
                shuffle.ListTrack.Move(i, k);
            }

            return shuffle;
        }

        /// <summary>
        /// Load playlist information
        /// </summary>
        /// <param name="playlist"></param>
        public static Playlist LoadPlaylist(Playlist playlist)
        {
            Playlist result = playlist;

            for (int i = 0; i < result.ListTrack.Count; i++)
            {
                if (result.ListTrack[i].IsExist)
                {
                    result.ListTrack[i] = Reader.GetBasicTrackFromID(result.ListTrack[i].ID);
                }
            }
            return result;
        }

        public override string ToString()
        {
            string s = "";

            if (ListTrack.Count == 0)
                return s += Application.Current.FindResource("emptyplaylist").ToString();

            if (ListTrack.Count < 1)
                s += this.ListTrack.Count + " " + Application.Current.FindResource("song").ToString();
            else
                s += this.ListTrack.Count + " " + Application.Current.FindResource("songs2").ToString();

            s += " ・ ";
            s += GetTotalDuration;

            return s;
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
                    ListTrack.Clear();
                    Name = "";
                    DateCreated = null;
                    Prio = 0;
                }
                _disposed = true;
            }
        }

        ~Playlist()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion
    }


    public enum Priority
    {
        Application,
        User
    }
}
