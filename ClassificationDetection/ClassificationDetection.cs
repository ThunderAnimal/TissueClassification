using System;
using System.Collections.Generic;
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
                    var tissueSlidePartitioner = new SlidePartitioner<Dictionary<string, TissuAnnotaionEnum>>(slideCache.Slide, 1f,
                        new Size(500, 500));
                    ComputeClass(tissueSlidePartitioner, slideCache);

                    //Draw Heatmaps
                    DrawHeatMapAll(tissueSlidePartitioner, slideCache);
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

        private static void ComputeClass(SlidePartitioner<Dictionary<string, TissuAnnotaionEnum>> tissueSlidePartitioner,
            SlideCache slideCache)
        {
            Parallel.ForEach(tissueSlidePartitioner.Values, (tile) =>
            {
                using (var tileImage = slideCache.Slide.GetImagePart(tile))
                {
                    var tissueAnnotaion = ComputeFeatues(tileImage);
                    var classifyDic = new Dictionary<string, TissuAnnotaionEnum>();

                    classifyDic.Add("ownClassificationJ48", ClassificationOwn.ClassifyJ48(tissueAnnotaion));
                    classifyDic.Add("ownClassifyJRip", ClassificationOwn.ClassifyJRip(tissueAnnotaion));

                    //Funktioiert leider nicht, immer leer ""
                    //classifyDic.Add("wekaClassifyBagging",ClassifyWeka(tissueAnnotaion, _classifier, _dataSet));

                    tile.Data = classifyDic;
                    Console.WriteLine(slideCache.SlideName + "-" + tile.Index + " done - Class: " + showOutput(tile.Data["ownClassificationJ48"]));
                }
            });
        }

        private static TissueAnnotationClass ComputeFeatues(Bitmap image)
        {
            if (!ComputeImageFeatures.IsTissue(image))
            {
                return null;
            }
            else
            {
                var tissueAnnotation = new TissueAnnotationClass();
                tissueAnnotation = tissueAnnotation.ComputeFeatureValues(image);
                return tissueAnnotation;
            }
        }

        private static TissuAnnotaionEnum ClassifyWeka(TissueAnnotationClass tissueAnnotation, Classifier classifier, Instances dataSet)
        {
            if (tissueAnnotation == null)
            {
                return TissuAnnotaionEnum.NoTissue;
            }
            if (tissueAnnotation.GetFeatures() == null)
            {
                return TissuAnnotaionEnum.UnknowClass;
            }
            switch (ClassifierExtensions.Classify(classifier, dataSet,
                            tissueAnnotation.GetFeatures()))
            {
                case "Fettgewebe": return TissuAnnotaionEnum.Fettgewebe;
                case "Tumor": return TissuAnnotaionEnum.Tumor;
                case "Entzündung": return TissuAnnotaionEnum.Entzuendung;
                case "Gefäß": return TissuAnnotaionEnum.Gefaess;
                case "Mikrokalk": return TissuAnnotaionEnum.Mikrokalk;
                case "Nerv": return TissuAnnotaionEnum.Nerv;
                case "Stroma": return TissuAnnotaionEnum.Stroma;
                case "DCIC": return TissuAnnotaionEnum.DCIC;
                case "Kalk": return TissuAnnotaionEnum.Kalk;
                case "Normales Mammaepithel": return TissuAnnotaionEnum.NormalesMammaepithel;
                default: return TissuAnnotaionEnum.UnknowClass;
            }
        }

        private static String showOutput(TissuAnnotaionEnum annotaion)
        {
            switch (annotaion)
            {
                case TissuAnnotaionEnum.Fettgewebe:
                    return "Fettgewebe";
                case TissuAnnotaionEnum.Tumor:
                    return "Tumor";
                case TissuAnnotaionEnum.Entzuendung:
                    return "Entzündung";
                case TissuAnnotaionEnum.Gefaess:
                    return "Gefäß";
                case TissuAnnotaionEnum.Mikrokalk:
                    return "Mikrokalk";
                case TissuAnnotaionEnum.Nerv:
                    return "Nerv";
                case TissuAnnotaionEnum.Stroma:
                    return "Stroma";
                case TissuAnnotaionEnum.DCIC:
                    return "DCIC";
                case TissuAnnotaionEnum.Kalk:
                    return "Kalk";
                case TissuAnnotaionEnum.NormalesMammaepithel:
                    return "Normales Mammaepithel";
                case TissuAnnotaionEnum.NoTissue:
                    return "Kein Gewebe";
                case TissuAnnotaionEnum.UnknowClass:
                    return "Keine Zuordnung";
                default:
                    return "Keine Zuordnung";
            }
        }

        private static void DrawHeatMapAll(SlidePartitioner<Dictionary<String, TissuAnnotaionEnum>> tissueSlidePartitioner, SlideCache slideCache)
        {
            Func<Dictionary<String, TissuAnnotaionEnum>, Color> drawWekaClassificationBaggingFunc = classifyDic => TissuAnnotaionToColor(classifyDic["wekaClassifyBagging"]);
            Func< Dictionary < String, TissuAnnotaionEnum >, Color > drawOwnClassificationJ48Func = classifyDic => TissuAnnotaionToColor(classifyDic["ownClassificationJ48"]);
            Func<Dictionary<String, TissuAnnotaionEnum>, Color> drawOwnClassificationJRipFunc = classifyDic => TissuAnnotaionToColor(classifyDic["ownClassifyJRip"]);
/*            using (var heatMap = tissueSlidePartitioner.GenerateHeatMap(drawWekaClassificationBaggingFunc))
            {
                slideCache.SetImage("ClassificationDetection_wekaClassifyBagging", heatMap);
            }*/
            using (var heatMap = tissueSlidePartitioner.GenerateHeatMap(drawOwnClassificationJ48Func))
            {
                slideCache.SetImage("ClassificationDetection_ownClassifyJ48", heatMap);
            }
            using (var heatMap = tissueSlidePartitioner.GenerateHeatMap(drawOwnClassificationJRipFunc))
            {
                slideCache.SetImage("ClassificationDetection_ownClassifyJRip", heatMap);
            }
        }

        private static void DrawHeatMapOneClass(SlidePartitioner<Dictionary<String, TissuAnnotaionEnum>> tissueSlidePartitioner, SlideCache slideCache, TissuAnnotaionEnum annotaion)
        {
            
        }

        private static Color TissuAnnotaionToColor(TissuAnnotaionEnum annotaion)
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
        }
    }

}
