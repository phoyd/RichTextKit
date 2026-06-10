using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;
using Topten.RichTextKit;
using Key = Avalonia.Input.Key;

namespace Sandbox.Avalonia;
/// <summary>
/// A custom control that draws circles and rectangles directly via the render API.
/// </summary>
public class SandboxControl : SkiaControl
{
    class ControlDrawOperation : ICustomDrawOperation
    {
        public SandboxDriver.SandboxDriver driver = new();
        public Rect Bounds { get; set; }

        public ControlDrawOperation()
        {
        }

        public void Render(ImmediateDrawingContext context)
        {
            var feature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (feature == null)
                throw new InvalidOperationException("Unable to obtain SkiaSharp API lease from drawing context.");
            using var lease = feature.Lease();

            SKCanvas canvas = lease.SkCanvas;
            driver.Render(canvas, (float)Bounds.Width, (float)Bounds.Height);
        }

        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;
        public void Dispose() { }
    }

    public SandboxDriver.SandboxDriver _driver = new();
    protected override void RenderSkia(SKCanvas canvas, Rect bounds)
    {
        _driver.Render(canvas, (float)bounds.Width, (float)bounds.Height);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        const float DeviceDpi = 96.0f;
        const float devScale = 96.0f/DeviceDpi;
        float x = (float)e.GetPosition(this).X;
        float y = (float)e.GetPosition(this).Y;
        _driver.HitTest(x * devScale, y * devScale);
        InvalidateVisual();
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Space:
                Console.WriteLine("spaces");
                _driver.ContentMode = (_driver.ContentMode + 1) % _driver.ContentModeCount;

                InvalidateVisual();
                break;

            case Key.Left:
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if (_driver.BaseDirection == TextDirection.RTL)
                        _driver.BaseDirection = TextDirection.LTR;
                    else if (_driver.BaseDirection == TextDirection.LTR)
                        _driver.BaseDirection = TextDirection.Auto;
                }
                else
                {
                    if (_driver.TextAlignment > Topten.RichTextKit.TextAlignment.Auto)
                        _driver.TextAlignment--;
                }
                InvalidateVisual();
                break;
            case Key.Right:
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if (_driver.BaseDirection == TextDirection.Auto)
                        _driver.BaseDirection = TextDirection.LTR;
                    else if (_driver.BaseDirection == TextDirection.LTR)
                        _driver.BaseDirection = TextDirection.RTL;
                }
                else
                {
                    if (_driver.TextAlignment < Topten.RichTextKit.TextAlignment.Right)
                        _driver.TextAlignment++;
                }
                InvalidateVisual();
                break;
            case Key.Up:
                _driver.Scale += 0.1f;
                InvalidateVisual();
                break;

            case Key.Down:
                _driver.Scale -= 0.1f;
                if (_driver.Scale < 0.5f)
                    _driver.Scale = 0.5f;
                InvalidateVisual();
                break;

            case Key.W:
                _driver.UseMaxWidth = !_driver.UseMaxWidth;
                InvalidateVisual();
                break;

            case Key.H:
                _driver.UseMaxHeight = !_driver.UseMaxHeight;
                InvalidateVisual();
                break;

            case Key.M:
                _driver.ShowMeasuredSize = !_driver.ShowMeasuredSize;
                InvalidateVisual();
                break;

            case Key.F1:
                _driver.SubpixelPositioning = !_driver.SubpixelPositioning;
                InvalidateVisual();
                break;

            case Key.F2:
                _driver.Hinting = (SKFontHinting)(((int)_driver.Hinting + 1) % 4);
                InvalidateVisual();
                break;

            case Key.F3:
                _driver.Edging = (SKFontEdging)(((int)_driver.Edging + 1) % 3);
                InvalidateVisual();
                break;

        }
    }
}
