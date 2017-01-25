using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassificatioDataGenerator;

namespace ClassificationDetection
{
    public class ClassificationOwn
    {
        /// <summary>
        /// Nachbildung von WEKA-Classifier J48
        /// </summary>
        /// <param name="tissueAnnotation"></param>
        /// <returns></returns>
        public static TissuAnnotaionEnum ClassifyJ48(TissueAnnotationClass tissueAnnotation)
        {
            if (tissueAnnotation == null)
            {
                return TissuAnnotaionEnum.NoTissue;
            }

            if (tissueAnnotation.MeanE <= 26)
            {
                if (tissueAnnotation.Q25E <= 11)
                    return TissuAnnotaionEnum.Fettgewebe;
                else
                    return TissuAnnotaionEnum.Stroma;
            }
            else
            {
                if (tissueAnnotation.Q75LuminaSize <= 706)
                {
                    if (tissueAnnotation.Q75H <= 44)
                        return TissuAnnotaionEnum.Mikrokalk;
                    else
                        return TissuAnnotaionEnum.Kalk;
                }
                else
                {
                    if (tissueAnnotation.MidCoresSize <= 383)
                    {
                        if (tissueAnnotation.MidDensityFormFactorLuminaCoresInNear <= 304004)
                        {
                            if (tissueAnnotation.Q25FormFactorCores <= 0)
                                return TissuAnnotaionEnum.Tumor;
                            else
                            {
                                if (tissueAnnotation.Q25H <= 7)
                                {
                                    if (tissueAnnotation.Q75DensityFormFactorLuminaCoresInNear <= 24175)
                                    {
                                        if (tissueAnnotation.MidLuminaSize <= 3029)
                                            return TissuAnnotaionEnum.Gefaess;
                                        else
                                            return TissuAnnotaionEnum.NormalesMammaepithel;
                                    }
                                    else
                                    {
                                        if (tissueAnnotation.CountCores <= 2397)
                                        {
                                            if (tissueAnnotation.MeanCoresSize <= 280)
                                                return TissuAnnotaionEnum.Gefaess;
                                            else
                                                return TissuAnnotaionEnum.Tumor;
                                        }
                                        else
                                        {
                                            if (tissueAnnotation.MidFormFactorCores <= 21)
                                            {
                                                if (tissueAnnotation.CountLumina <= 39368)
                                                    return TissuAnnotaionEnum.Gefaess;
                                                else
                                                    return TissuAnnotaionEnum.Nerv;
                                            }
                                            else
                                            {
                                                return TissuAnnotaionEnum.Gefaess;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return TissuAnnotaionEnum.Tumor;
                                }
                            }

                        }
                        else
                        {
                            if (tissueAnnotation.CountCores <= 17910)
                            {
                                if (tissueAnnotation.Q25E <= 28)
                                    return TissuAnnotaionEnum.Stroma;
                                else
                                    return TissuAnnotaionEnum.Gefaess;
                            }
                            else
                            {
                                return TissuAnnotaionEnum.Fettgewebe;
                            }
                        }
                    }
                    else
                    {
                        if (tissueAnnotation.Q75E <= 89)
                        {
                            if (tissueAnnotation.Q75LuminaSize <= 3126)
                            {
                                if (tissueAnnotation.Q25CoresSize <= 235)
                                    return TissuAnnotaionEnum.Stroma;
                                else
                                    return TissuAnnotaionEnum.Tumor;
                            }
                            else
                            {
                                if (tissueAnnotation.Q25H <= 1)
                                {
                                    if (tissueAnnotation.Q75E <= 52)
                                    {
                                        if (tissueAnnotation.CountCores <= 6595)
                                            return TissuAnnotaionEnum.Tumor;
                                        else
                                            return TissuAnnotaionEnum.Stroma;
                                    }
                                    else
                                    {
                                        if (tissueAnnotation.CountCores <= 2721)
                                        {
                                            if (tissueAnnotation.MidCoresSize <= 415)
                                            {
                                                return TissuAnnotaionEnum.Nerv;
                                            }
                                            else
                                            {
                                                if (tissueAnnotation.Q75DensityFormFactorLuminaCoresInNear <= 51892)
                                                    return TissuAnnotaionEnum.Gefaess;
                                                else
                                                    return TissuAnnotaionEnum.Tumor;
                                            }
                                        }
                                        else
                                        {
                                            if (tissueAnnotation.Q25E <= 17)
                                            {
                                                if (tissueAnnotation.Q75H <= 10)
                                                    return TissuAnnotaionEnum.Gefaess;
                                                else
                                                    return TissuAnnotaionEnum.Tumor;
                                            }
                                            else
                                            {
                                                return TissuAnnotaionEnum.Tumor;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (tissueAnnotation.Q25H <= 0)
                                        return TissuAnnotaionEnum.NormalesMammaepithel;
                                    else
                                        return TissuAnnotaionEnum.Gefaess;
                                }
                            }
                        }
                        else
                        {
                            if (tissueAnnotation.MeanDensityLuminaCoresInNear <= 148)
                            {
                                if (tissueAnnotation.MidFormFactorLuminaWithSize <= 20)
                                {
                                    if (tissueAnnotation.MeanFormFactorLuminaWithSize <= 9)
                                    {
                                        if (tissueAnnotation.MeanH <= 23)
                                        {
                                            if (tissueAnnotation.MidDensityLuminaCoresInNear <= 202)
                                                return TissuAnnotaionEnum.Stroma;
                                            else
                                                return TissuAnnotaionEnum.Tumor;
                                        }
                                        else
                                        {
                                            return TissuAnnotaionEnum.Tumor;
                                        }
                                    }
                                    else
                                    {
                                        if (tissueAnnotation.Q75FormFactorCores <= 23)
                                            return TissuAnnotaionEnum.NormalesMammaepithel;
                                        else
                                        {
                                            return TissuAnnotaionEnum.Tumor;
                                        }
                                    }
                                }
                                else
                                {
                                    if (tissueAnnotation.Q75E <= 97)
                                    {
                                        return TissuAnnotaionEnum.Tumor;
                                    }
                                    else
                                    {
                                        if (tissueAnnotation.CountCores <= 2010)
                                            return TissuAnnotaionEnum.NormalesMammaepithel;
                                        else
                                            return TissuAnnotaionEnum.DCIC;
                                    }
                                }
                            }
                            else
                            {
                                return TissuAnnotaionEnum.Tumor;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nachbildung vom WEKA-Classifier JRip
        /// </summary>
        /// <param name="tissueAnnotation"></param>
        /// <returns></returns>
        public static TissuAnnotaionEnum ClassifyJRip(TissueAnnotationClass tissueAnnotation)
        {

            if(tissueAnnotation == null)
                return TissuAnnotaionEnum.NoTissue;
            if(tissueAnnotation.Q25E >= 92 && tissueAnnotation.MeanH <= 16)
                return TissuAnnotaionEnum.Mikrokalk;
            if(tissueAnnotation.Q75LuminaSize >= 98155 && tissueAnnotation.Q75LuminaSize <= 142864 && tissueAnnotation.MidCoresSize <= 412)
                return TissuAnnotaionEnum.Nerv;
            if(tissueAnnotation.MidDensityLuminaCoresInNear <= 230 && tissueAnnotation.Q75LuminaSize >= 3000 && tissueAnnotation.Q25H <= 6 &&  tissueAnnotation.MeanH >= 18)
                return TissuAnnotaionEnum.NormalesMammaepithel;
            if(tissueAnnotation.MidLuminaSize <= 706)
                return TissuAnnotaionEnum.Kalk;
            if(tissueAnnotation.Q75E >= 145 && tissueAnnotation.Q25H <= 0)
                return TissuAnnotaionEnum.Kalk;
            if (tissueAnnotation.MidLuminaSize >= 189386 && tissueAnnotation.MeanLuminaSize <= 944)
                return TissuAnnotaionEnum.Stroma;
            if(tissueAnnotation.Q75E<= 88 && tissueAnnotation.MeanH >= 10 && tissueAnnotation.MidLuminaSize >= 5930 && tissueAnnotation.Q25E >= 27)
                return TissuAnnotaionEnum.Stroma;
            if(tissueAnnotation.MidCoresSize <= 383 && tissueAnnotation.Q25E >= 11 && tissueAnnotation.Q25DensityLuminaCoresInNear >= 124)
                return TissuAnnotaionEnum.Gefaess;
            if(tissueAnnotation.MeanE <= 24)
                return TissuAnnotaionEnum.Fettgewebe;
            if(tissueAnnotation.CountCores >= 8564 && tissueAnnotation.Q25E <= 11)
                return TissuAnnotaionEnum.Fettgewebe;

            return TissuAnnotaionEnum.Tumor;
        }
    }
}
