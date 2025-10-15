using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixTotals
{
    class NNList
    {

            public Guid targetid { get; set; }
            public Guid relatedid { get; set; }
            public string subject { get; set; }
            public int email { get; set; }
            public int response { get; set; }
            public int priority { get; set; }
            public int channel { get; set; }
            public int objectcode { get; set; }
            public DateTime recivedOn { get; set; }
    }
}
