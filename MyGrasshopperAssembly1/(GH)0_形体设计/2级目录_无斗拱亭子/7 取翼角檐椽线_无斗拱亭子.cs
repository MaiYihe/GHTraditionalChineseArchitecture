using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using OutPutElement;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using UtilityElement;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace MyGrasshopperAssembly1
{
    public class 取翼角檐椽线_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 翼角檐椽_小式亭子 class.
        /// </summary>
        public 取翼角檐椽线_无斗拱亭子()
          : base("取翼角檐椽线_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.last;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("正身檐椽线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("角梁图元信息", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("曲线控制点位置", "从直椽开始，0到1", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("翼角檐椽间距", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("翼角檐椽顶间隔", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("翼角檐椽线", "", "", GH_ParamAccess.tree);
            pManager.AddGenericParameter("翼角檐椽尾曲线", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("翼角檐椽顶线", "", "", GH_ParamAccess.tree);
            pManager.AddGenericParameter("翼角曲线控制点", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("翼角檐椽实际间距", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryInformation> jiaoLiangGIList = new List<GeometryInformation>();
            List<BaseCurve> zhengShenYanChuanBaseCurves = new List<BaseCurve>();
            double curveLocation = 0;
            double yiJiaoChuanDis = 0.0;
            double yiJiaoChuanTopDis = 0.0;
            if (!DA.GetDataList(0, zhengShenYanChuanBaseCurves) ||
                !DA.GetDataList(1, jiaoLiangGIList) ||
                !DA.GetData(2, ref curveLocation) ||
                !DA.GetData(3, ref yiJiaoChuanDis) ||
                !DA.GetData(4, ref yiJiaoChuanTopDis)) return;
            if (curveLocation <= 0.0 || curveLocation > 1 || yiJiaoChuanDis <= 0.0 || yiJiaoChuanTopDis <= 0.0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "间距不正确！");
                return;
            }

            //n边形，求切线
            int n = jiaoLiangGIList.Count;
            int index = zhengShenYanChuanBaseCurves.Count / n - 1;
            Curve tangent = new Line(zhengShenYanChuanBaseCurves[index].curve.PointAtEnd
                , zhengShenYanChuanBaseCurves[0].curve.PointAtEnd)
                .ToNurbsCurve();

            //取出第一项
            GeometryInformation jiaoLiangGI = jiaoLiangGIList[0];
            BaseCurve zhengShenYanChuanBaseCurve0 = zhengShenYanChuanBaseCurves[0];
            Point3d pointInside = zhengShenYanChuanBaseCurve0.curve.PointAtEnd;
            Point3d pointOutside = FindOutsidePoint(jiaoLiangGI, pointInside);//jiaoLiangGI求出来的

            //求平面线
            Plane pointInsideXYPlane = CurveUtility.GetPlanes(tangent, 1)[0];
            Plane pointInsideYZPlane = CurveUtility.GetPlanes(tangent, 1)[2];
            Point3d projectedOutsidePoint = new Point3d(pointOutside);
            projectedOutsidePoint.Transform(Transform.Translation(new Vector3d(0, 0, pointInsideXYPlane.OriginZ - pointOutside.Z)));
            //求位移直线moveCurve
            double distance = projectedOutsidePoint.DistanceTo(pointInsideYZPlane.ClosestPoint(projectedOutsidePoint));
            Vector3d v = new Vector3d(tangent.PointAtEnd - tangent.PointAtStart);
            v.Unitize();
            v = v * distance;
            Point3d moveEnding = new Point3d(pointInside);
            moveEnding.Transform(Transform.Translation(v));
            Curve moveCurve = new Line(pointInside, moveEnding).ToNurbsCurve();
            moveCurve.Domain = new Interval(0, 1);
            //控制点controlPoint
            Point3d controlPoint = moveCurve.PointAt(curveLocation);
            //翼角曲线nurbsCurve
            Point3d[] points = new Point3d[] { pointInside, controlPoint, pointOutside };
            Curve nurbsCurve = NurbsCurve.Create(false, 2, points.ToArray());

            //四舍五入，等分moveCurve，求得splitPlane
            int yiJiaoChuanCount = (int)Math.Round((double)moveCurve.GetLength() / yiJiaoChuanDis);
            double yiJiaoChuanTrueDis = moveCurve.GetLength() / yiJiaoChuanCount;
            double[] tList = moveCurve.DivideByLength(yiJiaoChuanTrueDis, false);
            List<Plane> splitPlanes = tList.Select(x => CurveUtility.GetPlanes(moveCurve, x)[2]).ToList();

            //Planes剪切Curve,得到pointsOnNurbsCurve
            List<Point3d> pointsOnNurbsCurve = splitPlanes
                .Select(x => Intersection.CurvePlane(nurbsCurve, x, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].PointA)
                .ToList();


            ///求顶部
            //正身檐椽需要第一项，求与角梁边面的交点然后连线；位移得到topPoints
            BaseCurve b0 = zhengShenYanChuanBaseCurve0.Duplicate();
            b0.Scale1D(2, 0.5);
            Plane jiaoLiangSidePlane = FindjiaoLiangSidePlane(jiaoLiangGI);
            Point3d pointA = Intersection
                .CurvePlane(b0.curve, jiaoLiangSidePlane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].PointA;
            List<Point3d> topPoints = new List<Point3d>();
            Vector3d topMoveVector = new Vector3d(pointOutside - pointA);
            topMoveVector.Unitize();
            topMoveVector = topMoveVector * yiJiaoChuanTopDis;
            Point3d lastPoint = pointA;
            for (int i = 0; i < yiJiaoChuanCount - 1; i++)
            {
                Point3d currentPoint = lastPoint;
                currentPoint.Transform(Transform.Translation(topMoveVector));
                topPoints.Add(currentPoint);
                lastPoint = currentPoint;
            }
            Curve topCurve = new Line(pointA, lastPoint).ToNurbsCurve();

            //顶部底部连线
            List<Curve> yiJiaoChuanCurves = topPoints
                .Select((x, i) => new Line(x, pointsOnNurbsCurve[i]).ToNurbsCurve() as Curve)
                .ToList();
            List<BaseCurve> yiJiaoChuanBaseCurves = BaseCurveUtility.CurvesToBaseCurves(yiJiaoChuanCurves);
            for (int i = 0; i < yiJiaoChuanBaseCurves.Count; i++)
            {
                if (b0.referencePlane.YAxis * yiJiaoChuanBaseCurves[i].referencePlane.XAxis > 0)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "间距错误！(顶部间距过大或底部间距过小)");
                    return;
                }

            }

            List<List<BaseCurve>> doubleList = listEdit.ConvertToDoubleList<BaseCurve>(yiJiaoChuanBaseCurves, 1);




            //对称变换
            Transform mirrorTransform = Transform.Scale(CurveUtility.GetPlanes(tangent, 0.5)[2], 1, 1, -1);
            List<List<BaseCurve>> mirroredDoubleList = DoubleAndMirror(doubleList, mirrorTransform);

            //原路返回
            List<Transform> backTransformList = jiaoLiangGIList
                .Select(x => Transform.PlaneToPlane(jiaoLiangGIList[0].referencePlane, x.referencePlane))
                .ToList();

            List<List<BaseCurve>> backedBaseCurvesList = new List<List<BaseCurve>>();
            for (int i = 0; i < mirroredDoubleList.Count; i++)
            {
                List<BaseCurve> wholeBackedBaseCurves = new List<BaseCurve>();
                for (int j = 0; j < backTransformList.Count; j++)
                {
                    List<BaseCurve> backedBaseCurves = mirroredDoubleList[i].Select(x => x.Duplicate()).ToList();
                    for (int k = 0; k < backedBaseCurves.Count; k++)
                    {
                        backedBaseCurves[k].Transform(backTransformList[j]);
                    }
                    wholeBackedBaseCurves.AddRange(backedBaseCurves);
                }
                backedBaseCurvesList.Add(wholeBackedBaseCurves);
            }
            //转化为树
            GH_Structure<IGH_Goo> tree = listEdit.TurnDoubleListToDataTree(backedBaseCurvesList);

            DA.SetDataTree(0, tree);
            DA.SetData(1, nurbsCurve);
            DA.SetData(2, topCurve);
            DA.SetDataList(3, points);
            DA.SetData(4, yiJiaoChuanTrueDis);

        }

        public static List<List<BaseCurve>> DoubleAndMirror(List<List<BaseCurve>> doubleList, Transform xform)
        {
            List<List<BaseCurve>> result = new List<List<BaseCurve>>();
            foreach (List<BaseCurve> item in doubleList)
            {
                List<BaseCurve> tmpResult = new List<BaseCurve>();
                tmpResult.AddRange(item);
                List<BaseCurve> newItem = new List<BaseCurve>(item);
                foreach (BaseCurve baseCurve in newItem)
                {
                    BaseCurve tmpBaseCurve = baseCurve.Duplicate();
                    tmpBaseCurve.Transform(xform);
                    // 调好平面
                    tmpBaseCurve.extrudePlane = new Plane(tmpBaseCurve.extrudePlane.Origin
                            , -tmpBaseCurve.extrudePlane.XAxis, tmpBaseCurve.extrudePlane.YAxis);
                    tmpResult.Add(tmpBaseCurve);
                }

                result.Add(tmpResult);
            }
            return result;
        }

        private Plane FindjiaoLiangSidePlane(GeometryInformation jiaoLiangGI)
        {
            List<AreaMassProperties> ampList = jiaoLiangGI.brep.Surfaces.Select(x => AreaMassProperties.Compute(x)).ToList();
            List<Point3d> centerPoints = ampList.Select(x => x.Centroid).ToList();
            List<Point3d> subPoints = centerPoints
                .Select(x => PlaneUtility.subWorldPlaneForPoint3d(jiaoLiangGI.referencePlane, x))
                .ToList();
            Surface jiaoLiangSideSurface = subPoints
                .Zip(jiaoLiangGI.brep.Surfaces, (subP, originSurface) => new { p0 = subP, p1 = originSurface })
                .OrderBy(x => x.p0.Y)
                .Select(x => x.p1)
                .Last();
            Plane jiaoLiangSidePlane = new Plane();
            jiaoLiangSideSurface.TryGetPlane(out jiaoLiangSidePlane);
            return jiaoLiangSidePlane;
        }

        private Point3d FindOutsidePoint(GeometryInformation jiaoLiangGI, Point3d pointInside)
        {
            Brep brep = jiaoLiangGI.brep;
            List<Point3d> vertices = brep.Vertices.Select(x => x.Location).ToList();
            List<Point3d> topFour = vertices
                .OrderByDescending(x => PlaneUtility.subWorldPlaneForPoint3d(jiaoLiangGI.referencePlane, x).Z)
                .Take(4)
                .ToList();
            Point3d OutsidePoint = topFour
                .OrderByDescending(x => PlaneUtility.subWorldPlaneForPoint3d(jiaoLiangGI.referencePlane, x).X)
                .Take(2)
                .OrderByDescending(x => PlaneUtility.subWorldPlaneForPoint3d(jiaoLiangGI.referencePlane, x).Y)
                .First();

            return OutsidePoint;
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
            get { return new Guid("5D18D33C-D181-4A69-9207-8460FD9C4222"); }
        }
    }
}