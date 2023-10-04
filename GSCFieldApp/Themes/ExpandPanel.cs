using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235
// This expandPanel was taken from this tutorial: https://comentsys.wordpress.com/2015/05/30/windows-10-universal-windows-platform-expand-control/ - Gab

namespace GSCFieldApp.Themes
{
    public sealed class ExpandPanel : ContentControl
    {
        public delegate void IsExpandedChangedHandler();
        public event IsExpandedChangedHandler ExpandedChanged;

        public ExpandPanel()
        {
            this.DefaultStyleKey = typeof(ExpandPanel);

        }


        private readonly bool _useTransitions = true;
        public static VisualState _collapsedState;
        public static Windows.UI.Xaml.Controls.Primitives.ToggleButton toggleExpander;
        public static Windows.UI.Xaml.Controls.RelativePanel relMainPanel;
        public static Windows.UI.Xaml.Controls.ContentPresenter headerToolContent;
        public bool headerToolContentTapped = false;

        private FrameworkElement contentElement;

        public static readonly DependencyProperty HeaderBorderBrushProperty = DependencyProperty.Register("HeaderBorderBrush", typeof(Windows.UI.Xaml.Media.Brush), typeof(ExpandPanel), null);

        public static readonly DependencyProperty HeaderContentProperty =
        DependencyProperty.Register("HeaderContent", typeof(object),
        typeof(ExpandPanel), null);

        public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register("IsExpanded", typeof(bool),
        typeof(ExpandPanel), new PropertyMetadata(false));

        //I saw the warning so I added new.  Jamel
        public new static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register("CornerRadius", typeof(CornerRadius),
        typeof(ExpandPanel), null);

        public static readonly DependencyProperty ToolContentProperty = DependencyProperty.Register("ToolContent", typeof(object), typeof(ExpandPanel), null);

        public object HeaderContent
        {
            get { return GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        public object ToolContent
        {
            get { return GetValue(ToolContentProperty); }
            set { SetValue(ToolContentProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set
            {

                SetValue(IsExpandedProperty, value);
                if (ExpandedChanged != null)
                {
                    ExpandedChanged();
                    setExpandState(value);

                }

            }
        }

        //I saw the warning so I added new.  Jamel
        public new CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Brush HeaderBorderBrush
        {
            get { return (Brush)GetValue(HeaderBorderBrushProperty); }
            set { SetValue(HeaderBorderBrushProperty, value); }
        }

        private void changeVisualState(bool useTransitions)
        {
            if (IsExpanded)
            {
                if (contentElement != null)
                {
                    contentElement.Visibility = Visibility.Visible;
                }
                VisualStateManager.GoToState(this, "Expanded", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Collapsed", useTransitions);
                _collapsedState = (VisualState)GetTemplateChild("Collapsed");
                if (_collapsedState == null)
                {
                    if (contentElement != null)
                    {
                        contentElement.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Single click/tap on header
            relMainPanel = (Windows.UI.Xaml.Controls.RelativePanel)GetTemplateChild("HeaderPanel");
            if (relMainPanel != null)
            {
                relMainPanel.Tapped += (object sender, TappedRoutedEventArgs e) =>
                {
                    if (!headerToolContentTapped)
                    {
                        IsExpanded = !IsExpanded;
                        setExpandState(IsExpanded);
                    }
                    else
                    {
                        headerToolContentTapped = !headerToolContentTapped;
                    }

                };
            }

            //Single click/tap on header
            headerToolContent = (Windows.UI.Xaml.Controls.ContentPresenter)GetTemplateChild("HeaderToolContent");
            if (headerToolContent != null)
            {
                headerToolContent.Tapped += (object sender, TappedRoutedEventArgs e) =>
                {
                    headerToolContentTapped = true;
                };
            }

            //Toggle button
            toggleExpander = (Windows.UI.Xaml.Controls.Primitives.ToggleButton)GetTemplateChild("ExpandCollapseButton");
            if (toggleExpander != null)
            {
                toggleExpander.Click += (object sender, RoutedEventArgs e) =>
                {
                    IsExpanded = !IsExpanded;
                    setExpandState(IsExpanded);
                    //IsExpanded = !IsExpanded;
                    //toggleExpander.IsChecked = IsExpanded;
                    //changeVisualState(_useTransitions);
                };
            }

            //Fill with content
            contentElement = (FrameworkElement)GetTemplateChild("Content");
            if (contentElement != null)
            {
                _collapsedState = (VisualState)GetTemplateChild("Collapsed");
                if ((_collapsedState != null) && (_collapsedState.Storyboard != null))
                {
                    _collapsedState.Storyboard.Completed += (object sender, object e) =>
                    {
                        contentElement.Visibility = Visibility.Collapsed;
                    };
                }
            }
            changeVisualState(false);
        }
        public void setExpandState(bool state)
        {
            if (toggleExpander != null)
            {
                toggleExpander.IsChecked = state;
                changeVisualState(_useTransitions);
            }


        }

    }
}
