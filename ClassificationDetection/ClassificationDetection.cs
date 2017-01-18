using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Glaukopis.SlideProcessing;
using Segmentation;
using ClassificatioDataGenerator;
using weka.classifiers;
using weka.core;
using Glaukopis.Adapters.PicNetML;


namespace ClassificationDetection
{
    internal static class ClassificationDetection
    {
        private static Classifier _classifier;
        private static Instances _dataSet;

        private static void Main(string[] args)
        {
            //Eingabe Parameter
            var inPathSlide = args[0];
            var inPathDataset = args[1];
            var inPathClassifier = args[2];

            //Start der Verarbeitung
            var startProcTime = DateTime.Now;
            Console.WriteLine("Start: " + startProcTime);
            Console.WriteLine("Verarbeite Daten...");

            //Verarbeitung

            //Load Dataset
            _dataSet = Glaukopis.Adapters.PicNetML.Util.LoadInstancesFromWekaArff(inPathDataset);

            //Load Classifier
            _classifier = (Classifier)SerializationHelper.read(inPathClassifier);

            
            foreach (var slideName in Glaukopis.SlideProcessing.Util.GetSlideFilenames(new string[] {inPathSlide}))
            {
                using (var slideCache = new SlideCache(slideName))
                {
                    //Compute Class
                    var tissueSlidePartitioner = new SlidePartitioner<TissuAnnotaionEnum>(slideCache.Slide, 1f,
                        new Size(300, 300));
                    ComputeClass(tissueSlidePartitioner, slideCache);

                    //Draw Heatmaps
                    drawHeatMapAll(tissueSlidePartitioner, slideCache);
                }
            }
          

            //Ender der Verarbeitung
            var endProcTime = DateTime.Now;
            Console.WriteLine("\n############ FINISHED ###############");
            Console.WriteLine("Ende: " + endProcTime);

            //FINISH
            Console.Write("Benötigte Zeit: {0:F} Minuten", endProcTime.Subtract(startProcTime).TotalMinutes);
            Console.ReadKey();
        }

        private static void ComputeClass(SlidePartitioner<TissuAnnotaionEnum> tissueSlidePartitioner,
            SlideCache slideCache)
        {
            Parallel.ForEach(tissueSlidePartitioner.Values, (tile) =>
            {
                using (var tileImage = slideCache.Slide.GetImagePart(tile))
                {
                    var classStr = "";
                    if (!ComputeImageFeatures.IsTissue(tileImage))
                    {
                        tile.Data = TissuAnnotaionEnum.NoTissue;
                        classStr = "NoTissue";
                    }
                    else
                    {
                        var tissueAnnotation = new TissueAnnotationClass();
                        tissueAnnotation = tissueAnnotation.ComputeFeatureValues(tileImage);
                        classStr = _classifier.Classify(_dataSet,
                            tissueAnnotation.GetFeatures());
                        switch (classStr)
                        {
                            case "Fettgewebe": tile.Data = TissuAnnotaionEnum.Fettgewebe;
                                break;
                            case "Tumor":
                                tile.Data = TissuAnnotaionEnum.Tumor;
                                break;
                            case "Entzündung":
                                tile.Data = TissuAnnotaionEnum.Entzuendung;
                                break;
                            case "Gefäß":
                                tile.Data = TissuAnnotaionEnum.Gefaess;
                                break;
                            case "Mikrokalk":
                                tile.Data = TissuAnnotaionEnum.Mikrokalk;
                                break;
                            case "Nerv":
                                tile.Data = TissuAnnotaionEnum.Nerv;
                                break;
                            case "Stroma":
                                tile.Data = TissuAnnotaionEnum.Stroma;
                                break;
                            case "DCIC":
                                tile.Data = TissuAnnotaionEnum.DCIC;
                                break;
                            case "Kalk":
                                tile.Data = TissuAnnotaionEnum.Kalk;
                                break;
                            case "Normales Mammaepithel":
                                tile.Data = TissuAnnotaionEnum.NormalesMammaepithel;
                                break;
                            default: tile.Data = TissuAnnotaionEnum.UnknowClass;
                                break;
                        }
                    }
                    Console.WriteLine(slideCache.SlideName + "-" + tile.Index + " done - Class: " + classStr);
                }
            });
        }

        private static void drawHeatMapAll(SlidePartitioner<TissuAnnotaionEnum> tissueSlidePartitioner, SlideCache slideCache)
        {
            Func<TissuAnnotaionEnum, Color> f = annotaion =>
            {
                switch (annotaion)
                {
                    case TissuAnnotaionEnum.Fettgewebe:
                        return Color.White;
                    case TissuAnnotaionEnum.Tumor:
                        return Color.Red;
                    case TissuAnnotaionEnum.Entzuendung:
                        return Color.Aqua;
                    case TissuAnnotaionEnum.Gefaess:
                        return Color.Green;
                    case TissuAnnotaionEnum.Mikrokalk:
                        return Color.BlueViolet;
                    case TissuAnnotaionEnum.Nerv:
                        return Color.SaddleBrown;
                    case TissuAnnotaionEnum.Stroma:
                        return Color.Yellow;
                    case TissuAnnotaionEnum.DCIC:
                        return Color.SpringGreen;
                    case TissuAnnotaionEnum.Kalk:
                        return Color.Tan;
                    case TissuAnnotaionEnum.NormalesMammaepithel:
                        return Color.DarkOrange;
                    case TissuAnnotaionEnum.NoTissue:
                        return Color.Gray;
                    case TissuAnnotaionEnum.UnknowClass:
                        return Color.Black;
                    default:
                        return Color.Black;
                }
            };
            using (var heatMap = tissueSlidePartitioner.GenerateHeatMap(f))
            {
                slideCache.SetImage("ClassificationDetection", heatMap);
            }
        }

        private static void drawHeatMapOneClass(SlidePartitioner<TissuAnnotaionEnum> tissueSlidePartitioner, SlideCache slideCache, TissuAnnotaionEnum annotaion)
        {
            
        }

    }

}
