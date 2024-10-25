using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Display;
using System.Runtime.Serialization;
using static Rhino.UI.Controls.RenderContentMenu;
using Rhino;
using YhmAssembly;

namespace UtilityElement //辅助类
{
    namespace ReferenceStandard//定位标准
    {
        public class BaseStandard
        {
            double offset { get; set; }//根据正方向的偏移
            string state { get; set; }//上/中/下
            public BaseStandard(double offset, string state)
            {
                this.offset = offset;
                this.state = state;
            }
        }
    }
}
