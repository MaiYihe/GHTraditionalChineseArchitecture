using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using static YhmAssembly.CurveUtility;

namespace MyGrasshopperAssembly1
{
    public class CurvePlaneSystem : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CurvePlaneSystem()
          : base("CurvePlaneSystem", "Nickname",
              "Description",
              "Yhm Toolbox", "通用工具")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "...", GH_ParamAccess.item);
            pManager.AddNumberParameter("Parameter", "P", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("P1","P","...",GH_ParamAccess.item);
            pManager.AddPlaneParameter("P2", "P", "...", GH_ParamAccess.item);
            pManager.AddPlaneParameter("P3", "P", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = default(Curve);
            double parameter = 0;
            DA.GetData(0,ref curve);
            DA.GetData(1, ref parameter);

            List<Plane> planes = GetPlanes(curve,parameter);
            DA.SetData(0, planes[0]);
            DA.SetData(1, planes[1]);
            DA.SetData(2, planes[2]);
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
            get { return new Guid("9A612B06-667C-42B0-AA5A-5774CA2CA228"); }
        }
    }
}