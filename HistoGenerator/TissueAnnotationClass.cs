using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace HistoGenerator
{
    class TissueAnnotationClass: IComparable<TissueAnnotationClass>
    {
        private String slideName;
        private String annotaionClass;
        private int annotaionId;
                
        public uint MeanH { get; set; } //Median H Färbung
        public uint Q25H { get; set; } //Quantil 25 H Färbung
        public uint Q75H { get; set; } //Quantil 75 H Färbung
        public uint MeanE { get; set; }  //Median E Färbung
        public uint Q25E { get; set; } //Quantil 25 E Färbung
        public uint Q75E { get; set; } //Quantil 57 E Färbung
        public uint CountCores { get; set; } //Anzahl der Zellkerne
        public uint CountLumina { get; set; } //Anzahl der Lumina
        public uint MidCoresSize { get; set; } //Durschnittliche Größe der Zellkerne
        public uint MeanCoresSize { get; set; }
        public uint Q25CoresSize { get; set; } 
        public uint Q75CoresSize { get; set; } 
        public uint MidLuminaSize { get; set; } //Durschnittliche Größe des Luminas
        public uint MeanLuminaSize { get; set; } 
        public uint Q25LuminaSize { get; set; } 
        public uint Q75LuminaSize { get; set; } 
        public uint DensityCores { get; set; } //Dichet der Zellkerne BildSize/CountCellCores
        public uint MidFormFactorCores { get; set; } //Durschnittlicher Fromfaktor der Zellkerne
        public uint MeanFormFactorCores { get; set; }
        public uint Q25FormFactorCores { get; set; }
        public uint Q75FormFactorCores { get; set; }
        public uint MidDensityLuminaCoresInNear { get; set; } //Durchshcnitt der Dichte der Zellkerne in der Nähe vom Lumina --> Mid(Range/CoresInNear)
        public uint MeanDensityLuminaCoresInNear { get; set; } 
        public uint Q25DensityLuminaCoresInNear { get; set; } 
        public uint Q75DensityLuminaCoresInNear { get; set; } 
        public uint MidDensityFormFactorLuminaCoresInNear { get; set; } //Durschnittliche  der dicht der Zellkerne in der Nähme vom Lumina im Bezug zum Formfaktor des Luminas --> Mid(Range*FormFaktor/CoresInNear)
        public uint MeanDensityFormFactorLuminaCoresInNear { get; set; } 
        public uint Q25DensityFormFactorLuminaCoresInNear { get; set; }
        public uint Q75DensityFormFactorLuminaCoresInNear { get; set; }

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
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29}",
                this.annotaionClass,
                this.Q25H,
                this.MeanH,
                this.Q75H,
                this.Q25E,
                this.MeanE,
                this.Q75E,
                this.CountCores,
                this.CountLumina,
                this.MidCoresSize,
                this.MeanCoresSize,
                this.Q25CoresSize,
                this.Q75CoresSize,
                this.MidLuminaSize,
                this.MeanLuminaSize,
                this.Q25LuminaSize,
                this.Q75LuminaSize,
                this.DensityCores,
                this.MidFormFactorCores,
                this.MeanFormFactorCores,
                this.Q25FormFactorCores,
                this.Q75FormFactorCores,
                this.MidDensityLuminaCoresInNear,
                this.MeanDensityLuminaCoresInNear,
                this.Q25DensityLuminaCoresInNear,
                this.Q75DensityLuminaCoresInNear,
                this.MidDensityFormFactorLuminaCoresInNear,
                this.MeanDensityFormFactorLuminaCoresInNear,
                this.Q25DensityFormFactorLuminaCoresInNear,
                this.Q75DensityFormFactorLuminaCoresInNear);
        }

        public static string getCsvHeadForamt()
        {
            return
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29}",
                    "Class",
                    "H - Q25",
                    "H - Median",
                    "H - Q75",
                    "E - Q25",
                    "E - Median",
                    "E - Q75",
                    "CountCores",
                    "CountLumina",
                    "MidCoresSize",
                    "MeanCoresSize",
                    "Q25CoresSize",
                    "Q75CoresSize",
                    "MidLuminaSize",
                    "MeanLuminaSize",
                    "Q25LuminaSize",
                    "Q75LuminaSize",
                    "DensityCores",
                    "MidFormFactorCores",
                    "MeanFormFactorCores",
                    "Q25FormFactorCores",
                    "Q75FormFactorCores",
                    "MidDensityLuminaCoresInNear",
                    "MeanDensityLuminaCoresInNear",
                    "Q25DensityLuminaCoresInNear",
                    "Q75DensityLuminaCoresInNear",
                    "MidDensityFormFactorLuminaCoresInNear",
                    "MeanDensityFormFactorLuminaCoresInNear",
                    "Q25DensityFormFactorLuminaCoresInNear",
                    "Q75DensityFormFactorLuminaCoresInNear");
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
