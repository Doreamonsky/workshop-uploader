using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workshop
{   
    [Serializable]
    public class Package
    {
        public ulong publishFileID =0;
        public string title = "null", description="null";
        public string tags = "Skin";
        public string previewUrl="/", contentUrl="/";
    }
}
