using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using OutPutElement;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;
using System.Linq;
using Rhino.Geometry.Intersect;

namespace MyGrasshopperAssembly1
{
    public class 取正身檐椽线_重檐_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取正身檐椽线_重檐_无斗拱亭子 class.
        /// </summary>
        public 取正身檐椽线_重檐_无斗拱亭子()
          : base("取正身檐椽线_重檐_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("承椽枋线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("檐檩顶部线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("檐檩图元信息", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("椽间距", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("出檐", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("真实椽间距", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("正身檐椽线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> chengChuanFangBaseCurves = new List<BaseCurve>();
            List<BaseCurve> yanLingTopBaseCurves = new List<BaseCurve>();
            List<GeometryInformation> yanLingGI = new List<GeometryInformation>();
            double jianJu = 0;
            double chuYan = 0;
            if (!DA.GetDataList(0, chengChuanFangBaseCurves) ||
                !DA.GetDataList(1, yanLingTopBaseCurves) ||
                !DA.GetDataList(2, yanLingGI) ||
                !DA.GetData(3, ref jianJu) ||
                !DA.GetData(4, ref chuYan)) return;

            if (jianJu <= 0 || chuYan < 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "间距/出檐错误！");
                return;
            }

            //得到交线
            List<BaseCurve> chengChuanFangCoreBaseCurves = 取横段内交线_无斗拱亭子.GetCoreBaseCurves(chengChuanFangBaseCurves);
            List<BaseCurve> yanLingCoreBaseCurves = 取横段内交线_无斗拱亭子.GetCoreBaseCurves(yanLingTopBaseCurves);

            //取出一项
            BaseCurve yanLingCoreBaseCurve = yanLingCoreBaseCurves[0];
            BaseCurve chengChuanFangCoreBaseCurve = chengChuanFangCoreBaseCurves[0];
            List<Plane> yanLingPlanes = CurveUtility.GetPlanes(yanLingCoreBaseCurve.curve, 0.5);
            List<Plane> chengChuanFangPlanes = CurveUtility.GetPlanes(chengChuanFangCoreBaseCurve.curve, 0.5);
            Plane yanLingPlane = yanLingPlanes[2];
            Brep yanLingBrep = yanLingGI[0].brep;

            //求平面与brep的相交部分intersectionCurves
            Curve[] intersectionCurves; Point3d[] intersectionPoints;
            Rhino.Geometry.Intersect.Intersection
                .BrepPlane(yanLingBrep, yanLingPlane, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intersectionCurves, out intersectionPoints);

            //求截面的中心点centroid
            Brep[] plannarBreps = Brep.CreatePlanarBreps(intersectionCurves[0], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (plannarBreps.Count() != 1) return;
            AreaMassProperties amp = AreaMassProperties.Compute(plannarBreps[0]);
            Point3d centroid = amp.Centroid;

            //爆炸线
            List<Curve> explodedCurves = new List<Curve>();
            List<Point3d> explodedPoints = new List<Point3d>();
            CurveUtility.ExplodeCurve(intersectionCurves[0], out explodedCurves, out explodedPoints);
            List<Point3d> subPoints = explodedPoints.Select(x => PlaneUtility.subWorldPlaneForPoint3d(yanLingPlane, x)).ToList();
            //局部坐标系中X最小且Y最大的点minXMaxYPoint
            var minXMaxYPoint = subPoints.Zip(explodedPoints, (sub, exploded) => new { SubPoint = sub, ExplodedPoint = exploded })
                .OrderBy(p => p.SubPoint.X, Comparer<double>.Create((x, y) => Math.Abs(x - y) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance ? 0 : x.CompareTo(y)))
                .ThenByDescending(p => p.SubPoint.Y, Comparer<double>.Create((x, y) => Math.Abs(x - y) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance ? 0 : x.CompareTo(y)))
                .Select(p => p.ExplodedPoint)
                .FirstOrDefault();

            ///
            ///0.5处的zhengShenChuanDu、finalBaseCurve，真实间距trueJianju
            ///
            //连线
            BaseCurve projectedchengChuanFangCoreBaseCurve = chengChuanFangCoreBaseCurve.Duplicate();
            projectedchengChuanFangCoreBaseCurve
                .Transform(Transform
                .Translation(new Vector3d(0, 0, yanLingCoreBaseCurve.referencePlane.OriginZ - chengChuanFangCoreBaseCurve.referencePlane.OriginZ)));
            Point3d chengChuanFangStartPoint = projectedchengChuanFangCoreBaseCurve.curve.PointAtStart;
            Point3d chengChuanFangEndPoint = projectedchengChuanFangCoreBaseCurve.curve.PointAtEnd;
            double t0 = 0.0; double t1 = 0.0;
            if (!(yanLingCoreBaseCurve.curve.ClosestPoint(chengChuanFangStartPoint, out t0) &&
                yanLingCoreBaseCurve.curve.ClosestPoint(chengChuanFangEndPoint, out t1)))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "");
                return;
            }
            Point3d chengChuanFangProjectedStartPoint = yanLingCoreBaseCurve.curve.PointAt(t0);
            Point3d chengChuanFangProjectedEndPoint = yanLingCoreBaseCurve.curve.PointAt(t1);
            Curve yanLingShortCurve = new Line(chengChuanFangProjectedStartPoint, chengChuanFangProjectedEndPoint).ToNurbsCurve();

            //正身椽线zhengShenChuanList
            Point3d[] points0; Point3d[] points1;
            //四舍五入取到真实的间距
            int count = (int)Math.Round(chengChuanFangCoreBaseCurve.curveLength / jianJu);
            double trueJianJu = chengChuanFangCoreBaseCurve.curveLength / count;

            chengChuanFangCoreBaseCurve.curve.DivideByLength(trueJianJu, true, out points0);
            yanLingShortCurve.DivideByLength(trueJianJu, true, out points1);
            List<Curve> zhengShenChuanList = points0
                .Select((x, i) => new Line(points0[i], points1[i]).ToNurbsCurve() as Curve)
                .ToList();

            Curve zhengShenChuanFirst = zhengShenChuanList[0].DuplicateCurve();
            zhengShenChuanFirst.Transform(Transform.Translation(new Vector3d(yanLingPlane.Origin - zhengShenChuanFirst.PointAtEnd)));
            double tt = 0.0;
            Plane plane = CurveUtility.GetPlanes(zhengShenChuanFirst, 1)[0];
            Point3d tmpP = plane.ClosestPoint(centroid);
            Vector3d v = new Vector3d(tmpP - centroid);
            v.Unitize();
            Curve tmpCurve = new Line(centroid, v).ToNurbsCurve();


            //判断Curve相交的Surface是不是平面
            isPlannarIntersection isPlannarIntersection = default(isPlannarIntersection);
            Point3d intersectPoint = new Point3d();
            for (int i = 0; i < yanLingBrep.Surfaces.Count; i++)
            {
                double t = 0.0;
                CurveIntersections curveIntersections = Intersection.CurveSurface(tmpCurve, yanLingBrep.Surfaces[i]
                    , Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (curveIntersections.Count > 0 &&
                    curveIntersections[0].PointA.DistanceTo(tmpCurve.PointAtStart) > 0.01)
                {
                    intersectPoint = curveIntersections[0].PointA;
                    if (yanLingBrep.Surfaces[i].IsPlanar() == true) isPlannarIntersection = isPlannarIntersection.plannar;
                    else isPlannarIntersection = isPlannarIntersection.notPlanar;
                }
            }

            Curve zhengShenChuanDu = zhengShenChuanFirst.DuplicateCurve();
            if (isPlannarIntersection == isPlannarIntersection.plannar)
            {
                //位移到minXMaxYPoint
                zhengShenChuanDu = CurveUtility.MoveCurveToPoint(zhengShenChuanDu, minXMaxYPoint);
            }
            else if (isPlannarIntersection == isPlannarIntersection.notPlanar)
            {
                //位移到intersectPoint
                zhengShenChuanDu = CurveUtility.MoveCurveToPoint(zhengShenChuanDu, intersectPoint);
            }
            //用Plane切出CoreCurve
            Plane chuYanCutPlane = yanLingPlanes[1].Clone();
            chuYanCutPlane.Transform(Transform.Translation(-chuYanCutPlane.ZAxis * chuYan));
            BaseCurve zhengShenChuanDuBaseCurve = BaseCurveUtility.CurveToBaseCurve(zhengShenChuanDu);
            BaseCurve zhengShenChuanDuBaseCurveToCut = zhengShenChuanDuBaseCurve.Duplicate();
            zhengShenChuanDuBaseCurveToCut.Scale1D((zhengShenChuanDuBaseCurveToCut.curveLength + chuYan * 2) * 3, 0.5);

            Curve tmpCutCurve = CurveUtility.PlaneCutLineAndGetCoreCurve(zhengShenChuanDuBaseCurveToCut.curve
                , zhengShenChuanDu.PointAt(0.5), chengChuanFangPlanes[1], chuYanCutPlane);
            BaseCurve finalBaseCurve = BaseCurveUtility.CurveToBaseCurve(tmpCutCurve);


            ///
            ///将finalBaseCurve原路返回,得到finalBaseCurves和finalBaeCurvesList
            ///
            List<Transform> backTransforms = zhengShenChuanList
                .Select(x => Transform.Translation(new Vector3d(x.PointAtStart - zhengShenChuanFirst.PointAtStart)))
                .ToList();
            List<BaseCurve> finalBaseCurves = backTransforms.Select(x => finalBaseCurve.Duplicate()).ToList();
            for (int i = 0; i < finalBaseCurves.Count; i++)
            {
                finalBaseCurves[i].Transform(backTransforms[i]);
            }
            List<Transform> backRoundTransforms = yanLingCoreBaseCurves
                .Select((x, i) => Transform.PlaneToPlane(x.referencePlane, yanLingCoreBaseCurves[0].referencePlane))
                .ToList();

            List<BaseCurve> finalBaseCurvesList = new List<BaseCurve>();
            for (int i = 0; i < backRoundTransforms.Count; i++)
            {
                List<BaseCurve> tmpBaseCurves = finalBaseCurves.Select(x => x.Duplicate()).ToList();
                for (int j = 0; j < tmpBaseCurves.Count; j++)
                {
                    tmpBaseCurves[j].Transform(backRoundTransforms[i]);
                }
                finalBaseCurvesList.AddRange(tmpBaseCurves);
            }

            DA.SetData(0, trueJianJu);
            DA.SetDataList(1, finalBaseCurvesList);

        }

        private enum isPlannarIntersection
        {
            plannar,
            notPlanar
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
            get { return new Guid("17307BFF-BE58-4C48-A93A-8DEE73449C81"); }
        }
    }
}