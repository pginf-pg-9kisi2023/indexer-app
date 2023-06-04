using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Indexer.Collections.Generic
{
    public abstract class ObservableKeyedCollection<TKey, TItem>
        : KeyedCollection<TKey, TItem>, INotifyCollectionChanged, INotifyPropertyChanged
        where TKey : notnull
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _deferCollectionChanged;

        protected override void ClearItems()
        {
            base.ClearItems();
            TriggerReset();
        }

        protected override void InsertItem(int index, TItem item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, index
                )
            );
        }

        protected override void SetItem(int index, TItem item)
        {
            base.SetItem(index, item);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, item, index
                )
            );
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, item
                )
            );
        }

        public void AddRange([NotNull] IEnumerable<TItem> items)
        {
            _deferCollectionChanged = true;
            try
            {
                foreach (var item in items)
                {
                    try
                    {
                        Add(item);
                    }
                    catch (ArgumentException)
                    {
                        Remove(GetKeyForItem(item));
                        Add(item);
                    }
                }
                _deferCollectionChanged = false;
            }
            finally
            {
                TriggerReset();
            }
        }

        public void RemoveRange([NotNull] IEnumerable<TKey> keys)
        {
            _deferCollectionChanged = true;
            try
            {
                foreach (var key in keys)
                {
                    Remove(key);
                }
                _deferCollectionChanged = false;
            }
            finally
            {
                TriggerReset();
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_deferCollectionChanged)
            {
                return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));

            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        public void TriggerReset()
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset
                )
            );
        }
    }
}
