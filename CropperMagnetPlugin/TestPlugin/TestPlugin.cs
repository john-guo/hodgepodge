using System;
using System.Drawing.Imaging;
using Fusion8.Cropper.Extensibility;

namespace TestPlugin
{
    public class Test : DesignablePluginThatUsesFetchOutputStreamAndSave
    {
        public override string Description
        {
            get
            {
                return "Test";
            }
        }

        public override string Extension
        {
            get
            {
                return "bmp";
            }
        }

        protected override ImageFormat Format
        {
            get
            {
                return ImageFormat.Bmp;
            }
        }

    }
}
