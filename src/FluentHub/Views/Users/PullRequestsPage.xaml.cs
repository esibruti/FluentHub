﻿using FluentHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace FluentHub.Views.Users
{
    public sealed partial class PullRequestsPage : Page
    {
        public PullRequestsPage()
        {
            InitializeComponent();
            navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        }

        private readonly INavigationService navigationService;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string login = e.Parameter as string;

            //Helpers.NavigationHelpers.AddPageInfoToTabItem($"Pull requests", "Viewer's pull requests", $"https://github.com/organizations", "\uE737");
            var currentItem = navigationService.TabView.SelectedItem.NavigationHistory.CurrentItem;
            currentItem.Header = "PullRequests".GetLocalized();
            currentItem.Description = "Viewer's pull requests";
            currentItem.Url = $"https://github.com/organizations";
            currentItem.Icon = new Microsoft.UI.Xaml.Controls.ImageIconSource
            {
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Icons/PullRequests.png"))
            };

            await ViewModel.GetRepoPRs(login);
        }
    }
}