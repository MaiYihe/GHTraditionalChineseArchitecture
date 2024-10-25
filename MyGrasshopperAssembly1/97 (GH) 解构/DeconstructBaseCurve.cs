using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using UtilityElement;

namespace MyGrasshopperAssembly1
{
    public class DeconstructBaseCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructBaseCurve()
          : base("DeconstructBaseCurve", "Nickname",
              "Description",
              "Yhm Toolbox", "解构")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BaseCurve", "BC", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Curve", "C", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("ReferencePlane", "RP", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("Positive", "P", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("ExtrudePlane", "EP", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BaseCurve baseCurve = new BaseCurve();
            DA.GetData(0,ref baseCurve);

            Curve curve = baseCurve.curve;
            Plane referencePlane = baseCurve.referencePlane;
            Vector3d positoveDorection = baseCurve.positiveDirection;
            Plane extrudePlane = baseCurve.extrudePlane;
            Curve vector3dCurve = BaseCurveUtility.Vector3dToLine(positoveDorection,referencePlane.Origin);

            DA.SetData(0, curve);
            DA.SetData(1, referencePlane);
            DA.SetData(2, vector3dCurve);
            DA.SetData(3, extrudePlane);
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
            get { return new Guid("B516FC76-39CE-4666-8B41-C0EE292FA844"); }
        }
    }
}