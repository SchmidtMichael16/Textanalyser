using Data.Contexts;
using Data.Entities;
using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.DbTasks
{
    public static class BackgroundTasks
    {
        public static void SplitNewTexts(TextContext textContext)
        {
            textContext.Texts.Where(t => t.Processed == false);

            foreach (Text text in textContext.Texts)
            {
                SeqLog.WriteNewLogMessage("{ID} - {Title} - {Processed}", text.ID, text.Title, text.Processed);
                string [] textDate = text.Data.Split('.');


                text.Processed = true;
            }

            textContext.SaveChanges();
        }
    }
}
