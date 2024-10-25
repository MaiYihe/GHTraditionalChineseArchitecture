using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityElement.Metadeta.Dimensions;
using UtilityElement.Metadeta;
using UtilityElement.ReferenceDatum;
using UtilityElement;
using YhmAssembly;
using static YhmAssembly.CurveUtility;

namespace TimberElement
{
    /// <summary>
    /// 木元素抽象类.剖面类抽象类
    /// </summary>
    public abstract class TimberSection : TimberElement
    {
        //几何图元字段，起到缓存结果的作用
        protected Brep timberBrep;        
        //标注辅助线
        private AnnotationGuideCurves annotationGuideCurves;//字段
        public AnnotationGuideCurves AnnotationGuideCurves
        {
            get
            {
                if (annotationGuideCurves == null)
                {
                    annotationGuideCurves = TimeberBrepUtility.SimpleBrepToAnnotationGuideline(timberBrep, CurveUtility.GetPlanes(baseCurveInfluenced.curve, 0.5));
                }
                return annotationGuideCurves;
            }
        }
        public void ResetAnnotationGuideline()
        {
            annotationGuideCurves = null;
        }
        //定位基线，根据长度来生成
        public BaseCurve baseCurveInfluenced { get; set; }//XY平面上的定位基线-该定位基线受出头影响，中心在线中间
        public BaseCurve baseCurve { get; set; }
        //定位基线的长度
        protected Double Linelength { get; set; }
    }

}
