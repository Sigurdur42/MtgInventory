using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.Dropbox
{
    public class DropboxApiSettings
    {
        public string path { get; set; }

        // public int host { get; set; }
        public bool is_team { get; set; }

        public string subscription_type { get; set; }
    }

    public class RootObject
    {
        public DropboxApiSettings personal { get; set; }
    }
}