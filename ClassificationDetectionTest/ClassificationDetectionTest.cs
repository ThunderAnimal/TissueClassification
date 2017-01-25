using System;
using System.Drawing;
using ClassificatioDataGenerator;
using weka.classifiers;
using weka.core;
using Glaukopis.Adapters.PicNetML;
using ClassificationDetection;

namespace ClassificationDetectionTest
{
    internal static class ClassificationDetectionTest
    {
        private static void Main(string[] args)
        {
            //Eingabe Parameter
            var inPathPicture = args[0];
            var inPathDataset = args[1];
            //J48
            var inPathClassifierTree = args[2];
            //Bagging
            var inPathClassifierMeta = args[3];
            //PART
            var inPathClassifierRules = args[4];
            //lBk
            //var inPathClassifierLayz = args[5];
            //SimpleLogistic
            //var inPathClassifierFunctions = args[6];
            //BayesNet
            //var inPathClassifierBayes = args[7];

            //Start der Verarbeitung
            var startProcTime = DateTime.Now;
            Console.WriteLine("Start: " + startProcTime);
            Console.WriteLine("Verarbeite Daten...");

            //Verarbeitung

            //Load Dataset
            var dataSet = Glaukopis.Adapters.PicNetML.Util.LoadInstancesFromWekaArff(inPathDataset);

            //Load Classifier
            var classifierTree = (Classifier)SerializationHelper.read(inPathClassifierTree);
            var classifierMeta = (Classifier)SerializationHelper.read(inPathClassifierMeta);
            var classifierRules = (Classifier)SerializationHelper.read(inPathClassifierRules);
            //var classifierLayz = (Classifier)SerializationHelper.read(inPathClassifierLayz);
            //var classifierFunctions = (Classifier)SerializationHelper.read(inPathClassifierFunctions);
            //var classifierBayes = (Classifier)SerializationHelper.read(inPathClassifierBayes);

            var image = new Bitmap(inPathPicture);
            var tissueAnnotaionClass = ComputeFeatues(image);

            Console.WriteLine("Result Tree: {0}", showOutput(ComputeClass(tissueAnnotaionClass,classifierTree, dataSet)));
            Console.WriteLine("Result Meta: {0}", showOutput(ComputeClass(tissueAnnotaionClass, classifierMeta, dataSet)));
            Console.WriteLine("Result Rules: {0}", showOutput(ComputeClass(tissueAnnotaionClass, classifierRules, dataSet)));
            Console.WriteLine("Result OWN J48: {0}", showOutput(ClassificationOwn.ClassifyJ48(tissueAnnotaionClass)));
            Console.WriteLine("Result OWN JRip: {0}", showOutput(ClassificationOwn.ClassifyJRip(tissueAnnotaionClass)));
            //Console.WriteLine("Result Layz: {0}", showOutput(ComputeClass(tissueAnnotaionClass, classifierLayz, dataSet)));
            //Console.WriteLine("Result Functions: {0}", showOutput(ComputeClass(tissueAnnotaionClass, classifierFunctions, dataSet)));
            //Console.WriteLine("Result Bayes: {0}", showOutput(ComputeClass(tissueAnnotaionClass, classifierBayes, dataSet)));

            //Ender der Verarbeitung
            var endProcTime = DateTime.Now;
            Console.WriteLine("\n############ FINISHED ###############");
            Console.WriteLine("Ende: " + endProcTime);

            //FINISH
            Console.Write("Benötigte Zeit: {0:F} Minuten", endProcTime.Subtract(startProcTime).TotalMinutes);
            Console.ReadKey();
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
        private static TissuAnnotaionEnum ComputeClass(TissueAnnotationClass tissueAnnotation, Classifier classifier, Instances dataSet)
        {
            if (tissueAnnotation == null)
            {
                return TissuAnnotaionEnum.NoTissue;
            }
            else
            {
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


    }

}
