using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimberElement;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    internal class RevolvedTimber : TimberSection
    {
        //属性
        public new bool isExist
        {
            get
            {
                base.isExist = false;
                if (ZhuJing > 0 && Curve != null && Curve.IsPlanar() == true && Curve.IsClosed == true) base.isExist = true;
                return base.isExist;
            }
        }//判断存在与否
        public Curve Curve { get; set; }
        double ZhuJing { get; set; }

        private new Brep timberBrep;
        public Brep TimberBrep
        {
            get
            {
                if (isExist == true)
                {
                    BoundingBox boundingBox = this.Curve.GetBoundingBox(true);
                    Point3d corner = boundingBox.Min;
                    Transform align = Transform.PlaneToPlane(new Plane(corner, Vector3d.XAxis, Vector3d.YAxis), Plane.WorldXY);
                    Curve alignedCurve = Curve.DuplicateCurve();
                    alignedCurve.Transform(align);

                    Transform xform = Transform
                        .Translation(new Vector3d(ZhuJing / 2, 0, 0));
                    alignedCurve.Transform (xform);

                    //在XY平面上的旋转体
                    RevSurface revSurface = RevSurface
                        .Create(alignedCurve, new Line(Point3d.Origin, Vector3d.YAxis));

                    timberBrep = revSurface.ToBrep();
                }
                return timberBrep;
            }
            set { timberBrep = value; }
        }

        public new string name
        {
            get { return base.name; }
            set { base.name = value; }
        }//构件名称

        public new string description
        {
            get
            {
                base.description = string.Format("");
                return base.description;
            }
        }//为父类的描述赋值并输出


        //构造函数
        public RevolvedTimber(Curve curve, double zhuJing, string name)
        {
            this.Curve = curve;
            this.ZhuJing = zhuJing;
            this.name = name;
        }

    }
}
