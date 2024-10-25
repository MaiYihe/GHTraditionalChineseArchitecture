using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using OutPutElement;
using UtilityElement.Metadeta;

namespace MyGrasshopperAssembly1
{
    public class DeconstructGeometryInformation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructGeometryInformation()
          : base("DeconstructGeometryInformation", "Nickname",
              "Description",
              "Yhm Toolbox", "解构")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GeometryInformation", "GI", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Brep", "B", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("AnnotationGuideline", "AG", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("ReferencePlane", "RP", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("Description", "D", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("BaseCurve", "BC", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryInformation geometryInformation = new GeometryInformation();
            DA.GetData(0, ref geometryInformation);

            DA.SetData(0, geometryInformation.brep);
            DA.SetData(1, geometryInformation.annotationGuideCurves);
            DA.SetData(2, geometryInformation.referencePlane);
            DA.SetData(3, geometryInformation.description);
            DA.SetData(4, geometryInformation.baseCurve);
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
            get { return new Guid("4AB531DF-2BB4-4803-8F1A-48EA6BB64427"); }
        }
    }
}