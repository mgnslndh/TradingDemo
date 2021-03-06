﻿using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Trader.Client.Infrastucture
{
    public class SelectorBinding : IDisposable// IAttachedReceiver
    {
        private readonly ObservableCollection<object> _selected = new ObservableCollection<object>();
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        private bool _isSelecting;

        internal void Receive(Selector selector)
        {
            _serialDisposable.Disposable = Observable
                .FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(
                    h => selector.SelectionChanged += h,
                    h => selector.SelectionChanged -= h)
                .Select(evt => evt.EventArgs)
                .Subscribe(HandleSelectionChanged);
        }

        private void HandleSelectionChanged(SelectionChangedEventArgs args)
        {
            if (_isSelecting) return;
            try
            {
                _isSelecting = true;

                foreach (var item in args.AddedItems)
                    _selected.Add(item);

                foreach (var item in args.RemovedItems)
                    _selected.Remove(item);
            }
            finally
            {

                _isSelecting = false;
            }
        }

        public ObservableCollection<object> Selected
        {
            get { return _selected; }
        }

        public void Dispose()
        {
            _serialDisposable.Dispose();
        }
    }
}