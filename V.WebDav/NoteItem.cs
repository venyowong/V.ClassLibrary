using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.WebDav
{
    public class NoteItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<NoteItem> Children { get; set; }
    }
}
