using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;

namespace YhmAssembly
{
    internal class ParamsManerger
    {
        /// <summary>
        /// 属性
        /// </summary>
        GH_Component _gH_Component;
        GH_Component gH_Component
        {
            get { return _gH_Component; }
            set
            {
                _gH_Component = value;
                inputs = _gH_Component.Params.Input;
                outputs = _gH_Component.Params.Output;
            }
        }
        List<IGH_Param> inputs { get; set; }//当gh_Component被set时关联生成
        List<IGH_Param> outputs { get; set; }//当gh_Component被set时关联生成
        public List<string[]> inputNickNames { get; set; }//输入参数名称列表
        public List<string[]> outputNickNames { get; set; }//输出参数名称列表


        /// <summary>
        /// 构造函数
        /// </summary>
        //空构造函数
        public ParamsManerger()
        {

        }
        //一般构造函数
        public ParamsManerger(ref GH_Component gH_Component)
        {
            this.gH_Component = gH_Component;
        }


        /// <summary>
        /// 方法
        /// </summary>
        //保持输入参数数量
        public void KeepInputParamsNum(int num)
        {
            while (inputs.Count < num)
            {
                IGH_Param new_param = CreateParameter(GH_ParameterSide.Input, inputs.Count);
                new_param.Optional = true;
                gH_Component.Params.RegisterInputParam(new_param, inputs.Count);
            }
            while (inputs.Count > num)
            {
                gH_Component.Params.UnregisterInputParameter(inputs[checked(inputs.Count - 1)]);
            }
            gH_Component.Params.OnParametersChanged();
        }
        //保持输出参数数量
        public void KeepOutputNum(int num)
        {
            while (outputs.Count < num)
            {
                IGH_Param new_param = CreateParameter(GH_ParameterSide.Output, outputs.Count);
                new_param.Optional = true;
                gH_Component.Params.RegisterOutputParam(new_param, outputs.Count);
            }
            while (outputs.Count > num)
            {
                gH_Component.Params.UnregisterOutputParameter(outputs[checked(outputs.Count-1)]);
            }
            gH_Component.Params.OnParametersChanged();
        }

        //切换到第index个nickName
        public void InputSwitch(int index)
        {
            if (inputNickNames.Count == 0) return;
            KeepInputParamsNum(inputNickNames[index].Count());
            for (int i = 0; i < inputNickNames[index].Count(); i++)
            {
                if (inputNickNames[index][i] != inputs[i].NickName)
                {
                    inputs[i].NickName = inputNickNames[index][i];
                }
            }
            gH_Component.Params.OnParametersChanged();
        }
        public void OutputSwitch(int index)
        {
            if (outputNickNames.Count == 0) return;
            KeepOutputNum(outputNickNames[index].Count());
            for(int i = 0; i < outputNickNames[index].Count(); i++)
            {
                if (outputNickNames[index][i] != outputs[i].NickName)
                {
                    outputs[i].NickName = outputNickNames[index][i];
                }
            }
            gH_Component.Params.OnParametersChanged();
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }
    }
}
