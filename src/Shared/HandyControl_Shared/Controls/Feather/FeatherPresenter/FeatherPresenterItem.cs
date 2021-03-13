using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HandyControl.Controls
{
    [TemplatePart(Name = FeatherPresenterItem.ElementRoot, Type = typeof(FrameworkElement))]
    public class FeatherPresenterItem : ContentControl, IAnimation
    {
        private const string ElementRoot = "scalepresenter";

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FeatherPresenterItem));

        public new static readonly DependencyProperty FontFamilyProperty;

        public new static readonly DependencyProperty FontWeightProperty;



        public static readonly DependencyProperty IsSelectedProperty =
      DependencyProperty.Register("IsSelected", typeof(bool), typeof(FeatherPresenterItem), new UIPropertyMetadata(false, new PropertyChangedCallback((o, e) =>
      {
      }))
      );

        //set item space value
        public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(FeatherPresenterItem), new PropertyMetadata("123"));

        private bool isDownOnSurface = false;

        private FrameworkElement scalepresenter = null;

        static FeatherPresenterItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FeatherPresenterItem), new FrameworkPropertyMetadata(typeof(FeatherPresenterItem)));

            FontFamilyProperty = FeatherPresenter.FontFamilyProperty.AddOwner(typeof(FeatherPresenterItem));

            FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata();
            meta.Inherits = true;

            FontFamilyProperty.OverrideMetadata(typeof(FeatherPresenterItem), meta);

            FontWeightProperty = FeatherPresenter.FontWeightProperty.AddOwner(typeof(FeatherPresenterItem));

            FrameworkPropertyMetadata metaW = new FrameworkPropertyMetadata();
            metaW.Inherits = true;

            FontWeightProperty.OverrideMetadata(typeof(FeatherPresenterItem), metaW);
        }

        public FeatherPresenterItem()
        {
            RenderTransformOrigin = new Point(.5, 1);
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(new TranslateTransform());
            tg.Children.Add(new ScaleTransform());
            tg.Children.Add(new RotateTransform());

            RenderTransform = tg;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();



        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public bool IsSelected
        {
            get
            {
                return Convert.ToBoolean(GetValue(IsSelectedProperty));
            }
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetValue(IsSelectedProperty, value);
                    ChangeVisualState(true);
                }));
            }
        }

        public string Title
        {
            get
            {
                return GetValue(TitleProperty).ToString();
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        internal Point Original
        {
            get;
            set;
        }

        private FrameworkElement ScalePresenter
        {
            get
            {
                if (this.scalepresenter == null)
                {
                    ControlTemplate template = this.Template;
                    this.scalepresenter = (FrameworkElement) template.FindName("scalePresenter", this);

                    this.scalepresenter.RenderTransformOrigin = new Point(.5, 1);
                    TransformGroup tgS = new TransformGroup();
                    tgS.Children.Add(new TranslateTransform());
                    tgS.Children.Add(new ScaleTransform());
                    tgS.Children.Add(new RotateTransform());

                    this.scalepresenter.RenderTransform = tgS;
                }
                return this.scalepresenter;
            }
        }

        public void AnimateTo(double x, double y, double sx, double sy, double r, double duration)
        {
            TransformGroup group = (TransformGroup) RenderTransform;
            TranslateTransform trans = (TranslateTransform) group.Children[0];
            //ScaleTransform scale = (ScaleTransform)group.Children[1];
            //RotateTransform rot = (RotateTransform)group.Children[2];

            TransformGroup groupS = (TransformGroup) ScalePresenter.RenderTransform;
            //TranslateTransform trans = (TranslateTransform)groupS.Children[0];
            ScaleTransform scale = (ScaleTransform) groupS.Children[1];
            RotateTransform rot = (RotateTransform) groupS.Children[2];

            if (duration == 0)
            {
                trans.BeginAnimation(TranslateTransform.XProperty, null);
                trans.BeginAnimation(TranslateTransform.YProperty, null);
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                rot.BeginAnimation(RotateTransform.AngleProperty, null);
                trans.X = x;
                trans.Y = y;
                scale.ScaleX = sx;
                scale.ScaleY = sy;
                rot.Angle = r;
                AnimationCompleted(null, null);
            }
            else
            {
                trans.BeginAnimation(TranslateTransform.XProperty, MakeAnimation(x, duration, AnimationCompleted));
                trans.BeginAnimation(TranslateTransform.YProperty, MakeAnimation(y, duration));
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, MakeAnimation(sx, duration));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, MakeAnimation(sy, duration));
                rot.BeginAnimation(RotateTransform.AngleProperty, MakeAnimation(r, duration));
            }
        }

        public void AnimationCompleted(object sender, EventArgs e)
        {
        }

        public DoubleAnimation MakeAnimation(double to, double duration)
        {
            return MakeAnimation(to, duration, null);
        }

        public DoubleAnimation MakeAnimation(double to, double duration, EventHandler endEvent)
        {
            DoubleAnimation anim = new DoubleAnimation(to, TimeSpan.FromMilliseconds(duration));
            anim.AccelerationRatio = 0.2;
            anim.DecelerationRatio = 0.7;
            if (endEvent != null)
                anim.Completed += endEvent;
            return anim;
        }

        internal void ChangeVisualState(bool useTransitions)
        {
            if (IsSelected)
            {
                VisualStateManager.GoToState(this, "Selected", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "UnSelected", useTransitions);
            }
        }

        protected virtual void OnClick()
        {
            RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(args);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            isDownOnSurface = true;
            CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();

            //for Click Event
            if (isDownOnSurface)
            {
                if (IsMouseOver)
                {
                    OnClick();
                }
            }
            isDownOnSurface = false;
        }
    }
    public interface IAnimation
    {
        void AnimateTo(double x, double y, double sx, double sy, double r, double duration);

        void AnimationCompleted(object sender, EventArgs e);

        DoubleAnimation MakeAnimation(double to, double duration);

        DoubleAnimation MakeAnimation(double to, double duration, EventHandler endEvent);
    }
}
