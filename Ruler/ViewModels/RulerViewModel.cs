using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ruler
{
    public partial class RulerViewModel : ObservableObject
    {
        public RulerViewModel()
        {
        }

        [ObservableProperty]
        private Point origin = new();

        [ObservableProperty]
        private Point mousePoint = new();

        [ObservableProperty]
        private Point trackPoint = new();

        [ObservableProperty]
        private RulerStyle rulerStyle = RulerStyle.Horizontal;

        [ObservableProperty]
        private Thickness resizeBorder = new(4, 0, 4, 0);

        [ObservableProperty]
        private Units scaleUnits = Units.DIP;

        [ObservableProperty]
        private ZeroPoint zeroPoint = ZeroPoint.Near;

        [ObservableProperty]
        private double longAxis = 800;

        [ObservableProperty]
        private double shortAxis = 80;

        [ObservableProperty]
        private double width = 800;

        [ObservableProperty]
        private double height = 80;

        [ObservableProperty]
        private bool flip = false;

        [ObservableProperty]
        private Edge activeEdge = Edge.Top;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RulerStyle))
            {
                if (RulerStyle == RulerStyle.Horizontal)
                {
                    ResizeBorder = new Thickness(4, 0, 4, 0);
                    ActiveEdge = Flip ? Edge.Bottom : Edge.Top;
                    Width = LongAxis;
                    Height = ShortAxis;
                }
                else if (RulerStyle == RulerStyle.Vertical)
                {
                    ResizeBorder = new Thickness(0, 4, 0, 4);
                    ActiveEdge = Flip ? Edge.Left : Edge.Right;
                    Width = ShortAxis;
                    Height = LongAxis;
                }
            }
            else if (e.PropertyName == nameof(Flip))
            {
                if (RulerStyle == RulerStyle.Horizontal)
                {
                    ActiveEdge = Flip ? Edge.Bottom : Edge.Top;
                }
                else if (RulerStyle == RulerStyle.Vertical)
                {
                    ActiveEdge = Flip ? Edge.Left : Edge.Right;
                }
            }
            else if (e.PropertyName == nameof(Width))
            {
                if (RulerStyle == RulerStyle.Horizontal)
                {
                    LongAxis = Width;
                }
                else if (RulerStyle == RulerStyle.Vertical)
                {
                    ShortAxis = Width;
                }
            }
            else if (e.PropertyName == nameof(Height))
            {
                if (RulerStyle == RulerStyle.Horizontal)
                {
                    ShortAxis = Height;
                }
                else if (RulerStyle == RulerStyle.Vertical)
                {
                    LongAxis = Height;
                }
            }

            base.OnPropertyChanged(e);
        }
    }
}
