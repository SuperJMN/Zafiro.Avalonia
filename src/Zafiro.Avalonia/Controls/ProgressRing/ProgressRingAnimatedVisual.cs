// Based on FluentAvalonia ProgressRingAnimatedVisual by amwx
// Original source: https://github.com/amwx/FluentAvalonia
// License: MIT — Copyright (c) 2025 amwx

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Avalonia.VisualTree;
using SkiaSharp;

namespace Zafiro.Avalonia.Controls;

/// <summary>
/// Animated visual source for <see cref="ProgressRing"/>.
/// Public only for XAML template support.
/// </summary>
public sealed class ProgressRingAnimatedVisual : Control
{
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var parent = this.FindAncestorOfType<ProgressRing>();
        if (parent is null) return;

        _handler = new CompositionHandler(
            parent.Minimum, parent.Maximum, parent.Value,
            parent.IsActive, parent.Background, parent.Foreground);

        if (_compositionVisual == null)
        {
            var vis = ElementComposition.GetElementVisual(this);
            if (vis is null) return;

            var comp = vis.Compositor;
            _compositionVisual = comp.CreateCustomVisual(_handler);
            _compositionVisual.Size = new Vector(80, 80);
            ElementComposition.SetElementChildVisual(this, _compositionVisual);
        }

        _compositionVisual.SendHandlerMessage(
            new HandlerMessage(MessageType.Indeterminate, parent.IsIndeterminate));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var minSize = Math.Min(e.NewSize.Width, e.NewSize.Height);
        _compositionVisual?.SendHandlerMessage(
            new HandlerMessage(MessageType.Scale, minSize / 80.0));
    }

    internal void SetMinimum(double min) =>
        _compositionVisual?.SendHandlerMessage(new HandlerMessage(MessageType.Min, (float)min));

    internal void SetMaximum(double max) =>
        _compositionVisual?.SendHandlerMessage(new HandlerMessage(MessageType.Max, (float)max));

    internal void SetValue(double val) =>
        _compositionVisual?.SendHandlerMessage(new HandlerMessage(MessageType.Value, (float)val));

    internal void SetActive(bool active) =>
        _compositionVisual?.SendHandlerMessage(new HandlerMessage(MessageType.Active, active));

    internal void SetIndeterminate(bool indeterminate) =>
        _compositionVisual?.SendHandlerMessage(new HandlerMessage(MessageType.Indeterminate, indeterminate));

    internal void SetBackground(IBrush brush)
    {
        _compositionVisual?.SendHandlerMessage(brush is ISolidColorBrush scb
            ? new HandlerMessage(MessageType.Background, scb.Color.ToSKColor())
            : new HandlerMessage(MessageType.Background, null));
    }

    internal void SetForeground(IBrush brush)
    {
        _compositionVisual?.SendHandlerMessage(brush is ISolidColorBrush scb
            ? new HandlerMessage(MessageType.Foreground, scb.Color.ToSKColor())
            : new HandlerMessage(MessageType.Foreground, SKColors.Transparent));
    }

    private CompositionHandler? _handler;
    private CompositionCustomVisual? _compositionVisual;

    private enum MessageType
    {
        Background, Foreground, Min, Max, Value, Active, Indeterminate, Scale
    }

    private sealed class HandlerMessage(MessageType type, object? data)
    {
        public MessageType Type { get; } = type;
        public object? Data { get; } = data;
    }

    private sealed class CompositionHandler : CompositionCustomVisualHandler
    {
        public CompositionHandler(double minimum, double maximum, double value, bool isActive,
            IBrush? background, IBrush? foreground)
        {
            _min = (float)minimum;
            _max = (float)maximum;
            _value = (float)value;
            _active = isActive;

            if (background is ISolidColorBrush scb)
                _background = scb.Color.ToSKColor();

            if (foreground is ISolidColorBrush scbF)
                _foreground = scbF.Color.ToSKColor();

            _paint = new SKPaint
            {
                IsAntialias = true,
                IsStroke = true,
                StrokeWidth = 4f,
                StrokeCap = SKStrokeCap.Round
            };

            _path = new SKPath();
        }

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            if (!_active) return;

            var feat = drawingContext.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (feat is null) return;

            using var lease = feat.Lease();
            var canvas = lease.SkCanvas;

            canvas.Save();
            canvas.Scale((float)_scale, (float)_scale);

            if (_background.HasValue)
            {
                _paint.Color = _background.Value;
                canvas.DrawArc(VisualBounds, 0, 360, false, _paint);
            }

            _paint.Color = _foreground;
            canvas.DrawPath(_path, _paint);

            canvas.Restore();
        }

        public override void OnAnimationFrameUpdate()
        {
            Invalidate();
            Update();

            if (_active && (_indeterminate || _isAnimatingToValue))
                RegisterForNextAnimationFrameUpdate();
        }

        private void Update()
        {
            if (_indeterminate)
            {
                var now = CompositionNow;
                if (!_lastTime.HasValue) _lastTime = now;
                var elapsed = now - _lastTime.Value;
                var seconds = elapsed.TotalSeconds;

                if (seconds > Duration)
                {
                    while (seconds > Duration) seconds -= Duration;
                    _lastTime = now - TimeSpan.FromSeconds(seconds);
                }

                var progress = (float)(seconds / Duration);

                float size;
                if (progress < 0.25)
                    size = 180 * (progress / 0.25f);
                else if (progress >= 0.75)
                    size = 180 * ((1 - progress) / 0.25f);
                else
                    size = 180;

                var size2 = size / 2;
                var position = 1080 * progress;

                _path.Reset();
                _path.MoveTo(40, 10);
                _path.AddArc(VisualBounds, -90 + (position - size2), size);
            }
            else if (_isAnimatingToValue)
            {
                var now = CompositionNow;
                if (!_lastTime.HasValue) _lastTime = now;
                var elapsed = now - _lastTime.Value;
                var seconds = elapsed.TotalSeconds;

                var progress = (float)(seconds / Duration);
                if (progress >= 1)
                {
                    _isAnimatingToValue = false;
                    _lastTime = null;
                    progress = 1;
                }

                var dV = _value - _lastValue;
                var currentValue = _lastValue + (dV * progress);
                _path.Reset();
                _path.MoveTo(40, 10);
                _path.AddArc(VisualBounds, -90, 360 * (currentValue - _min) / (_max - _min));
            }
            else
            {
                _path.Reset();
                _path.MoveTo(40, 10);
                _path.AddArc(VisualBounds, -90, 360 * (_value - _min) / (_max - _min));
            }
        }

        public override void OnMessage(object message)
        {
            if (message is not HandlerMessage hm) return;

            switch (hm.Type)
            {
                case MessageType.Min:
                    _min = (float)hm.Data!;
                    break;

                case MessageType.Max:
                    _max = (float)hm.Data!;
                    break;

                case MessageType.Value:
                {
                    var next = (float)hm.Data!;
                    _lastValue = _value;
                    if (next <= _value)
                    {
                        _value = next;
                        _isAnimatingToValue = false;
                    }
                    else
                    {
                        _value = next;
                        _isAnimatingToValue = true;
                        RegisterForNextAnimationFrameUpdate();
                        return;
                    }
                    break;
                }

                case MessageType.Active:
                    _active = (bool)hm.Data!;
                    if (_active && _indeterminate)
                    {
                        RegisterForNextAnimationFrameUpdate();
                        return;
                    }
                    _lastTime = null;
                    break;

                case MessageType.Indeterminate:
                    _indeterminate = (bool)hm.Data!;
                    if (_indeterminate && _active)
                    {
                        RegisterForNextAnimationFrameUpdate();
                        return;
                    }
                    _lastTime = null;
                    break;

                case MessageType.Background:
                    _background = hm.Data is SKColor c ? c : null;
                    break;

                case MessageType.Foreground:
                    _foreground = hm.Data is SKColor c2 ? c2 : SKColors.Transparent;
                    break;

                case MessageType.Scale:
                    _scale = (double)hm.Data!;
                    break;
            }

            Update();
            Invalidate();
        }

        private TimeSpan? _lastTime;
        private const float Duration = 2;
        private readonly SKPaint _paint;
        private readonly SKPath _path;
        private static readonly SKRect VisualBounds = new(10, 10, 70, 70);

        private SKColor? _background;
        private SKColor _foreground;
        private float _min, _max, _value;
        private bool _indeterminate;
        private bool _active;
        private bool _isAnimatingToValue;
        private float _lastValue;
        private double _scale = 1;
    }
}
