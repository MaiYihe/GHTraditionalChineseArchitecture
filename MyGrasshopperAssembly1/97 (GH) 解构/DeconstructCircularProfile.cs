using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class DeconstructCircularProfile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructCircularProfile class.
        /// </summary>
        public DeconstructCircularProfile()
          : base("DeconstructCircularProfile", "Nickname",
              "Description",
              "Yhm Toolbox", "解构")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CircularProfile", "CP", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Radius", "R", "...", GH_ParamAccess.item);
            pManager.AddNumberParameter("JinPan", "JP", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CircularProfile circularProfile = null;
            DA.GetData(0,ref circularProfile);
            DA.SetData(0, circularProfile.radius);
            DA.SetData(1, circularProfile.jinPan);
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
            get { return new Guid("94DDF3BB-C3C8-4461-9A06-F79454FC2AB3"); }
        }
    }
}