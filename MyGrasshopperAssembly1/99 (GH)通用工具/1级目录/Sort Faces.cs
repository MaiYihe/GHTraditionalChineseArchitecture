using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YhmAssembly.BrepUtility;

namespace MyGrasshopperAssembly1
{
    public class MyGrasshopperAssemblyComponent2 : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MyGrasshopperAssemblyComponent2()
          : base("Sort Faces", "Nickname",
            "Description",
            "Yhm Toolbox", "通用工具")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("East", "E", "...", GH_ParamAccess.item);
            pManager.AddBrepParameter("West", "W", "...", GH_ParamAccess.item);
            pManager.AddBrepParameter("South", "S", "...", GH_ParamAccess.item);
            pManager.AddBrepParameter("North", "N", "...", GH_ParamAccess.item);
            pManager.AddBrepParameter("Up", "U", "...", GH_ParamAccess.item);
            pManager.AddBrepParameter("Down", "D", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            DA.GetData(0, ref brep);

            Brep east = new Brep();
            Brep west = new Brep();
            Brep south = new Brep();
            Brep north = new Brep();
            Brep up = new Brep();
            Brep down = new Brep();

            SortFacesOfBox(brep, out east, out west, out south, out north, out up, out down);

            DA.SetData(0, east);
            DA.SetData(1, west);
            DA.SetData(2, south);
            DA.SetData(3, north);
            DA.SetData(4, up);
            DA.SetData(5, down);
        }
        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("B618F981-AF89-4E56-BA7B-5F4E2F37C5FE");

    }
}
