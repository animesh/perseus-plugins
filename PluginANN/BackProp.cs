using PerseusApi.Matrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerseusApi.Document;
using PerseusApi.Generic;

namespace PluginANN
{
    class BackProp : IMatrixProcessing
    {
        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public System.Drawing.Bitmap DisplayImage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float DisplayRank
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool HasButton
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Heading
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string[] HelpDocuments
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string HelpOutput
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string[] HelpSupplTables
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsActive
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int NumDocuments
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int NumSupplTables
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Url
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int GetMaxThreads(global::BaseLibS.Param.Parameters parameters)
        {
            throw new NotImplementedException();
        }

        public global::BaseLibS.Param.Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            throw new NotImplementedException();
        }

        public void ProcessData(IMatrixData mdata, global::BaseLibS.Param.Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {
            throw new NotImplementedException();
        }
    }
}
