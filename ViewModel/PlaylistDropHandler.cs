using GongSolutions.Wpf.DragDrop;
using Levitate.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace Levitate.ViewModel
{
    public class PlaylistListDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if(((dropInfo.Data is Track || dropInfo.Data is IEnumerable<Track>) && dropInfo.TargetItem is Playlist))
            {
                dropInfo.DropTargetAdorner = typeof(DropTargetHighlightAdorner);
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if(dropInfo.Data is Playlist)
            {
                dropInfo.DropTargetAdorner = typeof(DropTargetInsertionAdorner);
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var playlist = (Playlist)dropInfo.TargetItem;

            if (!playlist.CanEdit && playlist.Prio == Playlist.Priority.Application)
                return;

            if(dropInfo.Data is Track)
            {
                var track = (Track)dropInfo.Data;
                playlist.ListTrack.Add(track);
            }
            else if(dropInfo.Data is IEnumerable<Track>)
            {
                var tracks = (IEnumerable<Track>)dropInfo.Data;
                foreach (var track in tracks)
                {
                    playlist.ListTrack.Add(track);;
                }
            }
            else if(dropInfo.Data is Playlist)
            {
                var playlist2Move = (Playlist)dropInfo.Data;
                var collection = (ObservableCollection<Playlist>)dropInfo.DragInfo.SourceCollection;
                int newIndex;
                var currentIndex = collection.IndexOf(playlist2Move);


                if (dropInfo.InsertIndex > collection.Count - 1)
                    newIndex = collection.Count - 1;
                else
                {
                    newIndex = dropInfo.InsertIndex;
                    if (newIndex > 0 && newIndex > currentIndex)
                        newIndex--;
                }

                if (currentIndex == newIndex) return;
                collection.Move(currentIndex, newIndex);
                MainWindowViewModel.Instance.MusicManager.SelectedPlaylist = collection[newIndex];
            }
        }
    }
}
