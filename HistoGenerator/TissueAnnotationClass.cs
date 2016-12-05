using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoGenerator
{
    class TissueAnnotationClass: IComparable<TissueAnnotationClass>
    {
        private String slideName;
        private String annotaionClass;
        private int annotaionId;
                
        public uint MeanH { get; set; }
        public uint Q25H { get; set; }
        public uint Q75H { get; set; }
        public uint MeanE { get; set; }
        public uint Q25E { get; set; }
        public uint Q75E { get; set; }

        public TissueAnnotationClass(int annotaionId, String annotaionClass, String slideName)
        {
            this.slideName = slideName;
            this.annotaionId = annotaionId;
            this.annotaionClass = annotaionClass;
        }

        public String getSlideName()
        {
            return this.slideName;
        }
        public int getAnnotaionId()
        {
            return this.annotaionId;
        }
        public String getAnnotaionClass()
        {
            return this.annotaionClass;
        }
        public void setAnnotaionClassOther()
        {
            this.annotaionClass = "other";
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", this.annotaionClass, this.slideName, this.annotaionId);
        }
        public string getCsvFormat()
        {
            return string.Format("{0};{1};{2};{3};{4};{5};{6}",
                this.annotaionClass,
                this.Q25H,
                this.MeanH,
                this.Q75H,
                this.Q25E,
                this.MeanE,
                this.Q75E);
        }

        public int CompareTo(TissueAnnotationClass other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return this.annotaionClass.CompareTo(other.annotaionClass);
        }
        public TissueAnnotationClass Clone()
        {
            return (TissueAnnotationClass)this.MemberwiseClone();
        }
    }
}
