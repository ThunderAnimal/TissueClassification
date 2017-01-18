using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;

namespace ClassificatioDataGenerator
{
    public class TissueAnnotationClass: IComparable<TissueAnnotationClass>
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
        public uint MidFormFactorLuminaWithSize { get; set; } //Durschnittlier Formfactor des Luminas in Betrachtung der Groese --> MID(Range/ 2* FormFaktor)
        public uint MeanFormFactorLuminaWithSize { get; set; } 
        public uint Q25FormFactorLuminaWithSize { get; set; } 
        public uint Q75FormFactorLuminaWithSize { get; set; } 
        public uint MidDensityLuminaCoresInNear { get; set; } //Durchshcnitt der Dichte der Zellkerne in der Nähe vom Lumina --> Mid(Range/CoresInNear)
        public uint MeanDensityLuminaCoresInNear { get; set; } 
        public uint Q25DensityLuminaCoresInNear { get; set; } 
        public uint Q75DensityLuminaCoresInNear { get; set; } 
        public uint MidDensityFormFactorLuminaCoresInNear { get; set; } //Durschnittliche  der dicht der Zellkerne in der Nähme vom Lumina im Bezug zum Formfaktor des Luminas --> Mid(Range*FormFaktor/CoresInNear)
        public uint MeanDensityFormFactorLuminaCoresInNear { get; set; } 
        public uint Q25DensityFormFactorLuminaCoresInNear { get; set; }
        public uint Q75DensityFormFactorLuminaCoresInNear { get; set; }

        public TissueAnnotationClass()
        {

        }

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
        public void setAnnotaionClass(String annotaionClass)
        {
            this.annotaionClass = annotaionClass;
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", this.annotaionClass, this.slideName, this.annotaionId);
        }

        public TissueAnnotationClass ComputeFeatureValues(Bitmap image)
        {
            return ComputeImageFeatures.ComputeFeatures(this, image);

        }
        public string getCsvFormat()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33}",
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
                this.MidFormFactorLuminaWithSize,
                this.MeanFormFactorLuminaWithSize,
                this.Q25FormFactorLuminaWithSize,
                this.Q75FormFactorLuminaWithSize,
                this.MidDensityLuminaCoresInNear,
                this.MeanDensityLuminaCoresInNear,
                this.Q25DensityLuminaCoresInNear,
                this.Q75DensityLuminaCoresInNear,
                this.MidDensityFormFactorLuminaCoresInNear,
                this.MeanDensityFormFactorLuminaCoresInNear,
                this.Q25DensityFormFactorLuminaCoresInNear,
                this.Q75DensityFormFactorLuminaCoresInNear);
        }

        public IEnumerable<Tuple<string, double>> GetFeatures()
        {
            var featureList = new List<Tuple<string, double>>
            {
                new Tuple<string, double>("H - Q25", Q25H),
                new Tuple<string, double>("H - Median", MeanH),
                new Tuple<string, double>("H - Q75", Q75H),
                new Tuple<string, double>("E - Q25", Q25E),
                new Tuple<string, double>("E - Median", MeanE),
                new Tuple<string, double>("E - Q75", Q75E),
                new Tuple<string, double>("CountCores", CountCores),
                new Tuple<string, double>("CountLumina", CountLumina),
                new Tuple<string, double>("MidCoresSize", MidCoresSize),
                new Tuple<string, double>("MeanCoresSize", MeanCoresSize),
                new Tuple<string, double>("Q25CoresSize", Q25CoresSize),
                new Tuple<string, double>("Q75CoresSize", Q75CoresSize),
                new Tuple<string, double>("MidLuminaSize", MidLuminaSize),
                new Tuple<string, double>("MeanLuminaSize", MeanLuminaSize),
                new Tuple<string, double>("Q25LuminaSize", Q25LuminaSize),
                new Tuple<string, double>("Q75LuminaSize", Q75LuminaSize),
                new Tuple<string, double>("DensityCores", DensityCores),
                new Tuple<string, double>("MidFormFactorCores", MidFormFactorCores),
                new Tuple<string, double>("MeanFormFactorCores", MeanFormFactorCores),
                new Tuple<string, double>("Q25FormFactorCores", Q25FormFactorCores),
                new Tuple<string, double>("Q75FormFactorCores", Q75FormFactorCores),
                new Tuple<string, double>("MidFormFactorLuminaWithSize", MidFormFactorLuminaWithSize),
                new Tuple<string, double>("MeanFormFactorLuminaWithSize", MeanFormFactorLuminaWithSize),
                new Tuple<string, double>("Q25FormFactorLuminaWithSize", Q25FormFactorLuminaWithSize),
                new Tuple<string, double>("Q75FormFactorLuminaWithSize", Q75FormFactorLuminaWithSize),
                new Tuple<string, double>("MidDensityLuminaCoresInNear", MidDensityLuminaCoresInNear),
                new Tuple<string, double>("MeanDensityLuminaCoresInNear", MeanDensityLuminaCoresInNear),
                new Tuple<string, double>("Q25DensityLuminaCoresInNear", Q25DensityLuminaCoresInNear),
                new Tuple<string, double>("Q75DensityLuminaCoresInNear", Q75DensityLuminaCoresInNear),
                new Tuple<string, double>("MidDensityFormFactorLuminaCoresInNear", MidDensityFormFactorLuminaCoresInNear),
                new Tuple<string, double>("MeanDensityFormFactorLuminaCoresInNear",
                    MeanDensityFormFactorLuminaCoresInNear),
                new Tuple<string, double>("Q25DensityFormFactorLuminaCoresInNear", Q25DensityFormFactorLuminaCoresInNear),
                new Tuple<string, double>("Q75DensityFormFactorLuminaCoresInNear", Q75DensityFormFactorLuminaCoresInNear)
            };


            return featureList;
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
