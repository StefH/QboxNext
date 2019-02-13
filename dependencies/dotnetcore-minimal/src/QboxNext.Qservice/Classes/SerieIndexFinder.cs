using System;
using System.Collections.Generic;
using System.Linq;
using QboxNext.Storage;

namespace QboxNext.Qservice.Classes
{
    public class SerieIndexFinder
    {
        public SerieIndexFinder(List<SeriesValue> inSlots)
        {
            _slots = inSlots;
            // inSlots will change between calls to GetSerieIndex, but only at the end of the list. Therefore we need to store the info
            // about the original list.
            _serieBegin = inSlots.Any() ? inSlots.First().Begin : DateTime.MaxValue;
            _serieEnd = inSlots.Any() ? inSlots.Last().Begin : DateTime.MinValue;
            _nrSlots = inSlots.Count;

            _itemToFind = new SeriesValue();
            _comparer = new SeriesValueComparer();
            _lastFoundIndex = -1;
        }


        public int GetIndex(DateTime inBeginToFind)
        {
            // No items to search.
            if (_nrSlots <= 0)
                return -1;

            // Item to find is out of range
            if (inBeginToFind < _serieBegin || inBeginToFind > _serieEnd)
                return -1;

            // Efficient short cut: if the next slot is adjacent to the previous slot we directly know the new index.
            if (IsBeginOfNextSlot(inBeginToFind))
            {
                _lastFoundIndex++;
                return _lastFoundIndex;
            }

            _itemToFind.Begin = inBeginToFind;
            _lastFoundIndex = _slots.BinarySearch(0, _nrSlots, _itemToFind, _comparer);
            return _lastFoundIndex;
        }

        private bool IsBeginOfNextSlot(DateTime inBeginToFind)
        {
            return _lastFoundIndex >= 0 && 
                   _lastFoundIndex + 1 < _nrSlots && 
                   _slots[_lastFoundIndex + 1].Begin == inBeginToFind;
        }

        private class SeriesValueComparer : IComparer<SeriesValue>
        {
            public int Compare(SeriesValue inFirst, SeriesValue inSecond)
            {
                return inFirst.Begin.CompareTo(inSecond.Begin);
            }
        }


        private readonly List<SeriesValue> _slots;
        private readonly DateTime _serieBegin;
        private readonly DateTime _serieEnd;
        private readonly int _nrSlots;
        private readonly SeriesValue _itemToFind;
        private readonly SeriesValueComparer _comparer;
        private int _lastFoundIndex;
    }
}