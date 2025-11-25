using System.Windows;
using System.Windows.Controls;

namespace PortLog.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty HasPasswordProperty =
            DependencyProperty.RegisterAttached(
                "HasPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static bool GetHasPassword(DependencyObject obj)
            => (bool)obj.GetValue(HasPasswordProperty);

        public static void SetHasPassword(DependencyObject obj, bool value)
            => obj.SetValue(HasPasswordProperty, value);


        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty));

        public static string GetPlaceholder(DependencyObject obj)
            => (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value)
            => obj.SetValue(PlaceholderProperty, value);


        // Hook PasswordChanged automatically
        static PasswordBoxHelper()
        {
            EventManager.RegisterClassHandler(
                typeof(PasswordBox),
                PasswordBox.PasswordChangedEvent,
                new RoutedEventHandler(OnPasswordChanged));
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            SetHasPassword(pb, pb.Password.Length > 0);
        }
    }
}
