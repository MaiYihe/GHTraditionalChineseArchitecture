using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using YhmAssembly;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using System.Linq;
using OutPutElement;
using Rhino.Geometry.Intersect;
using Rhino;
using MoreLinq;
using MoreLinq.Extensions;

namespace MyGrasshopperAssembly1
{
    public class 取飞椽望板面_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取飞椽望板面_小式亭子 class.
        /// </summary>
        public 取飞椽望板面_无斗拱亭子()
          : base("取飞椽望板面_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("翼角飞椽顶部线", "", "", GH_ParamAccess.tree);
            pManager.AddGenericParameter("正身飞椽顶部线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("飞椽尾部曲线", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("飞椽尾部直线", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("翼角飞椽面", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("正身飞椽面", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryInformation> zaiJiaoLiangGI = new List<GeometryInformation>();
            GH_Structure<IGH_Goo> yiJiaoTop = new GH_Structure<IGH_Goo>();
            List<BaseCurve> zhengShenTopBaseCurves = new List<BaseCurve>();
            Curve feiChuanCurvedCurve = default(Curve);
            Curve feiChuanLinelikeCurve = default(Curve);
            if (!DA.GetDataList(0, zaiJiaoLiangGI) ||
                !DA.GetDataTree(1, out yiJiaoTop) ||
                !DA.GetDataList(2, zhengShenTopBaseCurves) ||
                !DA.GetData(3, ref feiChuanCurvedCurve) ||
                !DA.GetData(4, ref feiChuanLinelikeCurve)) return;

            //得到仔角梁的边面
            Plane sidePlane = new Plane();
            GetSideSurface(zaiJiaoLiangGI[0]).TryGetPlane(out sidePlane);

            //取出yiJiaoTopBaseCurves与zhengShenTopBaseCurvesSide
            int n = yiJiaoTop.Branches[0].Count;//n边形
            List<BaseCurve> yiJiaoTopBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < yiJiaoTop.Branches.Count / 2; i++)
            {
                yiJiaoTopBaseCurves.Add((BaseCurve)yiJiaoTop.Branches[i][0]);
            }
            List<BaseCurve> zhengShenTopBaseCurvesSide = zhengShenTopBaseCurves.Take(zhengShenTopBaseCurves.Count / n).ToList();
            yiJiaoTopBaseCurves.Insert(0, zhengShenTopBaseCurvesSide[0]);

            ///翼角上的飞椽望板
            //飞椽翼角顶部连线
            Curve yiJiaoTopCurve = new Line(yiJiaoTopBaseCurves[0].curve.PointAtStart
                , yiJiaoTopBaseCurves[yiJiaoTopBaseCurves.Count - 1].curve.PointAtStart).ToNurbsCurve();
            Curve extendedYiJiaoTopCurve = yiJiaoTopCurve.Extend(CurveEnd.End, 2 * yiJiaoTopCurve.GetLength()
                , CurveExtensionStyle.Line);
            Point3d p = Intersection.CurvePlane(extendedYiJiaoTopCurve, sidePlane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].PointA;
            //求得NurbsCurve sideCurve
            Curve sideCurve = NurbsCurve
                .Create(false, 2, new Point3d[] { yiJiaoTopBaseCurves[0].curve.PointAtStart, p, feiChuanCurvedCurve.PointAtStart });

            //得到networkSurface
            int m = 0;
            Surface surface = NurbsSurface.CreateNetworkSurface(new Curve[] { yiJiaoTopBaseCurves[0].curve, sideCurve, feiChuanCurvedCurve }
                , 0, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                , RhinoDoc.ActiveDoc.PageAngleToleranceRadians, out m);
            BaseFace baseFace = BaseFaceUtility.SurfaceToBaseSurface(surface);

            //对称
            Transform mirrorTrans = Transform.Mirror(CurveUtility.GetPlanes(feiChuanLinelikeCurve, 0.5)[2]);
            BaseFace baseFaceMirrored = baseFace.Duplicate();
            baseFaceMirrored.Transform(mirrorTrans);
            baseFaceMirrored.referencePlane = new Plane(baseFaceMirrored.referencePlane.Origin
                , -baseFaceMirrored.referencePlane.XAxis, baseFaceMirrored.referencePlane.YAxis);

            ///正身上的飞椽望板
            Surface zhengShenSurface = Brep.CreateFromCornerPoints(zhengShenTopBaseCurvesSide[0].curve.PointAtStart
                , zhengShenTopBaseCurvesSide[0].curve.PointAtEnd
                , zhengShenTopBaseCurvesSide[zhengShenTopBaseCurvesSide.Count - 1].curve.PointAtEnd
                , zhengShenTopBaseCurvesSide[zhengShenTopBaseCurvesSide.Count - 1].curve.PointAtStart
                ,RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                .Surfaces[0];
            BaseFace zhengShenBaseFace = BaseFaceUtility.SurfaceToBaseSurface(zhengShenSurface);

            //原路返回
            //翼角
            List<Transform> backTransformList = zaiJiaoLiangGI
                .Select((x, i) => Transform.PlaneToPlane(zaiJiaoLiangGI[0].referencePlane, x.referencePlane))
                .ToList();
            List<List<BaseFace>> doubleList = new List<List<BaseFace>>();
            List<BaseFace> baseFaces = new List<BaseFace>();
            List<BaseFace> baseFacesMirrored = new List<BaseFace>();
            //正身
            List<BaseFace> zhengShenBaseFaces = new List<BaseFace>();
            for (int i = 0; i < backTransformList.Count; i++)
            {
                BaseFace tmpBaseFace = baseFace.Duplicate();
                BaseFace tmpBaseFaceMirrored = baseFaceMirrored.Duplicate();
                tmpBaseFace.Transform(backTransformList[i]);
                tmpBaseFaceMirrored.Transform(backTransformList[i]);
                baseFaces.Add(tmpBaseFace);
                baseFacesMirrored.Add(tmpBaseFaceMirrored);

                BaseFace tmpZhengShenBaseFace = zhengShenBaseFace.Duplicate();
                tmpZhengShenBaseFace.Transform(backTransformList[i]);
                zhengShenBaseFaces.Add(tmpZhengShenBaseFace);
            }
            doubleList.Add(baseFaces);
            doubleList.Add(baseFacesMirrored);
            GH_Structure<IGH_Goo> tree = listEdit.TurnDoubleListToDataTree(doubleList);


            DA.SetDataTree(0, tree);  
            DA.SetDataList(1, zhengShenBaseFaces);

        }

        private Surface GetSideSurface(GeometryInformation GI)
        {
            List<Point3d> centriods = GI.brep.Surfaces
                .Select(x => AreaMassProperties.Compute(x.ToBrep()).Centroid)
                .ToList();
            List<Surface> sideSurfaces = GI.brep.Surfaces.Select(x => (Surface)x).ToList();
            Surface sideSurface = sideSurfaces
                .Zip(centriods, (surface, point) => new { S = surface, P = point })
                .OrderByDescending(x => PlaneUtility.subWorldPlaneForPoint3d(GI.referencePlane, x.P).Z)
                .Select(x => x.S)
                .First();
            return sideSurface;
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
            get { return new Guid("F3426165-DF44-4B0C-8ED0-0582188F238E"); }
        }
    }
}