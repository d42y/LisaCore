using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Nlp.BERT
{
    internal class BertPredictions
    {
        [VectorType(1, 256)]
        [ColumnName("unstack:1")]
        public float[] EndLogits { get; set; }

        [VectorType(1, 256)]
        [ColumnName("unstack:0")]
        public float[] StartLogits { get; set; }

        [VectorType(1)]
        [ColumnName("unique_ids:0")]
        public long[] UniqueIds { get; set; }
    }
}
