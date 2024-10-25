using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.Input.Custom;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 绘制挂落 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 绘制挂落 class.
        /// </summary>
        public 绘制挂落()
          : base("绘制挂落", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基线", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("柱径", "", "", GH_ParamAccess.item);
            pManager.AddCurveParameter("主体起始段", "一半", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("主体中间段", "一半", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("主体尾段", "一半", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("边段", "一半", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("主体宽", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("边段宽", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("主体定位基面", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("边段定位基面", "", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            double zhuJing = 0;
            List<Curve> startCurves = new List<Curve>();
            List<Curve> middleCurves = new List<Curve>();
            List<Curve> endCurves = new List<Curve>();
            List<Curve> bianDuan = new List<Curve>();
            double mainWidth = 0.0;
            double sideWidth = 0.0;

            if (!DA.GetDataList(0, baseCurves) ||
                !DA.GetData(1, ref zhuJing) ||
                !DA.GetDataList(2, startCurves) ||
                !DA.GetDataList(3, middleCurves) ||
                !DA.GetDataList(4, endCurves) ||
                !DA.GetDataList(5, bianDuan) ||
                !DA.GetData(6, ref mainWidth) ||
                !DA.GetData(7, ref sideWidth)) return;

            if (mainWidth <= 0 || sideWidth <= 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "宽不能小于0");
                return;
            }
            //得到横段线内角线
            List<BaseCurve> coreBaseCurves = 取横段内交线_无斗拱亭子.GetCoreBaseCurves(baseCurves);

            Curve[] startCurvesArray = Curve.JoinCurves(startCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Curve[] middleCurvesArray = Curve.JoinCurves(middleCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Curve[] endCurvesArray = Curve.JoinCurves(endCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            double startCurvesArrayX = GetXLen(startCurvesArray);
            double middleCurvesArrayX = GetXLen(middleCurvesArray);
            double endCurvesArrayX = GetXLen(endCurvesArray);

            BoundingBox boundingBox = GetBoudingBox(endCurvesArray);

            Curve[] startCurvesArrayTrans = new Curve[] { };
            Curve[] middleCurvesArrayTrans = new Curve[] { };
            List<Curve> newMiddleCurves = new List<Curve>();
            Curve[] endCurvesArrayTrans = new Curve[] { };

            ///主体位移/缩放
            double trueDis = coreBaseCurves[0].curveLength - zhuJing;
            double totalHalfLen = startCurvesArrayX + middleCurvesArrayX + endCurvesArrayX + sideWidth / 2;
            double delta = trueDis / 2 - totalHalfLen;
            double factor = 0;
            int middleCount = 0;
            Transform xform = default(Transform);
            if (delta <= 0 || (delta > 0 && delta < middleCurvesArrayX))
            {
                //直接缩放到trueDis/2
                middleCount = 1;
                totalHalfLen = startCurvesArrayX + middleCurvesArrayX * middleCount + endCurvesArrayX + sideWidth / 2;
                factor = trueDis / 2 / totalHalfLen;
                xform = Transform.Scale(Plane.WorldXY, factor, factor, 0);
                startCurvesArrayTrans = startCurvesArray.Select(x => x.DuplicateCurve()).ToArray();
                middleCurvesArrayTrans = middleCurvesArray.Select(x => x.DuplicateCurve()).ToArray();
                endCurvesArrayTrans = endCurvesArray.Select(x => x.DuplicateCurve()).ToArray();

                //缩放第一项
                foreach (Curve curve in startCurvesArrayTrans) curve.Transform(xform);
                //缩放第二项
                foreach (Curve curve in middleCurvesArrayTrans) curve.Transform(xform);
                //缩放第三项，并对齐到原点上
                foreach (Curve curve in endCurvesArrayTrans) curve.Transform(xform);

                //对齐第三项
                Point3d corner = GetBoudingBox(endCurvesArrayTrans).Max;
                Transform align = Transform.PlaneToPlane(new Plane(corner, Vector3d.XAxis, Vector3d.YAxis), Plane.WorldXY);
                foreach (Curve curve in endCurvesArrayTrans) curve.Transform(align);
                //对齐第二项
                Transform transform0 = Transform.PlaneToPlane(new Plane(GetBoudingBox(middleCurvesArrayTrans).Max, Vector3d.ZAxis)
                    , new Plane(new Point3d(GetBoudingBox(endCurvesArrayTrans).Min.X, GetBoudingBox(endCurvesArrayTrans).Max.Y, 0)
                    , Vector3d.ZAxis));
                foreach (Curve curve in middleCurvesArrayTrans) curve.Transform(transform0);
                //对齐第一项
                Transform transform1 = Transform.PlaneToPlane(new Plane(GetBoudingBox(startCurvesArrayTrans).Max, Vector3d.ZAxis)
                    , new Plane(new Point3d(GetBoudingBox(middleCurvesArrayTrans).Min.X, GetBoudingBox(middleCurvesArrayTrans).Max.Y, 0)
                    , Vector3d.ZAxis));
                foreach (Curve curve in startCurvesArrayTrans) curve.Transform(transform1);


                newMiddleCurves.AddRange(middleCurvesArrayTrans);
            }
            //判断middleCurvesArrayY还能不能插入，能则插入，插到不能插以后再缩放
            else if (delta > 0 && delta >= middleCurvesArrayX)
            {
                middleCount = (int)Math.Floor(delta / middleCurvesArrayX);
                totalHalfLen = startCurvesArrayX + middleCurvesArrayX * middleCount + endCurvesArrayX + sideWidth / 2;
                factor = trueDis / 2 / totalHalfLen;
                xform = Transform.Scale(Plane.WorldXY, factor, factor, 0);
                //复制
                startCurvesArrayTrans = startCurvesArray.Select(x => x.DuplicateCurve()).ToArray();
                endCurvesArrayTrans = endCurvesArray.Select(x => x.DuplicateCurve()).ToArray();

                //缩放第一项
                foreach (Curve curve in startCurvesArrayTrans) curve.Transform(xform);
                Point3d corner = GetBoudingBox(startCurvesArrayTrans).Max;
                Transform align = Transform.PlaneToPlane(new Plane(corner, Vector3d.XAxis, Vector3d.YAxis), Plane.WorldXY);
                foreach (Curve curve in startCurvesArrayTrans) curve.Transform(align);
                //缩放第三项，并对齐到原点上
                foreach (Curve curve in endCurvesArrayTrans) curve.Transform(xform);
                corner = GetBoudingBox(endCurvesArrayTrans).Max;
                align = Transform.PlaneToPlane(new Plane(corner, Vector3d.XAxis, Vector3d.YAxis), Plane.WorldXY);
                foreach (Curve curve in endCurvesArrayTrans) curve.Transform(align);

                //第三项的角点
                Plane endLeftUpCornerPlane = new Plane(
                    new Point3d(GetBoudingBox(endCurvesArrayTrans).Min.X, GetBoudingBox(endCurvesArrayTrans).Max.Y, 0)
                    , Vector3d.XAxis
                    , Vector3d.YAxis);

                //middle
                Plane formerCornerPlane = endLeftUpCornerPlane;
                for (int i = 0; i < middleCount; i++)
                {
                    //缩放
                    middleCurvesArrayTrans = middleCurvesArray.Select(x => x.DuplicateCurve()).ToArray();
                    foreach (Curve curve in middleCurvesArrayTrans) curve.Transform(xform);
                    //位移
                    Plane middleRightUpCornerPlane = new Plane(
                        new Point3d(GetBoudingBox(middleCurvesArrayTrans).Max.X, GetBoudingBox(middleCurvesArrayTrans).Max.Y, 0)
                        , Vector3d.XAxis
                        , Vector3d.YAxis);
                    Transform yform = Transform.PlaneToPlane(middleRightUpCornerPlane, formerCornerPlane);
                    foreach (Curve curve in middleCurvesArrayTrans) curve.Transform(yform);
                    newMiddleCurves.AddRange(middleCurvesArrayTrans);

                    Plane middleLeftUpCornerPlane = new Plane(
                        new Point3d(GetBoudingBox(middleCurvesArrayTrans).Min.X, GetBoudingBox(middleCurvesArrayTrans).Max.Y, 0)
                        , Vector3d.XAxis
                        , Vector3d.YAxis);
                    formerCornerPlane = middleLeftUpCornerPlane;
                }

                //start
                Plane startRightUpCornerPlane = new Plane(
                        new Point3d(GetBoudingBox(startCurvesArrayTrans).Max.X, GetBoudingBox(startCurvesArrayTrans).Max.Y, 0)
                        , Vector3d.XAxis
                        , Vector3d.YAxis);
                Transform zform = Transform.PlaneToPlane(startRightUpCornerPlane, formerCornerPlane);
                foreach (Curve curve in startCurvesArrayTrans) curve.Transform(zform);



                //为边段做修改
                double tmpFactor = (GetBoudingBox(endCurvesArrayTrans.ToArray()).Max.X - GetBoudingBox(startCurvesArrayTrans.ToArray()).Min.X)
                    / (GetBoudingBox(bianDuan.ToArray()).Max.X - GetBoudingBox(bianDuan.ToArray()).Min.X);
                xform = Transform.Scale(Plane.WorldXY, tmpFactor, factor, 0);
            }

            ///边段位移、缩放
            Curve[] bianDuanTrans = bianDuan.Select(x => x.DuplicateCurve()).ToArray();
            foreach (Curve curve in bianDuanTrans) curve.Transform(xform);
            Point3d bianDuanCorner = GetBoudingBox(bianDuanTrans.ToArray()).Max;
            Transform bianduanTransform = Transform.PlaneToPlane(new Plane(bianDuanCorner, Vector3d.XAxis, Vector3d.YAxis), Plane.WorldXY);
            foreach (var curve in bianDuanTrans) curve.Transform(bianduanTransform);


            ///生成形体
            List<Curve> curves0 = new List<Curve>();
            List<Curve> curves1 = new List<Curve>();
            curves0.AddRange(startCurvesArrayTrans);
            curves0.AddRange(newMiddleCurves.ToArray());
            curves0.AddRange(endCurvesArrayTrans);
            curves1.AddRange(bianDuanTrans);

            ///对称变换
            //主体变换    
            Transform mirrorTrans = Transform.Mirror(Plane.WorldYZ);
            List<Curve> mirroredCurves0 = curves0.Select(x => x.DuplicateCurve()).ToList();
            foreach (var curve in mirroredCurves0) curve.Transform(mirrorTrans);
            curves0.AddRange(mirroredCurves0);
            List<Curve> curves00 = MakeBody(mainWidth, curves0.ToArray()).ToList();
            Brep regionsBrep0 = Brep.CreatePlanarBreps(curves00.ToArray(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
            //边角变换
            List<Curve> mirroredCurves1 = curves1.Select(x => x.DuplicateCurve()).ToList();
            foreach (var curve in mirroredCurves1) curve.Transform(mirrorTrans);
            curves1.AddRange(mirroredCurves1);
            List<Curve> curves11 = MakeBody(sideWidth, curves1.ToArray()).ToList();
            Brep regionsBrep1 = Brep.CreatePlanarBreps(curves11.ToArray(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];

            BaseFace baseFace0 = new BaseFace(regionsBrep0, Plane.WorldXY, Vector3d.ZAxis);
            BaseFace baseFace1 = new BaseFace(regionsBrep1, Plane.WorldXY, Vector3d.ZAxis);

            //原路返回            
            List<Transform> backTransformList = coreBaseCurves
                .Select((x, i) => Transform.PlaneToPlane(new Plane(Point3d.Origin, -Plane.WorldZX.YAxis, Plane.WorldZX.XAxis), x.referencePlane)).ToList();
            List<BaseFace> baseFaces0 = new List<BaseFace>();
            List<BaseFace> baseFaces1 = new List<BaseFace>();
            for (int i = 0; i < backTransformList.Count; i++)
            {
                BaseFace tmpBaseFace0 = baseFace0.Duplicate();
                BaseFace tmpBaseFace1 = baseFace1.Duplicate();
                tmpBaseFace0.Transform(backTransformList[i]);
                tmpBaseFace1.Transform(backTransformList[i]);
                baseFaces0.Add(tmpBaseFace0);
                baseFaces1.Add(tmpBaseFace1);
            }

            DA.SetDataList(0, baseFaces0);
            DA.SetDataList(1, baseFaces1);
        }

        private static BoundingBox GetBoudingBox(Curve[] CurvesArray)
        {
            List<Point3d> points = CurvesArray.Select(x => x.PointAtStart).ToList();
            points.AddRange(CurvesArray.Select(x => x.PointAtEnd).ToList());
            BoundingBox boundingBox = new BoundingBox(points);
            return boundingBox;
        }

        private static double GetXLen(Curve[] CurvesArray)
        {

            BoundingBox boundingBox = GetBoudingBox(CurvesArray);
            return boundingBox.Max.X - boundingBox.Min.X;
        }

        private static Curve[] MakeBody(double width, Curve[] CurvesArray)
        {
            List<List<Curve>> offsetedCurvesList = new List<List<Curve>>();
            for (int i = 0; i < CurvesArray.Count(); i++)
            {
                offsetedCurvesList
                    .Add(CurvesArray[i].Offset(Plane.WorldXY, width / 2
                    , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp).ToList());
                offsetedCurvesList[i]
                    .AddRange(CurvesArray[i].Offset(Plane.WorldXY, -width / 2
                    , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp));
            }
            List<Brep> startLofts = offsetedCurvesList
                .Select(x => Brep.CreateFromLoft(x.ToArray(), Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0])
                .ToList();
            List<Curve> unionCurves = startLofts.Select(x => Curve.JoinCurves(x.Edges)[0]).ToList();
            Curve[] uion = Curve.CreateBooleanUnion(unionCurves.ToArray(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            return uion;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5D10FF89-5DE0-4587-84E3-9D058637E0C2"); }
        }
    }
}