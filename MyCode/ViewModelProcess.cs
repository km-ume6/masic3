using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace masic3.MyCode
{
    internal partial class ViewModelProcess : ObservableObject
    {
        [ObservableProperty] public string _myMessage = string.Empty;
        public ViewModelProcess()
        {
            ThreadPool.QueueUserWorkItem(ProcessThread);
        }

        private void ProcessThread(object? state)
        {
            Debug.WriteLine("スレッド開始！");
            masic3.MyCode.ModelShare.Working = true;

            do
            {
                MyMessage = $"Wellcom at {DateTime.Now}";

                Thread.Sleep(1000);
            } while (masic3.MyCode.ModelShare.EnableThreadLoop == true);

            masic3.MyCode.ModelShare.Working = false;
            Debug.WriteLine("スレッド終了！");
        }
    }
}
