using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PortLog.Components
{
    public partial class ConfirmDialog : UserControl
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        // ========= Dependency Properties ===========

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(ConfirmDialog),
                new PropertyMetadata(false));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ConfirmDialog),
                new PropertyMetadata("Confirm"));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(ConfirmDialog),
                new PropertyMetadata("Are you sure?"));

        public ICommand ConfirmCommand
        {
            get => (ICommand)GetValue(ConfirmCommandProperty);
            set => SetValue(ConfirmCommandProperty, value);
        }

        public static readonly DependencyProperty ConfirmCommandProperty =
            DependencyProperty.Register(
                nameof(ConfirmCommand),
                typeof(ICommand),
                typeof(ConfirmDialog));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
                nameof(CancelCommand),
                typeof(ICommand),
                typeof(ConfirmDialog));
    }
}
