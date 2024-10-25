using Eto.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using static Rhino.Geometry.BrepFace;
using static YhmAssembly.BrepUtility;
using static YhmAssembly.listEdit;
using static YhmAssembly.BrepUtility01;

namespace MyGrasshopperAssembly1
{
    public class SolidCutWithCurve : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SolidCutWithCurve()
          : base("SolidCutWithCurve", "Nickname",
            "Description",
            "Yhm Toolbox", "通用工具")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {  
            pManager.AddBrepParameter("Brep","B","...",GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "...", GH_ParamAccess.item);
            pManager.AddCurveParameter("Cutters", "C", "...", GH_ParamAccess.list);                                  
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("BrepCutters","B","...",GH_ParamAccess.list);
            pManager.AddCurveParameter("Sections", "S", "...", GH_ParamAccess.list);
            pManager.AddBrepParameter("OpenBreps", "OB", "...", GH_ParamAccess.list);
            pManager.AddBrepParameter("SplitedCutters", "SC", "...", GH_ParamAccess.list);
            pManager.AddBrepParameter("InsideSurfaces", "IS", "...", GH_ParamAccess.list);
            pManager.AddBrepParameter("Solids", "S", "...", GH_ParamAccess.list);
            pManager.AddBrepParameter("coreBrep", "CB", "...", GH_ParamAccess.list);
            pManager.AddBrepParameter("sideBreps", "SB", "...", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep originalBrep = null;
            Plane? plane = null;
            List<Curve> cutters = new List<Curve>();

            DA.GetData(0, ref originalBrep);
            DA.GetData(1, ref plane);
            DA.GetDataList(2, cutters);

            List<Brep> coreBrep, sideBreps, brepCutters, splitedBreps, splitedCutters, surfacesToJoin, solids;
            List<Curve> intersectCurves;
            SolidCutWithCurveALLRound(originalBrep, plane, cutters, out coreBrep, out sideBreps, out brepCutters, out splitedBreps, out intersectCurves, out splitedCutters, out surfacesToJoin, out solids);

            //Brep[] CutBrep = GetCutBrep();

            //Brep centriodBrep;
            //List<Brep> sideBreps;
            //GetCutBrepss(originalBrep, brepsList, out centriodBrep, out sideBreps);

            //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"???");
            DA.SetDataList(0, brepCutters);
            DA.SetDataList(1, intersectCurves);
            DA.SetDataList(2, splitedBreps);
            DA.SetDataList(3, splitedCutters);
            DA.SetDataList(4, surfacesToJoin);
            DA.SetDataList(5, solids);
            DA.SetDataList(6, coreBrep);
            DA.SetDataList(7, sideBreps);
        }
      


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f1b2c071-090e-400c-b723-262ed4fb6c91");
    }
}