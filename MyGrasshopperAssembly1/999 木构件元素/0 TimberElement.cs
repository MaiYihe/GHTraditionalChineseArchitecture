using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using UtilityElement.Metadeta;
using UtilityElement.Metadeta.Dimensions;
using YhmAssembly;

namespace TimberElement
{
    /// <summary>
    /// 木元素抽象类
    /// </summary>
    public abstract class TimberElement
    {
        protected string name {  get; set; } // 构件名称
        protected string description { get; set; } // 构件尺寸等描述
        protected bool isExist {  get; set; }//构件是否存在
    }

}
