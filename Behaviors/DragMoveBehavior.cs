using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace APR_Rudolph.Behaviors
{
    /// <summary>
    /// 커스텀 타이틀바 등에서 마우스 드래그로 윈도우를 이동할 수 있게 해주는 MVVM 스타일 Behavior.
    /// </summary>
    public class DragMoveBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Window.GetWindow(AssociatedObject);
            if (window != null && e.ButtonState == MouseButtonState.Pressed)
            {
                window.DragMove();
            }
        }
    }
} 