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
using System.Security.Cryptography;

namespace UtilityElement //辅助类
{
    namespace ReferenceDatum//定位基准
    {
        public class BaseCurve : GH_Goo<IGH_GeometricGoo>, IGH_PreviewData
        {
            public BaseCurve()
            {
            }

            public BaseCurve(Curve curve, Plane referencePlane, Vector3d positiveDirection, Plane extrudePlane)
            {
                this.curve = curve.Duplicate() as Curve;
                this.referencePlane = referencePlane.Clone();
                this.positiveDirection = new Vector3d(positiveDirection);
                this.extrudePlane = extrudePlane.Clone();
            }

            public BaseCurve(bool isSelected, Curve curve, Plane referencePlane, Vector3d positiveDirection, Plane extrudePlane)
            {
                this.isSelected = isSelected;
                this.curve = curve.Duplicate() as Curve;
                this.referencePlane = new Plane(referencePlane);
                this.positiveDirection = positiveDirection;
                this.extrudePlane = new Plane(extrudePlane);
            }

            public bool isSelected { get; }
            public Curve curve { get; set; }
            public Double curveLength
            {
                get { return curve.GetLength(); }
            }

            public Plane referencePlane { get; set; }
            public Vector3d positiveDirection { get; set; }
            public Plane extrudePlane { get; set; }

            public BoundingBox ClippingBox => new BoundingBox();

            public override bool IsValid => true;

            public override string TypeName => "MyCustomComponent";

            public override string TypeDescription => "Description of MyCustomComponent";

            public void DrawViewportMeshes(GH_PreviewMeshArgs args)
            {
                DisplayPen pen = new DisplayPen(); // 创建一个用于绘制直线的画笔
                DisplayPen pen1 = new DisplayPen();
                pen.Color = System.Drawing.Color.Red;
                pen.HaloColor = System.Drawing.Color.Green;
                pen.HaloThickness = 1;
                pen.Thickness = 2;

                pen1.Color = System.Drawing.Color.Green;
                pen1.Thickness = 2;

                if (isSelected)
                {
                    args.Pipeline.DrawCurve(curve, pen);
                }
                else
                {
                    args.Pipeline.DrawCurve(curve, pen1);
                }

            }

            public void DrawViewportWires(GH_PreviewWireArgs args)
            {
            }

            public override BaseCurve Duplicate()
            {
                BaseCurve baseCurve = new BaseCurve(curve, referencePlane, positiveDirection, extrudePlane);
                return baseCurve;
            }//复制

            public override string ToString()
            {
                return "BaseCurve";
            }

            //方法
            public void Transform(Transform xform)
            {
                curve = GeometryUtility.TransformGeometry(curve, xform) as Curve;
                referencePlane = (Plane)GeometryUtility.TransformGeometry(referencePlane, xform);
                positiveDirection = (Vector3d)GeometryUtility.TransformGeometry(positiveDirection, xform);
                extrudePlane = (Plane)GeometryUtility.TransformGeometry(extrudePlane, xform);

            }//位移
            public void Scale1D(double factor, double x)
            {
                if (x < 0) return;//限定x范围>0

                //将Curve缩放
                Curve originalCurve = curve.DuplicateCurve();
                Curve tmpCurve = curve.DuplicateCurve();
                tmpCurve.Transform(Rhino.Geometry.Transform
                    .Scale(CurveUtility.GetPlanes(curve, x)[0], factor, 0, 0));
                curve = tmpCurve;

                //移动referencePlane和extrudePlane的位置
                Vector3d v0 = new Vector3d(curve.PointAt(0.5) - originalCurve.PointAt(0.5));
                Vector3d v1 = new Vector3d(curve.PointAtStart - originalCurve.PointAtStart);
                Plane tmpExtrudePlane0 = new(referencePlane);
                Plane tmpExtrudePlane1 = new(extrudePlane);
                tmpExtrudePlane0.Transform(Rhino.Geometry.Transform.Translation(v0));
                tmpExtrudePlane1.Transform(Rhino.Geometry.Transform.Translation(v1));
                referencePlane = tmpExtrudePlane0;
                extrudePlane = tmpExtrudePlane1;
            }

        }//定位基线


        public class BaseFace : GH_Goo<IGH_GeometricGoo>, IGH_PreviewData
        {
            //构造函数
            public BaseFace() { }
            public BaseFace(Brep brep, Plane referencePlane, Vector3d positiveDirection)
            {
                this.brep = brep;
                this.referencePlane = referencePlane;
                this.positiveDirection = positiveDirection;
            }
            public BaseFace(bool isSelected, Brep brep, Plane referencePlane, Vector3d positiveDirection)
            {
                this.isSelected = isSelected;
                this.brep = brep;
                this.referencePlane = referencePlane;
                this.positiveDirection = positiveDirection;
            }

            public bool isSelected { get; }
            public Brep brep { get; set; }
            public Plane referencePlane { get; set; }
            public Vector3d positiveDirection { get; set; }

            public BoundingBox ClippingBox => new BoundingBox();

            public override bool IsValid => true;

            public override string TypeName => "MyCustomComponent";

            public override string TypeDescription => "Description of MyCustomComponent";

            public void DrawViewportMeshes(GH_PreviewMeshArgs args)
            {
                DisplayMaterial displayMaterial = new DisplayMaterial(System.Drawing.Color.Red);
                DisplayMaterial displayMaterial1 = new DisplayMaterial(System.Drawing.Color.Green);
                if (isSelected)
                {
                    args.Pipeline.DrawBrepShaded(brep, displayMaterial);
                }
                else
                {
                    args.Pipeline.DrawBrepShaded(brep, displayMaterial1);
                }

            }

            public void DrawViewportWires(GH_PreviewWireArgs args)
            {
            }



            public override BaseFace Duplicate()
            {
                if (brep == null) return null;
                Brep tmpBrep = brep.DuplicateBrep();
                Plane tmpPlane = referencePlane.Clone();
                Vector3d tmpVector = new Vector3d(positiveDirection);
                BaseFace baseFace = new BaseFace(tmpBrep, tmpPlane, tmpVector);
                return baseFace;
            }
            public override string ToString()
            {
                return "BaseFace";
            }

            //方法
            public void Transform(Transform xform)
            {
                brep = GeometryUtility.TransformGeometry(brep, xform) as Brep;
                referencePlane = (Plane)GeometryUtility.TransformGeometry(referencePlane, xform);
                positiveDirection = (Vector3d)GeometryUtility.TransformGeometry(positiveDirection, xform);
            }

        }//定位基面



        public class BaseAxis : GH_Goo<IGH_GeometricGoo>, IGH_PreviewData
        {
            public BaseAxis() { }

            public BaseAxis(bool isSelected, Curve curve, Plane referencePlane, Vector3d positiveDirection)
            {
                this.isSelected = isSelected;
                this.curve = curve;
                this.referencePlane = referencePlane;
                this.positiveDirection = positiveDirection;
            }
            public BaseAxis(Curve curve, Plane referencePlane, Vector3d positiveDirection)
            {
                this.isSelected = false;
                this.curve = curve;
                this.referencePlane = referencePlane;
                this.positiveDirection = positiveDirection;
            }

            public bool isSelected { get; }
            public Curve curve { get; set; }
            public Plane referencePlane { get; set; }
            public Vector3d positiveDirection { get; set; }

            public BoundingBox ClippingBox => new BoundingBox();

            public override bool IsValid => true;

            public override string TypeName => "MyCustomComponent";

            public override string TypeDescription => "Description of MyCustomComponent";

            public void DrawViewportMeshes(GH_PreviewMeshArgs args)
            {
                DisplayPen pen = new DisplayPen(); // 创建一个用于绘制直线的画笔
                DisplayPen pen1 = new DisplayPen();
                pen.Color = System.Drawing.Color.Red;
                pen.HaloColor = System.Drawing.Color.Green;
                pen.HaloThickness = 1;
                pen.Thickness = 2;

                pen1.Color = System.Drawing.Color.Green;
                pen1.Thickness = 2;

                if (isSelected)
                {
                    args.Pipeline.DrawCurve(curve, pen);
                }
                else
                {
                    args.Pipeline.DrawCurve(curve, pen1);
                }

            }

            public void DrawViewportWires(GH_PreviewWireArgs args)
            {
            }

            public override BaseAxis Duplicate()
            {
                if (curve == null) return null;
                Curve tmpCuvre = curve.DuplicateCurve();
                Plane tmpPlane = referencePlane.Clone();
                Vector3d tmpVector = new Vector3d(positiveDirection);
                BaseAxis baseAxis = new BaseAxis(tmpCuvre, tmpPlane, tmpVector);

                return baseAxis;
            }

            public override string ToString()
            {
                return "BaseAxis";
            }
            public void Transform(Transform xform)
            {
                curve = GeometryUtility.TransformGeometry(curve, xform) as Curve;
                referencePlane = (Plane)GeometryUtility.TransformGeometry(referencePlane, xform);
                positiveDirection = (Vector3d)GeometryUtility.TransformGeometry(positiveDirection, xform);
            }


        }//定位轴




        //public class BoundaryCondition //边界条件
        //{
        //    public List<BaseCurve> baseCurves { get; set; }
        //    public List<BaseFace> baseFaces { get; set; }

        //    public BoundaryCondition(BaseCurve baseCurve0, BaseCurve baseCurve1, BaseCurve baseCurve2)//上中下
        //    {
        //        baseCurves = new List<BaseCurve>();
        //        baseCurves.Add(baseCurve0);
        //        baseCurves.Add(baseCurve1);
        //        baseCurves.Add(baseCurve2);
        //    }

        //    public BoundaryCondition()//空构造函数
        //    {
        //    }

        //    public BoundaryCondition(BaseFace baseFace0, BaseFace baseFace1, BaseFace baseFace2)//上中下
        //    {
        //        baseFaces = new List<BaseFace>();
        //        baseFaces.Add(baseFace0);
        //        baseFaces.Add(baseFace1);
        //        baseFaces.Add(baseFace2);
        //    }
        //}
    }
}
