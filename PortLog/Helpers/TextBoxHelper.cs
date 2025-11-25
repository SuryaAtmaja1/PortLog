using System.Windows;
using System.Windows.Controls;

namespace PortLog.Helpers
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.RegisterAttached(
                "HasText",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(false));

        public static bool GetHasText(DependencyObject obj)
            => (bool)obj.GetValue(HasTextProperty);

        public static void SetHasText(DependencyObject obj, bool value)
            => obj.SetValue(HasTextProperty, value);


        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new PropertyMetadata(string.Empty));

        public static string GetPlaceholder(DependencyObject obj)
            => (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value)
            => obj.SetValue(PlaceholderProperty, value);


        // automatically check TextBox content
        static TextBoxHelper()
        {
            EventManager.RegisterClassHandler(
                typeof(TextBox),
                TextBox.TextChangedEvent,
                new TextChangedEventHandler(OnTextChanged));
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            SetHasText(tb, !string.IsNullOrEmpty(tb.Text));
        }
    }
}
