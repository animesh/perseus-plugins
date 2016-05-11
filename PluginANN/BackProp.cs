using PerseusApi.Matrix;
using PerseusApi.Document;
using PerseusApi.Generic;
using BaseLibS.Param;

namespace PluginANN
{
    class BackProp : IMatrixProcessing
    {

        public string Name
        {
            get
            {
                return "BP";
            }
        }

        public string Description
        {
            get
            {
                return "Back Propagation";
            }
        }

        public float DisplayRank
        {
            get
            {
                return 42;
            }
        }

        public bool IsActive
        {
            get
            {
                return true;
            }
        }

        public bool HasButton
        {
            get
            {
                return false;
            }
        }

        public System.Drawing.Bitmap DisplayImage
        {
            get
            {
                return null;
            }
        }

        public string Heading
        {
            get
            {
                return "ArtNN";
            }
        }

        public string[] HelpDocuments
        {
            get
            {
                return new string[0];
            }
        }

        public string HelpOutput
        {
            get
            {
                return "";
            }
        }

        public string[] HelpSupplTables
        {
            get
            {
                return new string[0];
            }
        }



        public int NumDocuments
        {
            get
            {
                return 0;
            }
        }

        public int NumSupplTables
        {
            get
            {
                return 0;
            }
        }

        public string Url
        {
            get
            {
                return "https://github.com/animesh/perseus-plugins/tree/master/PluginANN";
    }
        }

        public int GetMaxThreads(Parameters parameters)
        {
            return 1;
        }

        public Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            return new Parameters(new DoubleParam("factor", 1));
        }

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {
            double fctr = param.GetParam<double>("factor").Value;
            for (int i = 0; i < mdata.RowCount; i++)
            {
                for (int j = 0; j < mdata.ColumnCount; j++)
                {
                    //    mdata.Values[i, j] += (float)fctr;
                    mdata.Values[i, j] += (float)fctr;
                }
            }
        }
    }
}