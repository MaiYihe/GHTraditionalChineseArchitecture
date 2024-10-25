using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityElement.Metadeta.Dimensions;
using UtilityElement.Metadeta;
using UtilityElement.ReferenceDatum;
using UtilityElement;
using YhmAssembly;
using static YhmAssembly.CurveUtility;
using System.Windows.Forms;

namespace TimberElement
{    

    /// <summary>
    /// 圆剖面类
    /// </summary>
    public class CircleSectionTimber : TimberSection
    {
        ////输入数据(以及继承的父类属性)
        public CircularProfile circularProfile { get; set; }//圆剖面尺寸
        public new string name
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
                if (base.Linelength > 0 && circularProfile.height > 0 && circularProfile.radius > 0) base.isExist = true;
                return base.isExist;
            }
        }//判断存在与否
        public Brep TimberBrep
        {
            get
            {
                if (timberBrep == null) 
                {
                    //在XY平面创建圆
                    Circle circle = new Circle(circularProfile.radius);
                    Brep[] tmpBrep = Brep.CreatePlanarBreps(circle.ToNurbsCurve(), Grasshopper.Utility.DocumentTolerance());
                    Curve[] region;
                    if (circularProfile.jinPan > 0)
                    {
                        //sacle1D box
                        BoundingBox box = tmpBrep[0].GetBoundingBox(Plane.WorldXY);
                        box.Transform(Transform.Scale(Plane.WorldXY, 2, 1 - circularProfile.jinPan * 2, 1));
                        //得到box的edge并join
                        List<Curve> curves = box.GetEdges().Select(x => x.ToNurbsCurve()).Cast<Curve>().ToList();
                        Curve[] curvesA = Curve.JoinCurves(curves, Grasshopper.Utility.DocumentTolerance());
                        //region union 得到截面
                        region = Curve.CreateBooleanIntersection(curvesA[0], circle.ToNurbsCurve(), Grasshopper.Utility.DocumentTolerance());
                    }
                    else
                    {
                        List<Curve> curves = new List<Curve>();
                        curves.Add(circle.ToNurbsCurve() as Curve);
                        region = curves.ToArray();
                    }

                    //在XY平面挤出！
                    Vector3d translationVector = new Vector3d(0, 0, -circularProfile.expansion0);//0端朝Z轴负方向
                    Plane tmpPlane = Plane.WorldXY;
                    //位移到实际挤出平面(出头的位移)
                    tmpPlane.Translate(translationVector);                    
                    //创建XY轴上的定位基线
                    Curve tmpCurveInfluenced = new Line(tmpPlane.Origin, tmpPlane.ZAxis, extrudeHeight).ToNurbsCurve();
                    List<Curve> tmpCurveInfluencedList = new List<Curve>();
                    tmpCurveInfluencedList.Add(tmpCurveInfluenced);
                    baseCurveInfluenced = BaseCurveUtility.CurvesToBaseCurves(tmpCurveInfluencedList)[0];
                    Curve tmpCurve = new Line(Plane.WorldXY.Origin, Plane.WorldXY.ZAxis, Linelength).ToNurbsCurve();
                    List<Curve> tmpCurveList = new List<Curve>();
                    tmpCurveList.Add(tmpCurve);
                    baseCurve = BaseCurveUtility.CurvesToBaseCurves(tmpCurveList)[0];
                    //挤出
                    Extrusion extrusion = Extrusion.Create(region[0], tmpPlane, extrudeHeight, true);
                    timberBrep = extrusion?.ToBrep();
                }
                return timberBrep;
            }
        }//几何图元
        public void ResetTimberBrep()
        {
            timberBrep = null;
        }//重置timberBrep值的方法        
        Double extrudeHeight
        { 
            get
            {
                 return circularProfile.expansion0 + circularProfile.expansion1 + Linelength;
            }
        }//获取实际挤出高
        public new String description
        {
            get
            {
                base.description = string.Format("{0} CIR,R={1},jinpan={2},L={3}", name,circularProfile.radius.ToString("F2"), circularProfile.jinPan.ToString("F3"), extrudeHeight.ToString("F2"));
                return base.description; 
            }
        }//为父类的描述赋值并输出

        //Construction,构造函数，需输入"圆剖面尺寸""长度"
        public CircleSectionTimber(CircularProfile circularProfile, Double Linelength,String name)
        {
            this.circularProfile = circularProfile;
            base.Linelength = Linelength;
            base.name = name;
        }
        //空构造函数
        public CircleSectionTimber()
        {

        }        
    }

}
