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
    namespace Metadeta//元数据
    {
        //输入元数据
        namespace Dimensions//尺寸
        {
            interface IDimension { }
            //剖面尺寸
            public abstract class ProfileDimension : IDimension
            {
                // 共有的字段：出头 
                public double expansion0 { get; set; }
                public double expansion1 { get; set; }
                public ProfileDimension(double expansion0, double expansion1)
                {
                    this.expansion0 = expansion0;
                    this.expansion1 = expansion1;
                }
            }

            //圆剖面尺寸
            public class CircularProfile : ProfileDimension
            {
                public double radius { get; set; }//圆半径
                public double jinPan { get; set; }//金盘一侧的占总高的比例
                public double height
                {
                    get
                    {                        
                        return 2 * radius * (1 - 2*jinPan);
                    }
                }//剖面高度
                public CircularProfile(double expansion0, double expansion1, double radius, double jinPan) : base(expansion0, expansion1)
                {
                    this.expansion0 = expansion0;
                    this.expansion1 = expansion1;
                    this.radius = radius;
                    this.jinPan = jinPan;
                }

            }

            //方剖面尺寸
            public class SquareProfile : ProfileDimension
            {
                public double h { get; set; }
                public double b { get; set; }
                public SquareProfile(double expansion0, double expansion1, double h, double b) : base(expansion0, expansion1)
                {
                    this.expansion0 = expansion0;
                    this.expansion1 = expansion1;
                    this.h = h;
                    this.b = b;
                }
            }

            //截面尺寸
            public class SectionDimension: IDimension
            {
                Curve curve { get; set; }//RH拾取的截面
                public SectionDimension(Curve curve)
                {
                    this.curve = curve;
                }
            }

            //面类尺寸
            public class FaceDimension: IDimension
            {
                double d { get; set; }//面厚度
                public FaceDimension(double d)
                {
                    this.d = d;
                }
            }

            public class RevolveDimension: IDimension
            {
                Curve Curve { get; set; }//旋转面
                double ZhuJing { get; set; }
                public RevolveDimension(Curve curve,double zhuJing)
                {
                    this.Curve = curve;
                    this.ZhuJing = zhuJing;
                }
            }
        }       
    }
}
