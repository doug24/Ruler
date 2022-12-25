namespace Ruler
{
    public class ScaleTicks
    {
        public ScaleTicks(double step, double small, double medium, double mediumLarge, double large, double label)
        {
            Step = step;
            SmallTick = small;
            MediumTick = medium;
            MediumLargeTick = mediumLarge;
            LargeTick = large;
            LabelTick = label;
        }

        public double Step { get; private set; }
        public double SmallTick { get; private set; }
        public double MediumTick { get; private set; }
        public double MediumLargeTick { get; private set; }
        public double LargeTick { get; private set; }
        public double LabelTick { get; private set; }

    }
}
