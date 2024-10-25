using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class DeconstructBaseAxis : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructBaseAxis class.
        /// </summary>
        public DeconstructBaseAxis()
          : base("DeconstructBaseAxis", "Nickname",
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
            pManager.AddGenericParameter("BaseAxis", "BA", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AxisCurve", "C", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("ReferencePlane", "RP", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("Positive", "P", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BaseAxis baseAxis = new BaseAxis();
            if (!DA.GetData(0, ref baseAxis)) return;

            Curve vector3dCurve = BaseCurveUtility.Vector3dToLine(baseAxis.positiveDirection, baseAxis.referencePlane.Origin);

            DA.SetData(0, baseAxis.curve);
            DA.SetData(1, baseAxis.referencePlane);
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
            get { return new Guid("ECDC397C-9A7D-4CD9-9CE7-BAD442F20FEB"); }
        }
    }
}