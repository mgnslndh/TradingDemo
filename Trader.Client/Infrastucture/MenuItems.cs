﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData.Kernel;
using Trader.Client.Views;
using Trader.Domain.Infrastucture;

namespace Trader.Client.Infrastucture
{
    public enum MenuCategory
    {
        ReactiveUi,
        DynamicData
    }


    public class MenuItems:AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IObjectProvider _objectProvider;
        private readonly IEnumerable<MenuItem> _menuItems;
        private readonly ISubject<ViewContainer> _viewCreatedSubject = new Subject<ViewContainer>();

        private readonly IDisposable _cleanUp;
        private bool _showLinks=false;
        private MenuCategory _category= MenuCategory.DynamicData;
        private IEnumerable<MenuItem> _items;

        public MenuItems(ILogger logger, IObjectProvider objectProvider)
        {
            _logger = logger;
            _objectProvider = objectProvider;
            
            _menuItems = new List<MenuItem>
            {
                new MenuItem("Live Trades",
                    "A basic example, illustrating how to connect to a stream, inject a user filter and bind.",
                    () => Open<LiveTradesViewer>("Live Trades"),new []
                        {
                            new Link("Service","TradeService.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Domain/Services/TradeService.cs"), 
                            new Link("View Model","LiveTradesViewer.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/LiveTradesViewer.cs "), 
                            new Link("Blog","Ui Integration", "https://dynamic-data.org/2014/11/24/trading-example-part-3-integrate-with-ui/"), 
                        }),

                 new MenuItem("Live Trades (RxUI)",
                    "A basic example, illustrating where reactive-ui and dynamic data can work together",
                    () => Open<RxUiViewer>("Reactive UI Example"),
                    MenuCategory.ReactiveUi,
                    new []
                        {
                             new Link("View Model","RxUiViewer.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/RxUiViewer.cs "), 
                            new Link("Blog","Integration with reactive ui", "https://dynamic-data.org/2015/01/18/integration-with-reactiveui/"), 
                        }),

                new MenuItem("Near to Market",
                     "Dynamic filtering of calculated values.",
                     () => Open<NearToMarketViewer>("Near to Market"),new []
                        {
                            new Link("Service","NearToMarketService.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Domain/Services/NearToMarketService.cs"),
                            new Link("Rates Updater","TradePriceUpdateJob.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Domain/Services/TradePriceUpdateJob.cs"), 
                            new Link("View Model", "NearToMarketViewer.cs","https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/NearToMarketViewer.cs"), 
                            new Link("Blog i","Manage Market Data", "https://dynamic-data.org/2014/11/22/trading-example-part-2-manage-market-data/"),
                            new Link("Blog ii","Filter on calculated values", "https://dynamic-data.org/2014/12/21/trading-example-part-4-filter-on-calculated-values/"),
                            
                        }),

                new MenuItem("Trades By %",  
                       "Group trades by a constantly changing calculated value. With automatic regrouping.",
                        () => Open<TradesByPercentViewer>("Trades By % Diff"),new []
                            {
                                new Link("View Model", "TradesByPercentViewer.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/TradesByPercentViewer.cs"), 
                                new Link("Group Model","TradesByPercentDiff.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Domain/Model/TradesByPercentDiff.cs"),
                            }),

                new MenuItem("Trades By hh:mm",   
                       "Group items by time with automatic regrouping as time passes",
                        () => Open<TradesByTimeViewer>("Trades By hh:mm"),new []
                        {
                            new Link("View Model","TradesByTimeViewer.cs" ,"https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/TradesByTimeViewer.cs"), 
                            new Link("Group Model","TradesByTime.cs", "https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Domain/Model/TradesByTime.cs"),
                        }),
                
                new MenuItem("Recent Trades",   
                    "Operator which only includes trades which have changed in the last minute.",
                    () => Open<RecentTradesViewer>("Recent Trades"),new []
                    {
                        new Link("View Model", "RecentTradesViewer.cs","https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/RecentTradesViewer.cs"), 
                    }),

                    
                new MenuItem("Trading Positions",   
                       "Calculate overall position for each currency pair and aggregate totals",
                        () => Open<PositionsViewer>("Trading Positions"),new []
                    {
                        new Link("View Model", "PositionsViewer.cs","https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/PositionsViewer.cs"), 
                        new Link("Group Model", "CurrencyPairPosition.cs","https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Domain/Model/CurrencyPairPosition.cs"), 
                    }),

               new MenuItem("Log Entry",   
                       "Visualiser for log files. Also accumulates log entries and handles user interaction",
                        () => Open<LogEntryViewer>("Log Entry"),
                         MenuCategory.ReactiveUi
                        ,new []
                    {
                        new Link("View Model", "LogEntryViewer.cs","https://github.com/RolandPheasant/TradingDemo/blob/master/Trader.Client/Views/LogEntryViewer.cs"), 
                    }),


            };

            var filterApplier = this.ObserveChanges().Where(prop => prop == "Category")
                .StartWith(string.Empty)
                .Subscribe(_ =>
                {
                    Items = _menuItems.Where(menu => menu.Category == Category).ToArray();
                });

            _cleanUp = Disposable.Create(() =>
            {
                _viewCreatedSubject.OnCompleted();
                filterApplier.Dispose();
            });
        }

        private void Open<T>(string title)
        {

            _logger.Debug("Opening '{0}'", title);
           
            var content = _objectProvider.Get<T>();
            _viewCreatedSubject.OnNext(new ViewContainer(title, content));
            _logger.Debug("--Opened '{0}'", title);
        }

        public MenuCategory Category
        {
            get { return _category; }
            set { SetAndRaise(ref _category, value); }
        }

        public IEnumerable<MenuItem> Items
        {
            get { return _items; }
            set { SetAndRaise(ref _items, value); }
        }

        public bool ShowLinks
        {
            get { return _showLinks; }
            set { SetAndRaise(ref _showLinks, value); }
        }
        
        public IObservable<ViewContainer> ItemCreated
        {
            get { return _viewCreatedSubject.AsObservable(); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}