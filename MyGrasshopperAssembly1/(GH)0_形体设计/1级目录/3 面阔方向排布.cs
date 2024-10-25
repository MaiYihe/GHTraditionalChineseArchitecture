using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using YhmAssembly;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using static YhmAssembly.listEdit;
using UtilityElement.ReferenceDatum;//定位基准命名空间
using System.Net;
using System.Security.Cryptography;

namespace MyGrasshopperAssembly1
{
    public class 面阔方向排布 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public 面阔方向排布()
          : base("面阔方向排布", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("面阔", "MK", "以;分隔", GH_ParamAccess.item);
            pManager.AddPointParameter("柱顶点", "ZDD", "到梁架下", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        /// 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //MyCustomParam param = new MyCustomParam();
            pManager.AddNumberParameter("移动", "YD", "...", GH_ParamAccess.list);
            pManager.AddPointParameter("面阔方向柱顶点", "ZDD", "到梁架下", GH_ParamAccess.tree);
            pManager.AddPointParameter("外侧柱顶点", "WCZDD", "到梁架下", GH_ParamAccess.tree);
            pManager.AddPointParameter("内侧柱顶点", "NCZDD", "到梁架下", GH_ParamAccess.tree);
            pManager.Register_GenericParam("定位基线(檩向线)", "LCurve", "...", GH_ParamAccess.list);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string mianKuos = "";
            NegIndexList<double> miankuoArray = new NegIndexList<double>();
            List<double> move = new List<double>();

            List<Point3d> ZDD = new List<Point3d>();//柱顶点
            List<List<Point3d>> ZDDArray = new List<List<Point3d>>();//面阔方向各排柱顶点
            List<List<Point3d>> WCZDDArray = new List<List<Point3d>>();//外侧柱顶点
            List<List<Point3d>> NCZDDArray = new List<List<Point3d>>();//内侧柱顶点

            DA.GetData(0, ref mianKuos);
            DA.GetDataList(1, ZDD);

            List<string> mianKuoss = mianKuos.Split(';').ToList();//提取各间面阔到mianKuos，明次梢尽
            for (int i = 0; i < mianKuoss.Count; i++)
            {
                if (i == 0)
                {
                    miankuoArray.PositiveAdd(double.Parse(mianKuoss[i]));
                }
                else
                {
                    miankuoArray.PositiveAdd(double.Parse(mianKuoss[i]));
                    miankuoArray.NegtiveAdd(double.Parse(mianKuoss[i]));
                }
            }

            int watershed;
            double runningSum = 0.0;
            move = miankuoArray.ToNormal(out watershed).Aggregate(new List<double>(), (result, value) =>
            {
                runningSum += value;
                result.Add(runningSum);
                return result;
            });//累加
            move.Insert(0, 0.0);

            Vector3d translationVector = new Vector3d();
            Transform translation = new Transform();
            for (int i = 0; i < move.Count; i++)
            {
                translationVector = new Vector3d(0.0, move[i], 0.0);
                translation = Transform.Translation(translationVector);

                Point3d transformedPoint = new Point3d();
                List<Point3d> translatedPoints = new List<Point3d>();

                foreach (Point3d p in ZDD)
                {
                    transformedPoint = p;
                    transformedPoint.Transform(translation);
                    translatedPoints.Add(transformedPoint);
                }
                ZDDArray.Add(new List<Point3d>());
                ZDDArray[i].AddRange(translatedPoints);
            }

            WCZDDArray.AddRange(new List<List<Point3d>> { ZDDArray[0], ZDDArray[ZDDArray.Count - 1] });//得到两侧项
            NCZDDArray = ZDDArray.GetRange(1, ZDDArray.Count - 2);//得到中间项

            //连线
            List<Point3d> startPoints = WCZDDArray[0];
            List<Point3d> endPoints = WCZDDArray[1];
            List<Line> lines = startPoints.Zip(endPoints, (start, end) => new Line(start, end)).ToList();


            //判断当前电池是否被选中，若被选中则isSelected返回true
            GH_Document doc = OnPingDocument();
            bool isSelected = false;
            // 遍历文档中的所有对象
            foreach (IGH_DocumentObject obj in doc.SelectedObjects())
            {
                if (obj == this) // 如果当前组件被选中，则返回 true
                {
                    isSelected = true;
                }
            }

            //线赋予生成定位基线类
            List<BaseCurve> baseCurves = UtilityElement.BaseCurveUtility.CurvesToBaseCurves(isSelected, lines.Select(x => x.ToNurbsCurve() as Curve).ToList());

            GH_Structure<IGH_Goo> pts = TurnDoubleListToDataTree<Point3d>(ZDDArray);
            GH_Structure<IGH_Goo> WCpts = TurnDoubleListToDataTree<Point3d>(WCZDDArray);
            GH_Structure<IGH_Goo> NCpts = TurnDoubleListToDataTree<Point3d>(NCZDDArray);

            DA.SetDataList(0, move);
            DA.SetDataTree(1, pts);
            DA.SetDataTree(2, WCpts);
            DA.SetDataTree(3, NCpts);
            DA.SetDataList(4, baseCurves);
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
            get { return new Guid("42CC45FD-4142-43E9-808A-AC7E2C43B5B8"); }
        }
    }
}