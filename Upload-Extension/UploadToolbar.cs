using System.ComponentModel.Design;

namespace Trik.Upload_Extension
{
    internal class UploadToolbar
    {
        public MenuCommand Connect { get; set; }
        public MenuCommand Reconnect { get; set; }
        public MenuCommand Disconnect { get; set; }
        public MenuCommand Upload { get; set; }
        public MenuCommand RunProgram { get; set; }
        public MenuCommand StopProgram { get; set; }

    }
}