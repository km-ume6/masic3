using masic3.MyCode;
using System.Diagnostics;

namespace masic3
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            // Window位置を復元
            window.X = (double)Preferences.Get("Window.X", window.X);
            window.Y = (double)Preferences.Get("Window.Y", window.Y);

            // Windowサイズを設定
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.MinimumWidth = window.Width = displayInfo.Width / 3;
            window.MinimumHeight = window.Height = displayInfo.Height / 2;

            // Window破棄イベントを追加
            window.Destroying += (s, e) =>
            {
                // Window位置を保存
                Preferences.Set("Window.X", window.X);
                Preferences.Set("Window.Y", window.Y);

                // ワーカスレッド終了シグナル
                masic3.MyCode.ModelShare.EnableThreadLoop = false;
                while (masic3.MyCode.ModelShare.Working == true)
                {
                    Debug.WriteLine("スレッド終了待ち！");
                    Thread.Sleep(1000);
                }
            };

            return window;
        }
    }
}
