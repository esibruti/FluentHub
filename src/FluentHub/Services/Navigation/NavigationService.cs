﻿using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentHub.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;
        public ITabView TabView { get; private set; }
        public Type NewTabPage { get; set; }
        public bool IsConfigured { get; private set; }

        public void Configure(ITabView tabView!!, Frame frame!!, Type newTabPage)
        {
            UnsubscribeEvents();
            TabView = tabView;
            _frame = frame;
            NewTabPage = newTabPage;
            SubscribeEvents();
            IsConfigured = true;
        }

        private void OnTabViewSelectionChanged(object sender, TabViewSelectionChangedEventArgs e)
        {
            if (e.NewSelectedItem is ITabItemView item)
            {
                var currentHistoryItem = item.NavigationHistory.CurrentItem;
                if (currentHistoryItem == null) // No navigation history, go to new tab page
                {
                    if (NewTabPage != null)
                    {
                        _frame.Navigate(NewTabPage, null, e.RecommendedNavigationTransitionInfo);
                    }
                }
                else
                {
                    try
                    {
                        _frame.Navigating -= OnFrameNavigating;
                        _frame.ForwardStack.Clear();
                        _frame.BackStack.Clear();
                        _frame.Navigate(currentHistoryItem.PageStackEntry.SourcePageType,
                                        currentHistoryItem.PageStackEntry.Parameter,
                                        e.RecommendedNavigationTransitionInfo);



                        for (int i = 0; i < item.NavigationHistory.CurrentItemIndex; i++)
                        {
                            _frame.BackStack.Add(item.NavigationHistory[i].PageStackEntry);
                        }
                        for (int i = item.NavigationHistory.CurrentItemIndex + 1; i < item.NavigationHistory.Items.Count; i++)
                        {
                            _frame.ForwardStack.Add(item.NavigationHistory[i].PageStackEntry);
                        }
                    }
                    finally
                    {
                        _frame.Navigating += OnFrameNavigating;
                    }
                }
            }
        }

        public void Navigate(Type page, object parameter = null)
        {
            EnsureConfigured();

            if (TabView.SelectedItem is null)
            {
                var item = TabView.OpenTab(page, parameter, true);
                TabView.SelectedItem = item;
            }
            else
            {
                _frame.Navigate(page, parameter);
            }
        }
        public void Navigate<T>(object parameter = null) where T : Page => Navigate(typeof(T), parameter);

        public Guid OpenTab(Type page, object parameter)
        {
            EnsureConfigured();

            var item = TabView.OpenTab(page, parameter, true);
            return item.Guid;
        }
        public Guid OpenTab<T>(object parameter = null) where T : Page => OpenTab(typeof(T), parameter);

        public void GoToTab(Guid tabId)
        {
            var item = TabView.Items.FirstOrDefault(x => x.Guid == tabId);
            if (item != null)
            {
                TabView.SelectedItem = item;
            }
        }
        public void CloseTab(Guid tabId) => TabView.CloseTab(tabId);

        public void GoBack()
        {
            EnsureConfigured();
            _frame.GoBack();
        }

        public void GoForward()
        {
            EnsureConfigured();
            _frame.GoForward();
        }

        public bool CanGoBack()
        {
            EnsureConfigured();
            return _frame.CanGoBack;
        }

        public bool CanGoForward()
        {
            EnsureConfigured();
            return _frame.CanGoForward;
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var history = TabView.SelectedItem.NavigationHistory;
            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                    history.NavigateTo(new PageNavigationEntry(e.SourcePageType, e.Parameter, e.NavigationTransitionInfo), history.CurrentItemIndex + 1);
                    break;

                case NavigationMode.Back:
                    history.GoBack();
                    break;

                case NavigationMode.Forward:
                    history.GoForward();
                    break;
            }
        }
        private void SubscribeEvents()
        {
            if (TabView != null)
            {
                TabView.SelectionChanged += OnTabViewSelectionChanged;
            }
            if (_frame != null)
            {
                _frame.Navigating += OnFrameNavigating;
            }
        }

        private void UnsubscribeEvents()
        {
            if (TabView != null)
            {
                TabView.SelectionChanged -= OnTabViewSelectionChanged;
            }
            if (_frame != null)
            {
                _frame.Navigating -= OnFrameNavigating;
            }
        }

        private void EnsureConfigured()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("The Navigation Service has not been configured. Call INavigationService.Configure first");
            }
        }

        public void Disconnect()
        {
            UnsubscribeEvents();
            TabView = null;
            _frame = null;
            NewTabPage = null;
            IsConfigured = false;
        }
    }
}