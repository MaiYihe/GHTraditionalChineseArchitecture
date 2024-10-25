using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using OutPutElement;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取飞椽线_偏移_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取飞椽_小式亭子 class.
        /// </summary>
        public 取飞椽线_偏移_无斗拱亭子()
          : base("取飞椽线_偏移_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("仔角梁图元信息", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("翼角檐椽顶部线", "", "", GH_ParamAccess.tree);
            pManager.AddCurveParameter("翼角檐椽尾曲线", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("正身檐椽顶部线", "", "", GH_ParamAccess.list);

            pManager.AddNumberParameter("直飞椽顶端抬高", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("翼角飞椽尾端位置", "0到1", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("正身飞椽尾端位置", "0到1", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("翼角飞椽线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("正身飞椽线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("飞椽尾部曲线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("飞椽尾部直线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryInformation> GIList = new List<GeometryInformation>();
            Curve tailCurve = default(Curve);
            GH_Structure<IGH_Goo> yiJiaoYanChuanTopBaseCurves = new GH_Structure<IGH_Goo>();
            List<BaseCurve> zhengShenYanChuanTopBaseCurves = new List<BaseCurve>();
            double taiGao = 0.0;
            double yiJiaoBottomSideLocation = 0.0;
            double zhengShenBottomSideLocation = 0.0;
            if (!DA.GetDataList(0, GIList) ||
                !DA.GetDataTree(1, out yiJiaoYanChuanTopBaseCurves) ||
                !DA.GetData(2, ref tailCurve) ||
                !DA.GetDataList(3, zhengShenYanChuanTopBaseCurves) ||
                !DA.GetData(4, ref taiGao) ||
                !DA.GetData(5, ref yiJiaoBottomSideLocation) ||
                !DA.GetData(6, ref zhengShenBottomSideLocation)) return;

            if (taiGao < 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "抬高值错误！");
                return;
            }
            if (yiJiaoBottomSideLocation < 0 || yiJiaoBottomSideLocation > 1
                || zhengShenBottomSideLocation < 0 || zhengShenBottomSideLocation > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "位置错误！");
                return;
            }


            //求返回路径，并得到两个角点
            List<Transform> backTransformList = GIList
                .Select(x => Transform.PlaneToPlane(GIList[0].referencePlane, x.referencePlane))
                .ToList();

            List<Point3d> tmpPoints = new List<Point3d>();
            Point3d corner0 = GetCorner0(GIList[0]);
            Point3d corner1 = GetCorner1(GIList[0]);
            Point3d corner1Transformed = new Point3d(corner1);
            corner1Transformed.Transform(backTransformList[1]);
            //两corner连线为line，mirror翼角檐椽尾曲线，join获得边线joinedCurve
            Curve line = new Line(corner0, corner1Transformed).ToNurbsCurve();
            Plane mirrorPlane = CurveUtility.GetPlanes(line, 0.5)[2];
            Transform mirror = Transform.Mirror(mirrorPlane);

            Curve mirroredTailCurve = tailCurve.DuplicateCurve();
            mirroredTailCurve.Transform(mirror);
            Curve middleCurve = new Line(tailCurve.PointAtStart, mirroredTailCurve.PointAtStart).ToNurbsCurve();
            Curve[] curves = new Curve[] { tailCurve, middleCurve, mirroredTailCurve };
            Curve joinedCurve = Curve.JoinCurves(curves)[0];
            //保证方向是，offset为正的时候外偏
            Vector3d joinedCurveDirection = new Vector3d(joinedCurve.PointAtEnd - joinedCurve.PointAtStart);
            Vector3d lineDirection = new Vector3d(line.PointAtEnd - line.PointAtStart);
            if (joinedCurveDirection * lineDirection < 0) joinedCurve.Reverse();

            //找到中间线的中心平面，投影得到projectedCurve、proLine(缩放两倍长)，投影点proCorner0
            Plane middlePlane = CurveUtility.GetPlanes(middleCurve, 0.5)[0];
            Transform proTrans = Transform.PlanarProjection(middlePlane);
            Curve projectedCurve = joinedCurve.DuplicateCurve();
            projectedCurve.Transform(proTrans);
            Curve proLine = line.DuplicateCurve();
            proLine.Transform(proTrans);
            Point3d proCorner0 = proLine.PointAtStart;
            Transform scaleTrans = Transform.Scale(CurveUtility.GetPlanes(proLine, 0.5)[0], 2, 2, 2);
            proLine.Transform(scaleTrans);

            //将projectedCurve两侧延申,曲线偏移得到offsetedCurve,剪切得到splitedCurve
            Curve proAndExtendedCurve = projectedCurve.Extend(CurveEnd.Both, projectedCurve.GetLength(), CurveExtensionStyle.Line);
            double t = 0.0;
            proAndExtendedCurve.ClosestPoint(proCorner0, out t);
            double offsetDis = proCorner0.DistanceTo(proAndExtendedCurve.PointAt(t));

            Curve offsetedProCurve = projectedCurve.Offset(middlePlane, offsetDis
                , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Smooth)[0];
            Point3d middlePoint = offsetedProCurve.PointAt(0.5);

            Curve offsetedCurve = proAndExtendedCurve.Offset(middlePlane, offsetDis
                , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Smooth)[0];
            //用proLine剪切offsetedCurve
            Point3d point0 = Intersection.CurveCurve(offsetedCurve, proLine
                , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].PointA;
            Point3d point1 = Intersection.CurveCurve(offsetedCurve, proLine
                , RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[1].PointA;
            double t0 = 0.0; double t1 = 0.0;
            offsetedCurve.ClosestPoint(point0, out t0);
            offsetedCurve.ClosestPoint(point1, out t1);
            Curve[] splitedCurves = offsetedCurve.Split(new double[] { t0, t1 });
            foreach (Curve item in splitedCurves)
            {
                item.Domain = new Interval(0, 1);
            }
            Curve[] splitedCurvess = splitedCurves.OrderBy(x => x.PointAt(0.5).DistanceTo(middlePoint)).Take(1).ToArray();
            Curve splitedCurve = splitedCurvess[0];

            //取到上半段的圆弧段joinedCurvedCurve
            Curve halfSplitedCurve = CurveUtility.PlaneCutLineAndGetCoreCurve(splitedCurve, splitedCurve.PointAtStart
                , CurveUtility.GetPlanes(splitedCurve, 0.5)[2]);
            List<Curve> explodedCurves = halfSplitedCurve.DuplicateSegments().ToList();
            List<Curve> curvedCurves = explodedCurves
                .Where(x => !(x.ToNurbsCurve().Degree == 1 && x.ToNurbsCurve().Points.Count == 2))
                .ToList();//筛走两点一阶曲线

            //选出一半的，中心的两点一阶曲线linelikedCurve、一侧的两点一阶曲线linelikedCurveSide
            List<Curve> lineLikedCurves = explodedCurves
                .Where(x => x.ToNurbsCurve().Degree == 1 && x.ToNurbsCurve().Points.Count == 2)
                .ToList();
            Curve linelikedCurve = default(Curve);
            Curve linelikedCurveSide = default(Curve);
            foreach (Curve item in lineLikedCurves)
            {
                double tt;
                item.ClosestPoint(splitedCurve.PointAt(0.5), out tt);
                if (item.PointAt(tt).DistanceTo(splitedCurve.PointAt(0.5)) < 0.001)
                {
                    linelikedCurve = item;
                }
                else
                {
                    linelikedCurveSide = item;
                }
            }

            //选出完整的两点一阶曲线
            List<Curve> wholeLinelikeCurves = offsetedCurve.DuplicateSegments()
                .Where(x => x.ToNurbsCurve().Degree == 1 && x.ToNurbsCurve().Points.Count == 2)
                .ToList();
            Curve wholeLinelikeCurve = default(Curve);
            foreach (Curve item in wholeLinelikeCurves)
            {
                double tt;
                item.ClosestPoint(splitedCurve.PointAt(0.5), out tt);
                if (item.PointAt(tt).DistanceTo(splitedCurve.PointAt(0.5)) < 0.001)
                {
                    wholeLinelikeCurve = item;
                }
            }

            List<Curve> curvesTojoin = new List<Curve>();
            curvesTojoin.AddRange(curvedCurves);
            curvesTojoin.Add(linelikedCurveSide);
            Curve joinedCurvedCurve = Curve.JoinCurves(curvesTojoin)[0];
            //Curve.JoinCurves(joinedCurvedCurve,);
            ///难点
            //暴力求出不精确的拟合位置（平面拟合即可）
            Plane proPlane = CurveUtility.GetPlanes(linelikedCurve, 0)[1];
            Curve path = Curve.ProjectToPlane(joinedCurvedCurve, proPlane);
            Curve rebuiltPath = path.Rebuild(2, 1, true);
            double ttt = GetMaxDisMinParam(joinedCurvedCurve, rebuiltPath, 0.02);


            //创建出拟合的曲线fitCurve
            Point3d p0 = rebuiltPath.PointAt(ttt);
            p0.Transform(Transform.Translation(new Vector3d(0, 0, taiGao)));
            Point3d p1 = joinedCurvedCurve.PointAtEnd;
            p1.Transform(Transform.Translation(new Vector3d(0, 0, taiGao)));

            Curve fitCurve = NurbsCurve
                .Create(false, 2,
                new Point3d[] { corner0, p0, p1 });

            Curve movedLinelikeCurve = wholeLinelikeCurve.DuplicateCurve();
            movedLinelikeCurve.Transform(Transform.Translation(new Vector3d(0, 0, taiGao)));

            ///上面求出线movedLinelikeCurve与fitCurve，接下来联系椽线求出飞椽线
            //得到飞出曲线上的点
            IList<List<IGH_Goo>> yiJiaoYanChuanBottomBaseCurvesBranches = yiJiaoYanChuanTopBaseCurves.Branches;
            List<IGH_Goo> yiJiaoYanChuanCorner0 = yiJiaoYanChuanBottomBaseCurvesBranches
                .Select(x => x.FirstOrDefault())
                .ToList();
            List<BaseCurve> yiJiaoYanChuanCorner = new List<BaseCurve>();
            for (int i = 0; i < yiJiaoYanChuanCorner0.Count; i++)
            {
                yiJiaoYanChuanCorner.Add((BaseCurve)yiJiaoYanChuanCorner0[i]);
            }
            List<Plane> cornerPlanes = yiJiaoYanChuanCorner.Select(x => CurveUtility.GetPlanes(x.curve, 1)[1]).ToList();
            List<Point3d> cornerFlyPoints = cornerPlanes
                .Select(x => Intersection.CurvePlane(fitCurve, x, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].PointA)
                .ToList();

            int n = GIList.Count;//n边形
            List<BaseCurve> zhengShenYanChuanSide = zhengShenYanChuanTopBaseCurves
                .Take(zhengShenYanChuanTopBaseCurves.Count / n)
                .ToList();
            List<Plane> sidePlanes = zhengShenYanChuanSide.Select(x => CurveUtility.GetPlanes(x.curve, 1)[1]).ToList();

            List<Point3d> sidePoints = zhengShenYanChuanSide.Select(x => x.curve.PointAtEnd).ToList();
            List<Point3d> sideFlyPoints = new List<Point3d>();
            for (int i = 0; i < sidePoints.Count; i++)
            {
                double tt = 0.0;
                movedLinelikeCurve.ClosestPoint(sidePoints[i], out tt);
                sideFlyPoints.Add(movedLinelikeCurve.PointAt(tt));
            }
            //得到主体端的点
            List<Point3d> startYiJiaoPoint = yiJiaoYanChuanCorner.Select(x => x.curve.PointAt(yiJiaoBottomSideLocation)).ToList();
            List<Point3d> startZhengShenPoint = zhengShenYanChuanSide.Select(x => x.curve.PointAt(zhengShenBottomSideLocation)).ToList();
            //连线得到BaseCurves
            List<BaseCurve> cornerFlyBaseCurves = startYiJiaoPoint
                .Select((x, i) => BaseCurveUtility.CurveToBaseCurve(new Line(x, cornerFlyPoints[i]).ToNurbsCurve())).ToList();
            List<BaseCurve> zhengShenFlyBaseCurves = startZhengShenPoint
                .Select((x, i) => BaseCurveUtility.CurveToBaseCurve(new Line(x, sideFlyPoints[i]).ToNurbsCurve())).ToList();

            ////原路返回
            List<BaseCurve> wholeZhengShenFlyBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < backTransformList.Count; i++)
            {
                for (int j = 0; j < zhengShenFlyBaseCurves.Count; j++)
                {
                    BaseCurve tmpBaseCurve = zhengShenFlyBaseCurves[j].Duplicate();
                    tmpBaseCurve.Transform(backTransformList[i]);
                    wholeZhengShenFlyBaseCurves.Add(tmpBaseCurve);
                }
            }
            List<BaseCurve> wholeFlyBaseCurves = cornerFlyBaseCurves;
            List<BaseCurve> mirroredCornerFlyBaseCurves = cornerFlyBaseCurves.Select(x => x.Duplicate()).ToList();
            for (int i = 0; i < mirroredCornerFlyBaseCurves.Count; i++)
            {
                mirroredCornerFlyBaseCurves[i].Transform(mirror);
                mirroredCornerFlyBaseCurves[i].extrudePlane = new Plane(mirroredCornerFlyBaseCurves[i].extrudePlane.Origin
                    , -mirroredCornerFlyBaseCurves[i].extrudePlane.XAxis
                    , mirroredCornerFlyBaseCurves[i].extrudePlane.YAxis);
            }
            wholeFlyBaseCurves.AddRange(mirroredCornerFlyBaseCurves);

            List<List<BaseCurve>> wholeFlyBaseCurvesTree = listEdit.ConvertToDoubleList(wholeFlyBaseCurves, 1);
            for (int i = 0; i < wholeFlyBaseCurvesTree.Count; i++)
            {
                for (int j = 1; j < backTransformList.Count; j++)
                {
                    BaseCurve tmpBaseCurve = wholeFlyBaseCurvesTree[i][0].Duplicate();
                    tmpBaseCurve.Transform(backTransformList[j]);
                    wholeFlyBaseCurvesTree[i].Add(tmpBaseCurve);
                }
            }
            GH_Structure<IGH_Goo> tree = listEdit.TurnDoubleListToDataTree(wholeFlyBaseCurvesTree);



            DA.SetDataTree(0, tree);
            DA.SetDataList(1, wholeZhengShenFlyBaseCurves);
            DA.SetData(2, fitCurve);
            DA.SetData(3, movedLinelikeCurve);

            //DA.SetDataList(0, yiJiaoYanChuanCorner);
            //DA.SetData(1, fitCurve);
            //DA.SetDataList(2, cornerPlanes);
            //DA.SetData(3, offsetedCurve);


        }

        private double GetMaxDisMinParam(Curve target, Curve path, double len)
        {
            path.Domain = new Interval(0, 1);
            List<double> middlePointsT = path.DivideByLength(len, false).ToList();
            List<Point3d> middlePoints = middlePointsT.Select(x => path.PointAt(x)).ToList();
            double maxDisMin = double.MaxValue;
            double maxDisMinParam = 0;
            for (int i = 0; i < middlePoints.Count; i++)
            {
                Curve fitCurve = NurbsCurve.Create(false, 2, new Point3d[] { target.PointAtStart, middlePoints[i], target.PointAtEnd });
                double maxDis = 0; double maxPA = 0; double maxPB = 0;
                double minDis = 0; double minPA = 0; double minPB = 0;
                Curve.GetDistancesBetweenCurves(fitCurve, target, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                    , out maxDis, out maxPA, out maxPB
                    , out minDis, out minPA, out minPB);
                if (maxDis < maxDisMin)
                {
                    maxDisMin = maxDis;
                    path.ClosestPoint(middlePoints[i], out maxDisMinParam);
                }
            }
            return maxDisMinParam;
        }

        private Point3d GetCorner0(GeometryInformation zaiJiaoLiangGI)
        {
            Brep brep = zaiJiaoLiangGI.brep;
            List<Surface> surfaces = brep.Surfaces.ToList();
            List<Point3d> centriods = brep.Surfaces
                .Select(x => AreaMassProperties.Compute(x.ToBrep()).Centroid)
                .ToList();
            Surface topSurface = brep.Surfaces.Zip(centriods, (surface, point) => new { S = surface, P = point })
                .OrderByDescending(x => PlaneUtility.subWorldPlaneForPoint3d(zaiJiaoLiangGI.referencePlane, x.P).Z)
                .Select(x => x.S)
                .FirstOrDefault();
            List<Point3d> vertices = topSurface.ToBrep().Vertices.Select(x => x.Location).ToList();
            List<Point3d> subPoints = PlaneUtility.subWorldPlaneForPoint3ds(zaiJiaoLiangGI.referencePlane, vertices);
            Point3d corner0 = vertices.Zip(subPoints, (p0, p1) => new { origin = p0, sub = p1 })
                .OrderBy(p => p.sub.X)
                .Take(2)
                .OrderByDescending(p => p.sub.Y)
                .Select(x => x.origin)
                .First();

            return corner0;
        }

        private Point3d GetCorner1(GeometryInformation zaiJiaoLiangGI)
        {
            Brep brep = zaiJiaoLiangGI.brep;
            List<Surface> surfaces = brep.Surfaces.ToList();
            List<Point3d> centriods = brep.Surfaces
                .Select(x => AreaMassProperties.Compute(x.ToBrep()).Centroid)
                .ToList();
            Surface topSurface = brep.Surfaces.Zip(centriods, (surface, point) => new { S = surface, P = point })
                .OrderBy(x => PlaneUtility.subWorldPlaneForPoint3d(zaiJiaoLiangGI.referencePlane, x.P).Z)
                .Select(x => x.S)
                .FirstOrDefault();
            List<Point3d> vertices = topSurface.ToBrep().Vertices.Select(x => x.Location).ToList();
            List<Point3d> subPoints = PlaneUtility.subWorldPlaneForPoint3ds(zaiJiaoLiangGI.referencePlane, vertices);
            Point3d corner1 = vertices.Zip(subPoints, (p0, p1) => new { origin = p0, sub = p1 })
                .OrderBy(p => p.sub.X)
                .Take(2)
                .OrderByDescending(p => p.sub.Y)
                .Select(x => x.origin)
                .First();

            return corner1;
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
            get { return new Guid("F28D6A43-F2AD-4850-AE2F-3E841588BD20"); }
        }
    }
}