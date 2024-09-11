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
        public TimeSpan ProcessTime { get; set; }
        public string ProcessTimeToString { get => ProcessTime.ToString(@"hh\:mm"); }
        public string ProcessCommand { get; set; } = string.Empty;
        public int ProcessData { get; set; }

        public ModelProcessItem()
        {
        }

        public ModelProcessItem(int porecessId, TimeSpan processTime, string processName, int processData)
        {
            ProcessId = porecessId;
            ProcessTime = processTime;
            ProcessCommand = processName ?? throw new ArgumentNullException(nameof(processName));
            ProcessData = processData;
        }

        public ModelProcessItem(ModelProcessItem item) : this(item.ProcessId, item.ProcessTime, item.ProcessCommand, item.ProcessData)
        {
        }

    }
}
