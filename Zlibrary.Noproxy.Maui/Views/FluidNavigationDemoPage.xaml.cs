using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zlibrary.Noproxy.Maui.Views;

public partial class FluidNavigationDemoPage : ContentPage
{
    private readonly List<StackLayout> _contentViews;
    private int _currentIndex = 1; // Start with Search selected (like original)

    public FluidNavigationDemoPage()
    {
        InitializeComponent();

        _contentViews = new List<StackLayout>
        {
            HomeContent,
            SearchContent,
            HeartContent,
            ProfileContent
        };
    }

    private async void OnTabSelected(object sender, int selectedIndex)
    {
        if (selectedIndex == _currentIndex) return;

        await SwitchContent(_currentIndex, selectedIndex);
        _currentIndex = selectedIndex;
    }

    private async Task SwitchContent(int fromIndex, int toIndex)
    {
        var fromContent = _contentViews[fromIndex];
        var toContent = _contentViews[toIndex];

        // Fade out current content
        await fromContent.FadeTo(0, 200, Easing.CubicOut);
        fromContent.IsVisible = false;

        // Show and fade in new content
        toContent.IsVisible = true;
        toContent.Opacity = 0;
        await toContent.FadeTo(1, 300, Easing.CubicIn);
    }

    private async void OnTestAnimationClicked(object sender, EventArgs e)
    {
        // Demonstrate programmatic tab switching
        for (int i = 0; i < 4; i++)
        {
            FluidNav.SelectedIndex = i;
            await Task.Delay(1000);
        }
        
        // Return to home
        FluidNav.SelectedIndex = 0;
    }
}
