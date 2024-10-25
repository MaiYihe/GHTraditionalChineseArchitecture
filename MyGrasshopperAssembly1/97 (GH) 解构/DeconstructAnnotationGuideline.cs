using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class DeconstructAnnotationGuideline : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructAnnotationGuideline()
          : base("DeconstructAnnotationGuideline", "Nickname",
              "Description",
              "Yhm Toolbox", "解构")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AnnotationGuideline", "AG", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TopViewCurves", "TC", "...", GH_ParamAccess.list);
            pManager.AddGenericParameter("FrontViewCurves", "FC", "...", GH_ParamAccess.list);
            pManager.AddGenericParameter("RightViewCurves", "RC", "...", GH_ParamAccess.list);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AnnotationGuideCurves annotationGuideCurves = new AnnotationGuideCurves();
            DA.GetData(0, ref annotationGuideCurves);
            List<Curve> topViewCurves = annotationGuideCurves.topViewCurves;
            List<Curve> frontViewCurves = annotationGuideCurves.frontViewCurves;
            List<Curve> rightViewCurves = annotationGuideCurves.rightViewCurves;
            DA.SetDataList(0, topViewCurves);
            DA.SetDataList(1, frontViewCurves);
            DA.SetDataList(2, rightViewCurves);
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
            get { return new Guid("FFD557EA-A450-48E4-8444-DD79A6A576D2"); }
        }
    }
}