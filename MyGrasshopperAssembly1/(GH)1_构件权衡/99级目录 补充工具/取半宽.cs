using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Render.DataSources;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 取半宽 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取半宽 class.
        /// </summary>
        public 取半宽()
          : base("取半宽", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("尺寸", "Dimension", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("正半宽", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("负半宽", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IDimension dimension = null;
            DA.GetData(0, ref dimension);
            CircularProfile circularProfile = dimension as CircularProfile;
            SquareProfile squareProfile = dimension as SquareProfile;
            SectionDimension sectionDimension = dimension as SectionDimension;
            FaceDimension faceDimension = dimension as FaceDimension;
            if (circularProfile != null)
            {
                DA.SetData(0, circularProfile.radius / 2);
                DA.SetData(1, -circularProfile.radius / 2);
            }
            if (squareProfile != null)
            {
                DA.SetData(0, squareProfile.b / 2);
                DA.SetData(1, -squareProfile.b / 2);
            }
            if (sectionDimension != null)
            {
                DA.SetData(0, sectionDimension);
            }
            if (faceDimension != null)
            {
                DA.SetData(0, faceDimension);
            }
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
            get { return new Guid("2EB7FA10-FF5C-4102-AA6C-301880C0D7F7"); }
        }
    }
}