﻿namespace PerseusPluginLib.Basic{
	public class RocColumnTypeSensitivity : PerformanceColumnType{
		public override string Name => "TP/(TP+FN) (Sensitivity)";

		public override double Calculate(double tp, double tn, double fp, double fn, double np, double nn){
			return tp/(tp + fn);
		}
	}
}