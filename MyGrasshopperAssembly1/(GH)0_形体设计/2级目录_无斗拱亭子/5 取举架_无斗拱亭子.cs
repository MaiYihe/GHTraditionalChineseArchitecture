using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using System.Linq;
using YhmAssembly;
using UtilityElement;

namespace MyGrasshopperAssembly1
{
    public class 取举架_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取举架_小式亭子 class.
        /// </summary>
        public 取举架_无斗拱亭子()
          : base("取举架_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("金檩顶定位基线", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("檐檩顶定位基线", "", "", GH_ParamAccess.list);
            pManager.AddTextParameter("檐举架与脊举架比", "以“：”分隔", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("举架", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("举架顶点", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> jinLingTopBaseCurves = new List<BaseCurve>();
            List<BaseCurve> yanLingTopBaseCurves = new List<BaseCurve>();
            string R1 = "";
            if (!DA.GetDataList(0, jinLingTopBaseCurves) ||
                !DA.GetDataList(1, yanLingTopBaseCurves) ||
                !DA.GetData(2, ref R1)) return;
            List<double> R1_1 = R1.Split(':').Select(s => double.Parse(s)).ToList();
            double rate = R1_1[1] / R1_1[0];

            //举架
            List<double> juJia = new List<double>();
            //檐举架
            double jujia0 = jinLingTopBaseCurves[0].referencePlane.Origin.Z - yanLingTopBaseCurves[0].referencePlane.Origin.Z;
            //脊举架
            double jujia1 = jujia0 * rate;
            juJia.Add(jujia0); juJia.Add(jujia1);

            //金檩中心点
            Point3d jinLingCenter = Point3dUtility.Point3dAverage(jinLingTopBaseCurves.Select(x => x.curve.PointAtStart).ToList());
            //举架顶部点
            Point3d topPoint = new Point3d(jinLingCenter.X, jinLingCenter.Y, jinLingCenter.Z + jujia1);

            DA.SetDataList(0, juJia);
            DA.SetData(1, topPoint);
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
            get { return new Guid("60D76178-0E70-47C3-9371-33B4C88F95AB"); }
        }
    }
}