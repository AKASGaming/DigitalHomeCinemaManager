

namespace DigitalHomeCinemaControl.Controllers.Providers.HDFury
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum Rx
    {
        [Description("Rx 0")]
        Input1 = 0,
        [Description("Rx 1")]
        Input2 = 1,
        [Description("Rx 2")]
        Input3 = 2,
        [Description("Rx 3")]
        Input4 = 3,
        [Description("Follow Tx0")]
        Follow = 4,
    }

}
