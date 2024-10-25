using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;
using UtilityElement;

namespace MyGrasshopperAssembly1
{
    public class DeconstructBaseFace : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructBaseFace()
          : base("DeconstructBaseFace", "Nickname",
              "Description",
              "Yhm Toolbox", "解构")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BaseFace", "BF", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Brep", "B", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("ReferencePlane", "RP", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("Positive", "P", "...", GH_ParamAccess.item);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BaseFace baseFace = new BaseFace();
            DA.GetData(0,ref baseFace);
            Brep brep = baseFace.brep;
            Plane referencePlane = baseFace.referencePlane;
            Curve vector3dCurve = BaseCurveUtility.Vector3dToLine(baseFace.positiveDirection, referencePlane.Origin);

            DA.SetData(0, brep);
            DA.SetData(1, referencePlane);
            DA.SetData(2, vector3dCurve);
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
            get { return new Guid("B516FC76-39CE-4666-8B41-C0EE292FA840"); }
        }
    }
}