using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityElement;
using UtilityElement.Metadeta;
using UtilityElement.Metadeta.Dimensions;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace TimberElement
{
    /// <summary>
    /// 方剖面类
    /// </summary>
    public class RectangleSectionTimber : TimberSection
    {
        ////输入数据(以及继承的父类属性)
        public SquareProfile squareProfile { get; set; }//方剖面尺寸 
        public new String name
        {
            get { return base.name; }
            set { base.name = value; }
        }//构件名称

        ////输出数据
        public new bool isExist
        {
            get
            {
                base.isExist = false;
                if (base.Linelength > 0 && squareProfile.b > 0 && squareProfile.h > 0) base.isExist = true;
                return base.isExist;
            }
        }//判断存在与否
        public Brep TimberBrep
        {
            get
            {
                if (timberBrep == null)
                {
                    //在XY平面创建方形
                    Plane tmpPlane = Plane.WorldXY;
                    Rectangle3d rectangle = new Rectangle3d(tmpPlane, squareProfile.b, squareProfile.h);
                    // 获取长方形的中心点，平移长方形到中心
                    Point3d center = rectangle.Center;
                    Vector3d translation = new Vector3d(-center.X, -center.Y, 0.0);
                    rectangle.Transform(Transform.Translation(translation));
                    //得到截面
                    Curve[] region = new Curve[] { rectangle.ToNurbsCurve() };
                    //在XY平面挤出！
                    Vector3d translationVector = new Vector3d(0, 0, -squareProfile.expansion0);//0端朝Z轴负方向                    
                    //位移到实际挤出平面(出头的位移)
                    tmpPlane.Translate(translationVector);                    
                    //创建XY轴上的定位基线
                    Curve tmpCurveInfluenced = new Line(tmpPlane.Origin, tmpPlane.ZAxis, extrudeHeight).ToNurbsCurve();
                    List<Curve> tmpCurveInfluencedList = new List<Curve>();
                    tmpCurveInfluencedList.Add(tmpCurveInfluenced);
                    baseCurveInfluenced = BaseCurveUtility.CurvesToBaseCurves(tmpCurveInfluencedList)[0];
                    Curve tmpCurve = new Line(Plane.WorldXY.Origin, Plane.WorldXY.ZAxis, base.Linelength).ToNurbsCurve();
                    List<Curve> tmpCurveList = new List<Curve>();
                    tmpCurveList.Add(tmpCurve);
                    baseCurve = BaseCurveUtility.CurvesToBaseCurves(tmpCurveList)[0];
                    //挤出
                    Extrusion extrusion = Extrusion.Create(region[0], tmpPlane, extrudeHeight, true);
                    timberBrep = extrusion?.ToBrep();

                }
                return timberBrep;
            }//不是外面可以设置的，直接生成的，所以只有get
        }//几何图元        
        public void ResetTimberBrep()
        {
            timberBrep = null;
        }//重置timberBrep值的方法
        double extrudeHeight
        {
            get
            {
                return squareProfile.expansion0 + squareProfile.expansion1 + base.Linelength;
            }
        }//获取实际挤出高
        public new String description
        {
            get
            {
                base.description = string.Format("{0} CIR,H={1},B={2},L={3}",name,squareProfile.h.ToString("F2"), squareProfile.b.ToString("F3"), extrudeHeight.ToString("F2"));
                return base.description;
            }
        }//为父类的描述赋值并输出

        //Construction,构造函数，需输入"圆剖面尺寸""长度"
        public RectangleSectionTimber(SquareProfile squareProfile, Double Linelength,String name)
        {
            this.squareProfile = squareProfile;
            base.Linelength = Linelength;
            base.name = name;
        }
        //空构造函数
        public RectangleSectionTimber()
        {

        }
    }
}
