using System.Collections.Generic;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PluginANN
{
    public class Multiply : IMatrixProcessing
    {

        public string Name => "BP";
        public string Description => "Back Propagation";
        public float DisplayRank => 42;
        public bool IsActive => true;
        public int GetMaxThreads(Parameters parameters) => 1;
        public bool HasButton => true;
        public string Url => "https://github.com/animesh/perseus-plugins/tree/master/PluginANN";
        public Bitmap2 DisplayImage => null;
        public string Heading => "ArtNN";
        public string HelpOutput => "";
        public string[] HelpSupplTables => new string[0];
        public int NumSupplTables => 0;
        public string[] HelpDocuments => new string[0];
        public int NumDocuments => 0;

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            double factor = param.GetParam<double>("Factor").Value;
            for (int i = 0; i < mdata.ColumnCount; i++)
            {
                for (int j = 0; j < mdata.RowCount; j++)
                {
                    mdata.Values[j, i] *= (float)factor;
                }
            }
        }

        public Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            List<Parameter> tmpList = new List<Parameter> { new DoubleParam("Factor", 7) };
            return new Parameters(tmpList);
        }

    }
}
