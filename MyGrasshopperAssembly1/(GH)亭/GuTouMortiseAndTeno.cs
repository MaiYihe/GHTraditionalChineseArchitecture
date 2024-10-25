using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using static YhmAssembly.BrepUtility;
using static YhmAssembly.listEdit;
using static YhmAssembly.BrepUtility01;

namespace MyGrasshopperAssembly1
{
    public class GuTouMortiseAndTeno : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GuTouMortiseAndTeno()
          : base("GuTouMortiseAndTeno", "Nickname",
              "Description",
              "Yhm Toolbox", "亭")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("EavesFascia", "EF", "檐枋", GH_ParamAccess.item);
            pManager.AddBrepParameter("Columns", "C", "柱子", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "定位平面", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Outside", "OS", "柱外部分", GH_ParamAccess.list);
            pManager.AddBrepParameter("Iutside", "IS", "柱内部分", GH_ParamAccess.list);
            pManager.AddBrepParameter("Between", "BW", "柱间部分", GH_ParamAccess.list);
            pManager.AddBrepParameter("Result","R","上述三者Union",GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep EF = new Brep();
            List<Brep> C = new List<Brep>();
            Plane plane = new Plane();
            DA.GetData(0,ref EF);
            DA.GetDataList(1,C);
            DA.GetData(2,ref plane);

            //求出到WorldXY的Transform，存储在T；返回的存储在backT
            Transform T = new Transform();
            Transform backT = new Transform();
            T = Transform.PlaneToPlane(plane,Plane.WorldXY);
            backT = Transform.PlaneToPlane(Plane.WorldXY, plane);

            //求出柱的圆形截面,transform到XY平面，存储到list<Curve> circleSections中
            List<Curve> circleSections = new List<Curve>();
            foreach(Brep c in C)
            {
                Curve tmpC = GetBrepCircleSection(c);
                tmpC.Transform(T);
                circleSections.Add(tmpC);
            }

            //得出相交部分，即柱内部分，transform到XY平面,存储在List<Brep> intersectionParts
            List<Brep> EFList = new List<Brep> { EF };
            List<Brep> intersectionParts = SolidIntersection(EFList, C);
            foreach (Brep intersectionPart in intersectionParts) 
            {
                intersectionPart.Transform(T);
            }

            //得出非相交部分，transform到XY平面，即柱间部分(coreBrep)与柱外部分(sideBreps)
            List<Brep> coreBrep, sideBreps, brepCutters, splitedBreps, splitedCutters, surfacesToJoin, solids;
            List<Curve> intersectCurves;
            SolidCutWithCurveALLRound(EF, plane, circleSections, out coreBrep, out sideBreps, out brepCutters, out splitedBreps, out intersectCurves, out splitedCutters, out surfacesToJoin, out solids);
            foreach(Brep brep in coreBrep)
            {
                brep.Transform(T);
            }
            foreach(Brep brep in sideBreps)
            {
                brep.Transform(T);
            }

            DA.SetDataList(0, sideBreps);
            DA.SetDataList(1, intersectionParts);
            DA.SetDataList(2, coreBrep);


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
            get { return new Guid("5B615232-F645-4D94-9ED8-3F53098F1E01"); }
        }
    }
}