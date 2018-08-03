using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workshop
{
    class Package
    {
        public ulong publishFileID =0;
        public string title = "null", description="null";
        public List<string> tags =new List<string>();
        public string previewUrl="/", contentUrl="/";
    }
}
