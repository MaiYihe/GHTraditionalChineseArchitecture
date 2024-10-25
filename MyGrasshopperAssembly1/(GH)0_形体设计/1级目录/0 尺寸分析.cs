using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using IOComponents;
using Grasshopper.Kernel.Parameters;
using GH_IO.Serialization;
using UtilityElement.Metadeta.Dimensions;
using YhmAssembly;
using System.Windows.Forms;

namespace MyGrasshopperAssembly1
{
    public class 尺寸分析 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public 尺寸分析()
          : base("尺寸分析", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        private static readonly ParamDefinition[] inputs = new ParamDefinition[]
        {
        };
        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "-",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
        };

        public override GH_Exposure Exposure => GH_Exposure.primary;
        private List<List<(string, object)>> inputDataLists { get; set; }//menu下的所有层级
        private List<List<(string, Object)>> dataLists { get; set; }//menu下的所有层级
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Component currentComponent = this;
            double tongMiankuo = 0; double zhuGao = 0; double zhuJing = 0; double buJia = 0;

            string mianKuos0 = ""; string R1 = ""; string R2 = ""; string R3 = "";
            try
            {                
                //menu0的计算
                DA.GetData(0, ref mianKuos0);//面阔
                DA.GetData(1, ref R1);//柱高与面阔比
                DA.GetData(2, ref R2);//柱径与柱高比
                DA.GetData(3, ref R3);//步架与柱径比
                if (mianKuos0 != null && R1 != null && R2 != null && R3 != null)
                {
                    tongMiankuo = mianKuos0.Split(';').Select(x => double.Parse(x)).Sum();//输入字符串没有;时会返回原先的字符
                    List<double> R1_1 = R1.Split(':').Select(s => double.Parse(s)).ToList();//提取柱高与面阔比
                    List<double> R2_1 = R2.Split(':').Select(s => double.Parse(s)).ToList();//提取柱径与柱高比
                    List<double> R3_1 = R3.Split(':').Select(s => double.Parse(s)).ToList();//提取步架与柱径比
                    zhuGao = tongMiankuo * R1_1[0] / R1_1[1];
                    zhuJing = zhuGao * R2_1[0] / R2_1[1];
                    buJia = zhuJing * R3_1[0] / R3_1[1];
                }                
            }
            catch { }

            string mianKuos1 = ""; string RR1 = ""; string RR2 = "";string RR3 = "";
            try 
            {                
                //menu1的计算                
                DA.GetData(0, ref mianKuos1);//面阔
                DA.GetData(1, ref RR1);//柱高与面阔比
                DA.GetData(2, ref RR2);//柱径与柱高比
                DA.GetData(3, ref RR3);//步架与柱径比
                if (mianKuos1 != null)
                {
                    List<double> RR1_1 = R1.Split(':').Select(s => double.Parse(s)).ToList();//提取柱高与面阔比
                    List<double> RR2_1 = R2.Split(':').Select(s => double.Parse(s)).ToList();//提取柱径与柱高比
                    List<double> RR3_1 = R3.Split(':').Select(s => double.Parse(s)).ToList();//提取步架与柱径比
                    zhuGao = double.Parse(mianKuos1) * RR1_1[0] / RR1_1[1];
                    zhuJing = zhuGao * RR2_1[0] / RR2_1[1];
                    buJia = zhuJing * RR3_1[0] / RR3_1[1];
                }
            }
            catch { }

            //input,nickName,menu管理
            List<List<(string, object)>> inputDataLists = new List<List<(string, object)>>();
            List<(string, Object)> inputDataList0 = new List<(string, object)>
            {//menu0
                ("面阔",mianKuos0),
                ("柱高与面阔比",R1),
                ("柱径与柱高比",R2),
                ("步架与柱径比",R3),
            };
            List<(string, Object)> inputDataList1 = new List<(string, object)>
            {//menu1
                ("面阔",mianKuos1),
                ("柱高与面阔比",RR1),
                ("柱径与柱高比",RR2),
                ("步架与柱径比",RR3),
            };
            inputDataLists.Add(inputDataList0);
            inputDataLists.Add(inputDataList1);
            this.inputDataLists = inputDataLists;


            //output,nickName,outputResult,menu管理
            List<List<(string, object)>> dataLists = new List<List<(string, Object)>>();
            List<(string, Object)> dataList0 = new List<(string, object)>
            {//menu0
                ("通面阔",tongMiankuo),
                ("柱高",zhuGao),
                ("柱径",zhuJing),
                ("步架",buJia),
            };
            List<(string, Object)> dataList1 = new List<(string, object)>
            {//menu1
                ("柱高",zhuGao),
                ("柱径",zhuJing),
                ("步架",buJia),
            };
            dataLists.Add(dataList0);
            dataLists.Add(dataList1);
            this.dataLists = dataLists;


            //output的SolveInstance的输出参数管理
            OutputParamsManager outputParamsManager = new OutputParamsManager(ref currentComponent, ref DA);
            outputParamsManager.AddToDictionary(dataList0, dataList1);
            outputParamsManager.LookupDictionaryAndOutput();//找到对映的字典并输出
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_Component currentComponent = this;//获取当前组件
            MenuManager menuManager = new MenuManager(ref currentComponent, ref menu);//新建空菜单
            string[] menuItemNames = new string[]
            {
                "屋架建筑",
                "亭类建筑",
            };//菜单内容
            menuManager.menuItemNames = menuItemNames;//赋予名称的同时创建出GH中的菜单与eventHandlers、menuState

            for (int i = 0; i < menuItemNames.Count(); i++)
            {
                if (dataLists == null || inputDataLists == null) return;
                OutputParamsManager outputParamsManager = new OutputParamsManager(ref currentComponent, dataLists[i]);
                InputParamsManager inputParamsManager = new InputParamsManager(ref currentComponent, inputDataLists[i]);
                Action action0 = () => outputParamsManager.ResetOutputsParams();
                Action action1 = () => inputParamsManager.ResetInputsParams();
                menuManager.SetMenuItemAction(i, action0,action1);//为menu自定义的第i项设置操作方法
            }
            menuManager.UpdateMenu();
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
            get { return new Guid("E15CE6DA-2968-442A-9490-524F7AACD2AF"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}