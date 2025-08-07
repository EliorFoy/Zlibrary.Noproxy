using Microsoft.Maui.Graphics;
using System;

namespace Zlibrary.Noproxy.Maui.Controls;

public class FluidNavigationDrawable : IDrawable
{
    private float _animationProgress = 0f;
    private float _targetX = 0f;
    private float _currentX = 0f;
    private readonly Color _fluidColor = Color.FromArgb("#6C63FF");
    private readonly float _tabWidth = 0f;
    private int _selectedIndex = 0;

    public float AnimationProgress
    {
        get => _animationProgress;
        set
        {
            _animationProgress = Math.Max(0, Math.Min(1, value));
        }
    }

    public float TargetX
    {
        get => _targetX;
        set => _targetX = value;
    }

    public float CurrentX
    {
        get => _currentX;
        set => _currentX = value;
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => _selectedIndex = value;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Clear the canvas
        canvas.FillColor = Colors.Transparent;
        canvas.FillRectangle(dirtyRect);

        // Calculate current position with smooth interpolation
        var currentPos = _currentX + (_targetX - _currentX) * EaseOutCubic(_animationProgress);
        
        // Draw the fluid shape
        DrawFluidShape(canvas, dirtyRect, currentPos);
    }

    private void DrawFluidShape(ICanvas canvas, RectF bounds, float centerX)
    {
        canvas.FillColor = _fluidColor;
        canvas.StrokeColor = _fluidColor;
        canvas.StrokeSize = 0;

        var width = 120f;
        var height = 80f;
        var radius = 40f;
        
        // Create the main fluid path
        var path = new PathF();
        
        // Start from bottom left
        var startX = centerX;
        var startY = bounds.Height;
        
        // Create fluid blob shape with bezier curves
        path.MoveTo(startX - width/2, startY);
        
        // Left side curve
        path.CurveTo(
            startX - width/2, startY - height/3,
            startX - width/3, startY - height * 0.8f,
            startX - width/4, startY - height * 0.9f
        );
        
        // Top curve (main bubble)
        path.CurveTo(
            startX - width/6, startY - height,
            startX + width/6, startY - height,
            startX + width/4, startY - height * 0.9f
        );
        
        // Right side curve
        path.CurveTo(
            startX + width/3, startY - height * 0.8f,
            startX + width/2, startY - height/3,
            startX + width/2, startY
        );
        
        // Bottom line
        path.LineTo(startX - width/2, startY);
        path.Close();

        // Fill the main shape
        canvas.FillPath(path);

        // Add additional fluid blobs for more realistic effect
        DrawFluidBlobs(canvas, bounds, centerX);
    }

    private void DrawFluidBlobs(ICanvas canvas, RectF bounds, float centerX)
    {
        var blobSize = 20f + (10f * (float)Math.Sin(_animationProgress * Math.PI));
        var offsetY = bounds.Height - 30f;

        // Main center blob
        canvas.FillEllipse(centerX - blobSize/2, offsetY - blobSize/2, blobSize, blobSize);

        // Left blob
        var leftBlobSize = 15f + (5f * (float)Math.Sin((_animationProgress + 0.3f) * Math.PI));
        canvas.FillEllipse(centerX - 30f - leftBlobSize/2, offsetY + 10f - leftBlobSize/2, leftBlobSize, leftBlobSize);

        // Right blob
        var rightBlobSize = 15f + (5f * (float)Math.Sin((_animationProgress + 0.6f) * Math.PI));
        canvas.FillEllipse(centerX + 30f - rightBlobSize/2, offsetY + 10f - rightBlobSize/2, rightBlobSize, rightBlobSize);

        // Additional small blobs for more fluid effect
        if (_animationProgress > 0.2f)
        {
            var smallBlobSize = 8f * (_animationProgress - 0.2f);
            canvas.FillEllipse(centerX - 15f, offsetY + 5f, smallBlobSize, smallBlobSize);
            canvas.FillEllipse(centerX + 15f, offsetY + 5f, smallBlobSize, smallBlobSize);
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - (float)Math.Pow(1f - t, 3);
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 3) / 2f;
    }
}

// Custom GraphicsView for the fluid navigation
public class FluidNavigationView : GraphicsView
{
    private readonly FluidNavigationDrawable _drawable;
    private bool _isAnimating = false;

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(FluidNavigationView), 0,
            propertyChanged: OnSelectedIndexChanged);

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public FluidNavigationView()
    {
        _drawable = new FluidNavigationDrawable();
        Drawable = _drawable;
        HeightRequest = 80;
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FluidNavigationView view)
        {
            view.AnimateToIndex((int)newValue);
        }
    }

    private async void AnimateToIndex(int index)
    {
        if (_isAnimating) return;

        _isAnimating = true;

        try
        {
            var tabWidth = Width / 4;
            var targetX = (float)((index * tabWidth) + (tabWidth / 2));
            
            _drawable.TargetX = targetX;
            _drawable.SelectedIndex = index;

            // Animate over 800ms
            var duration = 800;
            var steps = 60;
            var stepDuration = duration / steps;

            for (int i = 0; i <= steps; i++)
            {
                _drawable.AnimationProgress = (float)i / steps;
                Invalidate();
                await Task.Delay(stepDuration);
            }
        }
        finally
        {
            _isAnimating = false;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        
        if (width > 0)
        {
            var tabWidth = width / 4;
            var currentX = (float)((SelectedIndex * tabWidth) + (tabWidth / 2));
            _drawable.CurrentX = currentX;
            _drawable.TargetX = currentX;
            Invalidate();
        }
    }
}
