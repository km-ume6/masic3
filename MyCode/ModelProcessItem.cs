using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace masic3.MyCode
{
    public class ModelProcessItem
    {
        public int ProcessId { get; set; }
        public string ProcessCommand { get; set; } = string.Empty;
        public string ProcessParam { get; set; } = string.Empty;
        public TimeSpan ProcessTime { get; set; }
        public string ProcessTimeToString { get => ProcessTime.ToString(@"hh\:mm"); }

        public ModelProcessItem()
        {
        }

        public ModelProcessItem(int porecessId, string processName, string processParam, TimeSpan processTime)
        {
            ProcessId = porecessId;
            ProcessCommand = processName ?? throw new ArgumentNullException(nameof(processName));
            ProcessParam = processParam;
            ProcessTime = processTime;
        }

        public ModelProcessItem(ModelProcessItem item) : this(item.ProcessId, item.ProcessCommand, item.ProcessParam, item.ProcessTime)
        {
        }
    }
}
