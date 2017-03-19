using System.Collections.Generic;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using BaseLibS.Num;
using BaseLibS.Util;
using System;

namespace PluginANN
{
    public class Normalize : IMatrixProcessing
    {

        public string Name => "BP";
        public string Description => "Back Propagation";
        public float DisplayRank => -1;
        public bool IsActive => true;
        public int GetMaxThreads(Parameters parameters) { return int.MaxValue; }
        public bool HasButton => true;
        public string Url => "https://github.com/animesh/perseus-plugins/tree/master/PluginANN";
        public Bitmap2 DisplayImage => null;
        public string Heading => "AddFun";
        public string HelpOutput => "";
        public string[] HelpSupplTables => new string[0];
        public int NumSupplTables => 0;
        public string[] HelpDocuments => new string[0];
        public int NumDocuments => 0;

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            Parameter<int> access = param.GetParam<int>("Matrix access");
            if (access.Value == 0)
            {
                new ThreadDistributor(processInfo.NumThreads, mdata.RowCount, i => BackPropR(i, mdata)).Start();
            }
            else
            {
                new ThreadDistributor(processInfo.NumThreads, mdata.ColumnCount, j => BackPropC(j, mdata)).Start();
            }
        }

        public Parameters GetParameters(IMatrixData mdata, ref string errorString)
        {
            return
                new Parameters(new Parameter[]{
                    new SingleChoiceParam("Matrix access"){
                        Values = new[]{"Rows", "Columns"},
                        Help = "Specifies if the analysis is performed on the rows or the columns of the matrix."
                    }
                });
        }
        private static void BackPropR(int i, IMatrixData data)
        {
            List<double> vals = new List<double>();
            List<double> ovals = new List<double>();
            for (int j = 0; j < data.ColumnCount; j++)
            {
                double q = data.Values.Get(i, j);
                if (!double.IsNaN(q) && !double.IsInfinity(q))
                {
                    vals.Add(q);
                    ovals.Add(1);
                }
                //else { ovals.Add(0); }
            }
            double[] rowv = vals.ToArray();
            double[] orowv = ovals.ToArray();
            //float rowvm = ArrayUtils.MaxInd(rowv);
            //double[] input = rowv;
            double[] input = new double[] { 0.05, 0.10 };
            double[,] inpw = new double[,] { { 0.15, 0.20 }, { 0.25, 0.3 } };
            double[] hidden = new double[2];
            double[,] hidw = new double[,] { { 0.4, 0.45 }, { 0.5, 0.55 } };
            double[] outputc = new double[2];
            //double[] outputr = orowv;
            double[] outputr = new double[] { 0.1, 0.9 };
            double[] bias = new double[] { 0.35, 0.6 };
            double[] cons = new double[] { 1, 1 };
            double lr = 0.5;
            double error = 1;
            double iter = 0;
            while (iter < 100)
            {
                iter++;
                error = 0;
                for (int n = 0; n < inpw.GetLength(0); n++)
                {
                    double collin = 0;
                    for (int m = 0; m < input.Length; m++)
                    {
                        collin += inpw[n, m] * input[m];
                    }
                    collin += bias[0] * cons[0];
                    collin = 1 / (1 + Math.Pow(Math.E, -1 * collin));
                    hidden[n] = collin;
                }
                for (int n = 0; n < hidw.GetLength(0); n++)
                {
                    double collin = 0;
                    for (int m = 0; m < hidden.Length; m++)
                    {
                        collin += hidw[n, m] * hidden[m];
                    }
                    collin += bias[1] * cons[1];
                    collin = 1 / (1 + Math.Pow(Math.E, -collin));
                    outputc[n] = collin;
                    error += Math.Pow(outputr[n] - outputc[n], 2) / 2;
                }

                for (int m = 0; m < input.Length; m++)
                {
                    for (int n = 0; n < inpw.GetLength(0); n++)
                    {
                        double delin = 0;
                        for (int o = 0; o < hidw.GetLength(0); o++)
                        {
                            delin += (outputc[o] - outputr[o]) * outputc[o] * (1 - outputc[o]) * hidw[o, n];
                        }
                        inpw[n, m] -= lr * delin * hidden[n] * (1 - hidden[n]) * input[m];
                    }
                }
                for (int m = 0; m < hidden.Length; m++)
                {
                    for (int n = 0; n < hidw.GetLength(0); n++)
                    {
                        hidw[n, m] -= lr * (outputc[n] - outputr[n]) * outputc[n] * (1 - outputc[n]) * hidden[m];
                    }
                }
            }
            float rowvm = (float)error;
            for (int j = 0; j < data.ColumnCount; j++)
            {
                data.Values.Set(i, j, data.Values.Get(i, j) * rowvm);
            }
        }
        private static void BackPropC(int j, IMatrixData data)
        {
            for (int i = 0; i < data.RowCount; i++)
            {
                data.Values.Set(i, j, data.Values.Get(i, j) * i);
            }
        }
    }
}
