using System.Collections.Generic;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using BaseLibS.Num;

namespace PluginQNorm
{
    public class Normalize : IMatrixProcessing
    {

        public string Name => "Qnorm";
        public string Description => "Quantile Normalization";
        public float DisplayRank => 42;
        public bool IsActive => true;
        public int GetMaxThreads(Parameters parameters) => 1;
        public bool HasButton => true;
        public string Url => "https://github.com/animesh/perseus-plugins/PluginQNorm";
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
            IMatrixData mdatac = (IMatrixData)mdata.Clone();
            if (access.Value==0)
            {
                for (int i = 0; i < mdata.RowCount; i++)
                {
                    List<float> v = new List<float>();
                    foreach (double f in mdata.Values.GetRow(i))
                    {
                        if (!double.IsNaN(f) && !double.IsInfinity(f))
                        {
                            v.Add((float)f);
                        }
                    }
                    float[] dr = v.ToArray();
                    System.Array.Sort(dr);
                    for (int j = 0; j < mdatac.ColumnCount; j++)
                    {
                        mdatac.Values.Set(i, j, dr[j]);
                    }
                }
                List<float> vm = new List<float>();
                for (int i = 0; i < mdatac.ColumnCount; i++)
                {
                    List<float> v = new List<float>();
                    foreach (double f in mdatac.Values.GetColumn(i))
                    {
                        if (!double.IsNaN(f) && !double.IsInfinity(f))
                        {
                            v.Add((float)f);
                        }
                    }
                    double meanc = ArrayUtils.Mean(v);
                    vm.Add((float)meanc);
                }
                float[] drm = vm.ToArray();
                for (int i = 0; i < mdata.RowCount; i++)
                {
                    List<float> v = new List<float>();
                    foreach (double f in mdata.Values.GetRow(i))
                    {
                        if (!double.IsNaN(f) && !double.IsInfinity(f))
                        {
                            v.Add((float)f);
                        }
                    }
                    float[] dr = v.ToArray();
                    double[] rankr = ArrayUtils.Rank(dr);
                    for (int j = 0; j < mdata.ColumnCount; j++)
                    {
                        mdata.Values.Set(i, j, drm[(int)System.Math.Floor(rankr[j])]);
                    }
                }

            }
            else
            {
                for (int i = 0; i < mdata.ColumnCount; i++)
                {
                    List<float> v = new List<float>();
                    foreach (double f in mdata.Values.GetColumn(i))
                    {
                        if (!double.IsNaN(f) && !double.IsInfinity(f))
                        {
                            v.Add((float)f);
                        }
                    }
                    float[] dr = v.ToArray();
                    System.Array.Sort(dr);
                    for (int j = 0; j < mdatac.RowCount; j++)
                    {
                        mdatac.Values.Set(j, i, dr[j]);
                    }
                }
                List<float> vm = new List<float>();
                for (int i = 0; i < mdatac.RowCount; i++)
                {
                    List<float> v = new List<float>();
                    foreach (double f in mdatac.Values.GetRow(i))
                    {
                        if (!double.IsNaN(f) && !double.IsInfinity(f))
                        {
                            v.Add((float)f);
                        }
                    }
                    double meanc = ArrayUtils.Mean(v);
                    vm.Add((float)meanc);
                }
                float[] drm = vm.ToArray();
                for (int i = 0; i < mdata.ColumnCount; i++)
                {
                    List<float> v = new List<float>();
                    foreach (double f in mdata.Values.GetColumn(i))
                    {
                        if (!double.IsNaN(f) && !double.IsInfinity(f))
                        {
                            v.Add((float)f);
                        }
                    }
                    float[] dr = v.ToArray();
                    double[] rankr = ArrayUtils.Rank(dr);
                    for (int j = 0; j < mdata.RowCount; j++)
                    {
                        mdata.Values.Set(j, i, drm[(int)System.Math.Floor(rankr[j])]);
                    }
                }

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

    }
}
