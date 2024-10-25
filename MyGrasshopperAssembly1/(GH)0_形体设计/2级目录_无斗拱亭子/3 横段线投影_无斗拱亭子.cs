using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class 横段线投影_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 横段线投影 class.
        /// </summary>
        public 横段线投影_无斗拱亭子()
          : base("横段线投影_无斗拱亭子", "Nickname",
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
            pManager.AddGenericParameter("定位基线", "", "", GH_ParamAccess.list);
            pManager.AddPlaneParameter("投影平面", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基线", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> inputBaseCurves = new List<BaseCurve>();
            Plane plane = new Plane();
            if (!DA.GetDataList(0, inputBaseCurves)) return;
            if (!DA.GetData(1, ref plane)) return;

            List<Transform> xformList = inputBaseCurves
                .Select((x, i) => Transform.Translation(new Vector3d(0, 0, plane.Origin.Z - inputBaseCurves[i].referencePlane.Origin.Z)))
                .ToList();

            List<BaseCurve> baseCurves = inputBaseCurves
                .Select(x => x.Duplicate())
                .ToList();
            for (int i = 0; i < xformList.Count; i++)
            {
                baseCurves[i].Transform(xformList[i]);
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
            get { return new Guid("FF243E22-0BA2-4266-B809-E9A97D0DAC2A"); }
        }
    }
}