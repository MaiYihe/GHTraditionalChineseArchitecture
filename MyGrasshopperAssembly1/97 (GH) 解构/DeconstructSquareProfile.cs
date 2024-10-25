using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;
using System.Windows.Forms;
using GH_IO.Serialization;
using IOComponents;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace MyGrasshopperAssembly1
{
    public class DeconstructSquareProfile : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructCircularProfile class.
        /// </summary>
        public DeconstructSquareProfile()
          : base("DeconstructSquareProfile", "Nickname",
              "Description",
              "Yhm Toolbox", "解构")
        {
        }
        private static readonly ParamDefinition[] inputs = new ParamDefinition[1]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "SquareProfile",
                NickName = "SP",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true
            }),
        };

        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "Height",
                NickName = "H",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "Width",
                NickName = "W",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true
            }),
        };
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// menuState全局参数
        /// </summary>
        private bool[] menuState = { false, true, true };
        private int trueButton = -1;//就是按下灰显的button，-1表示还没运行过

        // 保存参数值到 Grasshopper 文件
        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("trueButton", trueButton); // 保存参数值
            return base.Write(writer);
        }
        // 从 Grasshopper 文件加载参数值
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetInt32("trueButton", ref trueButton); // 从文件中读取参数值
            return base.Read(reader);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (trueButton == -1) HeightAndWidth(ref DA);
            else
            {
                menuState.Select((x, i) => i == trueButton);//第trueButtonx项设为true
                switch (trueButton)
                {
                    case 0:
                        HeightAndWidth(ref DA);
                        break;
                    case 1:
                        HeightAndWidthAndExpansions(ref DA);
                        break;
                    case 2:
                        Expansions(ref DA);
                        break;
                }
                //this.Params.OnParametersChanged();
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_DocumentObject.Menu_AppendItem(menu, "height and width", HeightAndWidth, menuState[0], @checked: false);
            GH_DocumentObject.Menu_AppendItem(menu, "height and width and expansions", HeightAndWidthAndExpansions, menuState[1], @checked: false);
            GH_DocumentObject.Menu_AppendItem(menu, "expansions", Expansions, menuState[2], @checked: false);
        }
        private void HeightAndWidthAndExpansions(ref IGH_DataAccess DA)
        {
            trueButton = 1;
            menuState = menuState.Select((x, i) => i != trueButton).ToArray();
            if (base.Params.Output.Count == 2)
            {
                //注册前面两个输出参数
                IGH_Param new_param0 = CreateParameter(GH_ParameterSide.Output, 0);
                IGH_Param new_param1 = CreateParameter(GH_ParameterSide.Output, 0);
                base.Params.RegisterOutputParam(new_param0);
                base.Params.RegisterOutputParam(new_param1);
                //为输出参数取名
                base.Params.Output[0].NickName = "Expansion0";
                base.Params.Output[1].NickName = "Expansion1";
                base.Params.Output[2].NickName = "Height";
                base.Params.Output[3].NickName = "Width";
                this.Params.OnParametersChanged();
            }
            if (base.Params.Input[0].VolatileDataCount > 0)
            {
                //获取squareProfile
                SquareProfile squareProfile = null;
                List<IGH_Goo> gH_Goos = base.Params.Input[0].VolatileData.AllData(true).ToList();
                try
                {
                    gH_Goos[0].CastTo<SquareProfile>(out squareProfile);
                }
                catch
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "squareProfile为空！");
                }
                //为输出参数赋值
                DA.SetData(0, squareProfile.expansion0);
                DA.SetData(1, squareProfile.expansion1);
                DA.SetData(2, squareProfile.h);
                DA.SetData(3, squareProfile.b);
            }
        }
        private void HeightAndWidthAndExpansions(object sender, EventArgs e)
        {
            trueButton = 1;
            menuState = menuState.Select((x, i) => i != trueButton).ToArray();
            if (base.Params.Output.Count == 2)
            {
                //注册前面两个输出参数
                IGH_Param new_param0 = CreateParameter(GH_ParameterSide.Output, 0);
                IGH_Param new_param1 = CreateParameter(GH_ParameterSide.Output, 0);
                base.Params.RegisterOutputParam(new_param0);
                base.Params.RegisterOutputParam(new_param1);
                //为输出参数取名
                base.Params.Output[0].NickName = "Expansion0";
                base.Params.Output[1].NickName = "Expansion1";
                base.Params.Output[2].NickName = "Height";
                base.Params.Output[3].NickName = "Width";
                this.Params.OnParametersChanged();
            }
            if (base.Params.Input[0].VolatileDataCount > 0)
            {
                //获取squareProfile
                SquareProfile squareProfile = null;
                List<IGH_Goo> gH_Goos = base.Params.Input[0].VolatileData.AllData(true).ToList();
                gH_Goos[0].CastTo<SquareProfile>(out squareProfile);
                //为输出参数赋值
                GH_Structure<IGH_Goo> gH_StructureOutput0 = (GH_Structure<IGH_Goo>)this.Params.Output[0].VolatileData;
                GH_Structure<IGH_Goo> gH_StructureOutput1 = (GH_Structure<IGH_Goo>)this.Params.Output[1].VolatileData;
                GH_Structure<IGH_Goo> gH_StructureOutput2 = (GH_Structure<IGH_Goo>)this.Params.Output[2].VolatileData;
                GH_Structure<IGH_Goo> gH_StructureOutput3 = (GH_Structure<IGH_Goo>)this.Params.Output[3].VolatileData;
                gH_StructureOutput0.Clear();
                gH_StructureOutput1.Clear();
                gH_StructureOutput2.Clear();
                gH_StructureOutput3.Clear();
                gH_StructureOutput0.Append(new GH_Number(squareProfile.expansion0));
                gH_StructureOutput1.Append(new GH_Number(squareProfile.expansion1));
                gH_StructureOutput2.Append(new GH_Number(squareProfile.h));
                gH_StructureOutput3.Append(new GH_Number(squareProfile.b));
                this.Params.OnParametersChanged();
            }
        }
        private void HeightAndWidth(ref IGH_DataAccess DA)
        {
            if (base.Params.Input[0].VolatileDataCount == 0)
            {
                return;
            }
            else trueButton = 0;
            menuState = menuState.Select((x, i) => i != trueButton).ToArray();
            if (base.Params.Output.Count > 2)
            {
                //取消注册前面两个输出参数
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
            }
            //为输出参数取名
            base.Params.Output[0].NickName = "Height";
            base.Params.Output[1].NickName = "Width";
            this.Params.OnParametersChanged();

            if (base.Params.Input[0].VolatileDataCount > 0)
            {
                //获取squareProfile
                SquareProfile squareProfile = null;
                List<IGH_Goo> gH_Goos = base.Params.Input[0].VolatileData.AllData(true).ToList();
                try
                {
                    gH_Goos[0].CastTo<SquareProfile>(out squareProfile);
                }
                catch
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "squareProfile为空！");
                }

                //为输出参数赋值
                if (squareProfile !=null)
                {
                    DA.SetData(0, squareProfile.h);
                    DA.SetData(1, squareProfile.b);
                }
            }
        }
        private void HeightAndWidth(object sender, EventArgs e)
        {
            trueButton = 0;
            menuState = menuState.Select((x, i) => i != trueButton).ToArray();
            if (base.Params.Output.Count > 2)
            {
                //取消注册前面两个输出参数
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
            }
            //为输出参数取名
            base.Params.Output[0].NickName = "Height";
            base.Params.Output[1].NickName = "Width";
            this.Params.OnParametersChanged();

            if (base.Params.Input[0].VolatileDataCount > 0)
            {
                //获取squareProfile
                SquareProfile squareProfile = null;
                List<IGH_Goo> gH_Goos = base.Params.Input[0].VolatileData.AllData(true).ToList();
                gH_Goos[0].CastTo<SquareProfile>(out squareProfile);
                //为输出参数赋值
                GH_Structure<IGH_Goo> gH_StructureOutput0 = (GH_Structure<IGH_Goo>)this.Params.Output[0].VolatileData;
                GH_Structure<IGH_Goo> gH_StructureOutput1 = (GH_Structure<IGH_Goo>)this.Params.Output[1].VolatileData;
                gH_StructureOutput0.Clear();
                gH_StructureOutput1.Clear();
                gH_StructureOutput0.Append(new GH_Number(squareProfile.h));
                gH_StructureOutput1.Append(new GH_Number(squareProfile.b));
                this.Params.OnParametersChanged();
            }
        }
        private void Expansions(ref IGH_DataAccess DA)
        {
            trueButton = 2;
            menuState = menuState.Select((x, i) => i != trueButton).ToArray();
            if (base.Params.Output.Count > 2)
            {
                //取消注册前面两个输出参数
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
            }
            //为输出参数取名
            base.Params.Output[0].NickName = "Extension0";
            base.Params.Output[1].NickName = "Extension1";
            this.Params.OnParametersChanged();
            if (base.Params.Input[0].VolatileDataCount > 0)
            {
                //获取squareProfile
                SquareProfile squareProfile = null;
                List<IGH_Goo> gH_Goos = base.Params.Input[0].VolatileData.AllData(true).ToList();
                gH_Goos[0].CastTo<SquareProfile>(out squareProfile);
                //为输出参数赋值
                DA.SetData(0, squareProfile.expansion0);
                DA.SetData(1, squareProfile.expansion1);
            }
        }
        private void Expansions(object sender, EventArgs e)
        {
            trueButton = 2;
            menuState = menuState.Select((x, i) => i != trueButton).ToArray();
            if (base.Params.Output.Count > 2)
            {
                //取消注册前面两个输出参数
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
                base.Params.UnregisterOutputParameter(base.Params.Output[checked(0)]);
            }
            //为输出参数取名
            base.Params.Output[0].NickName = "Extension0";
            base.Params.Output[1].NickName = "Extension1";
            this.Params.OnParametersChanged();
            if (base.Params.Input[0].VolatileDataCount > 0)
            {
                //获取squareProfile
                SquareProfile squareProfile = null;
                List<IGH_Goo> gH_Goos = base.Params.Input[0].VolatileData.AllData(true).ToList();
                gH_Goos[0].CastTo<SquareProfile>(out squareProfile);
                //为输出参数赋值
                GH_Structure<IGH_Goo> gH_StructureOutput0 = (GH_Structure<IGH_Goo>)this.Params.Output[0].VolatileData;
                GH_Structure<IGH_Goo> gH_StructureOutput1 = (GH_Structure<IGH_Goo>)this.Params.Output[1].VolatileData;
                gH_StructureOutput0.Clear();
                gH_StructureOutput1.Clear();
                gH_StructureOutput0.Append(new GH_Number(squareProfile.h));
                gH_StructureOutput1.Append(new GH_Number(squareProfile.b));
                this.Params.OnParametersChanged();
            }
        }

        public override IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
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
            get { return new Guid("94DDF3BB-C3C8-4461-9A06-F79454FC2AB4"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}