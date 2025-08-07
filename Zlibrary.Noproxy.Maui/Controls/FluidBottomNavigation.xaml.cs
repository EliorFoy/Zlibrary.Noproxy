using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Zlibrary.Noproxy.Maui.Controls;

public partial class FluidBottomNavigation : ContentView
{
    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(FluidBottomNavigation), 0,
            propertyChanged: OnSelectedIndexChanged);

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public event EventHandler<int>? TabSelected;

    private readonly double[] _tabPositions = new double[4];
    private bool _isAnimating = false;

    public FluidBottomNavigation()
    {
        InitializeComponent();
        BindingContext = this;

        // Set initial selection to match the original (second tab selected)
        SelectedIndex = 1;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        // Calculate tab positions
        var totalWidth = Width;
        var tabWidth = totalWidth / 4;

        for (int i = 0; i < 4; i++)
        {
            _tabPositions[i] = (i * tabWidth) + (tabWidth / 2) - 50; // 50 is half of fluid shape width
        }

        // Set initial position without animation
        AnimateToPosition(SelectedIndex, false);
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FluidBottomNavigation navigation)
        {
            navigation.AnimateToPosition((int)newValue, true);
        }
    }

    private void OnTabTapped(object sender, EventArgs e)
    {
        if (_isAnimating) return;

        if (sender is TapGestureRecognizer tapGesture &&
            tapGesture.CommandParameter is string parameter &&
            int.TryParse(parameter, out int index))
        {
            if (index != SelectedIndex)
            {
                SelectedIndex = index;
                TabSelected?.Invoke(this, index);
            }
        }
    }

    private async void AnimateToPosition(int index, bool animate)
    {
        if (_isAnimating || _tabPositions.Length == 0) return;

        _isAnimating = true;

        try
        {
            var targetX = _tabPositions[index];

            if (animate)
            {
                // Create fluid animation effect with multiple phases
                await Task.WhenAll(
                    // Main fluid shape movement
                    FluidShape.TranslateTo(targetX, 0, 800, Easing.CubicOut),

                    // Animate the fluid blobs
                    AnimateFluidBlobs(targetX)
                );
            }
            else
            {
                // Set initial position without animation
                FluidShape.TranslationX = targetX;
                FluidBlob1.TranslationX = targetX;
                FluidBlob2.TranslationX = targetX;
                FluidBlob3.TranslationX = targetX;
            }
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task AnimateFluidBlobs(double targetX)
    {
        var tasks = new List<Task>();

        // Animate blob positions with slight delays for fluid effect
        tasks.Add(Task.Run(async () =>
        {
            await Task.Delay(100);
            await FluidBlob1.TranslateTo(targetX, 0, 600, Easing.CubicOut);
            await FluidBlob1.ScaleTo(1.3, 200, Easing.CubicOut);
            await FluidBlob1.ScaleTo(1.0, 400, Easing.BounceOut);
        }));

        tasks.Add(Task.Run(async () =>
        {
            await Task.Delay(200);
            await FluidBlob2.TranslateTo(targetX, 0, 600, Easing.CubicOut);
            await FluidBlob2.ScaleTo(1.2, 150, Easing.CubicOut);
            await FluidBlob2.ScaleTo(1.0, 300, Easing.BounceOut);
        }));

        tasks.Add(Task.Run(async () =>
        {
            await Task.Delay(150);
            await FluidBlob3.TranslateTo(targetX, 0, 600, Easing.CubicOut);
            await FluidBlob3.ScaleTo(1.4, 180, Easing.CubicOut);
            await FluidBlob3.ScaleTo(1.0, 350, Easing.BounceOut);
        }));

        // Main shape scaling for fluid effect
        tasks.Add(Task.Run(async () =>
        {
            await FluidShape.ScaleTo(1.1, 200, Easing.CubicOut);
            await FluidShape.ScaleTo(1.0, 400, Easing.BounceOut);
        }));

        await Task.WhenAll(tasks);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width > 0 && height > 0)
        {
            // Recalculate tab positions when size changes
            var tabWidth = width / 4;

            for (int i = 0; i < 4; i++)
            {
                _tabPositions[i] = (i * tabWidth) + (tabWidth / 2) - 50;
            }

            // Update current position
            if (!_isAnimating)
            {
                AnimateToPosition(SelectedIndex, false);
            }
        }
    }
}

// Converter for tab colors - matches original project colors
public class IndexToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is string indexStr && int.TryParse(indexStr, out int tabIndex))
        {
            // Selected tab: White, Unselected tab: Light gray (matching original)
            return selectedIndex == tabIndex ? Colors.White : Color.FromArgb("#B2BEC3");
        }
        return Color.FromArgb("#B2BEC3");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
