using GongSolutions.Wpf.DragDrop;
using Levitate.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Levitate.ViewModel
{
    public class TrackDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = typeof(DropTargetInsertionAdorner);
        }

        public void Drop(IDropInfo dropInfo)
        {
            //var collection = dropInfo.TargetCollection as ObservableCollection<Track>;
            var collection = (ObservableCollection<Track>)((ICollectionView)dropInfo.TargetCollection).SourceCollection;

            if (dropInfo.Data is Track)
            {
                var track = (Track)dropInfo.Data;
                int newIndex;
                var currentIndex = dropInfo.DragInfo.SourceIndex;

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
            }
            else if (dropInfo.Data is IEnumerable<Track>)
            {
                var tracks = ((IEnumerable<Track>)dropInfo.Data).OrderBy(x => collection.IndexOf(x)).ToList();

                int index;
                if (dropInfo.InsertIndex >= collection.Count)
                    index = collection.Count - 1;
                else
                {
                    index = dropInfo.InsertIndex;
                    if (collection.IndexOf(tracks.Last()) < index)
                        index--;
                }

                if (tracks.Any(track => collection.IndexOf(track) == index))
                    return;

                foreach(var track in tracks)
                {
                    collection.Move(collection.IndexOf(track), index);
                }
            }
        }
    }
}
