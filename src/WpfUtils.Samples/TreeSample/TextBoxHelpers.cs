using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZumtenSoft.WpfUtils.Samples.TreeSample
{
    // Source: http://stackoverflow.com/questions/13251506/wpf-text-box-validation-on-numeric-value
    public class TextBoxHelpers : DependencyObject
    {
        public static bool GetIsNumeric(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsNumericProperty);
        }

        public static void SetIsNumeric(DependencyObject obj, bool value)
        {
            obj.SetValue(IsNumericProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsNumeric.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNumericProperty =
             DependencyProperty.RegisterAttached("IsNumeric", typeof(bool), typeof(TextBoxHelpers), new PropertyMetadata(false, ((s, e) =>
             {
                 TextBox targetTextbox = s as TextBox;
                 if (targetTextbox != null)
                 {
                     if ((bool)e.OldValue && !((bool)e.NewValue))
                     {
                         targetTextbox.PreviewTextInput -= targetTextbox_PreviewTextInput;

                     }
                     if ((bool)e.NewValue)
                     {
                         targetTextbox.PreviewTextInput += targetTextbox_PreviewTextInput;
                         targetTextbox.PreviewKeyDown += targetTextbox_PreviewKeyDown;
                     }
                 }
             })));

        static void targetTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        static void targetTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Char newChar = e.Text[0];
            e.Handled = !Char.IsNumber(newChar);
        }
    }
}
