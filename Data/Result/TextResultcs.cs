using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Result
{
    public class TextResultcs
    {
        public int TextID { get; set; }

        public int TotalScore
        {
            get
            {
                if (this.Sentences != null)
                {
                    return this.Sentences.Sum(s => s.Score);
                }

                return 0;
            }
        }

        public List<SentenceResult> Sentences { get; set; }

    }
}
