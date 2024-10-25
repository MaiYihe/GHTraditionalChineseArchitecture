using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取横段线中心平面_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取横段线中心 class.
        /// </summary>
        public 取横段线中心平面_无斗拱亭子()
          : base("取横段线中心平面_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.last;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基线", "BaseCurve", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.D
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("中心平面", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            if (!DA.GetDataList(0, baseCurves)) return;

            List<Point3d> points = baseCurves
                .Where(x => x != null && x.curve != null)
                .Select(x => x.curve.PointAt(0.5))
                .ToList();

            Point3d centerPoint = Point3dUtility.Point3dAverage(points);
            Plane centerPlane = new Plane(centerPoint, Plane.WorldXY.Normal);

            DA.SetData(0, centerPoint);

           
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
            get { return new Guid("812C8164-1A5B-49B4-ADD3-E457014C59B2"); }
        }
    }
}