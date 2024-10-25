using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using System.Linq;

namespace MyGrasshopperAssembly1
{
    public class 角梁_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 角梁_小式亭子 class.
        /// </summary>
        public 角梁_无斗拱亭子()
          : base("取角梁线_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("金枋底定位基线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("檐檩顶定位基线", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("檐檩顶下沉", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("角梁线", "", "", GH_ParamAccess.list);
        }

        public override GH_Exposure Exposure => GH_Exposure.last;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> jinFangBottomBaseCurves = new List<BaseCurve>();
            List<BaseCurve> yanLingTopBaseCurves = new List<BaseCurve>();
            double down = 0;
            double kaiDuanChuTou = 0; double moDuanChuTou = 0;
            if (!DA.GetDataList(0, jinFangBottomBaseCurves) ||
                !DA.GetDataList(1, yanLingTopBaseCurves) ||
                !DA.GetData(2, ref down)) return;

            //金枋点
            List<BaseCurve> jinFangCoreBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < jinFangBottomBaseCurves.Count; i++)
            {
                List<BaseCurve> tmpCutters = jinFangBottomBaseCurves.Select(x => x.Duplicate()).ToList();
                tmpCutters.RemoveAt(i);
                jinFangCoreBaseCurves.Add(BaseCurveUtility.CurvesSplitAndGetCoreBaseCurve(tmpCutters.Select(x => x.curve).ToArray(), jinFangBottomBaseCurves[i]));
            }
            List<Point3d> jinFangPoints = jinFangCoreBaseCurves.Select(x => x.curve.PointAtStart).ToList();

            //檐檩点
            List<BaseCurve> yanLingCoreBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < yanLingTopBaseCurves.Count; i++)
            {
                List<BaseCurve> tmpCutters = yanLingTopBaseCurves.Select(x => x.Duplicate()).ToList();
                tmpCutters.RemoveAt(i);
                yanLingCoreBaseCurves.Add(BaseCurveUtility.CurvesSplitAndGetCoreBaseCurve(tmpCutters.Select(x => x.curve).ToArray(), yanLingTopBaseCurves[i]));
            }
            List<Point3d> yanLingPoints = yanLingCoreBaseCurves.Select(x => x.curve.PointAtStart).ToList();
            List<Point3d> yanLingDownPoints = yanLingPoints.Select(x => new Point3d(x.X, x.Y, x.Z - down)).ToList();

            //角梁方向的矢量
            List<Vector3d> directionList = new List<Vector3d>();
            for (int i = 0; i < jinFangPoints.Count; i++)
            {
                Vector3d tmpVector = new Vector3d(yanLingDownPoints[i] - jinFangPoints[i]);
                tmpVector.Unitize();
                directionList.Add(tmpVector);
            }

            //角梁长度列表
            List<double> lengthList = jinFangPoints
                .Select((x, i) => x.DistanceTo(yanLingDownPoints[i]) + kaiDuanChuTou + moDuanChuTou)
                .ToList();

            List<Point3d> jinFangMovedPoints = new List<Point3d>();
            for (int i = 0; i < jinFangPoints.Count; i++)
            {
                Vector3d tmpVector = new Vector3d(directionList[i]);
                tmpVector.Reverse();
                Point3d tmpPoint = new Point3d(jinFangPoints[i]);
                tmpPoint.Transform(Transform.Translation(tmpVector * kaiDuanChuTou));

                jinFangMovedPoints.Add(tmpPoint);
            }

            List<BaseCurve> jiaoLiangBaseCurves = BaseCurveUtility
                .CurvesToBaseCurves(jinFangMovedPoints
                .Select((x, i) => new Line(x, directionList[i], lengthList[i])
                .ToNurbsCurve() as Curve)
                .ToList());

            DA.SetDataList(0, jiaoLiangBaseCurves);
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
            get { return new Guid("FA4D9AA3-A767-40BC-8E96-F0A329C929B4"); }
        }
    }
}