using SkiaSharp;
using Topten.RichTextKit;
using Avalonia;
using Avalonia.Input;
namespace Sandbox.Avalonia;

class RichStringSandboxControl : SkiaControl
{
    float margin = 60.0f;
    bool _useMaxWidth = true;
    bool _useMaxHeight = true;
    RichString _richString;
    HitTestResult _htr;
    SKFont _arial12;
    SKPaint _antialiasPaint;

    public RichStringSandboxControl()
    {
        _richString = new RichString()
       .MarginLeft(20).MarginRight(20)
       .Add("Big text\nMore Big Text\nSomething Else", fontSize: 40, letterSpacing: 0)
       .Add("Little text", fontSize: 12, letterSpacing: 0);

        _arial12 = new SKFont()
        {
            Typeface = SKTypeface.FromFamilyName("Arial"),
            Size = 12
        };

        _antialiasPaint = new SKPaint()
        {
            IsAntialias = true
        };
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        const float DeviceDpi = 96.0f;
        const float devScale = 96.0f / DeviceDpi;
        float x = (float)e.GetPosition(this).X;
        float y = (float)e.GetPosition(this).Y;
        var htr = _richString.HitTest(x * devScale - margin, y * devScale - margin);
        if (!_htr.Equals(htr))
        {
            _htr = htr;
            InvalidateVisual();
        }

    }

    protected override void RenderSkia(SKCanvas canvas, Rect bounds)
    {
        canvas.Clear(new SKColor(0xFFFFFFFF));
        float canvasWidth = (float)bounds.Width;
        float canvasHeight = (float)bounds.Height;
        float? height = (float)(canvasHeight - margin * 2);
        float? width = (float)(canvasWidth - margin * 2);

        if (!_useMaxHeight)
            height = null;
        if (!_useMaxWidth)
            width = null;

        using (var gridlinePaint = new SKPaint() { Color = new SKColor(0x20000000), StrokeWidth = 1 })
        {
            canvas.DrawLine(new SKPoint(margin, 0), new SKPoint(margin, (float)canvasHeight), gridlinePaint);
            if (width.HasValue)
                canvas.DrawLine(new SKPoint(margin + width.Value, 0), new SKPoint(margin + width.Value, (float)canvasHeight), gridlinePaint);
            canvas.DrawLine(new SKPoint(0, margin), new SKPoint((float)canvasWidth, margin), gridlinePaint);
            if (height.HasValue)
                canvas.DrawLine(new SKPoint(0, margin + height.Value), new SKPoint((float)canvasWidth, margin + height.Value), gridlinePaint);
        }

        _richString.MaxWidth = width;
        _richString.MaxHeight = height;

        var state = $"Measured: {_richString.MeasuredWidth} x {_richString.MeasuredHeight} Lines: {_richString.LineCount} Truncated: {_richString.Truncated} Length: {_richString.MeasuredLength} Revision: {_richString.Revision}";

        canvas.DrawText(state, margin, 20, _arial12, _antialiasPaint);

        state = $"Hit Test: Over {_htr.OverCodePointIndex} Line {_htr.OverLine}.  Closest: {_htr.ClosestCodePointIndex} Line {_htr.ClosestLine}";
        /*
        canvas.DrawText(state, margin, 40, new SKPaint()
        {
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 12,
            IsAntialias = true,
        });
        */
        canvas.DrawText(state, margin, 40, _arial12, _antialiasPaint);

        var options = new TextPaintOptions()
        {
            SelectionColor = new SKColor(0x60FF0000),
        };

        options.Selection = new TextRange(0, 10);
        options.SelectionColor = new SKColor(255, 192, 192);
        options.SelectionHandleColor = new SKColor(0, 0, 255);
        options.SelectionHandleScale = 1;

        _richString.Paint(canvas, new SKPoint(margin, margin), options);

        if (_htr.ClosestCodePointIndex >= 0)
        {
            var ci = _richString.GetCaretInfo(new CaretPosition(_htr.ClosestCodePointIndex, false));
            using (var paint = new SKPaint()
            {
                Color = new SKColor(0xFF000000),
                IsStroke = true,
                IsAntialias = true,
                StrokeWidth = 1,
            })
            {
                var rect = ci.CaretRectangle;
                rect.Offset(margin, margin);
                canvas.DrawLine(rect.Right, rect.Top, rect.Left, rect.Bottom, paint);
            }
        }

    }

}
