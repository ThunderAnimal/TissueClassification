/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.Adapters.PicNetML {
	using System;
	using System.Collections.Generic;
	using SharpAccessory.Imaging.Classification;
	using SharpAccessory.Imaging.Segmentation;
	using weka.classifiers;
	using weka.core;
	/// <summary>
	/// Erweiterung für WEKA-Klassifikatoren
	/// </summary>
	public static class ClassifierExtensions{
		/// <summary>
		/// Klassifiziert eine Instanz
		/// </summary>
		/// <param name="classifier">Klassifikator</param>
		/// <param name="instances">dem Klassifikator zugehöriger Datensatz</param>
		/// <param name="features">Merkmale der Instanz</param>
		/// <returns>Klassenname</returns>
		public static string Classify(this Classifier classifier,Instances instances,IEnumerable<Tuple<string,double>> features){
			var instance=instances.CreateWekaInstance(null,features,true);
			var classIndex=classifier.classifyInstance(instance);
			return instances.classAttribute().value((int)classIndex);
		}
		/// <summary>
		/// Klassifiziert ein <see cref="ImageObject"/> und weist ihm die Klasse zu
		/// </summary>
		/// <param name="classifier">Klassifikator</param>
		/// <param name="instances">dem Klassifikator zugehöriger Datensatz</param>
		/// <param name="imageObject"><see cref="ImageObject"/></param>
		/// <param name="resolveClassByName">Auflösung für die Klassen</param>
		/// <returns>Klasse</returns>
		public static Class Classify(this Classifier classifier,Instances instances,ImageObject imageObject,Func<string,Class> resolveClassByName){
			var instance=instances.CreateWekaInstance(imageObject,true);
			var classIndex=classifier.classifyInstance(instance);
			var @class=resolveClassByName(instances.classAttribute().value((int)classIndex));
			imageObject.Class=@class;
			return @class;
		}
	}
}
