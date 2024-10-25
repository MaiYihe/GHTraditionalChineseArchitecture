using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class 取角梁线_重檐_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取角梁线_重檐_无斗拱亭子 class.
        /// </summary>
        public 取角梁线_重檐_无斗拱亭子()
          : base("取角梁线_重檐_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("承椽枋端下沉", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("檐檩顶端下沉", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基线","","",GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> chengChuanFangBaseCurves = new List<BaseCurve>();
            List<BaseCurve> yanLingTopBaseCurves = new List<BaseCurve>();
            double chengChuanFangOffset = 0;
            double yanLingTopOffset = 0;
            if (!DA.GetDataList(0, chengChuanFangBaseCurves) ||
                !DA.GetDataList(1, yanLingTopBaseCurves) ||
                !DA.GetData(2, ref chengChuanFangOffset) ||
                !DA.GetData(3, ref yanLingTopOffset)) return;
            List<BaseCurve> chengChuanCoreBaseCurves = 取横段内交线_无斗拱亭子.GetCoreBaseCurves(chengChuanFangBaseCurves);
            List<BaseCurve> yanLingCoreBaseCurves = 取横段内交线_无斗拱亭子.GetCoreBaseCurves(yanLingTopBaseCurves);
            List<Point3d> chengChuanStartPoints = chengChuanCoreBaseCurves.Select(x => x.curve.PointAtStart).ToList();
            List<Point3d> yanLingStartPoints = yanLingCoreBaseCurves.Select(x => x.curve.PointAtStart).ToList();
            Transform chengChuanTrans = Transform.Translation(new Vector3d(0, 0, -chengChuanFangOffset));
            Transform yanLingTrans = Transform.Translation(new Vector3d(0, 0, -yanLingTopOffset));

            List<BaseCurve> baseCurves = new List<BaseCurve>();
            for (int i = 0; i < chengChuanStartPoints.Count; i++)
            {
                Point3d tmpChengChuan = chengChuanStartPoints[i];
                tmpChengChuan.Transform(chengChuanTrans);
                Point3d tmpYanLing = yanLingStartPoints[i];
                tmpYanLing.Transform(yanLingTrans);
                BaseCurve tmpBaseCurve = BaseCurveUtility
                    .CurveToBaseCurve(new Line(tmpChengChuan, tmpYanLing).ToNurbsCurve());
                baseCurves.Add(tmpBaseCurve);
            }

            DA.SetDataList(0, baseCurves);
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
            get { return new Guid("1DE32B1D-7B2B-4479-8201-0285676AA8FA"); }
        }
    }
}