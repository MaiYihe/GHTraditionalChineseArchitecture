using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取戗线_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取戗_雷公柱线_小式亭子 class.
        /// </summary>
        public 取戗线_无斗拱亭子()
          : base("取戗线_无斗拱亭子", "Nickname",
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
            pManager.AddPointParameter("戗续点", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("举架顶点", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("戗线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d juJiaTopPoint = new Point3d();
            List<Point3d> qiangPoints = new List<Point3d>();
            if (!DA.GetDataList(0, qiangPoints)||
                !DA.GetData(1, ref juJiaTopPoint)
                ) return;
            List<Curve> curves = qiangPoints.Select(x => new Line(juJiaTopPoint, x).ToNurbsCurve() as Curve).ToList();
            List<BaseCurve> baseCurves = BaseCurveUtility.CurvesToBaseCurves(curves);

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
            get { return new Guid("4D36A4E0-CACD-4329-B10F-F4A185E7E28E"); }
        }
    }
}