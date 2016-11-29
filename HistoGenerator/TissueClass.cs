using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoGenerator
{
    class TissueClass
    {
        private String annotaionName;
                
        public int MeanH { get; set; }
        public int Q25H { get; set; }
        public int Q75H { get; set; }
        public int MeanE { get; set; }
        public int Q25E { get; set; }
        public int Q75E { get; set; }


        public TissueClass(){}
        public TissueClass(String annotaionName)
        {
            this.annotaionName = annotaionName;
        } 

        public void setAnnotaionName(String annotaionName)
        {
            this.annotaionName = annotaionName;
        }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6}",
                this.annotaionName,
                this.Q25H,
                this.MeanH,
                this.Q75H,
                this.Q25E,
                this.MeanE,
                this.Q75E);
        }
    }
}
