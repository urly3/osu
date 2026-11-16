// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Input.Bindings;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        [Resolved]
        private MusicController musicController { get; set; } = null!;

        private Box seekBar;

        private Container seekDisplay;

        private bool blockSeek = false;

        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public ToolbarMusicButton()
        {
            Hotkey = GlobalAction.ToggleNowPlaying;
            ButtonContent.AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(true)]
        private void load(NowPlayingOverlay music)
        {
            StateContainer = music;

            Flow.Padding = new MarginPadding { Horizontal = Toolbar.HEIGHT / 4 };
            Flow.Add(seekDisplay = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Width = 3f,
                Height = IconContainer.Height,
                Margin = new MarginPadding { Left = 7.5f, Right = 2.5f },
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.25f),
                    },
                    seekBar = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0f,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Colour = Color4.White,
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();
            var track = musicController.CurrentTrack;
            seekBar.Height = (float)(track.CurrentTime / double.Max(track.Length, 1.0));
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!IsHovered)
                return false;

            switch (e.Key)
            {
                case Key.Up:
                    expandSeekBarTemporarily();
                    adjustSeek(1);
                    return true;

                case Key.Down:
                    expandSeekBarTemporarily();
                    adjustSeek(-1);
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            expandSeekBarTemporarily();
            adjustSeek(double.Sign(e.ScrollDelta.Y));
            return true;
        }

        private void adjustSeek(int sign)
        {
            if (blockSeek)
                return;

            var track = musicController.CurrentTrack;
            musicController.SeekTo(double.Clamp(track.CurrentTime + (5000.0 * sign), 0.0, track.Length));
            blockSeek = true;

            // de-bouncing on it
            Scheduler.AddDelayed(() => blockSeek = false, 33);
        }

        private TransformSequence<Container> expandTransform;
        private ScheduledDelegate contractTransform;

        private void expandSeekBarTemporarily()
        {
            // avoid starting a new transform if one is already active.
            if (expandTransform == null)
            {
                expandTransform = seekDisplay.ResizeWidthTo(6, 500, Easing.OutQuint);
                expandTransform.Finally(_ => expandTransform = null);
            }

            contractTransform?.Cancel();
            contractTransform = Scheduler.AddDelayed(() =>
            {
                seekDisplay.ResizeWidthTo(3f, 500, Easing.OutQuint);
            }, 1000);
        }
    }
}
