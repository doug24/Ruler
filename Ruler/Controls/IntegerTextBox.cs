using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ruler
{
    public class IntegerTextBox : Control
    {
        static IntegerTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntegerTextBox), new FrameworkPropertyMetadata(typeof(IntegerTextBox)));
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public readonly static DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(int.MaxValue));



        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public readonly static DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(int.MinValue));


        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetCurrentValue(ValueProperty, value); }
        }
        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(0, (o, e) =>
            {
                IntegerTextBox tb = (IntegerTextBox)o;
                tb.RaiseValueChangedEvent(e);
            }));

        public event EventHandler<DependencyPropertyChangedEventArgs>? ValueChanged;

        internal void RaiseValueChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        public readonly static DependencyProperty StepProperty = DependencyProperty.Register(
            "Step", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(1));


        private RepeatButton? upButton;
        private RepeatButton? downButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            upButton = Template.FindName("PART_UpButton", this) as RepeatButton;
            downButton = Template.FindName("PART_DownButton", this) as RepeatButton;
            if (upButton != null && downButton != null)
            {
                upButton.Click += ButtonUp_Click;
                downButton.Click += ButtonDown_Click;
            }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value += Step;
                if (Value > Maximum)
                    Value = Maximum;
            }
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum)
            {
                Value -= Step;
                if (Value < Minimum)
                    Value = Minimum;
            }
        }
    }
}
