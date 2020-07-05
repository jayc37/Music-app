using System.Collections.ObjectModel;

namespace Levitate.Model
{
    public interface IPlaylist
    {
        ObservableCollection<Track> ListTrack { get; set; }

        string Name { get; set; }
        Priority Prio { get; set; }

        void AddTrack(Track track);
        void RemoveTrack(Track track);
        void Clear();
    }
}
