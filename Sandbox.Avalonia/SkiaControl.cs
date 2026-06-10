using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;

namespace Sandbox.Avalonia;

public class SkiaControl : Control
{
    class ControlDrawOperation : ICustomDrawOperation
    {
        public delegate void ControlDrawFunction(SKCanvas canvas, Rect bounds); 
        public required Rect Bounds { get; set; }
        public required ControlDrawFunction DrawFunction { get; set; }
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
            DrawFunction?.Invoke(canvas, Bounds);
        }

        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;
        public void Dispose() { }
    }
    
    protected virtual void RenderSkia(SKCanvas canvas, Rect bounds)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle((float)(bounds.Width / 2), (float)(bounds.Height / 2), (float)(Math.Min(bounds.Width, bounds.Height) / 4), paint);
    }
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(
            new SolidColorBrush(Color.Parse("#ff00ff")),
            new Rect(0, 0, Bounds.Width, Bounds.Height));
        ControlDrawOperation drawOperation = new()
        {
            Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height),
            DrawFunction = RenderSkia
        }; 
        context.Custom(drawOperation);
    }
}
