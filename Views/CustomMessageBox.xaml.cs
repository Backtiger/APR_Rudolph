using System.Windows;

namespace APR_Rudolph.Views
{
    /// <summary>
    /// 프로그램 테마에 맞는 커스텀 메시지박스 창. 메시지 표시, 확인/닫기 버튼, 드래그 이동 지원.
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public static void Show(string message, Window? owner = null)
        {
            var box = new CustomMessageBox(message);
            if (owner == null && Application.Current.MainWindow != box)
                owner = Application.Current.MainWindow;
            if (owner != null) box.Owner = owner;
            box.ShowDialog();
        }
    }
} 