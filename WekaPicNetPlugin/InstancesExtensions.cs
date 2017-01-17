/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.Adapters.PicNetML{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using global::PicNetML;
	using SharpAccessory.Imaging.Segmentation;
	using weka.core;
	/// <summary>
	/// Erweiterungen von <see cref="Instances"/>, Fassade für WEKA mit PicNetML
	/// </summary>
	public static class InstancesExtensions{
		/// <summary>
		/// schreibt ein <see cref="Instances"/> in ein WEKA Attribute-Relation File
		/// </summary>
		/// <param name="instances">Datensatz</param>
		/// <param name="fileName">Dateiname, sollte auf .arff enden</param>
		public static void SaveAsWekaArff(this Instances instances,string fileName){
			var runtime=new Runtime(instances);
			runtime.SaveToArffFile(fileName);
		}
		/// <summary>
		/// erstellt eine WEKA-<see cref="Instance"/> und fügt sie den <see cref="Instances"/> hinzu
		/// </summary>
		/// <param name="instances">Datensatz</param>
		/// <param name="className">Name der Klasse, falls null wird die erste Klasse aus dem Datensatz zugewiesen</param>
		/// <param name="features">Merkmale</param>
		/// <param name="forceAdd">wenn true wird eine unklassifizierte <see cref="Instance"/> dem Datensatz hinzugefügt, sonst wird keine <see cref="Instance"/> erzeugt</param>
		/// <returns>WEKA-<see cref="Instance"/>, eingehangen in den übergebenen Datensatz oder null</returns>
		public static Instance CreateWekaInstance(this Instances instances,string className,IEnumerable<Tuple<string,double>> features,bool forceAdd=false){
			var instance=new SparseInstance(instances.numAttributes());
			foreach(var feature in features){
				var attribute=instances.attribute(feature.Item1);
				if(null!=attribute) instance.setValue(attribute,feature.Item2);
			}
			var classIndex=null==className?-1:instances.classAttribute().indexOfValue(className);
			if(!forceAdd&&-1==classIndex) return null;
			instance.setValue(instances.classAttribute(),classIndex);
			instances.add(instance);
			return instances.lastInstance();
		}
		/// <summary>
		/// erstellt eine WEKA-<see cref="Instance"/> aus einem <see cref="ImageObject"/> und fügt sie den <see cref="Instances"/> hinzu
		/// </summary>
		/// <param name="instances">Datensatz</param>
		/// <param name="imageObject"><see cref="ImageObject"/></param>
		/// <param name="forceAdd">wenn true wird eine unklassifizierte <see cref="Instance"/> dem Datensatz hinzugefügt, sonst wird keine <see cref="Instance"/> erzeugt</param>
		/// <returns>WEKA-<see cref="Instance"/>, eingehangen in den übergebenen Datensatz oder null</returns>
		public static Instance CreateWekaInstance(this Instances instances,ImageObject imageObject,bool forceAdd=false)=>instances.CreateWekaInstance(imageObject.Class?.Name,imageObject.Features.Select(f=>Tuple.Create(f.Name,f.Value)),forceAdd);
		/// <summary>
		/// fügt alle <see cref="ImageObject"/>s des <see cref="ObjectLayer"/> zu den <see cref="Instances"/> hinzu
		/// </summary>
		/// <param name="instances">Datensatz</param>
		/// <param name="objectLayer">auszuwertender <see cref="ObjectLayer"/></param>
		public static void AddImageObjects(this Instances instances,ObjectLayer objectLayer){
			foreach(var io in objectLayer.Objects) instances.CreateWekaInstance(io);
		}
	}
}