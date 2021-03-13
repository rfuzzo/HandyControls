using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HandyControl.Controls
{
    public partial class FeatherPresenter : Canvas
    {
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
                    "SelectionChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FeatherPresenter));

        public static DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(FeatherPresenter), new FrameworkPropertyMetadata(default(FontFamily), FrameworkPropertyMetadataOptions.Inherits));

        public static DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(FeatherPresenter), new FrameworkPropertyMetadata(System.Windows.FontWeights.Normal, FrameworkPropertyMetadataOptions.Inherits));

        public static DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(FeatherPresenter), new PropertyMetadata(default(Thickness)));

        //set item's title shown location.
        public static DependencyProperty PopupDeviationProperty = DependencyProperty.Register("PopupDeviation", typeof(Point), typeof(FeatherPresenter), new PropertyMetadata(default(Point)));

        public static DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(FeatherPresenter), new PropertyMetadata(-1));

        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(FeatherPresenterItem), typeof(FeatherPresenter), new PropertyMetadata(null));

        //set item space value
        public static DependencyProperty SpaceProperty = DependencyProperty.Register("Space", typeof(int), typeof(FeatherPresenter), new PropertyMetadata(0, new PropertyChangedCallback((o, e) =>
        {
            (o as FeatherPresenter).InvalidateArrange();
        })
            )
            );

        //animation path funtion.
        private Gauss gauss = new Gauss();

        static FeatherPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FeatherPresenter), new FrameworkPropertyMetadata(typeof(FeatherPresenter)));
        }

        public FeatherPresenter()
        {
            //set functin args
            gauss.Sigma = 128;
            gauss.Swing = 72;
            gauss.Offset = 88;

            //binding event handle form location mouse point.
            this.MouseMove += (o, e) =>
            {
                this.Transform(e.GetPosition(this));
            };

            this.MouseLeave += (o, e) =>
            {
                this.Reset();
            };
        }

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily) GetValue(FontFamilyProperty);
            }
            set
            {
                SetValue(FontFamilyProperty, value);
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight) GetValue(FontWeightProperty);
            }
            set
            {
                SetValue(FontWeightProperty, value);
            }
        }

        public Thickness Padding
        {
            get
            {
                return (Thickness) GetValue(PaddingProperty);
            }
            set
            {
                SetValue(PaddingProperty, value);
            }
        }

        public Point PopupDeviation
        {
            get
            {
                return (Point) GetValue(PopupDeviationProperty);
            }
            set
            {
                SetValue(PopupDeviationProperty, value);
            }
        }

        public int SelectedIndex
        {
            get
            {
                return (int) GetValue(SelectedIndexProperty);
            }
            set
            {
                if (this.SelectedIndex != value)
                {
                    SetValue(SelectedIndexProperty, value);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SelectNewIndex(value);
                    }));
                }
            }
        }

        public FeatherPresenterItem SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty) as FeatherPresenterItem;
            }
            set
            {
                SetValue(SelectedItemProperty, value);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SelectNewItem(value);
                }));
            }
        }

        public int Space
        {
            get
            {
                return Convert.ToInt32(GetValue(SpaceProperty));
            }
            set
            {
                SetValue(SpaceProperty, value);
            }
        }

        public bool CanMoveLeft()
        {
            if (this.SelectedIndex >= 0 + 1)
            {
                return true;
            }
            return false;
        }

        public bool CanMoveRight()
        {
            if (this.SelectedIndex <= this.Children.Count - 1 - 1)
            {
                return true;
            }
            return false;
        }

        public void MoveLeft()
        {
            this.SelectedIndex--;
        }

        public void MoveRight()
        {
            this.SelectedIndex++;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            double needsWidth = 0;
            foreach (FeatherPresenterItem item in this.Children)
            {
                if (needsWidth <= 0)
                    needsWidth += item.DesiredSize.Width;
                else
                    needsWidth += this.Space + item.DesiredSize.Width;
            }

            double left = (this.DesiredSize.Width - needsWidth) / 2;

            foreach (FeatherPresenterItem item in this.Children)
            {
                Point topLeft = new Point(left, this.DesiredSize.Height - item.DesiredSize.Height + Padding.Top - Padding.Bottom);
                item.Original = new Point(topLeft.X + item.DesiredSize.Width / 2, topLeft.Y - item.DesiredSize.Height / 2);
                item.Arrange(new Rect(topLeft, item.DesiredSize));

                left += this.Space + item.DesiredSize.Width;
                //regist click event handle
                item.Click += Item_Click;
            }
            return arrangeBounds; // base.ArrangeOverride(arrangeBounds);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            foreach (FrameworkElement item in this.Children)
            {
                item.Measure(constraint);
            }
            return constraint; // base.MeasureOverride(constraint);
        }

        protected virtual void OnSelectionChanged()
        {
            RoutedEventArgs args = new RoutedEventArgs(SelectionChangedEvent, this);
            RaiseEvent(args);
        }
    }

    public partial class FeatherPresenter
    {
        private bool wasMouseOver = false;

        public void Reset()
        {
            double duration = 0;
            if (wasMouseOver != this.IsMouseOver)
                duration = 200d;
            foreach (FeatherPresenterItem item in this.Children)
            {
                item.Dispatcher.BeginInvoke(new Action(() =>
                {
                    item.AnimateTo(0, 0, 1, 1, 0, duration);
                }));
            }

            wasMouseOver = this.IsMouseOver;
        }

        public void Transform(Point p)
        {
            double duration = 0;
            if (wasMouseOver != this.IsMouseOver)
                duration = 100d;

            this.gauss.Phase = p.X;
            double ps2Max = this.gauss.IntegrateGauss(this.gauss.Phase, int.MaxValue);

            foreach (FeatherPresenterItem item in this.Children)
            {
                item.Dispatcher.BeginInvoke(new Action(() =>
                {
                    double x = this.gauss.IntegrateGauss(this.gauss.Phase, item.Original.X);
                    double nx = (x / ps2Max) * this.gauss.Offset;

                    double s = (1 - Math.Abs(this.gauss.IntegrateGauss(this.gauss.Phase, item.Original.X) / ps2Max)) * 1 + 1;

                    item.AnimateTo(nx, 0, s, s, 0, duration);
                }));
            }

            wasMouseOver = this.IsMouseOver;
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            FeatherPresenterItem newSelected = sender as FeatherPresenterItem;
            SelectNewItem(newSelected);
        }

        private void SelectNewIndex(int index)
        {
            if (index >= this.Children.Count || index < 0)
            {
                return;
            }

            FeatherPresenterItem item = this.Children[index] as FeatherPresenterItem;
            SelectNewItem(item);
        }

        private void SelectNewItem(FeatherPresenterItem item)
        {
            if (item == this.SelectedItem || item == null)
            {
                return;
            }

            if (this.Children.Contains(item))
            {
                if (this.SelectedItem != null)
                {
                    this.SelectedItem.IsSelected = false;
                }
                item.IsSelected = true;

                this.SelectedItem = item;
                this.SelectedIndex = this.Children.IndexOf(item);
                OnSelectionChanged();
            }
        }
    }
    public class Gauss : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double sigma = 0d;
        public double Sigma
        {
            get
            {
                return this.sigma;
            }
            set
            {
                this.sigma = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Sigma"));
                }
            }
        }
        private double phase = 0d;
        public double Phase
        {
            get
            {
                return this.phase;
            }
            set
            {
                this.phase = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Phase"));
                }
            }
        }
        private double swing = 0d;
        public double Swing
        {
            get
            {
                return this.swing;
            }
            set
            {
                this.swing = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Swing"));
                }
            }
        }
        private double offset = 0d;
        public double Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                this.offset = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Offset"));
                }
            }
        }

        public double GaussFunction(double x)
        {
            double y = this.Swing * Math.Exp(-(Math.Pow((x - this.Phase), 2)) / (2 * Math.Pow(this.Sigma, 2))) + this.Offset;
            return y;
        }

        public double IntegrateGauss(double intervalBegin, double intervalEnd)
        {
            double integrateValue = MathNet.Numerics.Integrate.OnClosedInterval(GaussFunctionDelegate, intervalBegin, intervalEnd);
            return integrateValue;
        }

        private double GaussFunctionDelegate(double x)
        {
            double y = this.Swing * Math.Exp(-(Math.Pow((x - this.Phase), 2)) / (2 * Math.Pow(this.Sigma, 2)));
            return y;
        }
    }
}
