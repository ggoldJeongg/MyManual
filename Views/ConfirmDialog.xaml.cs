using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyManual.Views
{
    public partial class ConfirmDialog : Window
    {
        public enum DialogType
        {
            Warning,    // 경고 (주황색)
            Danger,     // 위험/삭제 (빨간색)
            Info,       // 정보 (파란색)
            Success     // 성공 (초록색)
        }

        public bool Result { get; private set; }

        public ConfirmDialog()
        {
            InitializeComponent();
            Result = false;
        }

        public static bool Show(
            string message,
            string title = "확인",
            string confirmText = "확인",
            string cancelText = "취소",
            DialogType type = DialogType.Warning,
            Window? owner = null)
        {
            var dialog = new ConfirmDialog();
            dialog.SetContent(message, title, confirmText, cancelText, type);

            if (owner != null)
            {
                dialog.Owner = owner;
            }

            dialog.ShowDialog();
            return dialog.Result;
        }

        private void SetContent(string message, string title, string confirmText, string cancelText, DialogType type)
        {
            TitleText.Text = title;
            MessageText.Text = message;
            ConfirmButton.Content = confirmText;
            CancelButton.Content = cancelText;

            // 타입별 스타일 설정
            switch (type)
            {
                case DialogType.Danger:
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                    IconText.Text = "!";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
                    SetConfirmButtonColor("#E53935", "#C62828");
                    break;

                case DialogType.Warning:
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                    IconText.Text = "!";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                    SetConfirmButtonColor("#FF9800", "#F57C00");
                    break;

                case DialogType.Info:
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                    IconText.Text = "i";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
                    SetConfirmButtonColor("#1976D2", "#1565C0");
                    break;

                case DialogType.Success:
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                    IconText.Text = "✓";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43A047"));
                    SetConfirmButtonColor("#43A047", "#388E3C");
                    break;
            }
        }

        private void SetConfirmButtonColor(string normalColor, string hoverColor)
        {
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString(normalColor))));
            style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(FontSizeProperty, 14.0));
            style.Setters.Add(new Setter(FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Border));
            borderFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
            borderFactory.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));
            borderFactory.SetValue(System.Windows.Controls.Border.PaddingProperty, new TemplateBindingExtension(PaddingProperty));

            var contentPresenterFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.ContentPresenter));
            contentPresenterFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentPresenterFactory);

            template.VisualTree = borderFactory;
            style.Setters.Add(new Setter(TemplateProperty, template));

            var trigger = new Trigger { Property = IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString(hoverColor))));
            style.Triggers.Add(trigger);

            ConfirmButton.Style = style;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
