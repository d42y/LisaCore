using LisaCore.Nlp.BERT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Keep this as LisaCore
namespace LisaCore
{
    /// <summary>
    /// Part of Lisa class
    /// </summary>
    public partial class Lisa
    {
        public void InitNlp(string vocabularyFilePath, string ModelPath, bool useGpu = false)
        {
            _bert = new Bert(vocabularyFilePath, ModelPath, null, useGpu);
            _chatlBot?.SetBert(_bert);
        }

        public void InitNlp(string vocabularyFilePath, string ModelPath, string contextFilePath, bool useGpu = false)
        {
            _bert = new Bert(vocabularyFilePath, ModelPath, contextFilePath, useGpu);
            _chatlBot?.SetBert(_bert);
        }

        public void LoadNlpContextFromFile(string contextFilePath)
        {
            if (_bert != null)
            {
                _bert.LoadContextFromFile(contextFilePath);
            }
            else
            {
                //thwor error
            }
        }

        public void SetNlpContext(Dictionary<int, string> context)
        {
            _bert?.SetContext(context);
            _chatlBot.SetNlpContext(context);
        }


    }
}
